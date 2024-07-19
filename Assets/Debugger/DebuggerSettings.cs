using UnityEngine;


[CreateAssetMenu(menuName = "DebuggerSettings", fileName = "DebuggerSettings")]
public class DebuggerSettings : ScriptableObject
{
    public bool isDebugLogsAllowed;
    [Range(0, 100)] // Range between 0 and 100
    public int AllowedPriorityLimit;
    [SerializeField]   
    public Object AllowedSystemType;
    public string IEnumeratorCallName;
  
}
