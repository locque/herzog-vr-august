//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Basic throwable object
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(Rigidbody))]
    public class BookThrowable : Throwable
    {
        protected override void HandHoverUpdate(Hand hand)
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();
            BookIndex Book = GetComponentInParent<BookIndex>();

            if (startingGrabType == GrabTypes.Grip)
            {
                if (Book.coverMapping.value < Book.lockThreshold || hand == Player.instance.rightHand)
                {
                    hand.AttachObject(gameObject, startingGrabType, attachmentFlags, attachmentOffset);
                    hand.HideGrabHint();
                }
            }
            else if (startingGrabType == GrabTypes.Pinch && hand == Player.instance.leftHand)
            {
                if (hand.LastCollider == Book.lowerBackHandle)
                {
                    hand.LastCollider = Book.backPageHandle.GetComponent<Collider>();
                    if (hand.LastCollider.GetComponent<PageHandle>().IsActive())
                    {
                        hand.LastCollider.GetComponentInParent<PageDrive>().SendMessage("HandHoverUpdate", hand);
                    }
                }
            }
        }
    }
}
