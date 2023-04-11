using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MongliCameraController : MonoBehaviour
{
    public virtual void SetInput(Vector2 cameraInput, float deltaTime){}
}
