using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using Valve.VR;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public SteamVR_Action_Vector2 movement;
    public float acceleration = 1f;
    public float maxVelocity = 2f;

    private Camera playerCamera;
    private CharacterController characterController;
    private bool pointerGrabState;
    private Vector2 playerRotation;
    private Vector3 initalRotation;
    private Vector2 velocity;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        velocity = Vector2.zero;
        playerRotation = transform.localEulerAngles;
        initalRotation = playerRotation;
    }

    public void Update()
    {
        Move(new Vector2(movement.axis.x, movement.axis.y));
    }

    private void Move(Vector2 direction)
    {
        for (int i = 0; i < 2; i++)
        {
            if (direction[i] != 0f)
            {
                velocity[i] += Mathf.Sign(direction[i]) * acceleration * Time.deltaTime;
            }
            else
            {
                float sign = Mathf.Sign(velocity[i]);
                velocity[i] -= sign * acceleration * Time.deltaTime;
                if (sign != Mathf.Sign(velocity[i]))
                {
                    velocity[i] = 0f;
                }
            }
            velocity[i] = Mathf.Clamp(velocity[i], -maxVelocity, maxVelocity);
        }
        Vector3 characterDirection = transform.right * velocity.x + transform.forward * velocity.y;
        characterController.SimpleMove(characterDirection);
    }

    public void SetEulerAngles(Vector3 eulerAngles)
    {
        playerRotation = eulerAngles;
        transform.eulerAngles = eulerAngles;
    }

    public void OnCollisionEnter(Collision collision) 
    {
        Debug.Log(collision.collider.gameObject);
    }

}