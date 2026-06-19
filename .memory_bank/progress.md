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
  - добавлен флаг `hideEchoOnIncrease` для устранения echo-артефактов при heal;
  - добавлен флаг `useEchoTimingOnIncrease`, позволяющий использовать `echoDelay/echoDuration` и на росте прогресса.
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
  - `UIProgressBarDemoPresenter` разделяет два независимых сценария:
    - `Health HitBar`: `Damage/Heavy/Heal/Reset`, урон с echo rollback, лечение с мгновенным обновлением HP;
    - `Energy Charge`: авто-набор `0..3` за `6` секунд с сегментацией.
  - `UIProgressBarDemoSceneBuilder` строит два режима:
    - верхний `Health` bar: `useSegments + useHitBar` (`DividersOnly`);
    - нижний `Energy` bar: `3` сегмента, плавное заполнение и фиксация завершенных сегментов в основном цвете;
  - в demo используются текстуры `Slider_HealthBar_Boss` (`Slider_Basic04_*`, `Slider_Icon04_Fill_Red`);
  - demo builder автоматически создает/назначает `DemoProgressBarDebug.action.asset`;
  - legacy `Auto Damage/Auto Heal` UI из старой сцены скрывается презентером для обратной совместимости.
- Добавлено действие расхода энергии:
  - кнопка `Spend 1 Super` (в новом builder-сценарии) списывает одно целое деление `Energy`;
  - при списании корректируется внутренний таймер auto-charge, поэтому набор продолжается без скачка;
  - для старой сцены без новой кнопки активирован fallback: `AutoDamage` toggle работает как `Spend 1 Super`.
- Библиотека подготовлена к UPM-экспорту:
  - package root в `Assets/UIControls`, без копирования runtime-исходников;
  - добавлены файлы UPM-метаданных (`package.json`, `README.md`, `CHANGELOG.md`, `LICENSE.md`);
  - добавлен `UIControls.Editor.asmdef` для editor-части пакета;
  - способ подключения: git URL с `?path=Assets/UIControls`.
- В UPM пакет добавлены importable samples:
  - `Samples~/DemoScenes/Scenes/UIControlsDemo.unity`;
  - `Samples~/DemoScenes/Scenes/UIProgressBarDemo.unity`.
- Для sample-сцен добавлены локальные sample-текстуры `Samples~/DemoScenes/Art/Slider/*`.
- Устранена потенциальная GUID-коллизия sample-артов:
  - GUID в `Samples~/DemoScenes/Art/Slider/*.meta` заменены на уникальные;
  - ссылки в `Samples~/DemoScenes/Scenes/UIProgressBarDemo.unity` обновлены на новые GUID.
- `Assets/UIControls/package.json` дополнен секцией `samples` для импорта `Demo Scenes` через Package Manager.
- Исправлена проблема импортов asmdef в чистом UPM-проекте:
  - ссылка `Unity.ugui` заменена на корректную `UnityEngine.UI` в `UIControls.Runtime.asmdef`;
  - убрана прямая зависимость от `DOTween.Modules` в asmdef.
- Добавлен `UIDOTweenUtility` (`DOTween.To`-адаптеры для `CanvasGroup`, `Graphic`, `RectTransform`), чтобы контролы не требовали `DOTween.Modules.asmdef`.
- Обновлены контролы/ядро на `UIDOTweenUtility`:
  - `UIStateAnimator`, `UIModalControl`, `UIToggleControl`, `UITabSliderControl`, `UIButtonAnchoredOffsetAction`.
- Версия UPM пакета увеличена до `0.1.1`, чтобы обновление отображалось пользователям в Package Manager.
- `Assets/UIControls/CHANGELOG.md` дополнен записью `0.1.1` с описанием fixes.
- Для runtime asmdef добавлена явная ссылка на DOTween:
  - `overrideReferences: true`
  - `precompiledReferences: ["DOTween.dll"]`
- Версия UPM пакета увеличена до `0.1.2` и зафиксирована в changelog.
- Исправлена ошибка `CS0246: TMPro` при импорте UPM пакета:
  - `UIControls.Editor.asmdef` дополнен `Unity.TextMeshPro`;
  - добавлена package dependency `com.unity.textmeshpro: 3.0.6`.
- Версия UPM пакета увеличена до `0.1.3`.
- UPM sample `Demo Scenes` расширен всеми тестовыми сценами:
  - `UIControlsDemo`, `UIProgressBarDemo`, `UITabSliderDemo`, `UIRubberBandPrototype`.
- Версия UPM пакета увеличена до `0.1.4`.
- Исправлено размножение автогенерируемых `AutoSegment/AutoDivider`:
  - генерация идет в отдельный служебный контейнер `AutoSegments`;
  - добавлена зачистка legacy-детей с префиксами `AutoSegment`/`AutoDivider`.
- Добавлен видимый с первого запуска сценарий автонабора энергии:
  - `Auto Heal` включен по умолчанию в `UIProgressBarDemo`;
  - стартовое значение демо выставлено в `0.35`.
