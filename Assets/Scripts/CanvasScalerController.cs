using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerController : MonoBehaviour
{
    private CanvasScaler canvasScaler;

    void Start()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        SetCanvasScale();
    }

    void SetCanvasScale()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        canvasScaler.referenceResolution = new Vector2(canvasScaler.referenceResolution.y * screenRatio, canvasScaler.referenceResolution.y);
    }
}
