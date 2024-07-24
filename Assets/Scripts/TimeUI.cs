using System;
using TMPro;
using UnityEngine;


public class TimeUI : MonoBehaviour
{
    [SerializeField] private FloatReference time24HFormat;

    [SerializeField] private TextMeshProUGUI _textMeshPro;

    private void Update()
    {

        TimeSpan time = TimeSpan.FromHours((double) time24HFormat.Value);
        int minutes = (int) Math.Round(time.TotalMinutes / 15) * 15;
        _textMeshPro.text = TimeSpan.FromMinutes(minutes).ToString(@"hh\:mm");
    }
}
