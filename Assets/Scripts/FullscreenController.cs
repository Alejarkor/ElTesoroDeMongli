using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class FullscreenController : MonoBehaviour
{
    public Image image;
    public Sprite fullScreenOn;
    public Sprite fullScreenOff;
    //public Button fullscreenButton;

    //void Start()
    //{
    //    fullscreenButton.onClick.AddListener(GoFullscreen);
    //}

    //void GoFullscreen()
    //{
    //    if (Screen.fullScreenMode == FullScreenMode.Windowed)
    //    {
    //        Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
    //    }
    //    else
    //    {
    //        Screen.fullScreenMode = FullScreenMode.Windowed;
    //    }
    //}

    public Button fullscreenButton;

    void Start()
    {
        fullscreenButton.onClick.AddListener(GoFullscreen);
    }
    void GoFullscreen()
    {
        Application.ExternalCall("toggleFullscreen");
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
