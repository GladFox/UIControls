# Changelog

## 0.22.0 - 2026-07-02

### UIStickyListControl — sticky section headers (Variant B)

New controls for ScrollRect lists with iOS-style sticky items:

- **`UIStickyListControl`** — attach to a ScrollRect root. Evaluates sticky items every
  `LateUpdate`. An item pins to the viewport edge once its natural position scrolls
  past it; when the next sticky item approaches, it pushes the pinned one out by the
  overlap (classic iOS section-header push-out). Any number of items can be marked
  sticky per edge. Fields: `stickyTopZone`, `stickyBottomZone` (overlay
  `RectTransform` containers inside the viewport).
- **`UIStickyItemControl`** — attach to any direct child of the content
  `VerticalLayoutGroup`. Field: `edge` (`Top` / `Bottom`).
  The control re-parents the stuck item into the zone and leaves a same-size
  `LayoutElement` placeholder in the content so the layout never collapses.
- **`UIStickyListDemo`** — demo scene (`Scrolling(C)/UIStickyListDemo.unity`) with
  5 lettered sections (Alpha–Epsilon), each containing 4 fruit rows. Headers stick to
  the top as you scroll; the zone has a subtle accent border.

## 0.21.0 - 2026-07-01

### UIProgressBarControl — custom Inspector & maxValue label format

**Custom Editor (`UIProgressBarControlEditor`)**

- Segments block collapses entirely when `useSegments` is off. Sub-fields are
  further conditional: sprite/gap fields only when `autoGenerate + FillBlocks`;
  divider colour/sprite only when `dividerWidth > 0`; manual `segmentFills`
  array only when `autoGenerate` is off; pulse and state fields only when their
  respective toggles are on.
- HitBar block collapses entirely when `useHitBar` is off. `echoFillImage`
  GameObject is automatically toggled active/inactive when the toggle changes.
  `useEchoTimingOnIncrease` is greyed out when it has no effect.
- Value Label sub-fields (`valueFormat`, `maxValue`, `animateLabel`) are hidden
  when no `TMP_Text` is assigned to `valueLabel`; a format hint box is shown.
- Events: `onSegmentCompleted` hidden without segments;
  `onEchoStarted/Completed` hidden without HitBar.
- `tween` moved above the label block — it controls bar-fill animation and is
  always relevant, independent of the label.

**`maxValue` field + multi-argument label format**

- New `[Min(1)] float maxValue` field (default 100).
- `UpdateLabel` now passes three arguments to `string.Format`:
  - `{0}` — normalised value (0–1), e.g. `{0:0%}` → "75%"
  - `{1}` — absolute current value (`value × maxValue`), e.g. `{1:0}` → "750"
  - `{2}` — max value, e.g. `{2:0}` → "1000"
  - Example: `{1:0}/{2:0}` → "750/1000"
- Existing `{0:0%}` format is fully backward-compatible.

### UIProgressBarDemo — layout & new bar

- Panel expanded from 980 × 620 to 980 × 820; all elements respaced with
  ~40 px section gaps.
- Added **plain segmented bar** (`DemoSimpleSegmentedProgress`): 5 segments,
  `DividersOnly`, no echo, value label using `{1:0}/{2:0}` format (300/500).

## 0.20.1 - 2026-06-26

### UIProgressBarControl — editor scene pollution fix

Three bugs combined caused `AutoDivider_*` / `AutoSegment_*` objects to accumulate
in the root of the editor scene and persist after stopping Play mode:

- **`delayCall` accumulation** — `OnValidate` added `OnValidateDelayed` to
  `EditorApplication.delayCall` with `+=` every call without first removing it,
  so N Inspector changes before the next frame triggered N full rebuilds.
  Fixed by doing `delayCall -= OnValidateDelayed` before `+=`.
- **Deferred callback survived Play→Edit transition** — a `delayCall` registered
  during Play mode could fire after Unity restored the Edit-mode scene, running
  `EnsureSegmentVisuals()` in an edit context and creating `new GameObject` objects
  that then persisted permanently. Fixed by calling `EnsureSegmentVisuals()` inline
  (no deferral) when `Application.isPlaying` is true during `OnValidate`.
- **No `OnDestroy` cleanup** — removing the component or its GameObject in the
  Editor left all generated child objects alive. Added `OnDestroy` that cancels
  any pending `delayCall` subscription and calls `ClearGeneratedVisuals()`.

## 0.20.0 - 2026-06-08

Categories F–I (22 new controls) and a shared `UIDemoSceneFactory` backing the
new demo builders.

