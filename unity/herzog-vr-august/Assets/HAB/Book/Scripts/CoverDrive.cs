//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Interactable that can be used to move in a circular motion
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Valve.VR.InteractionSystem
{

    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]

    //ebaender - need pose blender
    [RequireComponent(typeof(SteamVR_Skeleton_Poser))]
    public class CoverDrive : BookComponent
    {
        public enum Axis_t
        {
            XAxis,
            YAxis,
            ZAxis
        };

        [Tooltip("The axis around which the circular drive will rotate in local space")]
        public Axis_t axisOfRotation = Axis_t.XAxis;

        [Tooltip("Child GameObject which has the Collider component to initiate interaction, only needs to be set if there is more than one Collider child")]
        public Collider childCollider = null;
        private float lastMappingValue;

        [Tooltip("If true, the drive will stay manipulating as long as the button is held down, if false, it will stop if the controller moves out of the collider")]
        public bool hoverLock = false;

        [HeaderAttribute("Limited Rotation")]
        [Tooltip("If true, the rotation will be limited to [minAngle, maxAngle], if false, the rotation is unlimited")]

        // ebaender - force limited angles
        private bool limited = true;
        public Vector2 frozenDistanceMinMaxThreshold = new Vector2(0.1f, 0.2f);
        public UnityEvent onFrozenDistanceThreshold;

        [HeaderAttribute("Limited Rotation Min")]
        [Tooltip("If limited is true, the specifies the lower limit, otherwise value is unused")]

        // ebaender - changed default angle
        public float minAngle = 0f;
        [Tooltip("If limited, set whether drive will freeze its angle when the min angle is reached")]
        public bool freezeOnMin = false;
        [Tooltip("If limited, event invoked when minAngle is reached")]
        public UnityEvent onMinAngle;

        [HeaderAttribute("Limited Rotation Max")]
        [Tooltip("If limited is true, the specifies the upper limit, otherwise value is unused")]


        // ebaender - changed default angle
        public float maxAngle = 180.0f;
        [Tooltip("If limited, set whether drive will freeze its angle when the max angle is reached")]
        public bool freezeOnMax = false;
        [Tooltip("If limited, event invoked when maxAngle is reached")]
        public UnityEvent onMaxAngle;

        [Tooltip("If limited is true, this forces the starting angle to be startAngle, clamped to [minAngle, maxAngle]")]
        public bool forceStart = false;
        [Tooltip("If limited is true and forceStart is true, the starting angle will be this, clamped to [minAngle, maxAngle]")]
        public float startAngle = 0.0f;

        [Tooltip("If true, the transform of the GameObject this component is on will be rotated accordingly")]
        public bool rotateGameObject = true;

        // ebaender - don't rotate before detaching
        [Tooltip("If true and game object rotation is enabled, the transform of the GameObject this component is on will be rotated after detaching from the hand")]
        public bool rotateAfterDetach = true;

        [Tooltip("If true, the path of the Hand (red) and the projected value (green) will be drawn")]
        public bool debugPath = false;
        [Tooltip("If debugPath is true, this is the maximum number of GameObjects to create to draw the path")]
        public int dbgPathLimit = 50;

        [Tooltip("If not null, the TextMesh will display the linear value and the angular value of this circular drive")]
        public TextMesh debugText = null;

        [Tooltip("The output angle value of the drive in degrees, unlimited will increase or decrease without bound, take the 360 modulus to find number of rotations")]
        // ebaender - made out angle private
        private float outAngle;

        // ebaender - hand attachment flags;
        protected Hand.AttachmentFlags attachmentFlags = Hand.AttachmentFlags.DetachFromOtherHand;

        private Quaternion start;

        private Vector3 worldPlaneNormal = new Vector3(1.0f, 0.0f, 0.0f);
        private Vector3 localPlaneNormal = new Vector3(1.0f, 0.0f, 0.0f);

        private Vector3 lastHandProjected;

        private Color red = new Color(1.0f, 0.0f, 0.0f);
        private Color green = new Color(0.0f, 1.0f, 0.0f);

        private GameObject[] dbgHandObjects;
        private GameObject[] dbgProjObjects;
        private GameObject dbgObjectsParent;
        private int dbgObjectCount = 0;
        private int dbgObjectIndex = 0;

        private bool driving = false;
        private bool firstComputationAfterAttached = true;
        private PageHandle.Type handleError = PageHandle.Type.None;
        private PageHandle.Type attachedHandleType = PageHandle.Type.None;

        // If the drive is limited as is at min/max, angles greater than this are ignored

        // ebaender - increased threshold
        private float minMaxAngularThreshold = 100.0f;

        private bool frozen = false;
        private float frozenAngle = 0.0f;
        private Vector3 frozenHandWorldPos = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector2 frozenSqDistanceMinMaxThreshold = new Vector2(0.0f, 0.0f);

        private Hand handHoverLocked = null;

        private Interactable interactable;

        // ebaender - pose blender
        private CoverPoseBlender poseBlender;

        //-------------------------------------------------
        private void Freeze(Hand hand)
        {
            frozen = true;
            frozenAngle = outAngle;
            frozenHandWorldPos = hand.hoverSphereTransform.position;
            frozenSqDistanceMinMaxThreshold.x = frozenDistanceMinMaxThreshold.x * frozenDistanceMinMaxThreshold.x;
            frozenSqDistanceMinMaxThreshold.y = frozenDistanceMinMaxThreshold.y * frozenDistanceMinMaxThreshold.y;
        }


        //-------------------------------------------------
        private void UnFreeze()
        {
            frozen = false;
            frozenHandWorldPos.Set(0.0f, 0.0f, 0.0f);
        }

        private void Awake()
        {
            // ebaender - get pose blender
            poseBlender = this.GetComponent<CoverPoseBlender>();

            interactable = this.GetComponent<Interactable>();
        }

        //-------------------------------------------------
        protected override void Start()
        {
            base.Start();

            if (childCollider == null)
            {
                childCollider = GetComponentInChildren<Collider>();
            }

            if (Book.coverMapping == null)
            {
                Book.coverMapping = GetComponent<LinearMapping>();
            }

            if (Book.coverMapping == null)
            {
                Book.coverMapping = gameObject.AddComponent<LinearMapping>();
            }

            worldPlaneNormal = new Vector3(0.0f, 0.0f, 0.0f);
            worldPlaneNormal[(int)axisOfRotation] = 1.0f;

            localPlaneNormal = worldPlaneNormal;

            if (transform.parent)
            {
                worldPlaneNormal = transform.parent.localToWorldMatrix.MultiplyVector(worldPlaneNormal).normalized;
            }

            if (limited)
            {
                start = Quaternion.identity;
                outAngle = transform.localEulerAngles[(int)axisOfRotation];

                if (forceStart)
                {
                    outAngle = Mathf.Clamp(startAngle, minAngle, maxAngle);
                }
            }
            else
            {
                start = Quaternion.AngleAxis(transform.localEulerAngles[(int)axisOfRotation], localPlaneNormal);
                outAngle = 0.0f;
            }

            if (debugText)
            {
                debugText.alignment = TextAlignment.Left;
                debugText.anchor = TextAnchor.UpperLeft;
            }

            UpdateAll();
        }


        //-------------------------------------------------
        void OnDisable()
        {
            if (handHoverLocked)
            {
                handHoverLocked.HideGrabHint();
                handHoverLocked.HoverUnlock(interactable);
                handHoverLocked = null;
            }
        }


        //-------------------------------------------------
        private IEnumerator HapticPulses(Hand hand, float flMagnitude, int nCount)
        {
            if (hand != null)
            {
                int nRangeMax = (int)Util.RemapNumberClamped(flMagnitude, 0.0f, 1.0f, 100.0f, 900.0f);
                nCount = Mathf.Clamp(nCount, 1, 10);

                //float hapticDuration = nRangeMax * nCount;

                //hand.TriggerHapticPulse(hapticDuration, nRangeMax, flMagnitude);

                for (ushort i = 0; i < nCount; ++i)
                {
                    ushort duration = (ushort)Random.Range(100, nRangeMax);
                    hand.TriggerHapticPulse(duration);
                    yield return new WaitForSeconds(.01f);
                }
            }
        }


        //-------------------------------------------------
        private void OnHandHoverBegin(Hand hand)
        {
            hand.ShowGrabHint();
        }


        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            hand.HideGrabHint();

            if (driving && hand)
            {
                //hand.TriggerHapticPulse() //todo: fix
                StartCoroutine(HapticPulses(hand, 1.0f, 10));
            }

            driving = false;
            handHoverLocked = null;
        }

        private GrabTypes grabbedWithType;
        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            // ebaender
            worldPlaneNormal = new Vector3(0f, 0f, 0f);
            worldPlaneNormal[(int)axisOfRotation] = 1.0f;

            // ebaender
            if (transform.parent)
            {
                worldPlaneNormal = transform.parent.localToWorldMatrix.MultiplyVector(worldPlaneNormal).normalized;
            }

            GrabTypes startingGrabType = hand.GetGrabStarting();
            bool isGrabEnding = hand.IsGrabbingWithType(grabbedWithType) == false;

            // ebaender - only allow left hand grip
            if (grabbedWithType == GrabTypes.None && hand == Player.instance.leftHand)
            {
                if (startingGrabType == GrabTypes.Grip || (startingGrabType == GrabTypes.Pinch && Book.coverMapping.value < Book.lockThreshold))
                {
                    grabbedWithType = startingGrabType;
                    // Trigger was just pressed
                    lastHandProjected = ComputeToTransformProjected(hand.hoverSphereTransform);

                    if (hoverLock)
                    {
                        hand.HoverLock(interactable);
                        handHoverLocked = hand;
                    }

                    driving = true;

                    // ebaender - attach hand
                    hand.AttachObject(gameObject, startingGrabType, attachmentFlags);

                    // do this after attaching
                    ComputeSimpleAngle(hand);
                    UpdateAll();

                    hand.HideGrabHint();
                }
                else if (startingGrabType == GrabTypes.Pinch)
                {
                    if (hand.LastCollider.GetComponent<BookHandle>() == Book.lowerCoverHandle)
                    {
                        hand.LastCollider = Book.coverPageHandle.GetComponent<Collider>();
                        if (hand.LastCollider.GetComponent<PageHandle>().IsActive())
                        {
                            hand.LastCollider.GetComponentInParent<PageDrive>().SendMessage("HandHoverUpdate", hand);
                        }
                    }
                }
            }
            else if (grabbedWithType != GrabTypes.None && isGrabEnding)
            {
                // Trigger was just released
                if (hoverLock)
                {
                    hand.HoverUnlock(interactable);
                    handHoverLocked = null;
                }

                driving = false;
                grabbedWithType = GrabTypes.None;

                // ebaender - detach hand
                hand.DetachObject(gameObject);
            }

            if (driving && isGrabEnding == false && hand.hoveringInteractable == this.interactable)
            {
                ComputeSimpleAngle(hand);
                UpdateAll();
            }
        }

        //-------------------------------------------------
        private Vector3 ComputeToTransformProjected(Transform xForm)
        {
            Vector3 toTransform = (xForm.position - transform.position).normalized;
            Vector3 toTransformProjected = new Vector3(0.0f, 0.0f, 0.0f);

            // Need a non-zero distance from the hand to the center of the CircularDrive
            if (toTransform.sqrMagnitude > 0.0f)
            {
                toTransformProjected = Vector3.ProjectOnPlane(toTransform, worldPlaneNormal).normalized;
            }
            else
            {
                Debug.LogFormat("<b>[SteamVR Interaction]</b> The collider needs to be a minimum distance away from the CircularDrive GameObject {0}", gameObject.ToString());
                Debug.Assert(false, string.Format("<b>[SteamVR Interaction]</b> The collider needs to be a minimum distance away from the CircularDrive GameObject {0}", gameObject.ToString()));
            }

            if (debugPath && dbgPathLimit > 0)
            {
                DrawDebugPath(xForm, toTransformProjected);
            }

            return toTransformProjected;
        }


        //-------------------------------------------------
        private void DrawDebugPath(Transform xForm, Vector3 toTransformProjected)
        {
            if (dbgObjectCount == 0)
            {
                dbgObjectsParent = new GameObject("Circular Drive Debug");
                dbgHandObjects = new GameObject[dbgPathLimit];
                dbgProjObjects = new GameObject[dbgPathLimit];
                dbgObjectCount = dbgPathLimit;
                dbgObjectIndex = 0;
            }

            //Actual path
            GameObject gSphere = null;

            if (dbgHandObjects[dbgObjectIndex])
            {
                gSphere = dbgHandObjects[dbgObjectIndex];
            }
            else
            {
                gSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gSphere.transform.SetParent(dbgObjectsParent.transform);
                dbgHandObjects[dbgObjectIndex] = gSphere;
            }

            gSphere.name = string.Format("actual_{0}", (int)((1.0f - red.r) * 10.0f));
            gSphere.transform.position = xForm.position;
            gSphere.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            gSphere.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
            gSphere.gameObject.GetComponent<Renderer>().material.color = red;

            // ebaender - disable collider
            gSphere.gameObject.GetComponent<Collider>().enabled = false;

            if (red.r > 0.1f)
            {
                red.r -= 0.1f;
            }
            else
            {
                red.r = 1.0f;
            }

            //Projected path
            gSphere = null;

            if (dbgProjObjects[dbgObjectIndex])
            {
                gSphere = dbgProjObjects[dbgObjectIndex];
            }
            else
            {
                gSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gSphere.transform.SetParent(dbgObjectsParent.transform);
                dbgProjObjects[dbgObjectIndex] = gSphere;
            }

            gSphere.name = string.Format("projed_{0}", (int)((1.0f - green.g) * 10.0f));
            gSphere.transform.position = transform.position + toTransformProjected * 0.25f;
            gSphere.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            gSphere.transform.localScale = new Vector3(0.004f, 0.004f, 0.004f);
            gSphere.gameObject.GetComponent<Renderer>().material.color = green;

            // ebaender - disable collider
            gSphere.gameObject.GetComponent<Collider>().enabled = false;

            if (green.g > 0.1f)
            {
                green.g -= 0.1f;
            }
            else
            {
                green.g = 1.0f;
            }

            dbgObjectIndex = (dbgObjectIndex + 1) % dbgObjectCount;
        }


        //-------------------------------------------------
        // Updates the LinearMapping value from the angle
        //-------------------------------------------------
        private void UpdateLinearMapping()
        {
            if (limited)
            {
                // Map it to a [0, 1] value
                lastMappingValue = Book.coverMapping.value;
                Book.coverMapping.value = (outAngle - minAngle) / (maxAngle - minAngle);
            }
            else
            {
                // Normalize to [0, 1] based on 360 degree windings
                float flTmp = outAngle / 360.0f;
                Book.coverMapping.value = flTmp - Mathf.Floor(flTmp);
            }

            // ebaender = notify state controller
            Book.stateController.UpdateAll();
            UpdateDebugText();
        }


        //-------------------------------------------------
        // Updates the LinearMapping value from the angle
        //-------------------------------------------------
        private void UpdateGameObject()
        {
            // ebaender - rotate according to settings
            if (rotateGameObject && !rotateAfterDetach)
            {
                transform.localRotation = start * Quaternion.AngleAxis(outAngle, localPlaneNormal);
            }
        }
        protected virtual void OnDetachedFromHand(Hand hand)
        {
            attachedHandleType = PageHandle.Type.None;
            if (rotateGameObject && rotateAfterDetach)
            {
                transform.localRotation = start * Quaternion.AngleAxis(outAngle, localPlaneNormal);
            }

            if (Book.coverPhysicsController != null)
            {
                Book.coverPhysicsController.DriveDetached(hand.GetTrackedObjectVelocity(), Book.coverMapping.value - lastMappingValue);
            }
        }

        // ebaender
        protected virtual void OnAttachedToHand(Hand hand)
        {
            firstComputationAfterAttached = true;
            attachedHandleType = Book.coverMapping.value > 0.5f ? PageHandle.Type.Cover : PageHandle.Type.Back;
            hand.Hide();
            if (rotateGameObject && rotateAfterDetach)
            {
                transform.localRotation = start;
            }

            if (Book.coverPhysicsController != null)
            {
                Book.coverPhysicsController.DriveAttached();
            }
        }

        protected virtual void HandAttachedUpdate(Hand hand)
        {
            hand.Show();
            if (hand.IsGrabEnding(this.gameObject))
            {
                hand.DetachObject(gameObject);
            }
        }

        //-------------------------------------------------
        // Updates the Debug TextMesh with the linear mapping value and the angle
        //-------------------------------------------------
        private void UpdateDebugText()
        {
            if (debugText)
            {
                debugText.text = string.Format("Linear: {0}\nAngle:  {1}\n", Book.coverMapping.value, outAngle);
            }
        }


        //-------------------------------------------------
        // Updates the Debug TextMesh with the linear mapping value and the angle
        //-------------------------------------------------
        private void UpdateAll()
        {
            UpdateLinearMapping();
            UpdateGameObject();
            UpdateDebugText();
        }

        // ebaender - sync object transform rotation with physics controller
        void FixedUpdate()
        {
            if (!driving)
                transform.localRotation = Book.coverPhysicsController.transform.localRotation;
        }

        // ebaender - get angle between projected transform and starting transform
        private void ComputeSimpleAngle(Hand hand)
        {
            Vector3 toTransformProjected = ComputeToTransformProjected(hand.hoverSphereTransform);
            Vector3 fromTransform = transform.parent.localToWorldMatrix.MultiplyVector(new Vector3(-1f, 0f, 0f)).normalized;
            float newAngle = Vector3.SignedAngle(fromTransform, toTransformProjected, worldPlaneNormal);
            if (newAngle < 0f)
            {
                if (outAngle < 90f)
                {
                    outAngle = 0;
                }
                else
                {
                    outAngle = 180f;
                }
            }
            else
            {
                outAngle = newAngle;
            }

            if (firstComputationAfterAttached)
            {
                firstComputationAfterAttached = false;
                if (attachedHandleType == PageHandle.Type.Back && outAngle > 90f)
                {
                    handleError = PageHandle.Type.Back;
                }
                if (attachedHandleType == PageHandle.Type.Cover && outAngle < 90f)
                {
                    handleError = PageHandle.Type.Cover;
                }
            }

            if (attachedHandleType == PageHandle.Type.Back && outAngle < 90f)
            {
                handleError = PageHandle.Type.None;
            }
            if (attachedHandleType == PageHandle.Type.Cover && outAngle > 90f)
            {
                handleError = PageHandle.Type.None;
            }

            switch (handleError)
            {
                case PageHandle.Type.Back:
                    // Debug.Log("error back");
                    outAngle = 0f;
                    break;
                case PageHandle.Type.Cover:
                    // Debug.Log("error cover");
                    outAngle = 180f;
                    break;
                default:
                    // Debug.Log("no error");
                    break;
            }
        }


        //-------------------------------------------------
        // Computes the angle to rotate the game object based on the change in the transform
        //-------------------------------------------------
        private void ComputeAngle(Hand hand)
        {
            Vector3 toHandProjected = ComputeToTransformProjected(hand.hoverSphereTransform);

            if (!toHandProjected.Equals(lastHandProjected))
            {
                float absAngleDelta = Vector3.Angle(lastHandProjected, toHandProjected);

                if (absAngleDelta > 0.0f)
                {
                    if (frozen)
                    {
                        float frozenSqDist = (hand.hoverSphereTransform.position - frozenHandWorldPos).sqrMagnitude;
                        if (frozenSqDist > frozenSqDistanceMinMaxThreshold.x)
                        {
                            outAngle = frozenAngle + Random.Range(-1.0f, 1.0f);

                            float magnitude = Util.RemapNumberClamped(frozenSqDist, frozenSqDistanceMinMaxThreshold.x, frozenSqDistanceMinMaxThreshold.y, 0.0f, 1.0f);
                            if (magnitude > 0)
                            {
                                StartCoroutine(HapticPulses(hand, magnitude, 10));
                            }
                            else
                            {
                                StartCoroutine(HapticPulses(hand, 0.5f, 10));
                            }

                            if (frozenSqDist >= frozenSqDistanceMinMaxThreshold.y)
                            {
                                onFrozenDistanceThreshold.Invoke();
                            }
                        }
                    }
                    else
                    {
                        Vector3 cross = Vector3.Cross(lastHandProjected, toHandProjected).normalized;
                        float dot = Vector3.Dot(worldPlaneNormal, cross);

                        float signedAngleDelta = absAngleDelta;

                        if (dot < 0.0f)
                        {
                            signedAngleDelta = -signedAngleDelta;
                        }

                        if (limited)
                        {
                            float angleTmp = Mathf.Clamp(outAngle + signedAngleDelta, minAngle, maxAngle);

                            if (outAngle == minAngle)
                            {
                                if (angleTmp > minAngle && absAngleDelta < minMaxAngularThreshold)
                                {
                                    outAngle = angleTmp;
                                    lastHandProjected = toHandProjected;
                                }
                            }
                            else if (outAngle == maxAngle)
                            {
                                if (angleTmp < maxAngle && absAngleDelta < minMaxAngularThreshold)
                                {
                                    outAngle = angleTmp;
                                    lastHandProjected = toHandProjected;
                                }
                            }
                            else if (angleTmp == minAngle)
                            {
                                outAngle = angleTmp;
                                lastHandProjected = toHandProjected;
                                onMinAngle.Invoke();
                                if (freezeOnMin)
                                {
                                    Freeze(hand);
                                }
                            }
                            else if (angleTmp == maxAngle)
                            {
                                outAngle = angleTmp;
                                lastHandProjected = toHandProjected;
                                onMaxAngle.Invoke();
                                if (freezeOnMax)
                                {
                                    Freeze(hand);
                                }
                            }
                            else
                            {
                                outAngle = angleTmp;
                                lastHandProjected = toHandProjected;
                            }
                        }
                        else
                        {
                            outAngle += signedAngleDelta;
                            lastHandProjected = toHandProjected;
                        }
                    }
                }
            }
        }
    }
}
