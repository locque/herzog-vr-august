using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator))]
public class CogController : MonoBehaviour
{
    private enum Axis
    {
        X, Y, Z
    }

    [SerializeField] private Rigidbody wheelCasing;
    [SerializeField] private Axis rotationAxis = Axis.X;
    private Animator animator;

    void Start()
    {
        if (wheelCasing == null)
        {
            Debug.LogError("Wheel Casing is missing.");
        }
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        float casingAngle = wheelCasing.transform.localEulerAngles[(int)rotationAxis] / 360f;
        Vector3 casingAngles = wheelCasing.transform.localEulerAngles;
        float casingAngleSum = casingAngles.x + casingAngles.y + casingAngles.z; 
        if (casingAngleSum > 360f)
        {
            casingAngle = 0.5f - casingAngle;
        }
        animator.SetFloat("Rotation", casingAngle);
    }

}
