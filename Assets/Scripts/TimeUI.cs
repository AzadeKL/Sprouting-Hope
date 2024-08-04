using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeUI : MonoBehaviour
{
    [SerializeField] private FloatReference time24HFormat;
    [SerializeField] private FloatReference dayCounter;

    [SerializeField] private TextMeshProUGUI _textMeshProDayCounter;
    [SerializeField] private TextMeshProUGUI _textMeshProTimer;

    [SerializeField] private List<Sprite> spriteListForHourHand;
    [SerializeField] private List<Sprite> spriteListForMinuteArm;

    [SerializeField] private Image hourHand;
    [SerializeField] private GameObject minute;


    private void Update()
    {

        TimeSpan time = TimeSpan.FromHours((double) time24HFormat.Value);

        //Just different ways to show clock. Not deleting for quick demonstraions if need arises. 

        // int minutes = (int) Math.Round(time.TotalMinutes / 15) * 15;
        //_textMeshProTimer.text = TimeSpan.FromMinutes(minutes).ToString(@"hh\:mm");

        //_textMeshProTimer.text = "" + time.Hours + ":" + Mathf.Floor(time.Minutes / 15) * 15;

        _textMeshProTimer.text = time.ToString(@"hh\:mm");


        DoMinutes(time);
        _textMeshProDayCounter.text = "Day " + dayCounter.Value.ToString("F0");
        hourHand.overrideSprite = spriteListForHourHand[(time.Hours - 1 + 12) % 12];
    }

    private void DoMinutes(TimeSpan time)
    {
        //180 is 0;

        minute.transform.rotation = Quaternion.Euler(0, 0, 180 - time.Minutes * 6);
    }
}
