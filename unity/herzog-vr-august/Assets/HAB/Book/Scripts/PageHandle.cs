using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PageHandle : MonoBehaviour
    {
        public enum Type
        {
            Cover,
            Back,
            None
        }
        private bool active = true;
        private Collider handleCollider = null;
        private MeshRenderer meshRenderer = null;
        public Type handleType = Type.Back;
        public bool allowDebug = true;

        public void SetActive(bool isActive)
        {
            active = isActive;
            GetComponent<Collider>().enabled = isActive;
        }

        public bool IsActive()
        {
            return active;
        }

        void Update()
        {
            if (allowDebug)
            {
                DebugController instance = DebugController.instance;
                if (instance != null)
                {
                    GetComponent<MeshRenderer>().enabled = active && instance.active;
                }
                else
                {
                    Debug.LogWarning("could not find debug controller.");
                    GetComponent<MeshRenderer>().enabled = false;
                }
            }
        }
    }
}