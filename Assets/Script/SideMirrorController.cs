using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class SideMirrorController : MonoBehaviour
{
    LogitechGSDK.DIJOYSTATE2ENGINES rec;

    [Header("Mirror")]
    [SerializeField] GameObject LS_Mirror;
    [SerializeField] GameObject RS_Mirror;

    private float lastIndicatorChangeTime = -1f;
    private float changeDelay = 0.5f;
    private bool checkRight = false;
    private bool checkLeft = false;

    // Start is called before the first frame update
    void Start()
    {
        RS_Mirror.SetActive(checkRight);
        LS_Mirror.SetActive(checkLeft);
    }

    // Update is called once per frame
    void Update()
    {
        float t = Time.time;
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            rec = LogitechGSDK.LogiGetStateUnity(0);

            //왼쪽 사이드 미러 화면 띄우기
            if (LogitechGSDK.LogiButtonIsPressed(0, 11) && t - lastIndicatorChangeTime >= changeDelay)
            {
                if (checkLeft)
                {
                    checkLeft = false;
                }
                else
                {
                    checkRight = false;
                    checkLeft = true;
                }
                RS_Mirror.SetActive(checkRight);
                LS_Mirror.SetActive(checkLeft);
                lastIndicatorChangeTime = t;
            }

            //오른쪽 사이드 미러 화면 띄우기
            if (LogitechGSDK.LogiButtonIsPressed(0, 10) && t - lastIndicatorChangeTime >= changeDelay)
            {
                if (checkRight)
                {
                    checkRight = false;
                }
                else
                {
                    checkLeft = false;
                    checkRight = true;
                }
                RS_Mirror.SetActive(checkRight);
                LS_Mirror.SetActive(checkLeft);
                lastIndicatorChangeTime = t;
            }
        }
    }

}
