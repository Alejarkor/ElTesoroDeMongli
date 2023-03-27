using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class ResolutionAdjuster : MonoBehaviour
{
    public TMP_Text text;
    void Start()
    {
        int width = Screen.width;
        int height = Screen.height;
         
        // Cambiar la resolución del juego para que se ajuste a la pantalla del dispositivo
        Screen.SetResolution(width, height, Screen.fullScreen);
        
        // Si lo prefieres, puedes establecer una resolución específica en función del tipo de dispositivo (móvil o escritorio)
        // if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        // {
        //     Screen.SetResolution(1280, 720, Screen.fullScreen);
        // }
        // else
        // {
        //     Screen.SetResolution(1920, 1080, Screen.fullScreen);
        // }
    }
    public void Update()
    {
        text.text = "resolution= " + Screen.width + "x" + Screen.height;
    }
}
