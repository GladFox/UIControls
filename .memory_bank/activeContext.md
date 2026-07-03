# Active Context

## Текущее состояние
- Библиотека выросла до ~50 контролов по категориям A–I поверх базы
  (`Button/Toggle/Modal/ProgressBar/TabSlider`). Пакет `0.20.0`.
- Категории A–E смерджены в `main` отдельными PR. Категории **F, G, H, I**, контрол
  **`UISideMenu`** и реорганизация демо-сцен по папкам — в ветке `feature/uiCategoryFGHI`
  (открытый **PR #16**, base `main`, стоит поверх PR #15 = категория E).
- Все демо-сцены разложены по категорийным подпапкам в `Assets/Scenes/` и `Samples~`.

## Текущие задачи
- [x] [DOCS_WRITER] Привести `local/README.md` и Memory Bank в соответствие с проделанной работой A–I.
- [ ] Ожидается ревью/мёрж PR #15 (E), затем PR #16 (F–I + SideMenu) — мёрж выполняет пользователь.

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
