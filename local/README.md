# UIControls Architecture

`local/README.md` — источник правды по архитектуре библиотеки контролов.

## Цель
Переиспользуемая библиотека UI-контролов на базе Unity `uGUI` с анимациями через `DOTween`.
Поставляется как UPM-пакет (`com.gladfox.uicontrols`), текущая версия `0.20.0`.

## Границы решения
- Внешние UI-решения используются только как референс поведения/идей.
- Runtime-контролы не зависят от сторонних UI-библиотек.
- Все анимации реализуются через `DG.Tweening` (UI-твины — через `UIDOTweenUtility`, см. ниже).

## Модульная структура
```text
Assets/UIControls/
  Animations/UI/Button/        Profiles / Actions / States (SO-пресеты кнопок)
  Runtime/
    UIControls.Runtime.asmdef
    Core/
      UITweenSettings.cs  UIStateVisual.cs  UIStateVisualAsset.cs  UIStateAnimator.cs
      UIDOTweenUtility.cs        # DOTween.To-адаптеры (без DOTween.Modules.asmdef)
    Controls/                    # все контролы (см. каталог ниже)
      Actions/                   # SO-действия кнопки/прогрессбара
    Demo/                        # презентеры демо-сцен (UIXxxDemoPresenter)
  Editor/
    UIControls.Editor.asmdef
    UIDemoSceneFactory.cs        # общий фабричный слой для билдеров категорий E–I
    UIXxxDemoSceneBuilder.cs     # по одному билдеру на контрол
    UIControlsButtonAnimationLibraryBuilder.cs
  Samples~/DemoScenes/Scenes/<категория>/   # UPM-сэмплы (зеркало Assets/Scenes)
Assets/Scenes/<категория>/       # рабочие демо-сцены, сгруппированы по папкам
```

## Core-компоненты
- `UITweenSettings` — единые параметры tween (`duration`, `ease`, `delay`, `independentUpdate`) + применение.
- `UIStateVisual` — снимок состояния (`scale`, `alpha`, `color`) + tween-настройки.
- `UIStateVisualAsset` (`ScriptableObject`) — переиспользуемый пресет `UIStateVisual`.
- `UIStateAnimator` — унифицированная анимация перехода состояния (`RectTransform`/`CanvasGroup`/`Graphic`).
- `UIDOTweenUtility` — статические `DOTween.To`-адаптеры: `TweenAnchoredPosition`, `TweenSizeDelta`,
  `TweenGraphicColor`, `TweenCanvasGroupAlpha`. Контролы используют их вместо модульных шорткатов
  (`DOAnchorPos*`, `DOSizeDelta`, `Graphic.DOColor`), которых нет без `DOTween.Modules.asmdef`.
  Шорткаты `Transform.DOScale` / `DOLocalRotate` / `DOKill` доступны и используются напрямую.

## Каталог контролов

### База
- `UIButtonControl` — состояния `Normal/Hover/Pressed/Disabled`, `OnClick`, pointer+submit ввод; визуал из
  `UIButtonAnimationProfile` или локальных `UIStateVisualAsset`; SO-хуки `UIButtonCustomAction`.
- `UIToggleControl` — `On/Off`, анимация handle + цветов, `OnValueChanged(bool)`.
- `UIModalControl` — `Show/Hide/Toggle`, fade + scale + anchored position.
- `UIProgressBarControl` — continuous / segmented (`OnSegmentCompleted`) / hitbar с delayed echo;
  auto-generated segmented visuals; SO-хуки `UIProgressBarCustomAction`.
- `UITabSliderControl` — таб-слайдер со скользящим индикатором (rubber-band).

### A — selection / sliders (`Scenes/Selection(A)`)
- `UISegmentedControl` — iOS-style pill-табы со скользящей подсветкой.
- `UIChipGroup` — single/multi выбор чипов (`Mode {Single,Multi}`).
- `UIStepperControl` — `[-] n [+]` с hold-to-repeat и pop-scale.
- `UIRangeSliderControl` — два хэндла min/max, fill между, хэндлы не пересекаются.

### B — overlays (`Scenes/Overlays(B)`)
- `UIBottomSheetControl` — snap-точки, drag-to-dismiss, backdrop fade, Expand/Collapse.
- `UIToastControl` — FIFO-очередь, slide-in + auto-dismiss, swipe-to-dismiss.
- `UIAccordionControl` — анимированная высота секций, поворот шеврона, single/multi open.
- `UITooltipControl` (+ `UITooltipTrigger`) — hover/long-press, задержка, авто-флип у краёв.

