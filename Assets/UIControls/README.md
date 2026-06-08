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

- Runtime controls: `UIButtonControl`, `UIToggleControl`, `UIModalControl`, `UIProgressBarControl`, `UITabSliderControl`, `UISegmentedControl`, `UIChipGroup`, `UIStepperControl`, `UIRangeSliderControl`, `UIBottomSheetControl`, `UIToastControl`, `UIAccordionControl`, `UITooltipControl`, `UIPullToRefreshControl`, `UIVirtualListControl`, `UICarouselControl`, `UIInfiniteScrollControl`, `UIOTPInputControl`, `UISearchFieldControl`, `UIDatePickerControl`, `UIStarRatingControl`, `UIColorPickerControl`, `UISkeletonLoaderControl`, `UICircularProgressControl`, `UIBadgeControl`, `UIRippleEffectControl`, `UIMarqueeControl`, `UIVirtualJoystickControl`, `UIRadialMenuControl`, `UIKnobControl`, `UINumberTickerControl`, `UIReorderableListControl`, `UISwipeCardControl`, `UITabBarControl`, `UIPaginationControl`, `UIBreadcrumbsControl`, `UIWizardStepsControl`, `UIFloatingActionButtonControl`, `UIDropdownControl`, `UIWheelPickerControl`, `UIPasswordFieldControl`, `UITagInputControl`, `UIValueSliderControl`, `UIContextMenuControl`, `UITreeNodeControl`, `UIGaugeControl`, `UIAvatarControl`, `UIEmptyStateControl`, `UIBannerControl`
- Reusable ScriptableObject actions and visual-state assets
- Editor scene builders for demo/prototyping

## Samples

- `Demo Scenes` sample is available in Package Manager.
- All demo scenes are organised into category sub-folders:
  - `Basics/`: `UIControlsDemo`, `UIProgressBarDemo`, `UIRubberBandPrototype`
  - `Selection(A)/`: `UITabSliderDemo`, `UISegmentedDemo`, `UIChipGroupDemo`, `UIStepperDemo`, `UIRangeSliderDemo`
  - `Overlays(B)/`: `UIBottomSheetDemo`, `UIToastDemo`, `UIAccordionDemo`, `UITooltipDemo`
  - `Scrolling(C)/`: `UIPullToRefreshDemo`, `UIVirtualListDemo`, `UICarouselDemo`, `UIInfiniteScrollDemo`
  - `Input(D)/`: `UIOTPInputDemo`, `UISearchFieldDemo`, `UIDatePickerDemo`, `UIStarRatingDemo`, `UIColorPickerDemo`
  - `Feedback(E)/`: `UISkeletonLoaderDemo`, `UICircularProgressDemo`, `UIBadgeDemo`, `UIRippleEffectDemo`, `UIMarqueeDemo`
  - `Gameplay(F)/`: `UIVirtualJoystickDemo`, `UIRadialMenuDemo`, `UIKnobDemo`, `UINumberTickerDemo`, `UIReorderableListDemo`, `UISwipeCardDemo`
  - `Navigation(G)/`: `UITabBarDemo`, `UIPaginationDemo`, `UIBreadcrumbsDemo`, `UIWizardStepsDemo`, `UIFloatingActionButtonDemo`
  - `Forms(H)/`: `UIDropdownDemo`, `UIWheelPickerDemo`, `UIPasswordFieldDemo`, `UITagInputDemo`, `UIValueSliderDemo`
  - `Data(I)/`: `UIContextMenuDemo`, `UITreeViewDemo`, `UIGaugeDemo`, `UIAvatarDemo`, `UIEmptyStateDemo`, `UIBannerDemo`

## Notes

This package is developed directly inside the main project under `Assets/UIControls`.