- Выполнена проверка сборки после изменений:
  - `dotnet build UIControls.Runtime.csproj` (успешно, с известными предупреждениями Unity/SDK).

### Категории контролов A–I (пакет до 0.20.0)
- Проект апгрейднут до Unity `6000.4.9f1`; демо-сцены генерируются editor-билдерами через
  `-batchmode -executeMethod ...Create<Xxx>DemoSceneBatch`.
- **A — selection/sliders:** `UISegmentedControl`, `UIChipGroup`, `UIStepperControl`,
  `UIRangeSliderControl` (+ базовый `UITabSliderControl`).
- **B — overlays:** `UIBottomSheetControl`, `UIToastControl`, `UIAccordionControl`,
  `UITooltipControl` (+ `UITooltipTrigger`).
- **C — scrolling:** `UIPullToRefreshControl`, `UIVirtualListControl`, `UICarouselControl`
  (свайп-флик + autoplay ping-pong), `UIInfiniteScrollControl`.
- **D — input:** `UIOTPInputControl`, `UISearchFieldControl`, `UIDatePickerControl`
  (навигация месяц/год), `UIStarRatingControl`, `UIColorPickerControl` (pivot-независимый курсор).
- **E — feedback:** `UISkeletonLoaderControl`, `UICircularProgressControl`, `UIBadgeControl`,
  `UIRippleEffectControl`, `UIMarqueeControl`.
- **F — gameplay:** `UIVirtualJoystickControl`, `UIRadialMenuControl`, `UIKnobControl`,
  `UINumberTickerControl`, `UIReorderableListControl` (+ `UIReorderableItem`), `UISwipeCardControl`.
- **G — navigation:** `UITabBarControl`, `UIPaginationControl`, `UIBreadcrumbsControl`,
  `UIWizardStepsControl`, `UIFloatingActionButtonControl`, `UISideMenuControl` (4 края, адаптивная
  раскладка колонка↔строка, Brawl-Stars pop).
- **H — forms:** `UIDropdownControl`, `UIWheelPickerControl`, `UIPasswordFieldControl`,
  `UITagInputControl`, `UIValueSliderControl`.
- **I — data/overlay:** `UIContextMenuControl`, `UITreeNodeControl`, `UIGaugeControl`,
  `UIAvatarControl`, `UIEmptyStateControl`, `UIBannerControl`.
- Введён общий `UIDemoSceneFactory` (camera/canvas/panel, Text/Button/Image/InputField, SetRef*).
- Все демо-сцены (A–I + Basics) разложены по категорийным подпапкам в `Assets/Scenes/` и `Samples~`;
  поправлены `ScenePath` билдеров и пути в `EditorBuildSettings.asset`.
- Категории A–E смерджены в `main`; F/G/H/I + SideMenu + реорг — в открытом PR #16 (поверх PR #15=E).

## Известные проблемы
- Полноценная визуальная проверка UX demo-сцены требует запуска в Unity Editor.
- Билдеры нельзя запускать параллельно: lockfile `Temp/UnityLockfile` и лицензионный хендшейк дают
  read-only / rc=1. Запуск строго по одному с ожиданием выхода процесса.
- Модульные DOTween-шорткаты (`DOAnchorPos*`, `DOSizeDelta`, `Graphic.DOColor`) недоступны →
  только `UIDOTweenUtility`.
- `dotnet build` выводит предупреждения по конфликтам `System.Net.Http`/`System.Security.Cryptography.*`.

### Усвоенные уроки (по багам)
- `.meta` обязательно коммитить вместе с `.cs`/сценой — иначе на чужой машине новый GUID и
  «missing script» (кейс `UISideMenuControl`: панель «не выезжала» именно из-за этого).
- Контрол не должен прятать собственный объект `SetActive(false)` в `OnEnable` — реактивация
  повторно вызывает `OnEnable` (кейс `UIBannerControl`: сообщения не появлялись). Видимость — через
  `CanvasGroup`; бэкдропы — на отдельном объекте.
- `UIButtonControl` — только на graphic-less корне + дочерний `Bg` (иначе белый-override на Normal).
- MonoBehaviour = один файл по имени класса (кейс `UITooltipTrigger`).

## Развитие решений
- После мёржа PR #15 (E) и PR #16 (F–I + SideMenu) обновить `last_checked_commit` на свежий `main`.
- Устранить дубли `UISegmentedControl`/`UITabSliderControl`/`UITabBarControl`.
- Добавить prefab-набор и editor-валидаторы обязательных ссылок.
- Прогнать проверку импорта samples в чистом Unity-проекте через `Package Manager > Samples`.

## Контроль изменений
- last_checked_commit: 94a7988
- last_checked_date: 2026-06-20
- Диапазон с прошлой отметки: `git log ddb7433..94a7988` — 46 коммитов (категории A3→I, SideMenu,
  фиксы, реорганизация демо-сцен по папкам, версии пакета до 0.20.0).
