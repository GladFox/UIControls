# UIControls Architecture

`local/README.md` - источник правды по архитектуре библиотеки контролов.

## Цель
Создать переиспользуемую библиотеку контролов на базе uGUI с анимациями через DOTween.

## Границы решения
- Внешние UI-решения используются только как референс поведения/идей.
- Runtime-контролы библиотеки не зависят от сторонних UI-библиотек.
- Все анимации реализуются через `DG.Tweening`.

## Модульная структура
```text
Assets/UIControls/
  Animations/
    UI/Button/
      Profiles/
      Actions/
      States/
  Runtime/
    UIControls.Runtime.asmdef
    Core/
      UITweenSettings.cs
      UIStateVisual.cs
      UIStateVisualAsset.cs
      UIStateAnimator.cs
    Controls/
      UIButtonControl.cs
      UIButtonAnimationProfile.cs
      UIButtonCustomAction.cs
      UIProgressBarCustomAction.cs
      Actions/
        UIButtonActionTriggerFlags.cs
        UIButtonScalePulseAction.cs
        UIButtonAnchoredOffsetAction.cs
        UIProgressBarDebugLogAction.cs
      UIToggleControl.cs
      UIModalControl.cs
      UIProgressBarControl.cs
    Demo/
      UIControlsDemoPresenter.cs
      UIProgressBarDemoPresenter.cs
  Editor/
    UIControlsDemoSceneBuilder.cs
    UIProgressBarDemoSceneBuilder.cs
    UIControlsButtonAnimationLibraryBuilder.cs
```

## Core-компоненты
- `UITweenSettings`:
  единые параметры tween (`duration`, `ease`, `delay`, `independentUpdate`) и метод применения к tween.
- `UIStateVisual`:
  снимок визуального состояния (`scale`, `alpha`, `color`) + настройки tween.
- `UIStateVisualAsset` (`ScriptableObject`):
  переиспользуемый пресет `UIStateVisual` для применения в разных префабах.
- `UIStateAnimator`:
  унифицированная анимация перехода состояния для `RectTransform`, `CanvasGroup`, `Graphic`.

## Контролы v0.2
- `UIButtonControl`:
  состояния `Normal/Hover/Pressed/Disabled`, событие `OnClick`, pointer + submit ввод.
  Визуальные состояния берутся из `UIButtonAnimationProfile` (если назначен) или из локальных `UIStateVisualAsset` (fallback).
  Дополнительно поддерживаются кастомные SO-действия через:
  - `UIButtonAnimationProfile.customActions` (общие для профиля)
  - локальный `UIButtonCustomAction[]` на компоненте
- `UIToggleControl`:
  переключатель `On/Off` с анимацией позиции handle и цветов, событие `OnValueChanged(bool)`.
- `UIModalControl`:
  `Show/Hide/Toggle`, fade + scale + anchored position.
- `UIProgressBarControl`:
  поддерживает комбинируемые режимы:
  - обычный continuous progress
  - segmented progress (`useSegments`, `segmentsCount`, `OnSegmentCompleted`)
  - auto-generated segmented visuals (`autoGenerateSegments`, `segmentVisualMode`, `dividerWidth`, `dividerColor`)
  - hitbar (`useHitBar`: `primaryFillImage` + `echoFillImage` с delayed echo trail)
  - визуальные override для генерации сегментов (`segmentFillSprite`, `segmentDividerSprite`)
  - suppress echo trail на восстановлении (`hideEchoOnIncrease`)
  В режиме `useSegments + useHitBar` сегментация применяется к `Primary` слою.
  Поддерживаются кастомные SO-действия через `UIProgressBarCustomAction[]`.

## Кастомные SO-действия кнопки
- Базовый класс: `UIButtonCustomAction : ScriptableObject`.
- Доступные хуки:
  - `OnPointerEnter(UIButtonControl button)`
  - `OnPointerExit(UIButtonControl button)`
  - `OnPointerDown(UIButtonControl button)`
  - `OnPointerUp(UIButtonControl button)`
  - `OnSubmit(UIButtonControl button)`
  - `OnStateChanged(UIButtonControl button, UIButtonControl.ButtonVisualState state)`
  - `OnClick(UIButtonControl button)`
