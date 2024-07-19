using System.Diagnostics;
using UnityEngine;
using DEBUG = UnityEngine.Debug;
public static class Debugger
{
    static DebuggerSettings settings;   
    public static bool isGuiMenuOpen=false;
    public enum PriorityLevel
    {
        MustShown=0,
        High=10,
        Medium=30,
        Low=50,
        LeastImportant=100
    }
    static Debugger()
    {
        settings = Resources.Load<DebuggerSettings>("DebuggerSettings");
    }
    public static void Log(object obj)
    {
        var stackTrace = new StackTrace();
        var callingMethod = stackTrace.GetFrame(1).GetMethod();
        var callingClassName = callingMethod.DeclaringType.Name;
        var namedFlag = settings.AllowedSystemType==null || settings.AllowedSystemType.name == callingClassName;

        if (settings.isDebugLogsAllowed==false || namedFlag==false) return;
        DEBUG.Log(obj);
    }

    public static void Log(object obj,int priority)
    {
        var stackTrace = new StackTrace();
        var callingMethod = stackTrace.GetFrame(1).GetMethod();
        var callingClassName = callingMethod.DeclaringType.Name;
        var namedFlag = CheckCallingMethod(callingClassName);

        if (settings.isDebugLogsAllowed == false || namedFlag == false) return;
        if (settings.AllowedPriorityLimit < priority) return;
        DEBUG.Log(obj);
    }

    public static void Log(object obj, PriorityLevel priority)
    {
        var stackTrace = new StackTrace();
        var callingMethod = stackTrace.GetFrame(1).GetMethod();
        var callingClassName = callingMethod.DeclaringType.Name;
        var namedFlag = CheckCallingMethod(callingClassName);       

        if (settings.isDebugLogsAllowed == false || namedFlag == false) return;
        if (settings.AllowedPriorityLimit < (int) priority) return;
        DEBUG.Log(obj);
    }

    private static bool CheckCallingMethod(string callingClassName)
    {
        var flag = settings.AllowedSystemType == null || settings.AllowedSystemType.name == callingClassName;

        if (flag == false && settings.IEnumeratorCallName.Length > 0)
        {
            flag = callingClassName.Contains(settings.IEnumeratorCallName);
        }
        return flag;
    }
}
