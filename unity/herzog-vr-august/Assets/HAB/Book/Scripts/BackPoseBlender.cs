using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(SteamVR_Skeleton_Poser))]
    public class BackPoseBlender : PoseBlender
    {
        void Update()
        {
            if (Book.helper == null || currentPose == null)
            {
                return;
            }

            float open = Book.helper.OpenAmmount;
            if (open >= 0f && open < 0.05f)
            {
                poser.TryToSetBlendingBehaviourValue(currentPose + "-1", open * 25);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-2", 0f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-3", 0f);
                // Debug.Log(open + " : " + 1);
            }
            else if (open >= 0.05f && open < 0.5f)
            {
                poser.TryToSetBlendingBehaviourValue(currentPose + "-1", 1f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-2", (open - 0.05f) * 25);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-3", 0f);
                // Debug.Log(open + " : " + 2);
            }
            else if (open >= 0.5f)
            {
                poser.TryToSetBlendingBehaviourValue(currentPose + "-1", 1f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-2", 1f);
                poser.TryToSetBlendingBehaviourValue(currentPose + "-3", (open - 0.5f) * 2);
                // Debug.Log(open + " : " + 3);
            }
            else
            {
                // Debug.Log("! " + open);
            }
        }
    }
}