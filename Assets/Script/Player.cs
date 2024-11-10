using System.Collections;
using System.Text;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum InputCondition
{
    logitech_wheel = 0,
    keyboard = 1
};

public class Player : MonoBehaviour
{
    [SerializeField] GameObject SpeedCalaulator;

    [Header("Anti-roll Bar Settings")]                                                              //Anti-roll 제어
    [SerializeField] public bool antiRollEnabled = true;                                            //Anti-roll on/off
    [SerializeField] private float antiRollForce = 5000f;                                           //A

    [Header("Wheel Colliders")]                                                                     //바퀴 제어
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider rearRight;
    [SerializeField] WheelCollider rearLeft;

    [Header("Wheel Transforms")]                                                                    //바퀴 비주얼 업데이트
    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform rearRightTransform;
    [SerializeField] Transform rearLeftTransform;

    [Header("Text UI")]
    [SerializeField] Text rightSign;
    [SerializeField] Text leftSign;
    [SerializeField] Text GearSign;

    [Header("Image UI")]
    [SerializeField] Image LimitSpeedImage;

    [Header("Traffic System")]
    [SerializeField] GameObject TrafficLight;
    public bool isRedLight;
    bool checkingTrafficLight;

    //속도관련 변수
    private float accelerator;                                                                      //엑셀에 가하는 힘
    private float reverseForce;
    private float brakeForce;                                                                       //브레이크에 가하는 힘
    private float currentAccelerator = 0f;                                                          //현재 엑셀을 어느 정도 밟았는지
    private float currentReverseForce = 0f;
    private float currentBrakeForce = 0f;                                                           //현재 브레이크를 어느 정도 밟았는지
    private float currentTurnAngle = 0f;                                                            //현재 바퀴 각도
    private float maxTurnAngle = 6f;
    public int handleResistance = 30;                                                              //핸들 저항 변수
    public int gearInput = 1;
    public float limitSpeed = 50;

    public InputCondition inputcondition;                                                           //현재 Input이 wheel/keyboard 체크

    private float t;                                                                                //시간 측정 변수

    private Vector3 initialVelocity = Vector3.zero;                                                 //keyboard 제어에서 현재 속도

    static LogitechGSDK.DIJOYSTATE2ENGINES rec;                                                     //wheel에 담긴 변수를 쓰게 해줌

