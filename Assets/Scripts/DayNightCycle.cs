using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{

    [SerializeField] private Light2D globalLight;
    [SerializeField] private float dayDuration = 60f;
    [SerializeField] private float nightIntensity = 0.1f;
    [SerializeField] private float dayIntensity = 1f;
    [SerializeField] private Color dayColor = Color.white;
    [SerializeField] private Color nightColor = Color.blue;
    [Range(0f, 60f)]
    [SerializeField] private float cycleStartTime;

    [SerializeField] private FloatReference time24HFormat;
    [SerializeField] private FloatReference dayCounter;


    [SerializeField] private GameManager gameManager;

    [SerializeField] private GameEvent dayChange;

    private float cycleTimer;


    void Start()
    {
        if (globalLight == null)
        {
            globalLight = GetComponent<Light2D>();
        }
        cycleTimer = cycleStartTime;
        dayCounter.Value = 1;
    }

    void Update()
    {
        cycleTimer += Time.deltaTime;
        float cycleProgress = cycleTimer / dayDuration;
        time24HFormat.Value = cycleProgress * 24;
        float lightIntensity;
        Color lightColor;



        if (cycleProgress < 0.5f)
        {
            lightIntensity = Mathf.Lerp(nightIntensity, dayIntensity, cycleProgress * 2);
            lightColor = Color.Lerp(nightColor, dayColor, cycleProgress * 2);

        }
        else
        {
            lightIntensity = Mathf.Lerp(dayIntensity, nightIntensity, (cycleProgress - 0.5f) * 2);
            lightColor = Color.Lerp(dayColor, nightColor, (cycleProgress - 0.5f) * 2);
        }

        globalLight.intensity = lightIntensity;
        globalLight.color = lightColor;

        if (cycleProgress >= 1f)
        {
            cycleTimer = 0f;
            dayCounter.Value++;
            //gameManager.UpdateCrops("Wheat");
        }
    }
}
