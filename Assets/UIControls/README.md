# UIControls UPM Package

UI controls library for Unity uGUI with DOTween-based animations.

## Install (Git URL)

Use Unity Package Manager:

- Add package from git URL:
  - `https://github.com/GladFox/UIControls.git?path=Assets/UIControls`

## Requirements

- Unity `2021.3+`
- DOTween installed in project (`DG.Tweening` namespace must be available)
- `com.unity.ugui`
- `com.unity.textmeshpro`

### DOTween setup note

`UIControls.Runtime` does not depend on `DOTween.Modules.asmdef`,
but has an explicit asmdef link to `DOTween.dll` (precompiled reference).
So DOTween must be installed in the project before (or together with) UIControls.

## Included

- Runtime controls: `UIButtonControl`, `UIToggleControl`, `UIModalControl`, `UIProgressBarControl`, `UITabSliderControl`, `UISegmentedControl`, `UIChipGroup`
- Reusable ScriptableObject actions and visual-state assets
- Editor scene builders for demo/prototyping

## Samples

- `Demo Scenes` sample is available in Package Manager.
- It includes:
  - `UIControlsDemo.unity`
  - `UIProgressBarDemo.unity`
  - `UITabSliderDemo.unity`
  - `UISegmentedDemo.unity`
  - `UIChipGroupDemo.unity`
  - `UIRubberBandPrototype.unity`

## Notes

This package is developed directly inside the main project under `Assets/UIControls`.
