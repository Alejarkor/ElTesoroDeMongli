using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class TransformRigidbodyCharacterController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float rotationSpeed = 360f;
    public float stepHeight = 0.3f;
    public float groundDistance = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;
    private Vector3 _inputDirection;
    private bool _jump;
    private bool _isGrounded;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _rigidbody.isKinematic = true;
    }

    void Update()
    {
        // Leer entrada del usuario
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        _inputDirection = new Vector3(horizontal, 0, vertical).normalized;

        // Salto
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _jump = true;
        }
    }

    void FixedUpdate()
    {
        // Mover personaje
        if (!_jump)
        {
            Vector3 moveDirection = _inputDirection * moveSpeed;
            Vector3 targetPosition = transform.position + moveDirection * Time.fixedDeltaTime;

            // Ajustar la posición en Y para subir escaleras
            RaycastHit hit;
            if (Physics.Raycast(targetPosition + Vector3.up * stepHeight, -Vector3.up, out hit, stepHeight + 0.1f, groundLayer))
            {
                targetPosition.y = hit.point.y;
            }

            transform.position = targetPosition;

            // Rotar personaje
            if (_inputDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(_inputDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
            }
        }

        // Saltar
        if (_jump)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _jump = false;
        }

        // Verificar si está en el suelo
        _isGrounded = IsGrounded();
        if (_isGrounded && !_rigidbody.isKinematic)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.velocity = Vector3.zero;
        }
    }

    private bool IsGrounded()
    {
        if (Physics.Raycast(transform.position - Vector3.down * groundDistance, Vector3.down, groundDistance * 2f))
        {
            return true;
        }
        return false;
    }
}
