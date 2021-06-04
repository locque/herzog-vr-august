
using UnityEngine;

public class Debuggable
{
    private bool debugEnabled = false;
    private Component parent = null;

    public bool DebugEnabled
    {
        get { return debugEnabled; }
        set { debugEnabled = value; }
    }

    public Debuggable(Component parent, bool debugEnabled)
    {
        DebugEnabled = debugEnabled;
        this.parent = parent;
    }

    public void Log(object logObject)
    {
        if (debugEnabled)
            Debug.Log(parent.name + ":\n" + logObject);
    }
}