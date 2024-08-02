using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System;
using UnityEditor;


#if UNITY_EDITOR
using System.Reflection;
#endif

public class GetExposedMixerParameters : MonoBehaviour
{
    public AudioMixer audioMixer;
    [Tooltip("Input manually or use Context Menu/CollectExposedParamaters")]
    public List<string> exposedParameterNames;

    void Start()
    {
#if UNITY_EDITOR
        if (exposedParameterNames.Count == 0)
        {
            CollectExposedParamaters();
        }
#endif
        foreach (string parameterName in exposedParameterNames)
        {
            var value = PlayerPrefs.GetFloat(parameterName, 1f);
            audioMixer.SetFloat(parameterName, Mathf.Log10(value) * 20);
        }

    }
#if UNITY_EDITOR
    [ContextMenu("CollectExposedParamaters")]
    private void CollectExposedParamaters()
    {
        exposedParameterNames.Clear();
        exposedParameterNames = GetAllExposedParameters(audioMixer);
        EditorUtility.SetDirty(this);
    }
#endif
#if UNITY_EDITOR
    List<string> GetAllExposedParameters(AudioMixer mixer)
    {

        Array exposedParams = (Array) audioMixer.GetType().GetProperty("exposedParameters")?.GetValue(audioMixer, null);

        if (exposedParams != null)
        {
            foreach (var param in (System.Array) exposedParams)
            {
                FieldInfo nameField = param.GetType().GetField("name");
                if (nameField != null)
                {
                    string paramName = (string) nameField.GetValue(param);
                    exposedParameterNames.Add(paramName);
                    Debugger.Log(paramName, Debugger.PriorityLevel.MustShown);
                }
            }
        }


        return exposedParameterNames;
    }
#endif
}
