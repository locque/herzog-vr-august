using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class BookSitter : BookComponent
    {
        private Transform originalParent;
        private Collision lastCollision;
        private Rigidbody body;
        private Vector3 lockedPosition;
        private Vector3 lockedAngles;
        bool locked;
        bool attached;
        protected override void Start()
        {
            base.Start();
            originalParent = Book.transform.parent;
            body = GetComponent<Rigidbody>();
            locked = false;
            attached = false;
        }

        void LockToParent(Transform parent)
        {
            if (!locked && body.velocity.magnitude < 0.01f && body.angularVelocity.magnitude < 0.01f)
            {
                locked = true;
                lockedPosition = body.transform.localPosition;
                lockedAngles = body.transform.localEulerAngles;
            }
            Book.transform.SetParent(parent);
        }

        void OnCollisionStay(Collision collision)
        {
            if (attached)
                return;

            if (collision.collider.GetComponent<WheelBoard>() != null)
            {
                LockToParent(collision.collider.transform);
            }
            // else if (!locked && collision.collider.GetComponentInParent<WheelBoard>() != null)
            // {
            //     Debug.Log("Locking to grandparent");
            //     LockToParent(collision.collider.GetComponentInParent<WheelBoard>().transform);
            // }
            else if (collision.collider.GetComponentInParent<HandCollider>() == null && collision.collider.GetComponentInParent<BookIndex>() == null)
            {
                locked = false;
                Book.transform.SetParent(originalParent);
            }
        }

        // void OnCollisionEnter(Collision collision)
        // {
        //     lastCollision = collision;
        // }

        // void FixedUpdate()
        // {
        //     bookIndex.transform.SetParent(originalParent);
        //     if (lastCollision.collider.tag == wheelBoardTag)
        //     {
        //         Debug.Log("Parented to " + lastCollision.collider.transform.parent.parent.parent.name);
        //         bookIndex.transform.SetParent(lastCollision.collider.transform);
        //     }
        //     Debug.Log(GetComponent<Rigidbody>().IsSleeping());
        // }

        // void OnCollisionExit(Collision collision)
        // {
        //    Book.transform.SetParent(originalParent);
        // }

        void OnDetachedFromHand(Hand hand)
        {
            Book.transform.SetParent(originalParent);
            locked = false;
            attached = false;
        }
        void OnAttachedToHand(Hand hand)
        {
            Book.transform.SetParent(originalParent);
            locked = false;
            attached = true;
        }

        void LateUpdate()
        {
            if (locked)
            {
                body.transform.localPosition = lockedPosition;
                body.transform.localEulerAngles = lockedAngles;
                // Debug.Log("LOCKED");
            }
            else
            {
                // Debug.Log("UNLOCKED");
            }
        }
    }
}
