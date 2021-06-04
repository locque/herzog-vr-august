using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HingeJoint))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(AudioSource))]
    public class PagePhysicsController : BookComponent
    {
        public AudioClip slowFlip;
        public AudioClip fastFlip;

        private Collider[] leftHandColliders = null;
        public bool debug = false;
        public float maxTorque = 25f;
        private float baseProgress = 0f;
        private PageHandle.Type attachedHandleType = PageHandle.Type.None;
        private Debuggable debuggable;
        private JointLimits initalJointLimits;
        private float hingeMaxLimitError;
        private float pageTorque;
        private float adaptionTolerance = 0.25f;
        private float audioLockTimer = 0f;
        private PageState state = PageState.Back;
        private bool reachedStateThreshold = true;
        public enum PageState
        {
            None,
            Back,
            Cover,
        }
        private PageState pageLockState = PageState.None;
        private bool isDriving = false;
        private bool attachedToPrimary = false;

        public bool PageConstrained()
        {
            return pageLockState != PageState.None || isDriving;
        }

        public bool AttachedToBack
        {
            get { return attachedToPrimary; }
        }

        protected override void Start()
        {
            base.Start();
            baseProgress = Book.helper.Progress;
            hingeMaxLimitError = 1f - (Book.hingeMaxLimitActual / Book.hingeMaxLimit);
            hingeMaxLimitError = 0f;
            initalJointLimits = GetComponent<HingeJoint>().limits;
            GetComponent<Rigidbody>().maxAngularVelocity = maxTorque;
            GetComponent<Collider>().enabled = false;

            debuggable = new Debuggable(this, debug);

            SyncRigidbodyToMapping(1f);
        }

        public void PageDriveAttached(Hand hand)
        {
            isDriving = true;
            attachedHandleType = hand.LastCollider.GetComponent<PageHandle>().handleType;
            switch (attachedHandleType)
            {
                case PageHandle.Type.Cover:
                    baseProgress--;
                    Book.pageMapping.value = 1f;
                    SyncRigidbodyToMapping(1f);
                    state = PageState.Cover;
                    break;
                case PageHandle.Type.Back:
                    Book.pageMapping.value = 0f;
                    SyncRigidbodyToMapping(0f);
                    state = PageState.Back;
                    break;
            }
            Unlock();
            if (leftHandColliders == null)
            {
                leftHandColliders = hand.GetComponent<HandPhysics>().handCollider.GetComponentsInChildren<Collider>();
                SetLeftHandCollisionState(false);
            }
        }

        private void SetLeftHandCollisionState(bool enabled)
        {
            foreach (var item in leftHandColliders)
            {
                Physics.IgnoreCollision(item, GetComponent<Collider>(), true);
            }
        }

        public void PageDriveDetached(Vector3 handVelocity, float pageMappingDelta)
        {
            isDriving = false;
            pageTorque = Mathf.Pow(handVelocity.magnitude, 2f) * Mathf.Sign(pageMappingDelta);
            attachedHandleType = PageHandle.Type.None;
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
            if (Book.pageMapping.value > Book.lockThreshold)
            {
                float volume = Player.instance.leftHand.GetTrackedObjectVelocity().magnitude * 0.5f - 0.1f;
                if (state == PageState.Back)
                {
                    if (reachedStateThreshold && Book.pageMapping.value > 0.25f)
                    {
                        reachedStateThreshold = false;
                        state = PageState.Cover;
                        PlayAudio(volume, volume > 0.5f ? fastFlip : slowFlip);
                    }
                    else if (!reachedStateThreshold && Book.pageMapping.value <= 0.2f)
                    {
                        reachedStateThreshold = true;
                    }
                }
                else if (state == PageState.Cover)
                {
                    if (reachedStateThreshold && Book.pageMapping.value < 0.75f)
                    {
                        reachedStateThreshold = false;
                        state = PageState.Back;
                        PlayAudio(volume, volume > 0.5f ? fastFlip : slowFlip);
                    }
                    else if (!reachedStateThreshold && Book.pageMapping.value >= 0.8f)
                    {
                        reachedStateThreshold = true;
                    }
                }
            }
            audioLockTimer = Mathf.Max(audioLockTimer - Time.deltaTime, 0f);
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

        private void UpdateBaseProgress()
        {
            baseProgress = Mathf.Clamp(baseProgress, 0f, Book.helper.PageAmmount);
            baseProgress += Mathf.Round(Book.pageMapping.value);
        }

        public void Lock()
        {
            // if (attachedToPrimary)
            {
                if (pageLockState == PageState.None)
                {
                    if (Book.pageMapping.value < Book.lockThreshold)
                    {
                        pageLockState = PageState.Back;
                        SetPageColliderEnabled(false);
                        UpdateBaseProgress();
                    }
                    else if (Book.pageMapping.value > 1f - Book.lockThreshold - hingeMaxLimitError)
                    {
                        pageLockState = PageState.Cover;
                        SetPageColliderEnabled(false);
                        UpdateBaseProgress();
                    }
                }

                switch (pageLockState)
                {
                    case PageState.Back:
                        SyncRigidbodyToMapping(0f);
                        Book.stateController.UpdateAll();
                        break;
                    case PageState.Cover:
                        SyncRigidbodyToMapping(1f);
                        Book.stateController.UpdateAll();
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetPageColliderEnabled(bool enabled)
        {
            GetComponent<Collider>().enabled = enabled;
            if (debuggable.DebugEnabled)
                GetComponent<MeshRenderer>().enabled = enabled;
        }

        public void Unlock()
        {
            if (pageLockState != PageState.None)
            {
                pageLockState = PageState.None;
                SetPageColliderEnabled(true);
            }
        }

        private void SyncRigidbodyToMapping(float value)
        {
            Vector3 before = transform.localEulerAngles;
            float newAngle = Mathf.Clamp(value * Book.hingeMaxLimit * Book.coverMapping.value, Book.hingeMinLimit, Book.hingeMaxLimitActual * Book.coverMapping.value);
            transform.localEulerAngles = new Vector3(0f, newAngle, 0f);
            GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
            GetComponent<Rigidbody>().angularVelocity = new Vector3(0f, 0f, 0f);
        }

        private void SyncMappingToRigidbody()
        {
            if (pageLockState == PageState.Cover)
            {
                Book.pageMapping.value = 0f;
                Book.stateController.PageMappingChanged(0f);
                SyncRigidbodyToMapping(0f);
            }
            else if (transform.localEulerAngles.y > Book.hingeMinLimit && transform.localEulerAngles.y < Book.hingeMaxLimit)
            {
                Book.pageMapping.value = transform.localEulerAngles.y / (Book.coverMapping.value * Book.hingeMaxLimit);
                Book.stateController.PageMappingChanged(Book.pageMapping.value);
            }

        }

        private float AdaptToCoverMapping(float value, float scalar)
        {
            hingeMaxLimitError = 0f;
            value += scalar * (1f - Book.coverMapping.value - hingeMaxLimitError);
            return Mathf.Min(value, 1f);
        }

        private void ApplyTorqueBoost()
        {
            if (pageTorque != 0f)
            {
                GetComponent<Rigidbody>().AddTorque(new Vector3(0f, pageTorque, 0f), ForceMode.Impulse);
                pageTorque = 0f;
            }
        }

        void FixedUpdate()
        {

            if (isDriving)
            {
                SyncRigidbodyToMapping(Book.pageMapping.value);
            }
            else
            {
                ApplyTorqueBoost();
                Lock();
                SyncMappingToRigidbody();
            }

            PlayAudioOnTransit();

            Book.progressMapping.value = (Book.pageMapping.value + baseProgress);
            Book.progressMapping.value /= Book.helper.PageAmmount - 1;

        }
    }
}
