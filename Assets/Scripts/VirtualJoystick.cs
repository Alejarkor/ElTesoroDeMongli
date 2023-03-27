using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public Image bgImage;
    public Image joystickImage;

    public Vector2 InputDirection { get; private set; }
    public Vector2 value;
    private Vector2 startPosition;

    private void Start()
    {
        startPosition = Vector2.zero;
        InputDirection = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 currentPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgImage.rectTransform, eventData.position, eventData.pressEventCamera, out currentPosition))
        {
            currentPosition -= startPosition;
            float size = bgImage.rectTransform.sizeDelta.x;
            InputDirection = currentPosition / size;
            InputDirection = Vector2.ClampMagnitude(InputDirection, 1);
            value = InputDirection;
            joystickImage.rectTransform.anchoredPosition = InputDirection * size * 0.5f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InputDirection = Vector2.zero;
        value = InputDirection;
        joystickImage.rectTransform.anchoredPosition = Vector2.zero;
    }
}