All demo scenes (existing and new) are now organised into category sub-folders
under `Assets/Scenes/` and `Samples~/DemoScenes/Scenes/`: `Basics`,
`Selection(A)`, `Overlays(B)`, `Scrolling(C)`, `Input(D)`, `Feedback(E)`,
`Gameplay(F)`, `Navigation(G)`, `Forms(H)`, `Data(I)`. The scene builders and
build settings were updated to the new paths.

Category F — gameplay / gamepad:

- `UIVirtualJoystickControl` — analog touch stick (fixed/floating, dead zone), `Direction`/`Magnitude`. Demo `UIVirtualJoystickDemo`.
- `UIRadialMenuControl` — pie menu; items fan out/in with stagger; auto-wired selection. Demo `UIRadialMenuDemo`.
- `UIKnobControl` — rotary dial; drag-around value over an angular sweep with optional steps. Demo `UIKnobDemo`.
- `UINumberTickerControl` — rolling animated number (separators, prefix/suffix, pop-on-change). Demo `UINumberTickerDemo`.
- `UIReorderableListControl` (+ `UIReorderableItem`) — vertical drag-to-reorder list. Demo `UIReorderableListDemo`.
- `UISwipeCardControl` — Tinder-style swipe with tilt, like/nope overlays, fling/spring. Demo `UISwipeCardDemo`.

Category G — navigation:

- `UITabBarControl` — app-style bottom nav with sliding indicator and pages. Demo `UITabBarDemo`.
- `UIPaginationControl` — numeric pager with ellipsis collapsing. Demo `UIPaginationDemo`.
- `UIBreadcrumbsControl` — Home › Section › Page trail; click to navigate up. Demo `UIBreadcrumbsDemo`.
- `UIWizardStepsControl` — horizontal step indicator with filling connectors. Demo `UIWizardStepsDemo`.
- `UIFloatingActionButtonControl` — FAB speed-dial with icon rotation. Demo `UIFloatingActionButtonDemo`.
- `UISideMenuControl` — slide-in drawer that docks to any of the four edges (left/right column, top/bottom row) over a backdrop; menu items fly in one by one from the same side. The control owns item layout, so it adapts column↔row when the side changes. Demo `UISideMenuDemo` (Switch cycles all four edges).

Category H — input / forms:

- `UIDropdownControl` — animated single-select with flipping arrow and tinted rows. Demo `UIDropdownDemo`.
- `UIWheelPickerControl` — barrel/wheel picker with snap and distance scale/fade. Demo `UIWheelPickerDemo`.
- `UIPasswordFieldControl` — show/hide toggle + live strength meter. Demo `UIPasswordFieldDemo`.
- `UITagInputControl` — type-and-Enter removable chips from a template. Demo `UITagInputDemo`.
- `UIValueSliderControl` — single-value slider with a value bubble that follows the handle. Demo `UIValueSliderDemo`.

Category I — data / overlays:

- `UIContextMenuControl` — click-to-open popup at the cursor with edge-flipping. Demo `UIContextMenuDemo`.
- `UITreeNodeControl` — expandable/collapsible tree node (chevron + reflow). Demo `UITreeViewDemo`.
- `UIGaugeControl` — arc gauge with a needle tweened to the value. Demo `UIGaugeDemo`.
- `UIAvatarControl` — initials + derived colour + status dot, with "+N" overflow. Demo `UIAvatarDemo`.
- `UIEmptyStateControl` — placeholder with icon/title/message and a CTA. Demo `UIEmptyStateDemo`.
- `UIBannerControl` — persistent inline alert (info/success/warning/error). Demo `UIBannerDemo`.

## 0.19.0 - 2026-06-07

Category E (feedback & decoration controls):

- Added `UISkeletonLoaderControl` — shimmer placeholder shown while content loads: toggles a skeleton
  root (bones) vs the real content root, with a runtime-generated gradient that sweeps across the
  bones. `SetLoading`, `IsLoading`. Demo `UISkeletonLoaderDemo.unity`.
- Added `UICircularProgressControl` — ring progress with a runtime-generated donut sprite (radial-fill
  `Image`): determinate (`SetValue` 0–1 + percent label) or indeterminate (spinning arc). Demo
  `UICircularProgressDemo.unity`.
- Added `UIBadgeControl` — notification badge that overlays a count on an icon: hides at zero, clamps
  to "N+" past a cap, dot-only mode, and pops its scale on change. `SetCount`/`Increment`/`Decrement`,
  `Count`, `OnCountChanged`. Demo `UIBadgeDemo.unity`.
- Added `UIRippleEffectControl` — Material-style ripple that expands from the click point and fades,
  clipped to the element bounds (runtime-generated soft-circle sprite). Drop on any clickable surface.
  Demo `UIRippleEffectDemo.unity`.
