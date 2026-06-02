# Changelog

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
