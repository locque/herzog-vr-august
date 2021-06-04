using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Valve.VR.InteractionSystem.Sample
{
    public class LockToPoint : MonoBehaviour
    {
        public Transform snapTo;
        private Rigidbody body;
        public float snapTime = 2;

        // ebaender - option to disable colliders during repositioning
        public GameObject collisionReference = null;

        private float dropTimer;
        private Interactable interactable;

        // ebaender
        private int originalLayer;

        private void Start()
        {
            interactable = GetComponent<Interactable>();
            body = GetComponent<Rigidbody>();
            originalLayer = gameObject.layer;
        }

        private void FixedUpdate()
        {
            bool used = false;
            if (interactable != null)
                used = interactable.attachedToHand;

            if (used)
            {
                body.isKinematic = false;
                dropTimer = -1;
            }
            else
            {
                dropTimer += Time.deltaTime / (snapTime / 2);

                body.isKinematic = dropTimer > 1;

                if (dropTimer > 1)
                {
                    // ebaender
                    gameObject.layer = originalLayer;

                    //transform.parent = snapTo;
                    transform.position = snapTo.position;
                    transform.rotation = snapTo.rotation;
                }
                else
                {
                    // ebaender
                    if (collisionReference != null)
                    {
                        gameObject.layer = collisionReference.gameObject.layer;
                        Debug.Log("parent " + gameObject.layer);
                    }

                    float t = Mathf.Pow(35, dropTimer);

                    body.velocity = Vector3.Lerp(body.velocity, Vector3.zero, Time.fixedDeltaTime * 4);
                    if (body.useGravity)
                        body.AddForce(-Physics.gravity);

                    transform.position = Vector3.Lerp(transform.position, snapTo.position, Time.fixedDeltaTime * t * 3);
                    transform.rotation = Quaternion.Slerp(transform.rotation, snapTo.rotation, Time.fixedDeltaTime * t * 2);
                }
            }
        }
    }
}