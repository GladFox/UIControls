# Active Context

## Текущее состояние
- Библиотека выросла до ~50 контролов по категориям A–I поверх базы
  (`Button/Toggle/Modal/ProgressBar/TabSlider`). Пакет `0.22.0`.
- Категории A–I смерджены в `main`; последние работы коммитятся прямо в `main`.
- Все демо-сцены разложены по категорийным подпапкам в `Assets/Scenes/` и `Samples~`.
- `0.20.1` — фикс засорения сцены `AutoDivider_*` (delayCall-баги в `UIProgressBarControl.OnValidate`).
- `0.21.0` — кастомный инспектор `UIProgressBarControlEditor` (условная видимость полей),
  поле `maxValue` + трёхаргументный формат лейбла (`{1:0}/{2:0}` → "750/1000"),
  просторный лайаут `UIProgressBarDemo` + простой сегментированный бар.
- `0.22.0` — **`UIStickyListControl`/`UIStickyItemControl`** (вариант B): sticky-заголовки
  в ScrollRect с iOS push-out; механизм re-parent + placeholder; демо `Scrolling(C)/UIStickyListDemo`.

## Текущие задачи
- [ ] Проверить `UIStickyListDemo` в реальном скролле (play-mode), затем скопировать сцену в `Samples~`.

## Последние изменения (ветка feature/uiCategoryFGHI)
- **F (gameplay):** VirtualJoystick, RadialMenu, Knob, NumberTicker, ReorderableList(+Item), SwipeCard.
- **G (navigation):** TabBar, Pagination, Breadcrumbs, WizardSteps, FloatingActionButton, SideMenu.
- **H (forms):** Dropdown, WheelPicker, PasswordField, TagInput, ValueSlider.
- **I (data/overlay):** ContextMenu, TreeView (`UITreeNodeControl`), Gauge, Avatar, EmptyState, Banner.
- Добавлен общий `UIDemoSceneFactory` для билдеров E–I (+ хелпер `InputField`).
- Реорганизация: 26 старых демо-сцен (A–E + Basics) перенесены `git mv` в папки категорий;
  поправлены `ScenePath` билдеров и пути в `EditorBuildSettings.asset`.
- Фиксы:
  - `UIBannerControl` — видимость через `CanvasGroup`, не `SetActive` на собственном объекте.
  - `UISideMenu` — backdrop перекрывал триггер → `SetActive(false)` в закрытом состоянии.
  - `UISideMenu` — **закоммичены пропавшие `.meta`** (была причина «missing script» у пользователя).
  - `UISideMenu` — тайминги под Brawl-Stars (OutBack + pop-scale + плотный stagger).
  - `UISideMenu` — поддержка всех 4 краёв с адаптивной раскладкой колонка↔строка.

## Следующие шаги
- После мёржа #15 и #16 обновить `last_checked_commit` в `progress.md` на свежий `main`.
- Возможный рефактор: устранить дубли `Segmented`/`TabSlider`/`TabBar`.
- При желании пользователя — новые категории контролов поверх текущего фундамента.

## Дисциплина (из AGENTS.md)
- Один PR = одна ветка, обязателен worktree, работа не завершена без `git push`.
- Перед задачей читать Memory Bank; при изменении архитектуры/API — обновлять `local/README.md`
  и при необходимости `systemPatterns.md`/`techContext.md`.
- **`.meta` всегда коммитить вместе с `.cs`/сценой** (иначе «missing script» на чужой машине).
