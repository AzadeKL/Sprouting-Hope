using UnityEngine;
using UnityEngine.Events;

public class GameEventListener : MonoBehaviour
{
    [SerializeField]
    public GameEvent gameEvent;
    [SerializeField]
    public UnityEvent onEventTriggered;
    [SerializeField]
    public UnityEvent<GameObject> onEventTriggeredWithParameter;
    void OnEnable()
    {
        gameEvent.AddListener(this);
    }

    void OnDisable()
    {
        gameEvent.RemoveListener(this);
    }
    public void OnEventTriggered()
    {
        onEventTriggered.Invoke();
    }

    public void OnEventTriggered(GameObject go)
    {
        onEventTriggeredWithParameter.Invoke(go);
    }
}
