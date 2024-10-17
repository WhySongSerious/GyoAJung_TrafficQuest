using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class R_SideMirror : MonoBehaviour
{
    private Camera Cam;
    LogitechGSDK.DIJOYSTATE2ENGINES rec;
    public Camera leftMirror;

    private float lastIndicatorChangeTime = -1f;
    private float changeDelay = 0.5f;
    void Start()
    {
        Cam = GetComponent<Camera>();
        Cam.enabled = false;
    }

    void Update()
    {
        float t = Time.time;
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            rec = LogitechGSDK.LogiGetStateUnity(0);
            if (LogitechGSDK.LogiButtonIsPressed(0, 10) && t - lastIndicatorChangeTime >= changeDelay)
            {
                if (Cam.enabled == true)
                {
                    Cam.enabled = false;
                }
                else
                {
                    Cam.enabled = true;
                    leftMirror.enabled = !Cam.enabled;
                }
                lastIndicatorChangeTime = t;
            }
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            if (Cam.depth == 1)
                Cam.depth = -2;
            else
                Cam.depth = 1;
        }
    }
}
