using System.Collections;
using System.Text;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public enum InputCondition
{
    logitech_wheel = 0,
    keyboard = 1
};

public class Player : MonoBehaviour
{
    public static Player instance;
    public EffectControlInfo effectinfo;

    [Header("Wheel Colliders")]
    [SerializeField] WheelCollider frontRight;
    [SerializeField] WheelCollider frontLeft;
    [SerializeField] WheelCollider rearRight;
    [SerializeField] WheelCollider rearLeft;

    [Header("Wheel Transforms")]
    [SerializeField] Transform frontRightTransform;
    [SerializeField] Transform frontLeftTransform;
    [SerializeField] Transform rearRightTransform;
    [SerializeField] Transform rearLeftTransform;

    [Header("Text UI")]
    [SerializeField] Text rightSign;
    [SerializeField] Text leftSign;
    [SerializeField] Text GearSign;

    [Header("Traffic System")]
    [SerializeField] GameObject TrafficLight;
    private bool isRedLight;
    private bool checkingTrafficLight = false;
    private bool checkingLimit = false;
    private bool checkingFinish = false;

    [Header("Speed")]
    [SerializeField] GameObject SpeedCalculator;
    [SerializeField] Image LimitSpeedImage;

    [SerializeField] GameOver gameOver;

    //�ӵ����� ����
    public float accelerator;                                                             
    private float reverseForce;
    private float brakeForce;                                                               
    public float currentAccelerator = 0f;                                                    
    private float currentReverseForce = 0f;
    private float currentBrakeForce = 0f;   
    private float currentTurnAngle = 0f;
    private float maxTurnAngle = 30f;

    private float basicResistance = 1f;

    [Header("Anti-Roll")]
    private float antiRollForce = 5000f;                                                           

    [Header("Handle Resistance")]
    private int handleResistance = 30;                                                             

    [Header("Gear")]
    private int gearInput = 1;

    [Header("Limit Speed")]
    private float limitSpeed = 50;

    [Header("Input Condition")]
    public InputCondition inputcondition;                                                   

    private float t;                                                                       

    private Vector3 initialVelocity = Vector3.zero;                                                

    static LogitechGSDK.DIJOYSTATE2ENGINES rec;                                                     

    [Header("Light")]
    //private bool isFrontIndicatorOn = false;                                                      
    private float blinkInterval = 0.52f;                                                             
    public bool isLeftIndicatorOn = false;                                                         
    public bool isRightIndicatorOn = false;
    private bool isEmergencyOn = false;
    private float lastBlinkTime;                                                                  
    private float lastIndicatorChangeTime = -1f;                                                
    private float changeDelay = 0.5f;                                                              

    public bool reStartButtonPressed = false; 

