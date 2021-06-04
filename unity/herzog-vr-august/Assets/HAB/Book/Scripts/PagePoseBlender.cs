using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Valve.VR.InteractionSystem
{
    [RequireComponent(typeof(SteamVR_Skeleton_Poser))]
    public class PagePoseBlender : PoseBlender
    {
        protected override void Start()
        {
            base.Start();
        }

        void Update()
        {
            if (currentPose != null)
            {
                float value = Book.pageMapping.value;
                if (value >= 0f && value < 0.25f)
                {
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-1", value * 4);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-2", 0f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-3", 0f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-4", 0f);
                }
                else if (value >= 0.25f && value < 0.5f)
                {
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-1", 1f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-2", (value - 0.25f) * 4);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-3", 0f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-4", 0f);
                }
                else if (value >= 0.5f && value < 0.75f)
                {
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-1", 1f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-2", 1f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-3", (value - 0.5f) * 4);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-4", 0f);
                }
                else if (value >= 0.75f)
                {
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-1", 1f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-2", 1f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-3", 1f);
                    poser.TryToSetBlendingBehaviourValue(currentPose + "-4", (value - 0.75f) * 4);
                }
            }
        }
    }
}