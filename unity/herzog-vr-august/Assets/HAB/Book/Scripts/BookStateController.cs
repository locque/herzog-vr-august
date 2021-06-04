using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Interactable))]
    public class BookStateController : BookComponent
    {
        private Interactable interactable;
        private bool? backHandleState = null;
        private bool? pageHandleState = null;
        private bool? closedCoverColliderState = null;

        void Awake()
        {
            base.Start();
            interactable = GetComponent<Interactable>();
            InitializeHandleControllers();
            Book.closedCoverCollider.enabled = false;
            UpdateAll();
        }

        private void ResetHandles()
        {
            foreach (BookHandle handle in Book.backHandleController.GetBookHandles())
            {
                switch (handle.handleType)
                {
                    case BookHandle.HandleType.Edge:
                        handle.SetActive(true);
                        break;
                }
            }
            foreach (BookHandle handle in Book.coverHandleController.GetBookHandles())
            {
                handle.SetActive(false);
            }
            Book.pageHandleController.SetHandlesActive(false);
        }

        private void InitializeHandleControllers()
        {
            Book.backHandleController.GetHandles();
            Book.coverHandleController.GetHandles();
            Book.pageHandleController.GetHandles();
            ResetHandles();
        }

        private void HandleHandles(BookHandle.HandleType handleType)
        {
            BookHandle[] bookHandles = Book.backHandleController.GetBookHandles();
            switch (handleType)
            {
                case BookHandle.HandleType.Edge:
                    foreach (BookHandle h in bookHandles)
                    {
                        switch (h.handleType)
                        {
                            case BookHandle.HandleType.Edge:
                                h.SetActive(false);
                                break;
                            default:
                                break;
                        }
                    }
                    SetHandlesEnabled(Book.coverHandleController, true);
                    break;
                default:
                    break;
            }
        }

        protected virtual void OnAttachedToHand(Hand hand)
        {
            Collider lastCollider = hand.LastCollider;
            if (lastCollider)
            {
                BookHandle colliderHandle = lastCollider.GetComponent<BookHandle>();
                if (colliderHandle)
                {
                    HandleHandles(colliderHandle.handleType);
                }
                else
                {
                    Debug.LogError("Collider " + lastCollider + " does not have a Book Handle.");
                }
            }
            else
            {
                Debug.LogError("Hand " + hand + "did not report a last Collider.");
            }
            Book.coverPhysicsController.SetAttachedToBack(true);
            Book.pagePhysicsController.SetAttachedToBack(true);
        }

        private void SetHandlesEnabled(BookHandleController controller, bool enabled)
        {
            if (controller)
            {
                if (controller.GetBookHandles() != null)
                {
                    foreach (BookHandle h in controller.GetBookHandles())
                    {
                        h.SetActive(enabled);
                    }
                }
                else
                {
                    Debug.LogError("Controller " + controller + " returned null instead of Handles.");
                }
            }
            else
            {
                Debug.LogError("Handle controller is missing.");
            }
        }

        protected virtual void OnDetachedFromHand(Hand hand)
        {
            ResetHandles();
            hand.otherHand.DetachObject(Book.coverDrive.gameObject);
            hand.otherHand.DetachObject(Book.pageDrive.gameObject);
            Book.coverPhysicsController.SetAttachedToBack(false);
            Book.pagePhysicsController.SetAttachedToBack(false);
        }

        private void SetBackHandleState()
        {

            if (Book.coverMapping.value < Book.lockThreshold != backHandleState)
            {
                backHandleState = Book.coverMapping.value < Book.lockThreshold;
                foreach (BookHandle handle in Book.backHandleController.GetBookHandles())
                {
                    switch (handle.handleType)
                    {
                        case BookHandle.HandleType.Spine:
                        case BookHandle.HandleType.Square:
                            handle.SetActive(backHandleState.Value);
                            break;
                    }
                }
            }
        }
        private void SetPageHandleState()
        {
            // flip the page handle state whenever the page threshold is passed.
            pageHandleState = Book.coverMapping.value > Book.pageThreshold;
            Book.pageHandleController.SetHandlesActive(pageHandleState.Value);

            // detach hand from page if it is still attached when the cover is closing beyond the threshold.
            if (!pageHandleState.Value)
            {
                Hand attachedHand = Book.pageDrive.GetComponent<Interactable>().attachedToHand;
                if (attachedHand != null)
                {
                    attachedHand.DetachObject(Book.pageDrive.gameObject);
                }
            }

            // disable cover page handle on first page and back page handle on last page
            if (Book.helper.Progress < 0.9f)
            {
                Book.pageHandleController.SetHandlesActive(false, PageHandle.Type.Cover);
            }
            else if (Book.helper.Progress >= Book.helper.PageAmmount - 1f)
            {
                Book.pageHandleController.SetHandlesActive(false, PageHandle.Type.Back);
            }
        }

        private void SetClosedCoverColliderState()
        {
            if (Book.coverMapping.value < Book.lockThreshold != closedCoverColliderState)
            {
                closedCoverColliderState = Book.coverMapping.value < Book.lockThreshold;
                Book.closedCoverCollider.enabled = closedCoverColliderState.Value;
            }
        }

        public void UpdateAll()
        {
            SetBackHandleState();
            SetPageHandleState();
            SetClosedCoverColliderState();
        }

        public void PageMappingChanged(float value)
        {

        }
    }
}
