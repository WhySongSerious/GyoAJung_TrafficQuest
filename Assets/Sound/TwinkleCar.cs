using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwinkleCar : MonoBehaviour
{
    public AudioSource hamster;
    public AudioClip Twinkle;

    private float lastIndicatorChangeTime = -1f;        
    private float changeDelay = 0.5f;
    private float t;
    private bool isLeftPlaying = false;
    private bool isRightPlaying = false;

    static LogitechGSDK.DIJOYSTATE2ENGINES rec;

    void Start()
    {
        hamster = gameObject.AddComponent<AudioSource>();
        hamster.clip = Twinkle;
    }

    // Update is called once per frame
    void Update()
    {
        t = Time.time;
        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            rec = LogitechGSDK.LogiGetStateUnity(0);

            if (LogitechGSDK.LogiButtonIsPressed(0, 4) && t - lastIndicatorChangeTime >= changeDelay)
            {
                if (isLeftPlaying && !isRightPlaying)
                {
                    hamster.Stop();
                    isLeftPlaying = false;
                    lastIndicatorChangeTime = t;
                }
                else
                {
                    isRightPlaying = false;
                    isLeftPlaying = true;
                    hamster.Play();
                    lastIndicatorChangeTime = t;
                    Debug.Log("±ô");
                }

            }

            if (LogitechGSDK.LogiButtonIsPressed(0, 5) && t - lastIndicatorChangeTime >= changeDelay)
            {
                if (isRightPlaying && !isLeftPlaying)
                {
                    hamster.Stop();
                    isRightPlaying = false;
                    lastIndicatorChangeTime = t;
                }
                else
                {
                    isLeftPlaying = false;
                    isRightPlaying = true;
                    hamster.Play();
                    lastIndicatorChangeTime = t;
                    Debug.Log("±ô");
                }
            }
        }
    }
}
