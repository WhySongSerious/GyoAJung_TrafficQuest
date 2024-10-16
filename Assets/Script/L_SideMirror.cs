using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class L_SideMirror : MonoBehaviour
{
    private Camera Cam;
    void Start()
    {
        Cam = GetComponent<Camera>();
        Cam.depth = -2;
    }

    // Update is called once per frame
    void Update()
    {
        //if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0)) 

        if (Input.GetKeyUp(KeyCode.O))
        {
            if (Cam.depth == 1)
                Cam.depth = -2;
            else
                Cam.depth = 1;
        }
    }
}
