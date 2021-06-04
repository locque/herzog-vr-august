using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HingeJoint))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(AudioSource))]
    public class CoverPhysicsController : BookComponent
    {
        public AudioClip shutClip;
        public AudioClip slowOpenClip;
        public AudioClip fastOpenClip;

        private bool isDriving = false;
        private JointLimits lastJointLimits;
        private bool locked = false;
        private bool attachedToPrimary = false;
        private float torque;
        public float maxTorque = 50f;
        private float maxangv = 0f;
        private float audioLockTimer = 0f;
        private bool coverShut = false;
        private float usableVelocity;
        private bool reachedStateThreshold = true;
        public enum CoverState
        {
            FromShut,
            TowardsOpen,
            FromOpen,
            TowardsShut
        }

        private CoverState state = CoverState.FromShut;

        protected override void Start()
        {
            base.Start();
            Book.coverMapping.value = Book.initialCoverMappingValue;
            SyncRigidbodyToMapping();
            GetComponent<Rigidbody>().maxAngularVelocity = maxTorque;
        }

        public void Lock()
        {
            string debugStatus = "trying to lock:\n";
            if (!locked && Book.coverMapping.value < Book.lockThreshold && attachedToPrimary)
            {
                locked = true;
                JointLimits limits = GetComponent<HingeJoint>().limits;
                lastJointLimits = limits;
                limits.max = 0;
                GetComponent<HingeJoint>().limits = limits;
                debugStatus += "locked successfully\n";
            }
            else
            {
                if (Book.coverMapping.value >= Book.lockThreshold) debugStatus += "value too big\n";
                if (!attachedToPrimary) debugStatus += "not attached\n";
                if (locked) debugStatus += "locked already\n";
            }
            // Debug.Log(debugStatus);
        }

        public void Unlock()
        {
            if (locked)
            {
                locked = false;
                GetComponent<HingeJoint>().limits = lastJointLimits;
            }
        }

        public void DriveAttached()
        {
            isDriving = true;
            Unlock();
        }

        public void DriveDetached(Vector3 handVelocity, float mappingDelta)
        {
            isDriving = false;
            torque = Mathf.Pow(handVelocity.magnitude, 2f) * Mathf.Sign(mappingDelta);
        }

        public void SetAttachedToBack(bool attached)
        {
            attachedToPrimary = attached;
            if (attached)
            {
                Lock();
            }
            else
            {
                Unlock();
            }
        }

        void SyncRigidbodyToMapping()
        {
            Vector3 before = transform.localEulerAngles;
            float newAngle = Mathf.Clamp(Book.coverMapping.value * 180f, 0f, 177f);
            transform.localEulerAngles = new Vector3(0, newAngle, 0);
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
            // Debug.Log(newAngle + "\n" + transform.localEulerAngles);
        }
        private void ApplyTorqueBoost()
        {
            if (torque != 0f)
            {
                GetComponent<Rigidbody>().AddTorque(new Vector3(0f, torque, 0f), ForceMode.Impulse);
                torque = 0f;
            }
        }
        private void PlayAudio(float volume, AudioClip clip)
        {
            if (audioLockTimer == 0f)
            {
                audioLockTimer = 0.25f;
                AudioSource channel = GetComponent<AudioSource>();
                channel.clip = clip;
                channel.volume = volume;
                channel.pitch = Mathf.Clamp(0.35f * volume, 1f, 1.1f);
                channel.pitch += Random.Range(-0.05f, 0.05f);
                channel.Play();
            }
        }
        private void PlayAudioOnTransit()
        {
            if (state == CoverState.TowardsOpen && Book.coverMapping.value > 0.25f)
            {
                state = CoverState.FromOpen;
                float volume = Player.instance.leftHand.GetTrackedObjectVelocity().magnitude * 0.5f - 0.1f;
                PlayAudio(volume, slowOpenClip);
                // Debug.Log(state);
            }
            // else if (state == CoverState.TowardsShut && Book.coverMapping.value < 0.2f)
            // {
            //     state = CoverState.FromShut;
            //     Debug.Log(state);
            // }
            else if ((state == CoverState.FromOpen || state == CoverState.TowardsOpen) && Book.coverMapping.value < 0.01f)
            {
                state = CoverState.FromShut;
                float volume = isDriving ? Player.instance.leftHand.GetTrackedObjectVelocity().magnitude * 0.5f - 0.1f : 0.1f;
                if (attachedToPrimary)
                {
                    PlayAudio(volume, shutClip);
                }
                // Debug.Log(state);
            }
            else if (state == CoverState.FromShut && Book.coverMapping.value > 0.05f)
            {
                state = CoverState.TowardsOpen;
                // Debug.Log(state);
            }
            audioLockTimer = Mathf.Max(audioLockTimer - Time.deltaTime, 0f);
        }

        void FixedUpdate()
        {
            if (isDriving)
            {
                SyncRigidbodyToMapping();
            }
            else
            {
                ApplyTorqueBoost();
                Lock();
                if (transform.localEulerAngles.y > 0f && transform.localEulerAngles.y < 180f)
                {
                    Book.coverMapping.value = transform.localEulerAngles.y / 180f;
                    Book.stateController.UpdateAll();
                    // Debug.Log(transform.localEulerAngles.y);
                }
            }
            PlayAudioOnTransit();
        }
    }
}
