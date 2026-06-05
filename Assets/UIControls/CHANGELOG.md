# Changelog

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
