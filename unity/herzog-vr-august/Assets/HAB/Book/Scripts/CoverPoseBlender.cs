using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(SteamVR_Skeleton_Poser))]
    public class CoverPoseBlender : PoseBlender
    {
        void Update()
        {
            // Debug.Log(currentPose != null ? currentPose : "null");
            if (Book.helper == null || currentPose == null)
            {
                return;
            }

            float open = Book.helper.OpenAmmount;
            if (open >= 0f && open < 0.1f)
            {
                poser.TryToSetBlendingBehaviourValue(currentPose + "-1", open * 10);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-2", 0f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-3", 0f);
            }
            else if (open >= 0.1f && open < 0.5f)
            {
                poser.TryToSetBlendingBehaviourValue(currentPose + "-1", 1f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-2", (open - 0.1f) / 4 * 10);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-3", 0f);
            }
            else if (open >= 0.5f)
            {
                poser.TryToSetBlendingBehaviourValue(currentPose + "-1", 1f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-2", 1f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-3", (open - 0.5f) * 2);
            }
            // poser.TryToSetBlendingBehaviourValue(currentPose + "-3", Mathf.Pow(open, 0.5f));
            // Debug.Log(poser.GetBlendingBehaviourValue(currentPose + "-3"));
        }

    }
}