using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class CustomButton : Button
{
    public event System.Action<int> OnTouchId;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        
        if (eventData is ExtendedPointerEventData extendedEventData)
        {
            Debug.Log("button " + this.gameObject.name + " pressed. ID: " + extendedEventData.touchId);
            int touchId = extendedEventData.touchId;
            OnTouchId?.Invoke(touchId);
        }
    }
}
