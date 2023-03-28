using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove: MonoBehaviour
{       
    public CharacterController player;
    public PlayerAnimatorEvents animatorEvents; 
    //public float playerSpeed;
    public float gravity;
    public float fallVelocity;
    public float jumpForce;
    public float groundDistance;


    public bool isOnSlope = false;
    private Vector3 hitNormal;
    public float slideVelocity;
    public float slopeForceDown;

    public float jumpCooldown = 0.2f; // Tiempo de espera después del salto antes de que se pueda saltar nuevamente
    private float currentJumpCooldown; // Tiempo restante en el cooldown actual


    public bool onAir;
    private bool onJump;

    public Action OnJump;
    public Action OnAir;
    public Action OnFall;


    public void Start()
    {
        animatorEvents.OnEndJump += OnEndJump;
    }

    private void OnEndJump()
    {
        onJump = false;
    }

    public void UpdatePlayer(Vector3 movePlayer, bool buttonJump, float deltaTime, float playerSpeed)
    {
        movePlayer = movePlayer * playerSpeed;  //Y lo multiplicamos por la velocidad del jugador "playerSpeed"
        player.transform.LookAt(player.transform.position + movePlayer); //Hacemos que nuestro personaje mire siempre en la direccion en la que nos estamos moviendo.
        SetGravity(ref movePlayer, deltaTime); //Llamamos a la funcion SetGravity() para aplicar la gravedad
        PlayerSkills(ref movePlayer, buttonJump); //Llamamos a la funcion PlayerSkills() para invocar las habilidades de nuestro personaje
        player.Move(movePlayer * deltaTime);

        // Actualizar el cooldown del salto
        if (currentJumpCooldown > 0)
        {
            currentJumpCooldown -= deltaTime;
        }
    }


    //Funcion para las habilidades de nuestro jugador.

    public void PlayerSkills(ref Vector3 movePlayer, bool buttonJump)
    {
        //Si estamos tocanto el suelo y pulsamos el boton "Jump"
        if (IsGrounded() && buttonJump)
        {
            fallVelocity = jumpForce; //La velocidad de caida pasa a ser igual a la velocidad de salto
            movePlayer.y = fallVelocity; //Y pasamos el valor a movePlayer.y            
            onJump = true;
            OnJump?.Invoke();            
        }
    }

    //Funcion para la gravedad.
    public void SetGravity(ref Vector3 movePlayer, float deltaTime)
    {
        //Si estamos tocando el suelo
        if (IsGrounded() &&!onJump)
        {
            if (onAir)
            {
                OnFall?.Invoke();
                onAir = false;
            }
            //La velocidad de caida es igual a la gravedad en valor negativo * Time.deltaTime.
            fallVelocity = -gravity * deltaTime;
            movePlayer.y = fallVelocity;

            // Reiniciar el cooldown del salto
            currentJumpCooldown = jumpCooldown;
        }
        else //Si no...
        {
            if (!onAir) 
            {
                OnAir?.Invoke();
                onAir = true;
            }
            //aceleramos la caida cada frame restando el valor de la gravedad * Time.deltaTime.
            fallVelocity -= gravity * deltaTime;
            movePlayer.y = fallVelocity;
        }

        SlideDown(ref movePlayer); //Llamamos a la funcion SlideDown() para comprobar si estamos en una pendiente
    }

    //Esta funcion detecta si estamos en una pendiente muy pronunciada y nos desliza hacia abajo.
    public void SlideDown(ref Vector3 movePlayer)
    {
        //si el angulo de la pendiente en la que nos encontramos es mayor o igual al asignado en player.slopeLimit, isOnSlope es VERDADERO
        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= player.slopeLimit;

        if (isOnSlope) //Si isOnSlope es VERDADERO
        {
            //movemos a nuestro jugador en los ejes "x" y "z" mas o menos deprisa en proporcion al angulo de la pendiente.
            movePlayer.x += ((1f - hitNormal.y) * hitNormal.x) * slideVelocity;
            movePlayer.z += ((1f - hitNormal.y) * hitNormal.z) * slideVelocity;
            //y aplicamos una fuerza extra hacia abajo para evitar saltos al caer por la pendiente.
            movePlayer.y += slopeForceDown;
        }
    }

    //Esta funcion detecta cuando colisinamos con otro objeto mientras nos movemos
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Almacenamos la normal del plano contra el que hemos chocado en hitNormal.
        hitNormal = hit.normal;
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