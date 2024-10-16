using System.Collections;
using System.Text;
using UnityEngine;

public enum InputCondition
{
    logitech_wheel = 0,
    keyboard = 1
};

public class Player : MonoBehaviour
{
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider rearRight;
    [SerializeField] WheelCollider rearLeft;

    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform rearRightTransform;
    [SerializeField] Transform rearLeftTransform;    

    private float currentTurnAngle = 0f;
    private float maxTurnAngle = 15f;
    private float accelerator;
    private float brakeForce;
    private float currentAccelerator = 0f;
    private float currentBrakeForce = 0f;

    public InputCondition inputcondition;

    private float t;

    private Vector3 initialVelocity = Vector3.zero;

    static LogitechGSDK.DIJOYSTATE2ENGINES rec;

    public float blinkInterval = 0.5f; // 깜박임 간격 (초)
    private bool isLeftIndicatorOn = false;
    private bool isRightIndicatorOn = false;
    //private bool isFrontIndicatorOn = false;
    private float lastBlinkTime;
    private float lastIndicatorChangeTime = -1f;
    private float changeDelay = 1.0f;
    public EffectControlInfo effectinfo;


    void Start()
    {
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
    }
    void OnApplicationQuit()
    {
        Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            Accel();
            Brake();
            WheelControl();
            LightControl();
        }
        else if (!LogitechGSDK.LogiIsConnected(0))
        {
            Debug.Log("PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER");
        }
        else
        {
            Debug.Log("THIS WINDOW NEEDS TO BE IN FOREGROUND IN ORDER FOR THE SDK TO WORK PROPERLY");
        }
    }

    private void Accel()
    {

        t = Time.deltaTime;
        switch (inputcondition)
        {   
            case InputCondition.logitech_wheel:
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    accelerator = Mathf.Abs(rec.lY - 32767) / 400;
                    //Debug.Log(accelerator);
                    currentAccelerator = accelerator;
                }
                break;
            case InputCondition.keyboard:
                Debug.Log("keyboard");
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    accelerator = 3;
                    initialVelocity += accelerator * t * Vector3.forward;
                    transform.Translate(initialVelocity * t + Vector3.forward * t * t * accelerator);
                    //Debug.Log("현재속도: " + initialVelocity.z + ", 가속도: " + accelerator);
                }
                else
                    transform.Translate(initialVelocity * t + Vector3.forward * t * t * accelerator);
                break;
        }
    }

    private void Brake()
    {
        t = Time.deltaTime;
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    brakeForce = Mathf.Abs(rec.lRz - 32767) / 400;
                    //Debug.Log("현재속도: " + initialVelocity.z + ", 가속도: " + brakeForce);
                    if (brakeForce <= 0)
                        brakeForce = 0;
                    currentBrakeForce = brakeForce;
                }
                else
                    brakeForce = 0f;
                break;
            case InputCondition.keyboard:
                if (Input.GetKey(KeyCode.Space))
                {

                    Debug.Log("현재속도: " + initialVelocity.z + ", 가속도: -" + brakeForce);
                    if (initialVelocity.z <= 0)
                    {
                        brakeForce = initialVelocity.z;
                        initialVelocity.z = 0;
                    }
                    else
                    {
                        brakeForce = 3;
                        initialVelocity -= brakeForce * t * Vector3.forward;
                    }

                    transform.Translate(initialVelocity * t - Vector3.forward * t * t * brakeForce);
                }
                else
                    brakeForce = 0f;
                break;
        }
    }

    //휠 제어 함수
    void WheelControl()
    {
        frontRight.motorTorque = currentAccelerator;
        frontLeft.motorTorque = currentAccelerator;

        frontRight.brakeTorque = currentBrakeForce;
        frontLeft.brakeTorque = currentBrakeForce;
        rearRight.motorTorque = currentBrakeForce;
        rearLeft.brakeTorque = currentBrakeForce;

        currentTurnAngle = maxTurnAngle * rec.lX / 32767;
        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        UpdateWheelVisual(frontRightTransform, frontRight);
        UpdateWheelVisual(frontLeftTransform, frontLeft);
        UpdateWheelVisual(rearRightTransform, rearRight);
        UpdateWheelVisual(rearLeftTransform, rearLeft);
    }

    //휠 운동 시각화
    void UpdateWheelVisual(Transform trans, WheelCollider wheelCol)
    {
        Vector3 UpdatePos;
        Quaternion UpdateRot;

        //휠 운동 연산 결과를 월드 좌표로 변환
        wheelCol.GetWorldPose(out UpdatePos, out UpdateRot);

        trans.position = UpdatePos;
        trans.rotation = UpdateRot;
    }

    void LightControl()
    {
        float t = Time.time;
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            rec = LogitechGSDK.LogiGetStateUnity(0);

            // 오른쪽 방향지시등
            if (LogitechGSDK.LogiButtonIsPressed(0, 5) && t - lastIndicatorChangeTime >= changeDelay)
            {
                isRightIndicatorOn = !isRightIndicatorOn; // 상태 반전
                if (isRightIndicatorOn)
                {
                    isLeftIndicatorOn = false; // 왼쪽 방향 끄기
                    ToggleLights(effectinfo.leftLight, false);
                }
                ToggleLights(effectinfo.rightLight, isRightIndicatorOn);
                lastIndicatorChangeTime = t; // 마지막 변경 시간 기록
            }

            // 왼쪽 방향지시등
            if (LogitechGSDK.LogiButtonIsPressed(0, 4) && t - lastIndicatorChangeTime >= changeDelay)
            {
                isLeftIndicatorOn = !isLeftIndicatorOn; // 상태 반전
                if (isLeftIndicatorOn)
                {
                    isRightIndicatorOn = false; // 오른쪽 방향 끄기
                    ToggleLights(effectinfo.rightLight, false);
                }
                ToggleLights(effectinfo.leftLight, isLeftIndicatorOn);
                lastIndicatorChangeTime = t; // 마지막 변경 시간 기록
            }
            /*
            // 전방 라이트
            if (LogitechGSDK.LogiButtonReleased(0, 6))
            {
                if (isFrontIndicatorOn)
                {
                    ToggleLights(effectinfo.frontLight, false);
                    isFrontIndicatorOn = false;
                }
                else
                {
                    isFrontIndicatorOn = true;
                    ToggleLights(effectinfo.frontLight, true); // 전방 라이트 켜기
                }

            }
            */

            // 후진 라이트
            if (rec.lRz < 32766) // 후진 기어 위치
            {
                ToggleLights(effectinfo.backLight, true); // 후진 불 켜기
            }
            else
            {
                ToggleLights(effectinfo.backLight, false); // 후진 불 끄기
            }
        }

        HandleBlinking(effectinfo.leftLight, isLeftIndicatorOn, t);
        HandleBlinking(effectinfo.rightLight, isRightIndicatorOn, t);
    }

    void ToggleLights(Light[] lights, bool? state = null)
    {
        foreach (Light light in lights)
        {
            light.enabled = state ?? !light.enabled; // 상태에 따라 라이트 켜거나 끄기
        }
    }

    void HandleBlinking(Light[] lights, bool isIndicatorOn, float t)
    {
        if (isIndicatorOn && t - lastBlinkTime >= blinkInterval)
        {
            lastBlinkTime = t;
            foreach (Light light in lights)
            {
                light.enabled = !light.enabled; // 라이트 상태 깜빡임
            }
        }
    }
    //이펙트 제어 정보
    [System.Serializable]
    public struct EffectControlInfo
    {
        [Header("Light")]
        public Light[] frontLight;
        public Light[] backLight;
        public Light[] leftLight;
        public Light[] rightLight;
    }

    /* 
     시작 초기화
     ToggleLights(effectinfo.backLight, false);
     */
}
