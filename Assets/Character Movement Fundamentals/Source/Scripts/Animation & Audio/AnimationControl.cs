using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This script controls the character's animation by passing velocity values and other information ('isGrounded') to an animator component;
	public class AnimationControl : MonoBehaviour {
		
		
		public bool isLocal;
		public MongliAnimatorNetworkController animNetworkController;
		public MongliAnimatorLocalController animLocalController;

		MongliWallkerController controller;
		Animator animator;
		Transform animatorTransform;
		Transform tr;

		//Whether the character is using the strafing blend tree;
		public bool useStrafeAnimations = false;

		//Velocity threshold for landing animation;
		//Animation will only be triggered if downward velocity exceeds this threshold;
		public float landVelocityThreshold = 5f;

		private float smoothingFactor = 40f;
		Vector3 oldMovementVelocity = Vector3.zero;

		//Setup;
		void Awake () {
			controller = GetComponent<MongliWallkerController>();
			animator = GetComponentInChildren<Animator>();
			animatorTransform = animator.transform;

			tr = transform;
		}

		//OnEnable;
		void OnEnable()
		{
			//Connect events to controller events;
			controller.OnLand += OnLand;
			controller.OnJump += OnJump;
		}

		//OnDisable;
		void OnDisable()
		{
			//Disconnect events to prevent calls to disabled gameobjects;
			controller.OnLand -= OnLand;
			controller.OnJump -= OnJump;
		}
		
		//Update;
		void Update () {

			//Get controller velocity;
			Vector3 _velocity = controller.GetVelocity();

			//Split up velocity;
			Vector3 _horizontalVelocity = VectorMath.RemoveDotVector(_velocity, tr.up);
			Vector3 _verticalVelocity = _velocity - _horizontalVelocity;

			//Smooth horizontal velocity for fluid animation;
			_horizontalVelocity = Vector3.Lerp(oldMovementVelocity, _horizontalVelocity, smoothingFactor * Time.deltaTime);
			oldMovementVelocity = _horizontalVelocity;

			if (!isLocal)
			{
                animNetworkController.SetFloat("VerticalSpeed", _verticalVelocity.magnitude * VectorMath.GetDotProduct(_verticalVelocity.normalized, tr.up));
                animNetworkController.SetFloat("HorizontalSpeed", _horizontalVelocity.magnitude);
            }
			else 
			{
                animLocalController.SetFloat("VerticalSpeed", _verticalVelocity.magnitude * VectorMath.GetDotProduct(_verticalVelocity.normalized, tr.up));
                animLocalController.SetFloat("HorizontalSpeed", _horizontalVelocity.magnitude);
            }

			//If animator is strafing, split up horizontal velocity;
			if(useStrafeAnimations)
			{
				Vector3 _localVelocity = animatorTransform.InverseTransformVector(_horizontalVelocity);

                if (!isLocal)
                {
                    animNetworkController.SetFloat("ForwardSpeed", _localVelocity.z);
                    animNetworkController.SetFloat("StrafeSpeed", _localVelocity.x);
                }
                else
                {
                    animLocalController.SetFloat("ForwardSpeed", _localVelocity.z);
                    animLocalController.SetFloat("StrafeSpeed", _localVelocity.x);
                }
            }

            //Pass values to animator;

            if (!isLocal)
            {
                animNetworkController.SetBool("IsGrounded", controller.IsGrounded());
                animNetworkController.SetBool("IsStrafing", useStrafeAnimations);
            }
            else
            {
                animLocalController.SetBool("IsGrounded", controller.IsGrounded());
                animLocalController.SetBool("IsStrafing", useStrafeAnimations);
            }
        }

		void OnLand(Vector3 _v)
		{
			//Only trigger animation if downward velocity exceeds threshold;
			if(VectorMath.GetDotProduct(_v, tr.up) > -landVelocityThreshold)
				return;


            if (!isLocal)
            {
                animNetworkController.SetTrigger("OnLand", false);
            }
            else
            {
                animLocalController.SetTrigger("OnLand", false);
            }
		}

		void OnJump(Vector3 _v)
		{
			
		}
	}
}
