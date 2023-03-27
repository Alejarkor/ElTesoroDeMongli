using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualButton : Button 
{
    public Button button;
    public bool value { get { return IsPressed(); } }   

}