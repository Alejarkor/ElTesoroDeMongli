using UnityEngine;

public class ScreenOrientationManager : MonoBehaviour
{
    private ScreenOrientation currentOrientation;

    void Start()
    {
        currentOrientation = Screen.orientation;
        CheckOrientation();
    }

    void Update()
    {
        CheckOrientation();
    }

    void CheckOrientation()
    {
        if (currentOrientation != Screen.orientation)
        {
            currentOrientation = Screen.orientation;
            HandleOrientationChange();
        }
    }

    void HandleOrientationChange()
    {
        if (currentOrientation == ScreenOrientation.Portrait || currentOrientation == ScreenOrientation.PortraitUpsideDown)
        {
            // El dispositivo est� en vertical (portrait)
            // Realiza aqu� los ajustes necesarios para la orientaci�n vertical
            Debug.Log("Vertical");
        }
        else if (currentOrientation == ScreenOrientation.LandscapeLeft || currentOrientation == ScreenOrientation.LandscapeRight)
        {
            // El dispositivo est� en horizontal (landscape)
            // Realiza aqu� los ajustes necesarios para la orientaci�n horizontal
            Debug.Log("Horizontal");
        }
    }
}
