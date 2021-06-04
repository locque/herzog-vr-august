using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(MeshRenderer))]
    public class BookHandle : MonoBehaviour
    {
        public enum HandleType
        {
            Edge,
            Spine,
            Square
        }
        private bool active = true;
        public HandleType handleType = HandleType.Edge;
        public bool allowDebug = true;

        public void SetActive(bool isActive)
        {
            active = isActive;
            if (GetComponent<Collider>() == null)
            {
                Debug.Log(name);
            }
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