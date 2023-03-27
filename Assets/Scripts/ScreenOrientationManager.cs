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
            // El dispositivo está en vertical (portrait)
            // Realiza aquí los ajustes necesarios para la orientación vertical
            Debug.Log("Vertical");
        }
        else if (currentOrientation == ScreenOrientation.LandscapeLeft || currentOrientation == ScreenOrientation.LandscapeRight)
        {
            // El dispositivo está en horizontal (landscape)
            // Realiza aquí los ajustes necesarios para la orientación horizontal
            Debug.Log("Horizontal");
        }
    }
}
