using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class MovementController : MonoBehaviour
{
    public SteamVR_Action_Vector2 movement;
    public float maxVelocity;
    public float sensitivity;
    public float xsensitivity;
    public float backwardModifier;
    public float collisionDistance;
    public Rigidbody head;
    public bool collisonEnabled;

    private float velocity = 0f;
    private float xvelocity = 0f;
    private float lastVelocity = 0f;
    private float accelerationLimit = 0.01f;
    private Vector3 direction;
    private Vector3 xdirection;

    private void MovePlayer()
    {
        transform.position += velocity * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        transform.position += xvelocity * Time.deltaTime * Vector3.ProjectOnPlane(xdirection, Vector3.up).normalized;
    }

    private float GetAddedVelocity(float velocity, float sensitivity, float value, bool backward, Vector3 direction)
    {
        return (Mathf.Abs(value) > 0.5f ? value - Mathf.Sign(value) * 0.25f : Mathf.Sign(value) * Mathf.Pow(value, 2f)) * (backward ? sensitivity * backwardModifier : sensitivity);
    }

    void Update()
    {
        if (movement.axis.y != 0f)
        {
            direction = Player.instance.leftHand.transform.TransformDirection(new Vector3(0f, 0f, 1f));
            // Vector3 direction = Player.instance.hmdTransform.TransformDirection(new Vector3(0f, 0f, 1f));
            velocity += GetAddedVelocity(velocity, sensitivity, movement.axis.y, movement.axis.y < 0, direction);
            velocity = Mathf.Clamp(velocity, -maxVelocity, maxVelocity);
        }
        if (movement.axis.x != 0f)
        {
            xdirection = Player.instance.leftHand.transform.TransformDirection(new Vector3(1f, 0f, 0f));
            // Vector3 direction = Player.instance.hmdTransform.TransformDirection(new Vector3(0f, 0f, 1f));
            xvelocity += GetAddedVelocity(xvelocity, xsensitivity, movement.axis.x, movement.axis.x < 0, xdirection);
            velocity = Mathf.Clamp(velocity, -maxVelocity, maxVelocity);
        }
        velocity *= 0.9f;
        xvelocity *= 0.9f;
        MovePlayer();
    }


    // private void MovePlayer(float value, Vector3 direction, bool backward)
    // {
    //     velocity = (Mathf.Abs(value) > 0.5f ? value - Mathf.Sign(value) * 0.25f : Mathf.Sign(value) * Mathf.Pow(value, 2f)) * (backward ? backwardSensitivity : sensitivity);
    //     velocity = Mathf.Clamp(velocity, -maxVelocity, maxVelocity);
    //     transform.position += velocity * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
    // }
    // void Update()
    // {
    //     RaycastHit hit;
    //     if (!collisonEnabled || !head.SweepTest(Player.instance.hmdTransform.TransformDirection(Vector3.forward), out hit, collisionDistance) || movement.axis.y < 0f || hit.collider.tag != "Collision")
    //     {
    //         if (movement.axis.y != 0f)
    //         {
    //             // Vector3 direction = Player.instance.hmdTransform.TransformDirection(new Vector3(0f, 0f, 1f));
    //             Vector3 direction = Player.instance.leftHand.transform.TransformDirection(new Vector3(0f, 0f, 1f));
    //             MovePlayer(movement.axis.y, direction, movement.axis.y < 0);
    //         }
    //         if (movement.axis.x != 0f)
    //         {
    //             // Vector3 direction = Player.instance.hmdTransform.TransformDirection(new Vector3(1f, 0f, 0f));
    //             Vector3 direction = Player.instance.leftHand.transform.TransformDirection(new Vector3(1f, 0f, 0f));
    //             MovePlayer(movement.axis.x, direction, false);
    //         }
    //     }
    // }
}
