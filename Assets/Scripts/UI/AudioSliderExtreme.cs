using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSliderExtreme : MonoBehaviour
{


    [SerializeField]
    private Image HandleImage;
    [SerializeField]
    public Gradient HandleImagegradient;
    [SerializeField]
    private AudioMixer Mixer;

    [SerializeField]
    private AudioMixMode MixMode;

    [SerializeField]
    private string ExposedParameterName;

    [SerializeField]
    private AudioSource Example_effect;
    private void Start()
    {
        var volumeLevel = PlayerPrefs.GetFloat(ExposedParameterName, 1);
        var mySlider = GetComponent<Slider>();
        mySlider.value = volumeLevel;
        OnChangeSlider(volumeLevel);
    }

    public void OnChangeSlider(float Value)
    {

        HandleImage.fillAmount = 1 - ((1 - Value) / 2);
        HandleImage.color = HandleImagegradient.Evaluate(Value);
        switch (MixMode)
        {

            case AudioMixMode.LinearMixerVolume:
            Mixer.SetFloat(ExposedParameterName, (-80 + Value * 80));
            break;
            case AudioMixMode.LogrithmicMixerVolume:
            Mixer.SetFloat(ExposedParameterName, Mathf.Log10(Value) * 20);
            break;
        }

        float a = Mathf.Log10(Value) * 20;

        PlayerPrefs.SetFloat(ExposedParameterName, Value);
        PlayerPrefs.Save();

        if (Example_effect != null && Example_effect.isPlaying == false)
        {
            Example_effect.Play();
        }
    }


    public enum AudioMixMode
    {
        LinearMixerVolume,
        LogrithmicMixerVolume
    }


}