### C — scrolling (`Scenes/Scrolling(C)`)
- `UIPullToRefreshControl` — overscroll-триггер, rubber-band, `OnRefresh`/`EndRefreshing`.
- `UIVirtualListControl` — recycled-cells для больших списков, `SetData`/`OnBindItem`.
- `UICarouselControl` — горизонтальный snap-пейджинг, точки, свайп-флик, autoplay ping-pong.
- `UIInfiniteScrollControl` — load-more снизу, footer-loader, `OnLoadMore`/`HasMore`.

### D — input (`Scenes/Input(D)`)
- `UIOTPInputControl` — N ячеек, авто-переход фокуса, вставка кода целиком.
- `UISearchFieldControl` — clear-кнопка, debounced `OnSearch`, dropdown подсказок.
- `UIDatePickerControl` — календарь-сетка месяца + навигация по месяцам/годам.
- `UIStarRatingControl` — клик/драг рейтинг, half-star, hover-preview, read-only.
- `UIColorPickerControl` — HSV-квадрат + hue-бар (рантайм-текстуры), hex.

### E — feedback / decoration (`Scenes/Feedback(E)`)
- `UISkeletonLoaderControl` — шиммер-заглушка; toggle skeleton/content; рантайм-градиент.
- `UICircularProgressControl` — кольцевой прогресс (рантайм-спрайт-бублик), determinate/indeterminate.
- `UIBadgeControl` — счётчик-бейдж, скрытие на нуле, `N+`, dot-режим, pop-анимация.
- `UIRippleEffectControl` — Material-волна из точки клика (рантайм-спрайт круга), обрезается маской.
- `UIMarqueeControl` — бегущая строка, `Loop`/`PingPong`, только при переполнении.

### F — gameplay (`Scenes/Gameplay(F)`)
- `UIVirtualJoystickControl` — экранный стик (fixed/floating, dead-zone), `Direction`/`Magnitude`.
- `UIRadialMenuControl` — радиальное меню, веер с stagger, авто-привязка пунктов.
- `UIKnobControl` — поворотный регулятор, драг по кругу, угловой sweep + шаги.
- `UINumberTickerControl` — анимированный счётчик (разделители, prefix/suffix, pop).
- `UIReorderableListControl` (+ `UIReorderableItem`) — drag-to-reorder вертикальный список.
- `UISwipeCardControl` — Tinder-свайп с наклоном, like/nope overlay, fling/spring.

### G — navigation (`Scenes/Navigation(G)`)
- `UITabBarControl` — нижняя навигация, скользящий индикатор, переключение страниц.
- `UIPaginationControl` — нумерация страниц с эллипсисом в фиксированном пуле слотов.
- `UIBreadcrumbsControl` — хлебные крошки, клик = навигация вверх.
- `UIWizardStepsControl` — индикатор шагов (done/active/upcoming) с заполняемыми коннекторами.
- `UIFloatingActionButtonControl` — FAB speed-dial с поворотом иконки.
- `UISideMenuControl` — выдвижной ящик с **любого из 4 краёв**; собственная раскладка пунктов
  (колонка для Left/Right, строка для Top/Bottom), пункты влетают вдоль оси выезда с pop;
  `MenuSide`, `SetSide`, `OnItemSelected`, поля `menuDepth`/`itemSpacing`.

### H — forms (`Scenes/Forms(H)`)
- `UIDropdownControl` — анимированный селект, флип стрелки, подсветка строки.
- `UIWheelPickerControl` — барабан со снапом и scale/fade по дистанции.
- `UIPasswordFieldControl` — show/hide + индикатор силы поверх `TMP_InputField`.
- `UITagInputControl` — Enter → удаляемый чип из шаблона.
- `UIValueSliderControl` — одиночный слайдер с пузырём значения над хэндлом.

### I — data / overlays (`Scenes/Data(I)`)
- `UIContextMenuControl` — попап у курсора с авто-флипом у краёв.
- `UITreeNodeControl` — раскрываемый узел дерева (шеврон + reflow layout-групп).
- `UIGaugeControl` — дуговая шкала со стрелкой, плавный ход к значению.
- `UIAvatarControl` — инициалы + цвет от имени + статус-дот, `+N` overflow.
- `UIEmptyStateControl` — заглушка (иконка/заголовок/текст/CTA), `Show/Hide`.
- `UIBannerControl` — постоянная плашка (info/success/warning/error), slide/fade, dismiss.

