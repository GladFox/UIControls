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
- Базовый demo (`UIControlsDemo`) и специализированные demo (например `UIProgressBarDemo`) разделяются по отдельным сценам и builder-скриптам.
