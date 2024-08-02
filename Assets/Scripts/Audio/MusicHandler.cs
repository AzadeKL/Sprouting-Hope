using UnityEngine;


public class MusicHandler : MonoBehaviour
{
    [SerializeField] private AudioClip dayTimeMusic;
    [SerializeField] private AudioClip nightTimeMusic;

    [SerializeField] private FloatReference currentTime;

    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {

    }


    private void Update()
    {



    }
}
