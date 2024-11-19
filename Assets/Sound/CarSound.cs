using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
public Player player;

    public AudioSource CarDriving;
    public AudioSource CarWaiting;
    public float maxVolume = 1f;
    public float minVolume = 0.7f;
    public float accelDepth;
    public float currentVolume = 0f;
    private float volumeSmoothTime = 0.3f; // 변화 속도 (클 수록 빠르게 반영됨)

    public AudioClip CarClip;
    public AudioClip CarWaitClip;
    public GameObject Speed;

    // Start is called before the first frame update
    void Start()
    {
        // CarDriving과 CarWaiting에 AudioSource가 할당되었는지 확인
        CarDriving = gameObject.AddComponent<AudioSource>();
        CarWaiting = gameObject.AddComponent<AudioSource>();

        CarDriving.clip = CarClip;
        CarDriving.loop = true; // 반복 재생 설정
        CarDriving.Play();      // 재생 시작

        // CarWaiting 설정
        CarWaiting.clip = CarWaitClip;
        CarWaiting.loop = true;
    }

    // Update is called once per frame
    void Update()
    {

        float speed = Speed.GetComponent<SpeedCalculate>().speed;
        accelDepth = player.currentAccelerator;

        if (accelDepth > 1)
        {
            // accelDepth 값을 정규화하여 0과 1 사이로 조정
            float accelDepthNormalized = Mathf.InverseLerp(40, 100, accelDepth);

            // 현재 음량을 점진적으로 변경
            float targetVolume = Mathf.Lerp(minVolume, maxVolume, accelDepthNormalized);

            // 부드럽게 음량을 변경
            currentVolume = Mathf.SmoothDamp(currentVolume, targetVolume, ref volumeSmoothTime, 0.1f);

            // 음량 적용
            CarDriving.volume = currentVolume;
            Debug.Log("현재 소리 :" + currentVolume);

            if (CarWaiting.isPlaying)
            {
                CarWaiting.Stop();
                Debug.Log("엑셀 안 눌림");
            }
        }
        else
        {
            CarDriving.volume = minVolume; // 대기 중일 때 최소 볼륨으로 설정

            // 이미 재생 중인지 확인
            if (!CarWaiting.isPlaying)
            {
                CarWaiting.Play();
                Debug.Log("엑셀 안 눌림");
            }
        }
    }
}


