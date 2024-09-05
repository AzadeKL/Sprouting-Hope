using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class lamp : MonoBehaviour
{
    private DayNightCycle timer;

    [SerializeField] private GameObject light;

    [SerializeField] Sprite offLamp;
    [SerializeField] Sprite onLamp;

    void Start()
    {
        timer = GameObject.Find("Global Light 2D").GetComponent<DayNightCycle>();
    }

    void Update()
    {
        if (timer.isDay())
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = offLamp;
            light.SetActive(false);
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = onLamp;
            light.SetActive(true);
        }
    }
}
