using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class BookBlender : MonoBehaviour
{
    [SerializeField] private com.guinealion.animatedBook.LightweightBookHelper bookHelper = null;
    private SteamVR_Skeleton_Poser poser;
    void Start()
    {
        poser = GetComponent<SteamVR_Skeleton_Poser>();
    }

    void Update()
    {
        // Debug.Log(poser.GetBlendingBehaviourValue("transition") + " " + poser.GetBlendingBehaviourValue("middle") + " " + poser.GetBlendingBehaviourValue("end"));
        float open = bookHelper.OpenAmmount;
        if (open >= 0f && open < 0.05f)
        {
            poser.SetBlendingBehaviourValue("transition", open * 25);
            poser.SetBlendingBehaviourValue("middle", 0f);
            poser.SetBlendingBehaviourValue("end", 0f);
        }
        else if (open >= 0.05f && open < 0.1f)
        {
            poser.SetBlendingBehaviourValue("transition", 1f);
            poser.SetBlendingBehaviourValue("middle", (open - 0.05f) * 25);
            poser.SetBlendingBehaviourValue("end", 0f);
        }
        else if (open >= 0.5f)
        {
            poser.SetBlendingBehaviourValue("transition", 1f);
            poser.SetBlendingBehaviourValue("middle", 1f);
            poser.SetBlendingBehaviourValue("end", (open - 0.5f) * 2);
        }
    }
}
