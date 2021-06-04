using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(SteamVR_Skeleton_Poser))]
    public class PoseBlender : BookComponent
    {

        protected SteamVR_Skeleton_Poser poser;
        protected Collider[] childColliders;
        protected string currentPose = null;

        protected override void Start()
        {
            base.Start();
            poser = GetComponent<SteamVR_Skeleton_Poser>();
            childColliders = GetComponentsInChildren<Collider>();
        }

        protected string GetBlendName(string name, int i)
        {
            return name + "-" + i;
        }

        protected void DisableBlendingBehaviours()
        {
            foreach (Collider c in childColliders)
            {
                string blendName = GetBlendName(c.name, 0);
                if (c.isTrigger && poser.HasBlendingBehaviour(blendName))
                {
                    poser.SetBlendingBehaviourEnabled(blendName, false);
                    poser.SetBlendingBehaviourValue(blendName, 1f);
                }
            }
        }

        protected virtual void OnAttachedToHand(Hand hand)
        {
            Collider collider = hand.LastCollider;
            if (collider)
            {
                // Debug.Log("COL: " + collider.name + " INT: " + hand.GetLastInteractable().name);
                DisableBlendingBehaviours();
                string blendName = GetBlendName(collider.name, 0);
                if (poser.HasBlendingBehaviour(blendName))
                {
                    currentPose = collider.name;
                    poser.SetBlendingBehaviourEnabled(blendName, true);
                    // Debug.Log("activated " + currentPose);
                }
            }
            else
            {
                Debug.LogError(hand.name + " did not return a collider, can't blend.");
            }
        }

        protected virtual void OnDetachedFromHand(Hand hand)
        {
            if (currentPose != null)
            {
                poser.TryToSetBlendingBehaviourValue(currentPose + "-1", 0f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-2", 0f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-3", 0f);
                // Debug.Log("deactivated " + currentPose);
                currentPose = null;
            }
        }
    }
}