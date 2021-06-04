using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class BookIndex : MonoBehaviour
    {
        public com.guinealion.animatedBook.LightweightBookHelper helper = null;
        public BookStateController stateController = null;
        public CoverPhysicsController coverPhysicsController = null;
        public PagePhysicsController pagePhysicsController = null;
        public BackPoseBlender backPoseBlender = null;
        public CoverPoseBlender coverPoseBlender = null;
        public BookHandleController backHandleController = null;
        public BookHandleController coverHandleController = null;
        public PageHandleController pageHandleController = null;
        public CoverDrive coverDrive = null;
        public PageDrive pageDrive = null;
        public BookHandle lowerBackHandle = null;
        public BookHandle lowerCoverHandle = null;
        public PageHandle backPageHandle = null;
        public PageHandle coverPageHandle = null;
        public LinearMapping coverMapping = null;
        public LinearMapping progressMapping = null;
        public LinearMapping pageMapping = null;
        public Transform backPageHandleTransform = null;
        public Transform coverPageHandleTransform = null;
        public Collider closedCoverCollider = null;
        public float lockThreshold = 0.01f;
        public float pageThreshold = 0.9f;
        public float hingeMinLimit = 0f;
        public float hingeMaxLimit = 180f;
        public float hingeMaxLimitActual = 177f;
        public float initialCoverMappingValue = 0f;
        public Collider[] colliderLayer = null;

        void Start()
        {
            if (colliderLayer != null)
            {
                foreach (Collider firstCollider in colliderLayer)
                {
                    foreach (Collider secondCollider in colliderLayer)
                    {
                        Physics.IgnoreCollision(firstCollider, secondCollider);
                    }
                }
            }
        }
    }
}
