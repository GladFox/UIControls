# UIControls — Roadmap

Plan for growing the package into a full uGUI control library. Each control ships as **one PR**:
control + a runnable demo scene, following the conventions below. After a control is reviewed and
merged, the next one starts.

## Definition of Done (every control PR)

1. **Runtime control** `Runtime/Controls/UIXxxControl.cs` — `sealed MonoBehaviour`, namespace
   `UIControls.Runtime.Controls`. Style (colors/sizes) lives in serialized fields, not logic.
   Animations via `UITweenSettings` + `UIDOTweenUtility`; `Sequence activeSequence` killed in `OnDisable`.
2. **Full public API** — properties, `Set...(value, animate = true, notify = true)` methods,
   `UnityEvent`s for key events, `Interactable`.
3. **Accessibility** — keyboard/gamepad (`ISubmitHandler` / `IMoveHandler` where relevant);
   disabled state via `CanvasGroup` + `disabledAlpha`.
4. **Editor scene builder** `Editor/UIXxxDemoSceneBuilder.cs` with
   `[MenuItem("UIControls/Create Xxx Demo Scene")]` and a batch method `CreateXxxDemoSceneBatch()`.
5. **Demo presenter** `Runtime/Demo/UIXxxDemoPresenter.cs` showing two contrasting usages
   (e.g. "switches views" + "fires events only").
6. **Generated scene** `Assets/Scenes/UIXxxDemo.unity`, added to Build Settings, copied to
   `Samples~/DemoScenes/Scenes/`.
7. **Docs** — README (Included / Samples), CHANGELOG entry, `package.json` description + version bump.
8. **One control = one branch (`feature/uiXxx`) = one PR.**

## Status legend

- [ ] not started   [~] in progress   [x] merged   ✅ already in repo (polish only)

## Backlog (order = build order)

### A — close to TabSlider (reuse the animation stack)
- [ ] **A1 UISegmentedControl** — iOS pill tab; sliding pill indicator (reuse rubber-band via `localScale`).
- [x] **A2 UIToggleSwitch** — ✅ exists as `UIToggleControl`. Polish: dedicated demo + handle overshoot.
- [ ] **A3 UIRadioGroup / UIChipGroup** — single/multi select; `Mode {Single,Multi}`, exclusive logic in group.
- [ ] **A4 UIStepper** — `[-] n [+]` with hold-to-repeat acceleration; pop-scale on change.
- [ ] **A5 UIRangeSlider** — dual min/max handles, fill between; handles can't cross.

### B — containers / panels
- [ ] **B1 UIBottomSheet / UIDrawer** — snap-points, drag-to-dismiss, backdrop fade, rubber-band overshoot.
- [x] **B2 UIModalDialog** — ✅ exists as `UIModalControl`. Polish: backdrop fade+scale-in, queue, focus-trap.
- [ ] **B3 UIToast / UISnackbar** — FIFO queue, slide-in + auto-dismiss, swipe-to-dismiss, action button.
- [ ] **B4 UIAccordion** — animated section height, chevron rotation, single/multi open.
- [ ] **B5 UITooltip** — hover/long-press, appear delay, auto-flip near screen edges.

### C — lists / scrolling
- [ ] **C1 UIPullToRefresh** — overscroll trigger on `ScrollRect`, rubber-band return, `OnRefresh`/`EndRefreshing`.
- [ ] **C2 UIVirtualList** — recycled cells for huge lists; `SetData`/`OnBindItem`; fixed height first, variable later.
- [ ] **C3 UICarousel / UIPager** — horizontal snap scroll + dot indicator, optional autoplay.
- [ ] **C4 UIInfiniteScroll** — load-more at bottom, footer loader, `OnLoadMore`/`HasMore`/`AppendData`.

### D — input
- [ ] **D1 UIOTPInput** — N cells, auto-advance focus, paste whole code, backspace to previous.
- [ ] **D2 UISearchField** — clear button, debounced `OnSearch`, suggestions dropdown.
- [ ] **D3 UIDatePicker / UITimePicker** — calendar + time wheels; `SelectedDate`, min/max.
- [ ] **D4 UIStarRating** — click/swipe rating, half-star, hover preview, read-only mode.
- [ ] **D5 UIColorPicker** — HSV square + hue slider (+ optional alpha), hex field.

### E — feedback / decorative
- [ ] **E1 UISkeletonLoader** — shimmer placeholder bones, `IsLoading` toggle.
- [ ] **E2 UICircularProgress** — radial fill (`Image` Radial360) + tween, determinate + indeterminate.
- [ ] **E3 UIBadge** — count overlay, `99+`, dot mode, pop-in on change.
- [ ] **E4 UIRippleEffect** — Material ripple from pointer position, shape mask.
- [ ] **E5 UIMarquee** — scrolling text for overflowing labels, loop / ping-pong, auto-start only when clipped.

## Delivery order

```
A1 → A3 → A4 → A5
B1 → B3 → B4 → B5
C1 → C2 → C3 → C4
D1 → D2 → D3 → D4 → D5
E1 → E2 → E3 → E4 → E5
Polish: A2 (ToggleSwitch demo), B2 (ModalDialog enhancements)
```
