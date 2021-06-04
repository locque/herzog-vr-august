//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Drives a linear mapping based on position between 2 positions
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class LinearDrive : MonoBehaviour
    {
        public Transform startPosition;
        public Transform endPosition;

        // ebaender
        public Transform centerPosition;

        public LinearMapping linearMapping;
        public bool repositionGameObject = true;
        public bool maintainMomemntum = true;

        // ebaender
        public bool repositionAfterDetach = true;

        // ebaender
        public bool rotate = false;

        // ebeander
        private bool attachedToHand = false;

        public float momemtumDampenRate = 5.0f;

        protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;

        protected float initialMappingOffset;
        protected int numMappingChangeSamples = 5;
        protected float[] mappingChangeSamples;
        protected float prevMapping = 0.0f;
        protected float mappingChangeRate;
        protected int sampleCount = 0;

        protected Interactable interactable;


        protected virtual void Awake()
        {
            mappingChangeSamples = new float[numMappingChangeSamples];
            interactable = GetComponent<Interactable>();
        }

        protected virtual void Start()
        {
            if (linearMapping == null)
            {
                linearMapping = GetComponent<LinearMapping>();
            }

            if (linearMapping == null)
            {
                linearMapping = gameObject.AddComponent<LinearMapping>();
            }

            initialMappingOffset = linearMapping.value;

            if (repositionGameObject)
            {
                UpdateLinearMapping(transform);
            }
        }

        protected virtual void HandHoverUpdate(Hand hand)
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
            {
                initialMappingOffset = linearMapping.value - CalculateLinearMapping(hand.transform);
                sampleCount = 0;
                mappingChangeRate = 0.0f;

                Debug.Log("before attach: " + hand.LastCollider + " " + hand.GetLastInteractable());
                hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            }
        }

        protected virtual void HandAttachedUpdate(Hand hand)
        {
            UpdateLinearMapping(hand.transform);

            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject);
            }
        }

        protected virtual void OnDetachedFromHand(Hand hand)
        {
            CalculateMappingChangeRate();

            // ebaender
            Interpolate();
            attachedToHand = false;
        }

        // ebaender
        protected virtual void OnAttachedToHand(Hand hand)
        {
            attachedToHand = true;
            if (!repositionGameObject && repositionAfterDetach)
            {
                transform.position = startPosition.position;
            }
        }


        protected void CalculateMappingChangeRate()
        {
            //Compute the mapping change rate
            mappingChangeRate = 0.0f;
            int mappingSamplesCount = Mathf.Min(sampleCount, mappingChangeSamples.Length);
            if (mappingSamplesCount != 0)
            {
                for (int i = 0; i < mappingSamplesCount; ++i)
                {
                    mappingChangeRate += mappingChangeSamples[i];
                }
                mappingChangeRate /= mappingSamplesCount;
            }
        }

        protected void UpdateLinearMapping(Transform updateTransform)
        {
            prevMapping = linearMapping.value;
            linearMapping.value = Mathf.Clamp01(initialMappingOffset + CalculateLinearMapping(updateTransform));

            mappingChangeSamples[sampleCount % mappingChangeSamples.Length] = (1.0f / Time.deltaTime) * (linearMapping.value - prevMapping);
            sampleCount++;

            if (repositionGameObject)
            {
                // ebaender
                Interpolate();

                // transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
            }
        }

        protected float CalculateLinearMapping(Transform updateTransform)
        {
            Vector3 direction = endPosition.position - startPosition.position;
            float length = direction.magnitude;
            direction.Normalize();

            Vector3 displacement = updateTransform.position - startPosition.position;

            return Vector3.Dot(displacement, direction) / length;
        }


        protected virtual void Update()
        {
            if (maintainMomemntum && mappingChangeRate != 0.0f)
            {
                //Dampen the mapping change rate and apply it to the mapping
                mappingChangeRate = Mathf.Lerp(mappingChangeRate, 0.0f, momemtumDampenRate * Time.deltaTime);
                linearMapping.value = Mathf.Clamp01(linearMapping.value + (mappingChangeRate * Time.deltaTime));

                if (repositionGameObject || !attachedToHand)
                {
                    // ebaender
                    Interpolate();

                    // transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
                }
            }
        }

        // ebaender
        private void Interpolate()
        {
            if (rotate)
            {
                Slerp();
            }
            else
            {
                transform.position = Vector3.Lerp(startPosition.position, endPosition.position, linearMapping.value);
            }
        }

        protected void Slerp()
        {
            // Vector3 center = (startPosition.position + endPosition.position) * 0.5f;
            // center -= new Vector3(0, 0.15f, 0);
            Vector3 startRelCenter = startPosition.position - centerPosition.position;
            Vector3 endRelCenter = endPosition.position - centerPosition.position;
            transform.position = Vector3.Slerp(startRelCenter, endRelCenter, linearMapping.value);
            transform.position += centerPosition.position;
        }
    }
}
