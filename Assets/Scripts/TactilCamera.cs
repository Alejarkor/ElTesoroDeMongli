using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class TactilCamera : MonoBehaviour
{
    public Transform target;
    public Transform camera;
    public float targetDistance = 17f;
    public float currentDistance;
    public float height = 2.0f;
    public float rotationSpeed = 3.0f;
    public LayerMask collisionLayers;
    public float minDistance = 1.0f;    

    public float sensitivity;
    private Vector2 moveCamInput;
    
        

    void LateUpdate()
    {
        if (target)
        {
            float rotationSpeed = 5f; // Ajusta esto según tus preferencias
            Vector3 currentEulerAngles = camera.localEulerAngles;
            currentEulerAngles.y += moveCamInput.x * rotationSpeed;
            currentEulerAngles.x -= moveCamInput.y * rotationSpeed;
            currentEulerAngles.x = Mathf.Clamp(currentEulerAngles.x, -80f, 80f); // Ajusta estos valores según tus preferencias
            camera.localEulerAngles = currentEulerAngles;
        }
    }    

    private void OnCamMovement(InputValue value)
    {
        moveCamInput = value.Get<Vector2>() * sensitivity * Time.deltaTime;        
    }    
}
