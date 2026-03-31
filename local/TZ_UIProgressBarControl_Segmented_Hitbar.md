# ТЗ: UIProgressBarControl v2 (Segmented + HitBar, composable)

## 1. Цель
Подготовить требования для одного контрола прогресса, который поддерживает одновременно:
- сегментированное заполнение (деления);
- hitbar-логику (primary + delayed echo);
- кастомные события/хуки на ключевые моменты анимации.

## 2. Контекст и ограничения
- Стек: `uGUI` + `DOTween`.
- Настройка через `SerializeField`.
- Без runtime-создания `Canvas/Camera/EventSystem`.
- Работа в статичных сценах и префабах.

## 3. Архитектурное решение (принято)
Используется **один контрол**: `UIProgressBarControl` (v2), где поведение **суммируется**:
- `useSegments` отвечает за визуал делений/события сегментов;
- `useHitBar` отвечает за primary/echo поведение при изменении значения.

Режимы:
- `useSegments = false, useHitBar = false` -> обычный continuous bar.
- `useSegments = true, useHitBar = false` -> только сегментация.
- `useSegments = false, useHitBar = true` -> только hitbar.
- `useSegments = true, useHitBar = true` -> сегментация + hitbar одновременно.

## 4. Термины
- `Value` — целевое значение `[0..1]`.
- `Segment` — одно деление.
- `SegmentLocalFill` — локальное заполнение сегмента `[0..1]`.
- `Primary` — основной слой (например, зеленый).
- `Echo` — догоняющий слой (например, серый/красный).

## 5. Функциональные требования

### FR-01. Базовая логика значения
- Контрол хранит `Value` в диапазоне `[0..1]`.
- Публичный вход: `SetValue(float newValue, bool animate = true, bool notify = true)`.

### FR-02. Сегментация (`useSegments`)
- Поддержка `segmentsCount >= 1`.
- Распределение `Value` по сегментам с плавным tween-заполнением.
- Цвета сегмента:
  - `fillingColor` — сегмент в процессе;
  - `filledColor` — сегмент завершен.
- Событие `OnSegmentCompleted(int index)` при переходе сегмента в completed.
- На `OnSegmentCompleted` должна быть настройка:
  - анимация всего контрола;
  - анимация конкретного сегмента;
  - кастомные listeners/actions.

### FR-03. HitBar (`useHitBar`)
- Два слоя:
  - `primaryFillImage`;
  - `echoFillImage`.
- При уроне (`newValue < currentValue`):
  - `Primary` уходит быстро/мгновенно (`primaryDropDuration`);
  - `Echo` стартует после `echoDelay` и плавно догоняет за `echoDuration` с `echoEase`.
- При росте (`newValue > currentValue`) поведение настраивается `increaseMode`:
  - `SyncBoth`;
  - `InstantEchoToPrimary`.

### FR-04. Суммирование поведения
- При `useSegments && useHitBar`:
  - сегментация считается по `Primary` (обязательное поведение);
  - `Echo` остается continuous (минимально обязательный вариант);
  - события сегментов генерируются по `Primary`.
- Для уменьшения/увеличения значения должны одновременно выполняться правила сегментации и hitbar.

### FR-05. Кастомные эвенты
- Минимальные UnityEvent:
  - `OnValueChanged(float value)`
  - `OnSegmentCompleted(int segmentIndex)`
  - `OnEchoStarted(float from, float to)`
  - `OnEchoCompleted(float value)`
- Поддержать массив `UIProgressBarCustomAction[]` (SO-подход).
- Исключение в одном action не должно ломать работу контрола.

## 6. Предлагаемые сериализуемые поля
- `bool useSegments`
- `int segmentsCount`
- `Color fillingColor`
- `Color filledColor`
- `bool triggerControlStateOnSegmentCompleted`
- `bool triggerSegmentStateOnSegmentCompleted`
- `UnityEvent<int> onSegmentCompleted`
- `bool useHitBar`
- `Image primaryFillImage`
- `Image echoFillImage`
- `float primaryDropDuration`
- `float echoDelay`
- `float echoDuration`
- `Ease echoEase`
- `HitBarIncreaseMode increaseMode`
- `UIProgressBarCustomAction[] customActions`

## 7. API (минимум)
- `SetValue(float newValue, bool animate = true, bool notify = true)`
- `SetSegmentsCount(int count, bool rebuild = true)`
- `SetUseSegments(bool enabled, bool syncVisual = true)`
- `SetUseHitBar(bool enabled, bool syncVisual = true)`
- `ForceSyncVisual()`

## 8. Edge cases
- `Value` всегда clamp `[0..1]`.
- `segmentsCount <= 0` -> `1`.
- Быстрые последовательные `SetValue` корректно перезапускают tween (`Kill`/recreate) без артефактов.
- Если включен hitbar и нет обязательных ссылок (`primaryFillImage`/`echoFillImage`) -> лог ошибки + safe fallback.

## 9. Критерии приемки
- При `segmentsCount = 10` сегменты заполняются последовательно, `OnSegmentCompleted` срабатывает корректно и без дублей.
- На заполнении сегмента цвет меняется из `fillingColor` в `filledColor`.
- В hitbar-режиме при уроне:
  - `Primary` уходит первым;
  - `Echo` с задержкой плавно догоняет;
  - визуально читается «эхо-урон».
- В комбинированном режиме (`useSegments && useHitBar`) оба поведения работают одновременно без конфликтов.
- При выключенных флагах контрол остается обратно совместим с текущим простым progress bar.

## 10. Вне рамок текущего ТЗ
- Автогенерация сегментов в runtime.
- Shader/VFX-эффекты (glow/distortion/trails).
- Сетевые/репликационные сценарии.
