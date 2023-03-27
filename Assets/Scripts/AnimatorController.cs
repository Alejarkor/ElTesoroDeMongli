using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorController : MonoBehaviour
{
    
    public PlayerNetwork character3d;
    public Transform characterTransform;
    public Animator anim;
    public Vector3 velocity;
    public float velocityMagnitude;
    private Vector3 lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        velocity = (characterTransform.position - lastPosition)/Time.deltaTime;
        velocityMagnitude = Mathf.Lerp(velocityMagnitude, velocity.magnitude,0.5f);
        lastPosition = characterTransform.position;

        if (character3d.isGrounded)
        {
            anim.SetBool("air", false);
        }
        else
        {
            anim.SetBool("air", true);
        }

        if (velocityMagnitude >= 0.1f)
        {
            anim.SetBool("run", true);
        }
        else
        {
            anim.SetBool("run", false);
        }
    }
}
