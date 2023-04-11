using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;

public class CaptureTouchId : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    public int touchID;
    public int fingerID;

   
    public void OnPointerDown(PointerEventData eventData)
    {
        touchID = eventData.pointerId;        
        Debug.Log("ID del toqueDown: " + touchID);        
    }


    void Update()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (!IsTouchOverUI(touch))
            {
                Vector2 deltaPosition = touch.deltaPosition;
                // Aquí puedes usar deltaPosition para lo que necesites
                Debug.Log("Finger ID: " + touch.fingerId + ", Delta Position: " + deltaPosition);
            }
        }
    }



    public void OnPointerUp(PointerEventData eventData)
    {
        touchID = eventData.pointerId;
        Debug.Log("ID del toqueUp: " + touchID);
        touchID = -1;
    }

    private bool IsTouchMatchingPointer(PointerEventData eventData, Vector2 touchPosition)
    {
        Vector2 screenPoint = eventData.pressEventCamera.WorldToScreenPoint(eventData.pointerPressRaycast.worldPosition);
        return Vector2.Distance(screenPoint, touchPosition) < 1.0f;
    }

    private bool IsTouchOverUI(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}