- Added `UIMarqueeControl` — scrolling marquee for overflowing text: `Loop` (news-ticker) or
  `PingPong`, optional scroll-only-when-overflowing. `Text`. Demo `UIMarqueeDemo.unity`.

## 0.18.0 - 2026-06-02

Category D (input controls):

- Added `UISearchFieldControl` — text input with a clear (×) button, debounced `OnSearch`, and a
  suggestions dropdown filtered from a source list. Demo `UISearchFieldDemo.unity`.
- Added `UIDatePickerControl` — month-grid calendar (header + prev/next, 6×7 days) with selected/today
  highlighting; `SelectedDate`, `OnDateChanged`. Demo `UIDatePickerDemo.unity`.
- Added `UIStarRatingControl` — clickable/draggable stars with half-star precision, hover preview and
  read-only mode (half-stars clip the fill via a mask). `Value`, `OnRatingChanged`. Demo `UIStarRatingDemo.unity`.
- Added `UIColorPickerControl` — HSV picker with a runtime-generated saturation/value square and hue bar,
  live preview swatch and hex label; `Color`, `SetColor`, `OnColorChanged`. Demo `UIColorPickerDemo.unity`.

## 0.14.0 - 2026-06-02

- Added `UIOTPInputControl` — one-time-code input: a row of single-character cells backed by one
  hidden `TMP_InputField`, so paste, backspace and auto-advance work natively while the cells render
  the value and highlight the active one. Digits-only / masking options; `OnChanged` / `OnCompleted`,
  `Code` / `Clear` / `SetCode` / `Focus`.
- Added `UIOTPInputDemo.unity` demo scene (6-digit code with Clear) and editor scene builder.

## 0.13.0 - 2026-06-02

- Added `UIInfiniteScrollControl` — load-more-at-bottom infinite scroll for a vertical `ScrollRect`:
  - fires `OnLoadMore` when scrolled within a pixel threshold of the bottom (once, while `HasMore` and not loading);
  - footer shows a spinner while loading and an "all caught up" message when done; call `EndLoadMore(hasMore)` after appending rows.
- Added `UIInfiniteScrollDemo.unity` demo scene (paged list that loads batches up to a cap) and editor scene builder.

## 0.12.0 - 2026-06-02

- Added `UICarouselControl` — horizontal paged carousel on a `ScrollRect`:
  - drag snaps to the nearest page; dot indicators highlight the current page; optional autoplay (paused while dragging);
  - `GoTo` / `Next` / `Previous`, `CurrentPage` / `PageCount`, `OnPageChanged`.
- Added `UICarouselDemo.unity` demo scene (4 pages, dots, Prev/Next, autoplay) and editor scene builder.

## 0.11.0 - 2026-06-02

- Added `UIVirtualListControl` — recycled-cell virtual list for huge datasets:
  - instantiates only enough fixed-height cells to fill the viewport (plus a buffer) and recycles them while scrolling;
  - `SetItems(count, binder)` / `SetItemCount`, `ScrollToIndex`, `RefreshActiveCells`, `OnBindCell`.
- Added `UIVirtualListDemo.unity` demo scene (10,000-row list backed by ~12 cells) and editor scene builder.

## 0.10.0 - 2026-06-02

- Added `UIPullToRefreshControl` — pull-to-refresh wrapper for a vertical `ScrollRect`:
  - overscroll past the top reveals a spinner indicator; releasing beyond a threshold fires `OnRefresh`;
  - call `EndRefreshing()` when done — the indicator springs back; spinner rotates with the pull and spins while refreshing.
- Added `UIPullToRefreshDemo.unity` demo scene (scrollable list that prepends a fresh item on refresh) and editor scene builder.

## 0.9.0 - 2026-06-02

- Added `UITooltipControl` + `UITooltipTrigger` — shared hover/long-press tooltip:
  - bubble sizes to its text and is placed next to the target at a preferred side;
  - auto-flips to the opposite side and clamps within the canvas so it never runs off screen;
  - hover delay and optional touch long-press; `Show` / `Hide`, `OnShown` / `OnHidden`.
- Added `UITooltipDemo.unity` demo scene (edge markers demonstrating auto-flip) and editor scene builder.

## 0.8.0 - 2026-06-02

- Added `UIAccordionControl` — stack of collapsible sections:
  - clickable headers expand/collapse content with an animated height; chevron rotates to match;
  - single-open (classic accordion) or multi-open mode; content clips via a masked viewport;
  - `Toggle` / `Expand` / `Collapse` / `SetExpanded`, `IsExpanded`, `OnSectionToggled`.
- Added `UIAccordionDemo.unity` demo scene (single-open FAQ + multi-open settings) and editor scene builder.

## 0.7.0 - 2026-06-02

