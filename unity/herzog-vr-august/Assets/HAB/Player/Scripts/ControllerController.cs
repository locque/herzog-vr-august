using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class ControllerController : MonoBehaviour
    {
        public SteamVR_Action_Boolean controllerAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("ToggleController");

        public bool active = false;
        public bool overwriteActiveWithInput = true;
        private Player player = null;

        private static ControllerController _instance;
        public static ControllerController instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ControllerController>();
                }
                return _instance;
            }
        }
        private IEnumerator Start()
        {
            _instance = this;
            player = InteractionSystem.Player.instance;

            while (SteamVR.initializedState == SteamVR.InitializedStates.None || SteamVR.initializedState == SteamVR.InitializedStates.Initializing)
                yield return null;
        }

        void Update()
        {
            if (overwriteActiveWithInput)
            {
                active = controllerAction.GetState(player.leftHand.handType) ^ controllerAction.GetState(player.rightHand.handType);
                for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
                {
                    Hand hand = Player.instance.hands[handIndex];
                    if (hand != null)
                    {
                        if (active)
                        {
                            hand.ShowController(true);
                            hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithController);
                        }
                        else
                        {
                            hand.HideController(true);
                            hand.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithoutController);
                        }
                    }
                }
            }
        }
    }
}
