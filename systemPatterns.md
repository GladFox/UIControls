# System Patterns

## Базовые паттерны
- Модульная организация: `Core`, `Controls`, `Demo` внутри runtime-модуля; editor-утилиты вынесены в `Assets/UIControls/Editor`.
- Разделение ответственности: состояние контрола отделено от анимационной реализации.
- Единый стиль tween-настроек через `UITweenSettings`.

## Паттерны состояния
- Кнопки работают через конечный набор UI-состояний (`Normal/Hover/Pressed/Disabled`).
- Визуальные состояния кнопки задаются `UIButtonAnimationProfile` (`ScriptableObject`) или локальными `UIStateVisualAsset` с fallback.
- `UIButtonAnimationProfile` инкапсулирует:
  - ссылки на `Normal/Hover/Pressed/Disabled` state assets
  - общий набор `UIButtonCustomAction[]` для переиспользования на разных префабах
- `Toggle/Modal/ProgressBar` используют явные state-transition методы (`SetIsOn`, `SetVisible`, `SetValue`).
- `UIProgressBarControl` поддерживает комбинируемые режимы (`useSegments`, `useHitBar`) в одном контроле.

## Паттерны расширения
- Для кнопки предусмотрен расширяемый SO-хук `UIButtonCustomAction`.
- SO-хук поддерживает событийные точки `PointerEnter/Exit/Down/Up`, `Submit`, `Click`, `StateChanged`.
- Для событийных SO-анимаций используются trigger flags (`UIButtonActionTriggerFlags`), что позволяет переиспользовать один action asset на разных событиях.
- Для прогрессбара предусмотрен SO-хук `UIProgressBarCustomAction` с событиями value/segment/echo.
- Стандартные события (`UnityEvent`) и кастомные SO-действия работают одновременно, не исключая друг друга.
- Кастомные действия должны быть изолированы: ошибка в одном действии не должна ломать выполнение кнопки целиком.

## Паттерны API
- Доступ из кода + Inspector (`SerializeField` + публичные методы).
- Сигнализация состояния через `UnityEvent`.
- Минимальная скрытая магия: все важные ссылки (`Graphic`, `RectTransform`, `CanvasGroup`) можно переопределить вручную.

## Паттерны пресетов
- Пресеты кнопок (`Tap`, `Touch`) хранятся в `Assets/UIControls/Animations/UI/Button`.
- Пресеты состоят из трех слоев: `States` + `Actions` + `Profiles`.
- Библиотека может быть пересобрана editor-утилитой `UIControlsButtonAnimationLibraryBuilder`.

## Паттерны demo
- Demo-сцена и demo-префабы строятся как статичные объекты сцены/префабов.
- `Canvas`, `Camera`, `EventSystem` и demo-контролы не создаются в runtime.
- Все настраиваемые параметры задаются через сериализуемые поля компонентов.
- Базовый demo (`UIControlsDemo`) и специализированные demo разделяются по отдельным сценам и builder-скриптам.

## Паттерны генерации сцен (editor builders)
- На каждый контрол — `UIXxxDemoSceneBuilder` с `[MenuItem]` и batch-точкой `CreateXxxDemoSceneBatch()`.
- Сцены генерируются детерминированно через `-batchmode -executeMethod ...CreateXxxDemoSceneBatch`,
  сохраняются в `Assets/Scenes/<категория>/` и копируются в `Samples~/DemoScenes/Scenes/<категория>/`.
- Категории E–I используют общий `UIDemoSceneFactory` (камера/canvas/панель, `Text`/`Button`/`Image`/
  `InputField`, `SetRef`/`SetRefArray`/`SetStringArray`, `Save` c авто-папкой и Build Settings).
- Билдеры запускаются по одному с ожиданием выхода процесса Unity (lockfile/лицензия не любят
  параллельный запуск). Для регрессий применяется play-mode зонд (editor-метод + `EnterPlaymode`).

## Паттерны анимации UI
- UI-твины (`anchoredPosition`, `sizeDelta`, `Graphic.color`, `CanvasGroup.alpha`) — только через
  `UIDOTweenUtility` (модульные шорткаты недоступны без `DOTween.Modules.asmdef`).
- `Transform.DOScale` / `DOLocalRotate` / `DOKill` доступны и используются напрямую.
- Анимации, не зависящие от timescale, ставят `SetUpdate(true)`.
- Визуальные контролы без спрайтов/шейдеров генерируют текстуры/спрайты в рантайме
  (`Texture2D`/`Sprite.Create`): ColorPicker (HSV/hue), CircularProgress (кольцо), Ripple (круг),
  Skeleton (градиент).

## Правила, выстраданные на багах (обязательны)
- **Graphic-less корень кнопки**: `UIButtonControl` — на объекте без `Graphic` + дочерний `Bg`-Image,
  иначе `UIStateAnimator.AutoAssign` форсит первый `Graphic` в белый (текст «сливается»).
- **Видимость через `CanvasGroup`, не `SetActive` на собственном объекте**: реактивация повторно
  вызывает `OnEnable` и «дерётся» с `Show()` (кейс `UIBannerControl`). Бэкдропы выносятся на отдельный
  объект — там `SetActive(false)` в закрытом состоянии безопасен и надёжнее `blocksRaycasts`.
- **MonoBehaviour = один файл по имени класса** (иначе «missing script»; кейс `UITooltipTrigger`).
- **`.meta` коммитятся вместе с `.cs`/сценой**: иначе на чужой машине Unity генерит новый GUID и
  ссылки в сцене превращаются в «missing script» (кейс `UISideMenuControl`).
- Pivot-независимая pointer-математика: нормализация через `rect.rect.xMin/yMin/width/height`.

## Паттерны адаптивной раскладки
- `UISideMenuControl`: сторона задаётся 2D-направлением (`Left/Right/Top/Bottom`); контрол сам
  раскладывает пункты по поперечной оси (колонка для L/R, строка для T/B) и центрирует их, а пункты
  влетают вдоль оси выезда. Раскладка владельцем-контролом позволяет переключать ориентацию в runtime.
