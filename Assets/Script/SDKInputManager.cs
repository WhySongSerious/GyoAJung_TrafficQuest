using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SDKInputManager : MonoBehaviour
{

    public float vertical;
    public float horizontal;
    public float brake;

    // 페달
    [HideInInspector] public float accelInput;
    [HideInInspector] public float clutchInput;
    [HideInInspector] public float brakeInput;
    // 핸들
    [HideInInspector] public float steerInput;
    // 기어
    [HideInInspector] public int gearInput;
    // 핸들버튼
    [HideInInspector] public bool TInput; // 미러 
    [HideInInspector] public bool OInput;
    [HideInInspector] public bool SInput;
    // 핸들버튼
    // 메뉴 선택 : 위 아래 오른쪽 왼쪽
    [HideInInspector] public bool UpInput;
    [HideInInspector] public bool DownInput;
    [HideInInspector] public bool RightInput;
    [HideInInspector] public bool LeftInput;

    //깜박이
    [HideInInspector] public bool RightBumperInput;
    [HideInInspector] public bool LeftBumperInput;


    public InputCondition inputcondition;

    private void Start()
    {
        print("LogiSteeringInitialize: " + LogitechGSDK.LogiSteeringInitialize(false));
    }

    void Update()
    {
        switch (inputcondition)
        {
            case InputCondition.keyboard:
                vertical = Input.GetAxis("Vertical");
                horizontal = Input.GetAxis("Horizontal");
                break;
            case InputCondition.logitech_wheel:
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    #region
                    accelInput = LogitechInput.GetAxis("Accel_Vertical");
                    clutchInput = LogitechInput.GetAxis("Clutch_Vertical");
                    brakeInput = LogitechInput.GetAxis("Brake_Vertical");
                    steerInput = LogitechInput.GetAxis("Steering_Horizontal");


                    // 범퍼
                    RightBumperInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.RightBumper);
                    LeftBumperInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.LeftBumper);


                    if (LeftBumperInput)
                    {
                        Debug.Log("왼쪽범퍼getkeytriggered");
                    }


                    /*
                    if (LeftBumperInput2)
                    {
                        Debug.Log("왼쪽범퍼GetKeyPressed");
                    }

                    if (LeftBumperInput3)
                    {
                        Debug.Log("왼쪽범퍼GetKeyReleased");
                    }
                    */

                    // 핸들 버튼
                    TInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.Triangle);
                    SInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.Square);
                    OInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.Circle);
                    UpInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.Up);
                    DownInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.Down);
                    RightInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.Right);
                    LeftInput = LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, LogitechKeyCode.Left);


                    vertical = accelInput;
                    horizontal = steerInput;
                    brake = brakeInput;
                    #endregion


                    // Gear Setting 
                    // 맨왼쪽 페달 밟고 기어 변동 가능
                    if (clutchInput >= 0.7)
                    {
                        Debug.Log($"clutchInput: {clutchInput}");
                        for (int i = 14; i < 16; i++)
                        {
                            if (LogitechInput.GetKeyPressed(LogitechKeyCode.FirstIndex, (LogitechKeyCode)i))
                            {

                                gearInput = i;
                                //Debug.Log($"gearInput: {gearInput}");
                            }
                            else gearInput = 0;
                        }
                    }

                    switch (gearInput)
                    {
                        // 전진
                        case 14:
                            gearInput = 0;
                            break;

                        // 후진
                        case 15:
                            gearInput = 2;
                            break;

                        // 중립
                        case 0:
                            gearInput = 1;
                            break;
                    }
                    Debug.Log($"gearInput: {gearInput}");

                }
                break;
        }
    }

}