    private Rigidbody rb;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        gameOver = GetComponent<GameOver>();
    }

    void Start()
    {
        Debug.Log("SteeringInit:" + LogitechGSDK.LogiSteeringInitialize(false));
        LimitSpeedImage.enabled = !enabled;
        leftSign.enabled = !enabled;
        rightSign.enabled = !enabled;
        checkingFinish = false;
        ToggleLights(effectinfo.backLight, false);
        LogitechGSDK.LogiPlayDamperForce(0, handleResistance);
        if (Player.instance == null)
        {
            Player.instance = this;
        }
    }

    //������ �� wheel ������ ���� �ִ��� üũ
    void OnApplicationQuit()
    {
        Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());                       
    }
    private void Update()
    {
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    if (LogitechGSDK.LogiButtonIsPressed(0, 4) && t - lastIndicatorChangeTime >= changeDelay)
                    {
                        reStartButtonPressed = true;
                    }
                }
                break;
            case InputCondition.keyboard:
                if(Input.GetKey(KeyCode.R))
                    reStartButtonPressed = true;
                break;
        }
    }
    void FixedUpdate()
    {
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))                       
                {
                    ShiftGear();
                    Reverse();
                    Accel();                                                                         
                    Brake();                                                                        
                    WheelControl();                                                                   
                    LightControl();                                                                   
                    ApplyAntiRoll();                                                          
                }
                else if (!LogitechGSDK.LogiIsConnected(0))
                {
                    Debug.Log("PLEASE PLUG IN A STEERING WHEEL OR A FORCE FEEDBACK CONTROLLER");
                }
                else
                {
                    Debug.Log("THIS WINDOW NEEDS TO BE IN FOREGROUND IN ORDER FOR THE SDK TO WORK PROPERLY");
                }
                break;
            case InputCondition.keyboard:
                ShiftGear();
                Reverse();
                Accel();                                                                                
                Brake();                                                                                
                WheelControl();                                                                         
                LightControl();                                                                         
                ApplyAntiRoll();
                break;
        }
        
        }

    public void Accel()
    {
        t = Time.deltaTime;
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:                                                   
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    accelerator = Mathf.Abs(rec.lY - 32767) / 5000;                                  
                    currentAccelerator += accelerator * t;
                    //Debug.Log("Logitech Accel Force: " + currentAccelerator);

                }
                break;
            case InputCondition.keyboard:
                //Debug.Log("keyboard");
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    accelerator = 6;
                    currentAccelerator += accelerator * t;
                    //Debug.Log("����ӵ�: " + initialVelocity.z + ", ���ӵ�: " + accelerator);
                }
                break;
        }
    }

    public void Reverse()
    {
        t = Time.deltaTime;
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:                                               
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    reverseForce = Mathf.Abs(rec.lY - 32767) / 5000;                             
                    currentReverseForce += reverseForce * t;
                    Debug.Log("Logitech Accel Force: " + currentReverseForce);

                }
                break;
            case InputCondition.keyboard:
                //Debug.Log("keyboard");
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    reverseForce = 6;
                    currentReverseForce += reverseForce * t;
                    //Debug.Log("����ӵ�: " + initialVelocity.z + ", ���ӵ�: " + reverseForce);
                }
                break;
        }
    }

    public void Brake()
    {
        t = Time.deltaTime;
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);
                    brakeForce = Mathf.Abs(rec.lRz - 32767) * 100;                             
                    //Debug.Log("Logitech Brake Force: " + brakeForce / 100);
                    if(brakeForce > 1)
                    {
                        if(currentAccelerator != 0 || currentReverseForce != 0)
                        {
                            currentAccelerator = -brakeForce;
                            currentReverseForce = -brakeForce;
                        }
                        else
                        {
                            currentAccelerator = 0;
                            currentReverseForce = 0;
                        }
                    }
                    currentBrakeForce = brakeForce;
                    //Debug.Log("����ӵ�: " + initialVelocity.z + ", ���ӵ�: " + brakeForce);
                }
                break;
            case InputCondition.keyboard:
                if (Input.GetKey(KeyCode.Space))
                {
                    brakeForce += 60;
                    currentAccelerator = 0;
                    currentReverseForce = 0;
                }
                else
                    brakeForce = 0;

                currentBrakeForce = brakeForce;
                break;
        }
    }


    void WheelControl()
    {
        float speed = SpeedCalculator.GetComponent<SpeedCalculate>().speed;
        switch (gearInput)
        {
            case 0:                                                    
                if (reverseForce < 1)
                {
                    if (speed > 20)
                    {
                        currentReverseForce = -basicResistance;
                    }
                    else if (speed <= 20)
                    {
                        currentReverseForce = 40;
                    }
                }
                frontRight.motorTorque = -currentReverseForce;
                frontLeft.motorTorque = -currentReverseForce;
                Debug.Log("motorTorque Reverse Force: " + currentReverseForce);
                break;

            case 1:                                                          
                frontRight.motorTorque = 0;
                frontLeft.motorTorque = 0;
                break;

            case 2:                                                      
                frontRight.motorTorque = 0;
                frontLeft.motorTorque = 0;
                break;

            case 3:                                                      
                if (accelerator < 1)
                {
                    if (brakeForce < 1)
                    {
                        if (speed > 20)
                        {
                            currentAccelerator = -basicResistance;
                        }
                        else if (speed <= 20)
                        {
                            currentAccelerator = 40;
                        }
                    }
                }

                frontRight.motorTorque = currentAccelerator;                                               
                frontLeft.motorTorque = currentAccelerator;
                Debug.Log("motorTorque Accel Force: " + accelerator);
                break;

        }

        frontRight.brakeTorque = currentBrakeForce;                                         
        frontLeft.brakeTorque = currentBrakeForce;
        rearRight.brakeTorque = currentBrakeForce;
        rearLeft.brakeTorque = currentBrakeForce;

        LogitechGSDK.LogiPlayDamperForce(0, handleResistance);
        switch (inputcondition) {
            case InputCondition.logitech_wheel:
                currentTurnAngle = maxTurnAngle * rec.lX / 32767;
                break;
            case InputCondition.keyboard:
                currentTurnAngle = 15 * Input.GetAxis("Horizontal");
                break;
        }



        frontLeft.steerAngle = currentTurnAngle;
        frontRight.steerAngle = currentTurnAngle;

        UpdateWheelVisual(frontRightTransform, frontRight);                                      
        UpdateWheelVisual(frontLeftTransform, frontLeft);
        UpdateWheelVisual(rearRightTransform, rearRight);
        UpdateWheelVisual(rearLeftTransform, rearLeft);
    }


    void UpdateWheelVisual(Transform trans, WheelCollider wheelCol)                                  
    {
        Vector3 UpdatePos;
        Quaternion UpdateRot;

        wheelCol.GetWorldPose(out UpdatePos, out UpdateRot);                                           

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
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:
                if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
                {
                    rec = LogitechGSDK.LogiGetStateUnity(0);

                    if (LogitechGSDK.LogiButtonIsPressed(0, 4) && t - lastIndicatorChangeTime >= changeDelay)
                    {
                        isRightIndicatorOn = !isRightIndicatorOn;
                        rightSign.enabled = isRightIndicatorOn;
                        if (isRightIndicatorOn)
                        {
                            isLeftIndicatorOn = false;
                            leftSign.enabled = false;
                            ToggleLights(effectinfo.leftLight, false);
                        }
                        ToggleLights(effectinfo.rightLight, isRightIndicatorOn);
                        lastIndicatorChangeTime = t;
                    }

                    if (LogitechGSDK.LogiButtonIsPressed(0, 5) && t - lastIndicatorChangeTime >= changeDelay)
                    {
                        isLeftIndicatorOn = !isLeftIndicatorOn;
                        leftSign.enabled = isLeftIndicatorOn;
                        if (isLeftIndicatorOn)
                        {
                            isRightIndicatorOn = false;
                            rightSign.enabled = false;
                            ToggleLights(effectinfo.rightLight, false);
                        }
                        ToggleLights(effectinfo.leftLight, isLeftIndicatorOn);
                        lastIndicatorChangeTime = t;
                    }

                    if (LogitechGSDK.LogiButtonIsPressed(0, 6) && t - lastIndicatorChangeTime >= changeDelay)
                    {
                        isEmergencyOn = true;
                        isLeftIndicatorOn = !isEmergencyOn;
                        isRightIndicatorOn = !isEmergencyOn;
                        if (isLeftIndicatorOn || isRightIndicatorOn)
                        {
                            rightSign.enabled = false;
                            leftSign.enabled = false;
                            ToggleLights(effectinfo.rightLight, false);
                            ToggleLights(effectinfo.rightLight, false);
                        }
                        ToggleLights(effectinfo.leftLight, isEmergencyOn);
                        ToggleLights(effectinfo.rightLight, isEmergencyOn);

                        lastIndicatorChangeTime = t;
                    }
                    /*

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
                            ToggleLights(effectinfo.frontLight, true); // ���� ����Ʈ �ѱ�
                        }

                    }
                    */

                    if (rec.lRz < 32766 || reverseForce > 1)
                    {
                        ToggleLights(effectinfo.backLight, true);
                    }
                    else
                    {
                        ToggleLights(effectinfo.backLight, false);
                    }
                }
                break;
            case InputCondition.keyboard:
                if (Input.GetKey(KeyCode.M) && t - lastIndicatorChangeTime >= changeDelay)
                {
                    isRightIndicatorOn = !isRightIndicatorOn;
                    rightSign.enabled = isRightIndicatorOn;
                    if (isRightIndicatorOn)
                    {
                        isLeftIndicatorOn = false;
                        leftSign.enabled = false;
                        ToggleLights(effectinfo.leftLight, false);
                    }
                    ToggleLights(effectinfo.rightLight, isRightIndicatorOn);
                    lastIndicatorChangeTime = t;
                }

                if (Input.GetKey(KeyCode.N) && t - lastIndicatorChangeTime >= changeDelay)
                {
                    isLeftIndicatorOn = !isLeftIndicatorOn;
                    leftSign.enabled = isLeftIndicatorOn;
                    if (isLeftIndicatorOn)
                    {
                        isRightIndicatorOn = false;
                        rightSign.enabled = false;
                        ToggleLights(effectinfo.rightLight, false);
                    }
                    ToggleLights(effectinfo.leftLight, isLeftIndicatorOn);
                    lastIndicatorChangeTime = t;
                }

                if (Input.GetKey(KeyCode.B) && t - lastIndicatorChangeTime >= changeDelay)
                {
                    isEmergencyOn = !isEmergencyOn; // Toggle emergency state

                    // Set both indicators to the state of the emergency
                    isLeftIndicatorOn = isEmergencyOn;
                    isRightIndicatorOn = isEmergencyOn;

                    // Enable or disable the signs
                    leftSign.enabled = isEmergencyOn;
                    rightSign.enabled = isEmergencyOn;

                    // Control the lights based on emergency state
                    ToggleLights(effectinfo.rightLight, isEmergencyOn);
                    ToggleLights(effectinfo.leftLight, isEmergencyOn);

                    lastIndicatorChangeTime = t;
                }
                break;
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
            light.enabled = state ?? !light.enabled;                                              
        }
    }

    void HandleBlinking(Light[] lights, bool isIndicatorOn, float t)
    {
        if (isIndicatorOn && t - lastBlinkTime >= blinkInterval)
        {
            lastBlinkTime = t;
            foreach (Light light in lights)
            {
                light.enabled = !light.enabled;         
            }
        }
    }

    void SignBlinking(Text sign, bool isIndicatorOn, float t)
    {
        if (isIndicatorOn && t - lastBlinkTime >= blinkInterval)
        {
            //lastBlinkTime = t;
            sign.enabled = !sign.enabled;                
        }
    }

    void ShiftGear()
    {
        float speed = SpeedCalculator.GetComponent<SpeedCalculate>().speed;
        switch (inputcondition)
        {
            case InputCondition.logitech_wheel:
                rec = LogitechGSDK.LogiGetStateUnity(0);
                if (speed <= 1)
                {
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
                break;
            case InputCondition.keyboard:
                if (speed <= 1)
                {
                    if (Input.GetKey(KeyCode.P))
                    {
                        gearInput = 0;
                        GearSign.text = "R";
                        GearSign.color = Color.yellow;
                    }
                    else if (Input.GetKey(KeyCode.O))
                    {
                        gearInput = 1;
                        GearSign.text = "P";
                        GearSign.color = Color.red;
                    }
                    else if (Input.GetKey(KeyCode.I))
                    {
                        gearInput = 2;
                        GearSign.text = "N";
                        GearSign.color = Color.yellow;
                    }
                    else if (Input.GetKey(KeyCode.U))
                    {
                        gearInput = 3;
                        GearSign.text = "D";
                        GearSign.color = Color.green;
                    }
                }
                break;
        }
    }

    IEnumerator CheckTrafficLight()
    {
        yield return new WaitForSeconds(8f);
        if (checkingTrafficLight)
            StartCoroutine(gameOver.TrafficLightGameOver());
        Debug.Log("checkingTrafficLimit:  " + reStartButtonPressed);
    }

    IEnumerator CheckingLimitSpeed()
    {
        yield return new WaitForSeconds(8f);
        if (checkingLimit)
            StartCoroutine(gameOver.LimitGameOver());
    }

    IEnumerator FinishGame()
    {
        yield return new WaitForSeconds(3f);
        if(checkingFinish)
            SceneManager.LoadScene("GameSelect");
    }

    void OnTriggerStay(Collider col)
    {
        if (col.CompareTag("TrafficLightArea"))
        {
            isRedLight = TrafficLight.GetComponent<TrafficLightController>().redLight;
            if (isRedLight)
            {
                checkingTrafficLight = true;
                StartCoroutine(CheckTrafficLight());
                //Debug.Log("In TrafficLightArea" + "  current isRedLight: " + checkingTrafficLight + "  current Checking Traffic Light: " + checkingTrafficLight);
            }
        }
        if (col.CompareTag("LimitSpeedArea"))
        {
            float speed = SpeedCalculator.GetComponent<SpeedCalculate>().speed;
            LimitSpeedImage.enabled = true;
            if (limitSpeed + 9 < speed)
            {
                checkingLimit = true;
                StartCoroutine(CheckingLimitSpeed());
            }
            else
                checkingLimit = false;
            Debug.Log("In LimitSpeedArea, Current Speed: " + speed);
        }
        if (col.CompareTag("Finish"))
        {
            checkingFinish = true;
            //Debug.Log(checkingFinish);
            if (gearInput == 1)
            {
                StartCoroutine(FinishGame());
            }
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
            checkingLimit = false;
            LimitSpeedImage.enabled = false;
        }
        if (col.CompareTag("Finish"))
        {
            checkingFinish = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Building"))
        {
            StartCoroutine (gameOver.CrashGameOver());
            Debug.Log("Crash Building");
        }
    }

    [System.Serializable]
    public struct EffectControlInfo
    {
        [Header("Light")]
        public Light[] frontLight;
        public Light[] backLight;
        public Light[] leftLight;
        public Light[] rightLight;
    }
}
