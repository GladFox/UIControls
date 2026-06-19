# Tech Context

## Окружение
- Unity Editor `6000.4.9f1` (проект апгрейднут с 6000.3.x).
- UPM-пакет `com.gladfox.uicontrols`, версия `0.20.0`.
- Платформа разработки: Windows; сборки сцен — через Unity `-batchmode`.

## Зависимости
- Unity `uGUI` (`com.unity.ugui`)
- TextMeshPro (`com.unity.textmeshpro` `3.0.6`)
- DOTween (`DG.Tweening`, `DOTween.dll`)
- Unity Input System package (для `InputSystemUIInputModule` в demo EventSystem; билдеры подключают
  модуль рефлексией с fallback на `StandaloneInputModule`)

## Модуль
- Runtime asmdef: `Assets/UIControls/Runtime/UIControls.Runtime.asmdef`
  - references: `UnityEngine.UI`, `Unity.TextMeshPro`
  - `overrideReferences: true`, `precompiledReferences: ["DOTween.dll"]`
- Editor asmdef: `Assets/UIControls/Editor/UIControls.Editor.asmdef`
  - references: `UIControls.Runtime`, `Unity.TextMeshPro`
- Важно: модульные DOTween-шорткаты (`DOAnchorPos*`, `DOSizeDelta`, `Graphic.DOColor`) недоступны
  без `DOTween.Modules.asmdef` → используется `UIDOTweenUtility` (`DOTween.To`).

## Конвейер сборки демо-сцен
- Точка входа: `Unity -batchmode -quit -projectPath <proj> -executeMethod
  UIControls.Editor.<Builder>.Create<Xxx>DemoSceneBatch -logFile <log>`.
- Запуск билдеров строго по одному с ожиданием выхода процесса (lockfile `Temp/UnityLockfile`,
  лицензионный хендшейк не терпят параллельных инстансов).
- Проверка успеха: в логе строка `Demo scene created: …` и отсутствие `error CS`.
- Регрессионная проверка интерактива: editor-зонд с `EnterPlaymode` (DisableDomainReload) +
  `Debug.Log` позиций/альфы, завершение через `EditorApplication.Exit(0)`.

## Расширяемость
- Переиспользуемые visual state через `UIStateVisualAsset` (`ScriptableObject`).
- Переиспользуемые button profiles через `UIButtonAnimationProfile` (`ScriptableObject`).
- Кастомные действия кнопки через наследование `UIButtonCustomAction` (`ScriptableObject`) с хуками:
  - `OnPointerEnter/Exit/Down/Up`
  - `OnSubmit`
  - `OnClick`
  - `OnStateChanged`
- Trigger-ориентированные SO-анимации кнопки:
  - `UIButtonScalePulseAction`
  - `UIButtonAnchoredOffsetAction`
  - `UIButtonActionTriggerFlags`
- ProgressBar v2:
  - комбинируемые режимы `useSegments` + `useHitBar` в одном `UIProgressBarControl`
  - SO-хуки через `UIProgressBarCustomAction`
  - события `OnSegmentCompleted`, `OnEchoStarted`, `OnEchoCompleted`

## Контент-библиотеки
- Библиотека пресетов кнопок:
  - `Assets/UIControls/Animations/UI/Button/States/*`
  - `Assets/UIControls/Animations/UI/Button/Actions/*`
  - `Assets/UIControls/Animations/UI/Button/Profiles/*`
- Editor generator:
  - `UIControls.Editor.UIControlsButtonAnimationLibraryBuilder`
  - menu: `UIControls/Create Button Animation Library`

## Политика
- Внешние UI-библиотеки не используются как runtime-зависимость, только как референс.
- Новые внешние зависимости добавляются только с фиксацией в этом файле.
