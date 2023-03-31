using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NetworkCanvas : NetworkBehaviour
{
    public RectTransform camTouchArea;

    public override void OnStartClient()
    {
        base.OnStartClient();
        SetupClientCanvas();
    }

    private void SetupClientCanvas()
    {
        if (!isLocalPlayer) 
        {
            CleanCanvas();
        }        
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SetupServerCanvas();
    }

    private void SetupServerCanvas()
    {
        CleanCanvas();
    }

    private void CleanCanvas() 
    {        
        Destroy(GetComponent<CanvasScaler>());
        Destroy(GetComponent<GraphicRaycaster>());
        Destroy(GetComponent<Canvas>());
        Destroy(GetComponent<PlayerInputCustom>());
        Transform[] childs = transform.GetComponentsInChildren<Transform>();
        foreach (Transform child in childs) 
        {
            Destroy(child.gameObject);
        }
    }

    internal RectTransform GetTouchArea()
    {
        throw new NotImplementedException();
    }
}
