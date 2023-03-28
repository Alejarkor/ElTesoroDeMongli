using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenController : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void FullscreenRequest();

    public Image image;
    public Sprite fullScreenOn;
    public Sprite fullScreenOff;


    public Button fullscreenButton;

    void Start()
    {
        fullscreenButton.onClick.AddListener(GoFullscreen);
    }
    void GoFullscreen()
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            FullscreenRequest();
        }
        else
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
        SwitchSprite();
    }

    void SwitchSprite() 
    {
        if (image.sprite == fullScreenOn)
        {
            image.sprite = fullScreenOff;
        }
        else 
        {
            image.sprite = fullScreenOn;
        }
    }
}
