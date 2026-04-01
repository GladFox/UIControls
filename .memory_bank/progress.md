# Progress

## Что работает
- Репозиторий инициализирован, рабочая ветка `codex/ugui-dotween-controls` создана.
- Реализована библиотека контролов v0.1 на uGUI + DOTween:
  - `UIButtonControl`
  - `UIToggleControl`
  - `UIModalControl`
  - `UIProgressBarControl`
- Добавлен общий слой анимации и состояний (`UITweenSettings`, `UIStateVisual`, `UIStateAnimator`).
- Добавлен `UIStateVisualAsset` (`ScriptableObject`) для переиспользования visual state между префабами.
- Добавлен `UIButtonCustomAction` (`ScriptableObject`) для кастомной логики в дополнение к стандартным событиям кнопки.
- `UIButtonControl` теперь поддерживает одновременно:
  - стандартный `UnityEvent OnClick`
  - кастомные SO-хуки (`OnClick`, `OnStateChanged`)
- Добавлен `UIButtonAnimationProfile` (`ScriptableObject`) для переиспользуемых профильных пресетов кнопки:
  - state assets (`Normal/Hover/Pressed/Disabled`)
  - профильные `UIButtonCustomAction[]`
- `UIButtonControl` расширен событийными SO-хуками:
  - `OnPointerEnter/Exit/Down/Up`
  - `OnSubmit`
  - `OnClick`
  - `OnStateChanged`
- Добавлены trigger-based button actions:
  - `UIButtonActionTriggerFlags`
  - `UIButtonScalePulseAction`
  - `UIButtonAnchoredOffsetAction`
- Добавлена готовая библиотека пресетов:
  - `Assets/UIControls/Animations/UI/Button/States/{Tap,Touch}`
  - `Assets/UIControls/Animations/UI/Button/Actions`
  - `Assets/UIControls/Animations/UI/Button/Profiles`
- Выполнен рефактор нейминга библиотеки:
  - профили переименованы в `TapProfile.asset` и `TouchProfile.asset`
  - обновлены ссылки в editor scripts, docs и Memory Bank.
- Подготовлен ТЗ `local/TZ_UIProgressBarControl_Segmented_Hitbar.md`:
  - сегментация прогресса;
  - hitbar с delayed echo;
  - архитектурное решение: один `UIProgressBarControl` v2 с комбинируемыми флагами `useSegments` + `useHitBar`.
- Реализован `UIProgressBarControl` v2:
  - segmented режим (`useSegments`, `segmentsCount`, `segmentFills`, цвета filling/filled)
  - событие `OnSegmentCompleted(int)` и segment pulse feedback
  - hitbar режим (`useHitBar`, `primaryFillImage`, `echoFillImage`)
  - delayed echo поведение (`echoDelay`, `echoDuration`, `echoEase`) + режимы роста
- `UIProgressBarControl` расширен для production-сценариев:
  - auto-generated segmented visuals (`autoGenerateSegments`, `SegmentVisualMode.FillBlocks/DividersOnly`);
  - авто-генерация divider/segment объектов без ручной раскладки;
  - fallback sprite для `Image.Type.Filled`, чтобы `fillAmount`/echo корректно работали при пустом `sprite`.
  - добавлены sprite override-поля для авто-генерации (`segmentFillSprite`, `segmentDividerSprite`);
  - добавлен флаг `hideEchoOnIncrease` для устранения echo-артефактов при heal.
- Добавлен `UIProgressBarCustomAction` для кастомной логики:
  - `OnValueChanged`
  - `OnSegmentCompleted`
  - `OnEchoStarted`
  - `OnEchoCompleted`
- Добавлен `UIProgressBarDebugLogAction` (`ScriptableObject`) как готовый пример пользовательского SO-action.
- Demo-разделен на две сцены:
  - `UIControlsDemo` оставлен как базовый пример контролов;
  - `UIProgressBarDemo` выделен как специализированный сценарий ProgressBar v2.
- Для `UIProgressBarDemo`:
  - `UIProgressBarDemoPresenter` управляет `Damage/Heavy/Heal/Reset` + `Auto Damage`/`Auto Heal`;
  - `Auto Damage` и `Auto Heal` работают как взаимоисключающие режимы;
  - `UIProgressBarDemoPresenter` синхронизирует два прогрессбара в одном сценарии;
  - `UIProgressBarDemoSceneBuilder` строит два режима:
    - верхний `useSegments + useHitBar` (`DividersOnly`, echo только на уроне);
    - нижний `FillBlocks` (плавное заполнение делений на восстановлении);
  - в demo используются текстуры `Slider_HealthBar_Boss` (`Slider_Basic04_*`, `Slider_Icon04_Fill_Red`);
  - demo builder автоматически создает/назначает `DemoProgressBarDebug.action.asset`.
- Исправлено размножение автогенерируемых `AutoSegment/AutoDivider`:
  - генерация идет в отдельный служебный контейнер `AutoSegments`;
  - добавлена зачистка legacy-детей с префиксами `AutoSegment`/`AutoDivider`.
- Добавлен видимый с первого запуска сценарий автонабора энергии:
  - `Auto Heal` включен по умолчанию в `UIProgressBarDemo`;
  - стартовое значение демо выставлено в `0.35`.
- Выполнена проверка сборки после изменений:
  - `dotnet build UIControls.Runtime.csproj` (успешно, с известными предупреждениями Unity/SDK).

## Известные проблемы
- Полноценная визуальная проверка UX demo-сцены требует запуска в Unity Editor.
- `dotnet build` выводит предупреждения по конфликтам `System.Net.Http`/`System.Security.Cryptography.*` в Unity окружении.
- `git push` пока невозможен без настройки удаленного `origin`.
- Невозможно выполнить Unity `-batchmode` для этого проекта, пока он открыт во втором инстансе Unity.

## Развитие решений
- Пересобрать `UIProgressBarDemo.unity` через `UIControls/Create ProgressBar Demo Scene` и визуально проверить оба demo-бара.
- После визуального прохода в Unity Editor зафиксировать финальные значения размеров/отступов для обеих demo-сцен.
- Добавить prefab-набор для типовых сценариев ProgressBar v2 (segmented, hitbar, combined).

## Контроль изменений
- last_checked_commit: 1987c18
- last_checked_date: 2026-04-01
