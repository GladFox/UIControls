# Changelog

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
