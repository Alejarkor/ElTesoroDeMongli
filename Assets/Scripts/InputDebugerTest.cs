using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDebugerTest : MonoBehaviour
{
    public TMP_Text text;
    public TMP_Text text2;
    public Vector2 joyL;
    public Vector2 joyR;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMovement(InputValue value) 
    {
        joyL = value.Get<Vector2>();
        text.text = joyL.ToString();
    }

    private void OnCamMovement(InputValue value)
    {
        joyR = value.Get<Vector2>();
        text2.text = joyR.ToString();
    }
}