- Любая внешняя библиотека (например, аудио) может создать свой наследник и назначить asset в `UIButtonControl`.

## Кастомные SO-действия ProgressBar
- Базовый класс: `UIProgressBarCustomAction : ScriptableObject`.
- Доступные хуки:
  - `OnValueChanged(UIProgressBarControl progressBar, float value)`
  - `OnSegmentCompleted(UIProgressBarControl progressBar, int segmentIndex)`
  - `OnEchoStarted(UIProgressBarControl progressBar, float from, float to)`
  - `OnEchoCompleted(UIProgressBarControl progressBar, float value)`

## Button Animation Library
- Путь библиотеки: `Assets/UIControls/Animations/UI/Button`.
- Профили:
  - `TapProfile.asset`
  - `TouchProfile.asset`
- Предустановленные экшены:
  - `Tap_DownOffset.action.asset` (аналог Down Transition с `anchoredPosition.y = -10`)
  - `Tap_ClickPulse.action.asset` (аналог Click scale pulse `1.0 -> 1.1 -> 1.0`)
  - `Touch_DownPulse.action.asset` (аналог Touch Down pulse)
- Генератор библиотеки:
  - `UIControls/Create Button Animation Library`
  - batch entrypoint: `UIControls.Editor.UIControlsButtonAnimationLibraryBuilder.CreateButtonAnimationLibraryBatch`

## Demo-сцены
- Базовая сцена: `Assets/Scenes/UIControlsDemo.unity`.
- В сцене заранее размещены `Main Camera`, `Canvas`, `EventSystem` и demo-контролы.
- Контролы не создаются динамически в runtime.
- Связи демонстрации настраиваются через сериализуемые поля `UIControlsDemoPresenter`.
- Отдельная сцена ProgressBar v2: `Assets/Scenes/UIProgressBarDemo.unity`.
- ProgressBar demo включает полноценный сценарий:
  - `Damage -12%`, `Heavy -35%`, `Heal +8%`, `Reset`;
  - верхний бар (`Health`): `useSegments + useHitBar`, при уроне основной слой быстро падает и эхо догоняет, при лечении HP обновляется сразу;
  - нижний бар (`Energy`): авто-набор энергии от `0` до `3` за `6` секунд, `3` сегмента, плавный переход между сегментами с окраской завершенных делений в основной цвет;
  - текстуры из `Assets/ThirdParty/Layer Lab/.../Slider_*` в качестве визуального примера;
  - вывод статусов по событиям `segment/echo` и текстовый индикатор энергии.
- `UIControlsDemoSceneBuilder` собирает базовую сцену контролов.
- `UIProgressBarDemoSceneBuilder` собирает специализированную ProgressBar сцену.
- Оба builder'а автоматически назначают `TapProfile` на demo-кнопки.
- Для демонстрации расширяемости через SO создается/назначается asset:
  - `Assets/UIControls/Animations/UI/ProgressBar/Actions/DemoProgressBarDebug.action.asset`
  - тип: `UIProgressBarDebugLogAction`.

## Пример кастомного действия
```csharp
using UIControls.Runtime.Controls;
using UnityEngine;

[CreateAssetMenu(menuName = "UIControls/Actions/Play Sound")]
public sealed class UIButtonPlaySoundAction : UIButtonCustomAction
{
    public override void OnClick(UIButtonControl button)
    {
        // Вызов вашей аудио-библиотеки
        // MyAudioFacade.Instance.Play("ui_click");
    }
}
```

## Ограничения текущей версии
- Полная визуальная проверка demo требует запуска сцены в Unity Editor.

## План развития
- Добавить prefab-набор на базе демо-контролов.
- Добавить анимированные `Dropdown`, `Tabs`, `Slider`.
- Добавить editor-валидаторы и контроль обязательных ссылок.
