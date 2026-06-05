using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIControls.Runtime.Controls
{
    /// <summary>
    /// A month-grid date picker. A header shows the current month/year with prev/next arrows; a
    /// 6×7 grid shows the days. Click a day to select it (clicking an adjacent-month day moves to
    /// that month). The selected day and today are highlighted. Read the chosen day via
    /// <see cref="SelectedDate"/>; <see cref="OnDateChanged"/> fires on selection.
    /// </summary>
    public sealed class UIDatePickerControl : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text headerLabel;
        [SerializeField] private UIButtonControl prevButton;
        [SerializeField] private UIButtonControl nextButton;

        [Header("Grid (42 cells = 6 rows × 7 cols)")]
        [SerializeField] private UIButtonControl[] dayButtons = Array.Empty<UIButtonControl>();
        [SerializeField] private TMP_Text[] dayLabels = Array.Empty<TMP_Text>();
        [SerializeField] private Image[] dayBackgrounds = Array.Empty<Image>();

        [Header("Options")]
        [Tooltip("Start weeks on Monday (otherwise Sunday).")]
        [SerializeField] private bool mondayFirst = true;

        [Header("Colors")]
        [SerializeField] private Color cellColor = new Color(0.16f, 0.2f, 0.29f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.24f, 0.55f, 0.95f, 1f);
        [SerializeField] private Color todayColor = new Color(0.28f, 0.34f, 0.46f, 1f);
        [SerializeField] private Color inMonthText = new Color(0.95f, 0.97f, 1f, 1f);
        [SerializeField] private Color outMonthText = new Color(0.5f, 0.56f, 0.68f, 1f);

        [Header("Events")]
        [SerializeField] private UnityEvent onDateChanged = new UnityEvent();

        private readonly DateTime[] cellDates = new DateTime[42];
        private DateTime selectedDate = DateTime.Today;
        private int displayYear;
        private int displayMonth;

        public UnityEvent OnDateChanged => onDateChanged;
        public DateTime SelectedDate => selectedDate;

        private void Awake()
        {
            for (var i = 0; i < dayButtons.Length; i++)
            {
                var captured = i;
                if (dayButtons[i] != null)
                {
                    dayButtons[i].OnClick.AddListener(() => OnCellClicked(captured));
                }
            }

            if (prevButton != null)
            {
                prevButton.OnClick.AddListener(PreviousMonth);
            }

            if (nextButton != null)
            {
                nextButton.OnClick.AddListener(NextMonth);
            }
        }

        private void OnEnable()
        {
            displayYear = selectedDate.Year;
            displayMonth = selectedDate.Month;
            Rebuild();
        }

        public void SetSelectedDate(DateTime date, bool notify = true)
        {
            selectedDate = date.Date;
            displayYear = selectedDate.Year;
            displayMonth = selectedDate.Month;
            Rebuild();

            if (notify)
            {
                onDateChanged?.Invoke();
            }
        }

        public void PreviousMonth()
        {
            ShiftMonth(-1);
        }

        public void NextMonth()
        {
            ShiftMonth(1);
        }

        private void ShiftMonth(int delta)
        {
            var d = new DateTime(displayYear, displayMonth, 1).AddMonths(delta);
            displayYear = d.Year;
            displayMonth = d.Month;
            Rebuild();
        }

        private void OnCellClicked(int index)
        {
            if (index < 0 || index >= 42)
            {
                return;
            }

            var date = cellDates[index];
            selectedDate = date;
            displayYear = date.Year;
            displayMonth = date.Month;
            Rebuild();
            onDateChanged?.Invoke();
        }

        private void Rebuild()
        {
            var first = new DateTime(displayYear, displayMonth, 1);
            var offset = mondayFirst
                ? (((int)first.DayOfWeek + 6) % 7)
                : (int)first.DayOfWeek;
            var gridStart = first.AddDays(-offset);
            var today = DateTime.Today;

            if (headerLabel != null)
            {
                headerLabel.text = first.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }

            for (var i = 0; i < 42; i++)
            {
                var date = gridStart.AddDays(i);
                cellDates[i] = date;
                var inMonth = date.Month == displayMonth && date.Year == displayYear;

                if (dayLabels != null && i < dayLabels.Length && dayLabels[i] != null)
                {
                    dayLabels[i].text = date.Day.ToString();
                    dayLabels[i].color = inMonth ? inMonthText : outMonthText;
                }

                if (dayBackgrounds != null && i < dayBackgrounds.Length && dayBackgrounds[i] != null)
                {
                    Color color;
                    if (date == selectedDate)
                    {
                        color = selectedColor;
                    }
                    else if (date == today)
                    {
                        color = todayColor;
                    }
                    else
                    {
                        color = cellColor;
                    }

                    dayBackgrounds[i].color = color;
                }
            }
        }
    }
}