- Added `UIToastControl` — queued toast / snackbar:
  - FIFO queue plays messages one at a time with slide-in + fade and auto-dismiss;
  - optional action button (snackbar) fires a callback and dismisses early; swipe down to dismiss;
  - info / success / error kinds tint an accent strip; `Show` / `ShowAction`, `OnShown` / `OnDismissed`.
- Added `UIToastDemo.unity` demo scene (info/success/error triggers + snackbar with UNDO) and editor scene builder.

## 0.6.0 - 2026-06-02

- Added `UIBottomSheetControl` — sheet that slides up from the bottom:
  - multiple snap points (e.g. collapsed / expanded); drag between them with an overshoot ease;
  - flick down or drag below a threshold to dismiss; backdrop dims proportionally and closes on click;
  - `Open` / `Close` / `SnapTo`, `OnStateChanged`, `Interactable`.
- Added `UIBottomSheetDemo.unity` demo scene and editor scene builder.

## 0.5.0 - 2026-06-02

- Added `UIRangeSliderControl` — dual-thumb range slider:
  - two handles over one track with a fill between them; handles can't cross and keep a `minDistance` gap;
  - drag a handle or click the track to jump the nearer one; optional `wholeNumbers`;
  - animated jumps/programmatic sets, arrow-key nudge of the active handle, `Interactable` + `CanvasGroup`.
- Added `UIRangeSliderDemo.unity` demo scene (price filter + time window) and editor scene builder.

## 0.4.0 - 2026-06-02

- Added `UIStepperControl` — numeric stepper `[ - ] value [ + ]`:
  - tap to nudge by `step`; hold a button for accelerating auto-repeat;
  - clamp or `wrapAround`, configurable numeric format, value-label pop on change;
  - arrows dim at bounds, pointer hit-testing, arrow-key stepping, `Interactable` + `CanvasGroup`.
- Added `UIStepperDemo.unity` demo scene (integer quantity + fractional volume) and editor scene builder.

## 0.3.0 - 2026-06-02

- Added `UISegmentedControl` — iOS-style segmented control:
  - rounded container with equal-width segments and a sliding thumb;
  - optional X-axis rubber-band slide (leading edge first, trailing catches up; driven via `localScale`);
  - selected-label recolor, pointer hit-testing, arrow-key / submit navigation, `Interactable` + `CanvasGroup`.
- Added `UISegmentedDemo.unity` demo scene (view-switching + event-only variants) and editor scene builder.
- Added `ROADMAP.md` — sequential control backlog and per-PR Definition of Done.

## 0.2.0 - 2026-06-02

- Added `UIChipGroup` — a group of independently-toggling chips (no sliding indicator):
  - `Single` mode behaves like a radio group (one always selected, or optionally none);
  - `Multi` mode toggles each chip freely (any number on at once);
  - per-chip color + pop animation, optional checkmark, pointer hit-testing, keyboard focus/move/submit, `Interactable` + `CanvasGroup`.
- Added `UIChipGroupDemo.unity` demo scene (radio + tags variants) and editor scene builder.
- Project upgraded to Unity `6000.4.9f1`.

## 0.1.4 - 2026-05-20

- Expanded `Demo Scenes` sample to include all current test/demo scenes:
  - `UIControlsDemo.unity`
  - `UIProgressBarDemo.unity`
  - `UITabSliderDemo.unity`
  - `UIRubberBandPrototype.unity`

## 0.1.3 - 2026-05-20

- Fixed `TMPro` compile errors in imported projects:
  - added `Unity.TextMeshPro` reference to `UIControls.Editor.asmdef`.
- Added explicit package dependency:
  - `com.unity.textmeshpro` (`3.0.6`) in `package.json`.

## 0.1.2 - 2026-05-20

- Added explicit DOTween assembly linkage in `UIControls.Runtime.asmdef`:
  - `overrideReferences: true`
  - `precompiledReferences: ["DOTween.dll"]`
- Kept package independent from `DOTween.Modules.asmdef` (modules asmdef no longer required).

## 0.1.1 - 2026-05-20

- Fixed UPM import compatibility for asmdef references:
  - `Unity.ugui` reference replaced with `UnityEngine.UI`.
  - removed hard dependency on `DOTween.Modules` asmdef.
- Added `UIDOTweenUtility` and switched UI tweens to `DOTween.To(...)` adapters for `Graphic`, `CanvasGroup`, and `RectTransform`.
- Added package `Samples` entry with demo scenes (`UIControlsDemo`, `UIProgressBarDemo`).

## 0.1.0 - 2026-05-20

- Initial UPM package metadata.
- Runtime controls and editor tooling are now exportable via Git URL with `?path=Assets/UIControls`.
- Added `UIControls.Editor` asmdef for package editor scripts.
