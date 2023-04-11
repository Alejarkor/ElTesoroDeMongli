using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
//using UnityEngine.InputSystem.EnhancedTouch;

public class MongliTouchManager : MonoBehaviour
{
    public List<int> validFingerIDs;
    public List<int> notValidFingerIDs;

    private void Start()
    {
        validFingerIDs = new List<int>();
        notValidFingerIDs = new List<int>();
    }

    public void UpdateValidFingerIDs(int finguerID) 
    {
        if (!validFingerIDs.Contains(finguerID) && !notValidFingerIDs.Contains(finguerID)) 
        {
            if (!IsTouchOverUI(finguerID))
            {
                validFingerIDs.Add(finguerID);               
            }
            else
            {
                notValidFingerIDs.Add(finguerID);               
            }
        }     
    }

    public void Update()
    {
        try
        {
            RemoveInvalidFingerIDs();
        }
        catch 
        {
            Debug.Log("Imposible removeInvalidFingers"); ;
        }
        
    }


    private bool IsTouchOverUI(Touch touch)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private bool IsTouchOverUI(int fingerID)
    {
        Touch touch = Input.GetTouch(fingerID);
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(touch.position.x, touch.position.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void RemoveInvalidFingerIDs()
    {
        Touch[] activeTouches = Input.touches;
        List<int> activeFingerIDs = new List<int>();

        // Llenar la lista de activeFingerIDs con los IDs de los toques activos
        foreach (Touch touch in activeTouches)
        {
            activeFingerIDs.Add(touch.fingerId);
        }

        validFingerIDs.RemoveAll(id => !activeFingerIDs.Contains(id));
        notValidFingerIDs.RemoveAll(id => !activeFingerIDs.Contains(id));
    }

    public bool IsFingerIDValid(int fingerID)
    {
        return validFingerIDs.Contains(fingerID);
    }
}
