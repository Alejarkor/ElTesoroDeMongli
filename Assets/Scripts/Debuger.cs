using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuger : Singleton<Debuger>
{
    public enum debugType 
    {
        normal = 0,
        warning = 1,
        error = 2
    }
    public GameObject debugEntryPrefab;
    public Transform content;
    public void DebugMsg(string msg, debugType type) 
    {        
        GameObject newEntry = Instantiate(debugEntryPrefab);
        newEntry.transform.parent = content;
        newEntry.GetComponent<DebugEntry>().Debug(msg, type);        
    }
}