## Конвейер демо-сцен (editor builders)
- Каждый контрол сопровождается editor-билдером `UIXxxDemoSceneBuilder` с двумя точками входа:
  `[MenuItem] Create … Demo Scene` и `CreateXxxDemoSceneBatch()` для `-batchmode -executeMethod`.
- Сцены генерируются детерминированно и сохраняются в `Assets/Scenes/<категория>/`, затем копируются
  в `Samples~/DemoScenes/Scenes/<категория>/`.
- Категории E–I используют общий `UIDemoSceneFactory` (камера/canvas/панель, `Text`, `Button`,
  `Image`, `InputField`, `SetRef`/`SetRefArray`/`SetStringArray`, `Save` с авто-созданием папки и
  добавлением в Build Settings).
- Презентеры (`UIXxxDemoPresenter`) держат демо-логику и связываются через сериализуемые ссылки;
  сами контролы UI-структуру в runtime не создают (кроме рантайм-спрайтов/текстур у визуальных
  контролов: ColorPicker, CircularProgress, Ripple, Skeleton).

## Ключевые архитектурные правила (см. `systemPatterns.md`)
- **Graphic-less корень кнопки**: `UIButtonControl` ставится на объект без `Graphic` + дочерний `Bg`,
  иначе `UIStateAnimator.AutoAssign` форсит первый `Graphic` в белый на Normal.
- **Видимость через `CanvasGroup`, не `SetActive` на себе**: контрол не должен прятать собственный
  объект в `OnEnable` — реактивация повторно вызовет `OnEnable` (баг `UIBannerControl`). Бэкдропы
  на отдельных объектах можно выключать через `SetActive`.
- **MonoBehaviour = один файл по имени класса** (иначе «missing script»; кейс `UITooltipTrigger`).
- **`.meta` коммитятся вместе с `.cs`/сценой** — иначе на чужой машине новый GUID → «missing script»
  (кейс `UISideMenuControl`).
- `TMP_Text.textWrappingMode = TextWrappingModes.Normal` вместо obsolete `enableWordWrapping`.

## UPM-экспорт
- Пакет — `Assets/UIControls`. Подключение: `https://github.com/GladFox/UIControls.git?path=Assets/UIControls`.
- Метаданные: `package.json` (`0.20.0`), `README.md`, `CHANGELOG.md`, `UIControls.Editor.asmdef`.
- Зависимости в `package.json`: `com.unity.ugui`, `com.unity.textmeshpro`.
- `UIControls.Runtime.asmdef` → `UnityEngine.UI`; `DG.Tweening` через
  `overrideReferences + precompiledReferences: ["DOTween.dll"]`.
- Сэмпл `Demo Scenes` (`Samples~/DemoScenes`) включает все демо-сцены, разложенные по категорийным
  подпапкам (`Basics`, `Selection(A)`…`Data(I)`), + sample art (`Art/Slider`).

## Button Animation Library
- Путь: `Assets/UIControls/Animations/UI/Button` (Profiles: `TapProfile`/`TouchProfile`; Actions; States).
- Генератор: `UIControls/Create Button Animation Library`
  (`UIControlsButtonAnimationLibraryBuilder.CreateButtonAnimationLibraryBatch`).

## Пример кастомного SO-действия кнопки
```csharp
using UIControls.Runtime.Controls;
using UnityEngine;

[CreateAssetMenu(menuName = "UIControls/Actions/Play Sound")]
public sealed class UIButtonPlaySoundAction : UIButtonCustomAction
{
    public override void OnClick(UIButtonControl button)
    {
        // MyAudioFacade.Instance.Play("ui_click");
    }
}
```

## Ограничения текущей версии
- Полная визуальная проверка demo требует запуска в Unity Editor; для регрессий применяется
  play-mode зонд (editor-метод + `EnterPlaymode` + логирование позиций/альфы).
- Сборка сцен требует свободного инстанса Unity (lockfile) — билдеры запускаются по одному с
  ожиданием выхода процесса.

## План развития
- Объединить дубли (`UISegmentedControl`/`UITabSliderControl`, `UITabBarControl`).
- Prefab-набор на базе демо-контролов и editor-валидаторы обязательных ссылок.
- Опционально: новые категории контролов поверх текущего фундамента.