    public float blinkInterval = 0.5f;                                                              //방향지시등이 켜졌을 때 깜박임 간격 (초)
    private bool isLeftIndicatorOn = false;                                                         //좌측 방향지시등이 켜져있는지 체크
    private bool isRightIndicatorOn = false;                                                        //우측 방향지시등이 켜져있는지 체크
    //private bool isFrontIndicatorOn = false;                                                      //전조등이 켜져있는지 체크
    private float lastBlinkTime;                                                                    //방향지시등이 깜빡일 때 언제를 기준으로 켜지고 꺼질지를 판단하는 변수
    private float lastIndicatorChangeTime = -1f;                                                    //방향지시등을 켜고 끌 때 입력값이 중복되는 경우를 방지하기 위해 딜레이 관련 변수
    private float changeDelay = 0.5f;                                                               //방향지시등을 켜고 끌 때 입력값이 중복되는 경우를 방지하기 위해 딜레이 관련 변수
    public EffectControlInfo effectinfo;                                                            //라이트 제어

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        LimitSpeedImage.enabled = !enabled;
        leftSign.enabled = !enabled;
        rightSign.enabled = !enabled;
        ShiftGear();
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));                    //wheel 연결이 되어 있는지 체크
    }
    void OnApplicationQuit()
    {
        Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());                       //종료할 때 wheel 연결을 끊어 주는지 체크
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))                           //wheel 업테이트 && 컨트롤러 중 0번째가 연결되어 있는지 체크
        {
            ShiftGear();
            Reverse();
            Accel();                                                                                //엑셀 제어 함수
            Brake();                                                                                //브레이크 제어 함수
            WheelControl();                                                                         //핸들 제어 함수
            LightControl();                                                                         //방향지시등과 같은 라이트 제어 함수

            if (antiRollEnabled)                                                                    //Anti-roll on/off 체크
            {
                ApplyAntiRoll();                                                                    //Anti-roll 제어 함수
            }
                                                                               //지정된 자리로 돌아오고 각도도 초기화
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
            case InputCondition.logitech_wheel:                                                     //wheel 제어 시
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    accelerator = Mathf.Abs(rec.lY - 32767) / 1;                                    //엑셀을 얼마나 밟았는지 연산 (각도는 -32768 ~ 32767)
                    currentAccelerator = accelerator;
                    Debug.Log("Logitech Accel Force: " + currentAccelerator / 10000);

                }
                break;
            case InputCondition.keyboard:
                Debug.Log("keyboard");
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    accelerator = 30000;
                    currentAccelerator = accelerator;
                    initialVelocity += accelerator * t * Vector3.forward;
                    transform.Translate(initialVelocity * t + Vector3.forward * t * t * accelerator);
                    //Debug.Log("현재속도: " + initialVelocity.z + ", 가속도: " + accelerator);
                }
                else
                    transform.Translate(initialVelocity * t + Vector3.forward * t * t * accelerator);
                break;
        }
    }
    private void Reverse()
    {

        t = Time.deltaTime;
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:                                                     //wheel 제어 시
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    reverseForce = Mathf.Abs(rec.lY - 32767) / 1;                                    //엑셀을 얼마나 밟았는지 연산 (각도는 -32768 ~ 32767)
                    currentReverseForce = -reverseForce;
                    Debug.Log("Logitech Accel Force: " + currentReverseForce / 10000);

                }
                break;
            case InputCondition.keyboard:
                Debug.Log("keyboard");
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    reverseForce = 3;
                    initialVelocity += reverseForce * t * Vector3.forward;
                    transform.Translate(initialVelocity * t + Vector3.forward * t * t * reverseForce);
                    //Debug.Log("현재속도: " + initialVelocity.z + ", 가속도: " + reverseForce);
                }
                else
                    transform.Translate(initialVelocity * t + Vector3.forward * t * t * reverseForce);
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
                    brakeForce = Mathf.Abs(rec.lRz - 32767) * 100;                                    //브레이크를 얼마나 밟았는지 연산 (각도는 -32768 ~ 32767)
                    //Debug.Log("Logitech Brake Force: " + brakeForce / 100);

                    currentBrakeForce = brakeForce;
                    //Debug.Log("현재속도: " + initialVelocity.z + ", 가속도: " + brakeForce);
                }
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
                    brakeForce = 0f;                                                                    //브레이크에서 발을 떼었을 때는 항상 브레이크를 걸어주지 않음
                break;
        }
    }

    //휠 제어 함수
    void WheelControl()
    {
        switch (gearInput)
        {
            case 0:                                                                 //후진
                frontRight.motorTorque = currentReverseForce;
                frontLeft.motorTorque = currentReverseForce;
                break;
            case 1:                                                                 //주차
                frontRight.motorTorque = 0;
                frontLeft.motorTorque = 0;
                break;
            case 2:                                                                 //중립


                break;
            case 3:                                                                 //전진
                frontRight.motorTorque = currentAccelerator;                                                    //앞바퀴에 엑셀을 밟은 만큼의 힘을 전달하여 바퀴를 굴려줌
                frontLeft.motorTorque = currentAccelerator;
                break;

        }

        frontRight.brakeTorque = currentBrakeForce;                                                    //모든 바퀴에 브레이크를 밟은 만큼의 힘을 전달하여 바퀴를 멈춰줌
        frontLeft.brakeTorque = currentBrakeForce;
        rearRight.brakeTorque = currentBrakeForce;
        rearLeft.brakeTorque = currentBrakeForce;


        LogitechGSDK.LogiPlayDamperForce(0, handleResistance);                                         //핸들에 handleResistance만큼 저항 부여
        currentTurnAngle = maxTurnAngle * rec.lX / 32767;                                              //앞바퀴에 핸들을 돌린 만큼의 힘을 전달하여 바퀴를 최대 각도까지 돌려줌
        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        UpdateWheelVisual(frontRightTransform, frontRight);                                             //모든 바퀴가 굴러가고 바퀴가 돌아가게 비주얼 업데이트
        UpdateWheelVisual(frontLeftTransform, frontLeft);
        UpdateWheelVisual(rearRightTransform, rearRight);
        UpdateWheelVisual(rearLeftTransform, rearLeft);
    }

    //휠 운동 시각화
    void UpdateWheelVisual(Transform trans, WheelCollider wheelCol)                                     //모든 바퀴가 굴러가고 바퀴가 돌아가게 비주얼 업데이트 관련 함수
    {
        Vector3 UpdatePos;
        Quaternion UpdateRot;

        wheelCol.GetWorldPose(out UpdatePos, out UpdateRot);                                            //휠 운동 연산 결과를 월드 좌표로 변환

        trans.position = UpdatePos;
        trans.rotation = UpdateRot;
    }
    private void ApplyAntiRoll()
    {
        ApplyAntiRollForAxle(frontLeft, frontRight);
        ApplyAntiRollForAxle(rearLeft, rearRight);
    }
    private void ApplyAntiRollForAxle(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = leftWheel.GetGroundHit(out hit);
        if (groundedL)
        {
            travelL = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        }

        bool groundedR = rightWheel.GetGroundHit(out hit);
        if (groundedR)
        {
            travelR = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
        }

        float antiRollForce = (travelL - travelR) * this.antiRollForce;

        if (groundedL)
        {
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        }

        if (groundedR)
        {
            rb.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
        }
    }
    void LightControl()
    {
        t = Time.time;
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            rec = LogitechGSDK.LogiGetStateUnity(0);

            // 오른쪽 방향지시등
            if (LogitechGSDK.LogiButtonIsPressed(0, 4) && t - lastIndicatorChangeTime >= changeDelay)
            {
                isRightIndicatorOn = !isRightIndicatorOn;                                                   // 상태 반전
                rightSign.enabled = isRightIndicatorOn;
                if (isRightIndicatorOn)
                {
                    isLeftIndicatorOn = false;                                                              // 왼쪽 방향 끄기
                    leftSign.enabled = false;
                    ToggleLights(effectinfo.leftLight, false);
                }
                ToggleLights(effectinfo.rightLight, isRightIndicatorOn);
                lastIndicatorChangeTime = t;                                                                // 마지막 변경 시간 기록
            }

            // 왼쪽 방향지시등
            if (LogitechGSDK.LogiButtonIsPressed(0, 5) && t - lastIndicatorChangeTime >= changeDelay)
            {
                isLeftIndicatorOn = !isLeftIndicatorOn;                                                     // 상태 반전
                leftSign.enabled = isLeftIndicatorOn;
                if (isLeftIndicatorOn)
                {
                    isRightIndicatorOn = false;                                                             // 오른쪽 방향 끄기
                    rightSign.enabled = false;
                    ToggleLights(effectinfo.rightLight, false);
                }
                ToggleLights(effectinfo.leftLight, isLeftIndicatorOn);
                lastIndicatorChangeTime = t;                                                                // 마지막 변경 시간 기록
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
                ToggleLights(effectinfo.backLight, true);                                                   // 후진 불 켜기
            }
            else
            {
                ToggleLights(effectinfo.backLight, false);                                                  // 후진 불 끄기
            }
        }

        SignBlinking(leftSign, isLeftIndicatorOn, t);
        SignBlinking(rightSign, isRightIndicatorOn, t);
        HandleBlinking(effectinfo.leftLight, isLeftIndicatorOn, t);
        HandleBlinking(effectinfo.rightLight, isRightIndicatorOn, t);
    }

    void ToggleLights(Light[] lights, bool? state = null)
    {
        foreach (Light light in lights)
        {
            light.enabled = state ?? !light.enabled;                                                        // 상태에 따라 라이트 켜거나 끄기
        }
    }

    void HandleBlinking(Light[] lights, bool isIndicatorOn, float t)
    {
        if (isIndicatorOn && t - lastBlinkTime >= blinkInterval)
        {
            lastBlinkTime = t;
            foreach (Light light in lights)
            {
                light.enabled = !light.enabled;                                                             // 라이트 상태 깜빡임
            }
        }
    }

    void SignBlinking(Text sign, bool isIndicatorOn, float t)
    {
        if (isIndicatorOn && t - lastBlinkTime >= blinkInterval)
        {
            lastBlinkTime = t;
            sign.enabled = !sign.enabled;                                                                   // 방향지시등 상태 깜빡임
        }
    }

    IEnumerator ReStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        yield return new WaitForSeconds(1f);
    }
    void ShiftGear()
    {
        rec = LogitechGSDK.LogiGetStateUnity(0);

        if (LogitechGSDK.LogiButtonIsPressed(0, 0))
        {
            gearInput = 0;
            GearSign.text = "R";
            GearSign.color = Color.yellow;
        }
        else if (LogitechGSDK.LogiButtonIsPressed(0, 1))
        {
            gearInput = 1;
            GearSign.text = "P";
            GearSign.color = Color.red;
        }
        else if (LogitechGSDK.LogiButtonIsPressed(0, 2))
        {
            gearInput = 2;
            GearSign.text = "N";
            GearSign.color = Color.yellow;
        }
        else if (LogitechGSDK.LogiButtonIsPressed(0, 3))
        {
            gearInput = 3;
            GearSign.text = "D";
            GearSign.color = Color.green;
        }
    }

    IEnumerator CheckTrafficLight()
    {
        yield return new WaitForSeconds(8f);
        if (checkingTrafficLight)
            StartCoroutine(ReStart());
    }

    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("TrafficLightArea"))
        {
            isRedLight = TrafficLight.GetComponent<TrafficLightController>().redLight;
            if (isRedLight)
            {
                checkingTrafficLight = true;
                //빨간불일때 현재 위치를 일정시간 안에 통과하지 못하면 게임오버
                StartCoroutine(CheckTrafficLight());
                Debug.Log("In TrafficLightArea" + "  current isRedLight: " + checkingTrafficLight + "  current Checking Traffic Light: " + checkingTrafficLight);
            }
        }
        if (col.CompareTag("LimitSpeedArea"))
        {
            float speed = SpeedCalaulator.GetComponent<SpeedCalculate>().speed;
            LimitSpeedImage.enabled = enabled;
            if (limitSpeed < speed)
            {
                StartCoroutine(ReStart());
            }
            Debug.Log("In LimitSpeedArea, Current Speed: " + speed);
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.CompareTag("TrafficLightArea"))
        {
            checkingTrafficLight = false;
            Debug.Log("In TrafficLightArea" + "  current isRedLight: " + checkingTrafficLight + "  current Checking Traffic Light: " + checkingTrafficLight);
        }
        if (col.CompareTag("LimitSpeedArea"))
        {
            LimitSpeedImage.enabled = !enabled;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Building"))
        {
            Debug.Log("Bumped Building");
            StartCoroutine(ReStart());
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
