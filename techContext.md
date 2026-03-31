# Tech Context

## Зависимости
- Unity `uGUI`
- DOTween (`DG.Tweening`)
- Unity Input System package (для `InputSystemUIInputModule` в demo EventSystem)

## Модуль
- Runtime asmdef: `Assets/UIControls/Runtime/UIControls.Runtime.asmdef`
- Runtime references:
  - `Unity.ugui`
  - `DOTween.Modules`

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
