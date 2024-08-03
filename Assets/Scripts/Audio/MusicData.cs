using System;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
public class MusicData : ScriptableObject
{
    public AudioClip audioClip;

    public int loopStart;
    public int loopEnd;


}
