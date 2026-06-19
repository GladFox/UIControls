# UIControls

Reusable Unity **uGUI** controls with **DOTween** animations — ~50 polished, self-contained controls
(buttons, sliders, overlays, scrollers, inputs, gameplay HUD widgets, navigation, data displays) that
you drive from code or wire in the Inspector. Each control ships with a runnable demo scene.

- **Unity:** 6000.x (developed on `6000.4.9f1`; package minimum `2021.3`)
- **Package:** `com.gladfox.uicontrols` · current version `0.20.0`
- **License:** see [LICENSE.md](Assets/UIControls/LICENSE.md)

---

## Installation

UIControls is a UPM package that lives in `Assets/UIControls`. You can add it straight from this repo
via a git URL.

### Prerequisites
- **DOTween** must already be in your project (it is a hard dependency, linked as `DOTween.dll`).
  Install it from the [Asset Store](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)
  and run its setup once (`Tools → Demigiant → DOTween Utility Panel → Setup DOTween…`).
- **TextMeshPro** and **uGUI** are pulled in automatically as package dependencies.

### Option A — Package Manager (UI)
1. `Window → Package Manager`
2. `+` → **Add package from git URL…**
3. Paste:
   ```
   https://github.com/GladFox/UIControls.git?path=Assets/UIControls
   ```

### Option B — manifest.json
Add this line to `Packages/manifest.json`:
```json
{
  "dependencies": {
    "com.gladfox.uicontrols": "https://github.com/GladFox/UIControls.git?path=Assets/UIControls"
  }
}
```

To pin a version, append `#<tag-or-commit>` to the URL.

### Demo scenes (samples)
In Package Manager, select **UIControls (uGUI + DOTween)** → **Samples** → import **Demo Scenes**.
Every control has its own scene, grouped into category sub-folders
(`Selection(A)`, `Overlays(B)`, … `Data(I)`). Open one and press **Play**.

---

## Quick start

```csharp
using UIControls.Runtime.Controls;
using UnityEngine;

public sealed class Example : MonoBehaviour
{
    [SerializeField] private UIButtonControl button;
    [SerializeField] private UINumberTickerControl score;

    private void OnEnable() => button.OnClick.AddListener(AddPoints);
    private void OnDisable() => button.OnClick.RemoveListener(AddPoints);

    private void AddPoints() => score.Add(250); // smoothly rolls the number up
}
```

Every control exposes:
- a clear API (`Open/Close`, `SetValue`, `SetCount`, `Toggle`, …),
- `UnityEvent`s for state changes (`OnClick`, `OnValueChanged`, `OnItemSelected`, …),
- serialized references you can override in the Inspector.

---

## What's inside

**Base** — `UIButtonControl` (state machine + SO action hooks), `UIToggleControl`, `UIModalControl`,
`UIProgressBarControl` (segmented + hitbar echo), `UITabSliderControl`.

| Category | Controls |
|---|---|
| **A — Selection / sliders** | Segmented, ChipGroup, Stepper, RangeSlider |
| **B — Overlays** | BottomSheet, Toast, Accordion, Tooltip |
| **C — Scrolling** | PullToRefresh, VirtualList, Carousel, InfiniteScroll |
| **D — Input** | OTPInput, SearchField, DatePicker, StarRating, ColorPicker |
| **E — Feedback / decoration** | SkeletonLoader, CircularProgress, Badge, RippleEffect, Marquee |
| **F — Gameplay** | VirtualJoystick, RadialMenu, Knob, NumberTicker, ReorderableList, SwipeCard |
| **G — Navigation** | TabBar, Pagination, Breadcrumbs, WizardSteps, FloatingActionButton, **SideMenu** (4-edge drawer) |
| **H — Forms** | Dropdown, WheelPicker, PasswordField, TagInput, ValueSlider |
| **I — Data / overlays** | ContextMenu, TreeView, Gauge, Avatar, EmptyState, Banner |

All animations go through DOTween; visual controls that need no sprites (ColorPicker, CircularProgress,
RippleEffect, SkeletonLoader) generate their textures at runtime.

---

## Highlights

- **Drawer that docks to any edge** — `UISideMenuControl` slides in from left/right/top/bottom; items
  lay out as a column or a row automatically and fly in along the open direction with a Brawl-Stars-style pop.
- **Game-ready HUD** — virtual joystick, radial menu, rotary knob, animated score ticker, swipeable cards.
- **Production progress bar** — combinable continuous / segmented / hitbar-with-echo modes in one control.
- **Extensible buttons** — reusable `ScriptableObject` visual-state presets and custom action hooks.

---

## Requirements

- Unity 6000.x (package manifest minimum `2021.3`)
- `com.unity.ugui`, `com.unity.textmeshpro` (auto), **DOTween** (manual, see Installation)
- Input System package is supported (demo `EventSystem` uses `InputSystemUIInputModule` with a
  `StandaloneInputModule` fallback)

## Documentation

- Package overview & sample list: [`Assets/UIControls/README.md`](Assets/UIControls/README.md)
- Architecture (source of truth): [`local/README.md`](local/README.md)
- Changelog: [`Assets/UIControls/CHANGELOG.md`](Assets/UIControls/CHANGELOG.md)

## Notes

The package is developed in-repo under `Assets/UIControls`, so the same checkout works both as the
development project and as the importable UPM package.
