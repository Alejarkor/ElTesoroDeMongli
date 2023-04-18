using UnityEngine;
using TMPro;
public class MongliBoxName : MonoBehaviour
{
    public TMP_Text text;
    Camera cam;
   
    internal void SetName(string nickName)
    {
        Debug.Log("set nickname: " + nickName);
        text.text = nickName;
        if(!cam) cam = FindObjectOfType<Camera>();
    }
    private void Start()
    {
        cam = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (cam)
        {
            transform.LookAt(cam.transform);
        }      
    }
}
