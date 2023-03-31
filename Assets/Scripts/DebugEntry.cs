using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Debuger;

public class DebugEntry : MonoBehaviour
{
    public TMP_Text text;

    public void Debug(string msg, debugType dtype) 
    {
        switch (dtype) 
        {
            case debugType.normal:
                text.color = Color.green;
                break;
            case debugType.warning:
                text.color = Color.yellow;
                break;
            case debugType.error:
                text.color = Color.red;
                break;
        }

        text.text = msg;        
    }
}
