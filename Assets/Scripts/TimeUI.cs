using System;
using TMPro;
using UnityEngine;


public class TimeUI : MonoBehaviour
{
    [SerializeField] private FloatReference time24HFormat;
    [SerializeField] private FloatReference dayCounter;

    [SerializeField] private TextMeshProUGUI _textMeshProDayCounter;
    [SerializeField] private TextMeshProUGUI _textMeshProTimer;

    private void Update()
    {

        TimeSpan time = TimeSpan.FromHours((double) time24HFormat.Value);
        int minutes = (int) Math.Round(time.TotalMinutes / 15) * 15;
        _textMeshProTimer.text = TimeSpan.FromMinutes(minutes).ToString(@"hh\:mm");

        _textMeshProDayCounter.text = "Day " + dayCounter.Value.ToString("F0");
    }
}
