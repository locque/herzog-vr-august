using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class Dolly : MonoBehaviour
{
    // Start is called before the first frame update
    private Camera camera;
    private float basefov;
    private Vector3 basepos;
    private float runtime = 0f;
    void Start()
    {
        camera = GetComponent<Camera>();
        basefov = camera.fieldOfView; 
        basepos = camera.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        camera.enabled = true;
        if (ControllerController.instance.active) {
            float timeFactor = Mathf.Pow(runtime / 10f, 2f);
            camera.fieldOfView -= 1f * timeFactor;
            camera.transform.position -= camera.transform.forward * Time.deltaTime * 0.5f * timeFactor;
            runtime += Time.deltaTime;
        } else {
            camera.fieldOfView = basefov;
            camera.transform.localPosition = basepos;
            runtime = 0f;
        }
    }
}
