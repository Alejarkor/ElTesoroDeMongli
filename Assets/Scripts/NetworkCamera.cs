using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class NetworkCamera : NetworkBehaviour
{
    private Transform target;
    public float targetDistance = 17f;
    public float currentDistance;
    public float height = 2.0f;
    public float rotationSpeed = 3.0f;
    public LayerMask collisionLayers;
    public float minDistance = 1.0f;
    private RectTransform touchArea;
    private bool initialTouchInside = false;

    private float x = 0.0f;
    private float y = 0.0f;

    private Vector3 transformPosition;
    private Quaternion transformRotation;

    public override void OnStartServer()
    {
        Destroy(GetComponent<Camera>());
        Destroy(GetComponent<AudioListener>());
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (isLocalPlayer)
        {
            InitializeCamera();
        }
        else 
        {
            Destroy(GetComponent<Camera>());
            Destroy(GetComponent<AudioListener>());
        }
    }


    public void Update()
    {
        if (isLocalPlayer)
        {
            ProcessCam();
        }
        else if (isServer) 
        {
            SendTransformDataToClients(transform.position, transform.rotation);
        }
    }
    public void LateUpdate()
    {
        if (isLocalPlayer) 
        {
            LateProcessCam();
            SendTransformDataToServer(transform.position, transform.rotation);
        }
        
    }


    public void InitializeCamera() 
    {
        currentDistance = targetDistance;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }   

    public void SetTarget(Transform tgt) 
    {
        target = tgt;
    }
    public void SetTouchArea(RectTransform rect) 
    {
        touchArea = rect;
    }

    void ProcessCam()
    {
        if (!target || !touchArea) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Verifica si el primer toque está dentro del área touchArea
            initialTouchInside = IsInsideTouchArea(Input.mousePosition);
        }

        // Si el toque se mantiene presionado y el primer toque fue dentro de touchArea
        if (Input.GetMouseButton(0) && initialTouchInside)
        {
            x += Input.GetAxis("Mouse X") * rotationSpeed;
            y -= Input.GetAxis("Mouse Y") * rotationSpeed;

            y = ClampAngle(y, -80, 80);
        }

        // Si el botón izquierdo del mouse (toque en pantalla táctil) se suelta
        if (Input.GetMouseButtonUp(0))
        {
            // Reinicia la variable initialTouchInside
            initialTouchInside = false;
        }
    }

    void LateProcessCam() 
    {
        if (!target || !touchArea) return;
            
        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 desiredPosition = rotation * new Vector3(0.0f, height, -targetDistance) + target.position;

        if (Physics.Linecast(target.position + new Vector3(0, height, 0), desiredPosition, out RaycastHit hit, collisionLayers))
        {
            float newDistance = Vector3.Distance(target.position + new Vector3(0, height, 0), hit.point);
            currentDistance = Mathf.Clamp(newDistance, minDistance, targetDistance);
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 5.0f);
        }
        else
        {
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * 5.0f);
        }

        Vector3 position = rotation * new Vector3(0.0f, height, -currentDistance) + target.position;

        transform.rotation = rotation;
        transform.position = position;   
    }

    [Command]
    private void SendTransformDataToServer(Vector3 pos, Quaternion rot) 
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    [ClientRpc]
    private void SendTransformDataToClients(Vector3 pos, Quaternion rot) 
    {
        if (isLocalPlayer) return;
        transform.position = Vector3.Lerp(transform.position, pos, 0.01f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, 0.01f);
    }

    bool IsInsideTouchArea(Vector2 screenPosition)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(touchArea, screenPosition, null, out localPoint);
        return touchArea.rect.Contains(localPoint);
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
