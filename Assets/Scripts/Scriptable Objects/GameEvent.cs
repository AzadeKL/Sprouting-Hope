using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game Event")]
public class GameEvent : ScriptableObject
{
    private List<GameEventListener> listeners = new List<GameEventListener>();
    public void TriggerEvent()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventTriggered();
        }
    }

    public void TriggerEvent(GameObject gameObjectToSend)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventTriggered(gameObjectToSend);
            listeners[i].OnEventTriggered();
        }
    }
    public void AddListener(GameEventListener listener)
    {
        listeners.Add(listener);
    }
    public void RemoveListener(GameEventListener listener)
    {
        listeners.Remove(listener);
    }

    [ContextMenu("Invoke Me")]
    private void MyButtonFunction()
    {
        // Add your button functionality here
        TriggerEvent();
    }
}
