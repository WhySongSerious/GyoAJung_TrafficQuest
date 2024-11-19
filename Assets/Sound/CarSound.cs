using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
public Player player;

public AudioSource CarDriving;
public float maxVolume = 50f;
public float minVolume = 0f;
public float accelDepth;
public float currentVolume;
private float volumeSmoothTime = 0.3f; // 변화 속도 (클 수록 빠르게 반영됨)

public AudioClip CarClip;


 // Start is called before the first frame update
    void Start()
    {
        CarDriving = GetComponent<AudioSource>();
        CarDriving.clip = CarClip;
        CarDriving.loop = true; // 반복 재생 설정
        CarDriving.Play();      // 재생 시작

    }

    // Update is called once per frame
    void Update()
    {
        accelDepth = player.currentAccelerator;

        // accelDepth 값을 정규화하여 0과 1 사이로 조정
        float accelDepthNormalized = Mathf.InverseLerp(-32768f, 32767f, accelDepth);

        // 현재 음량을 점진적으로 변경
        float targetVolume = Mathf.Lerp(minVolume, maxVolume, accelDepthNormalized);

        // 부드럽게 음량을 변경
        currentVolume = Mathf.SmoothDamp(currentVolume, targetVolume, ref volumeSmoothTime, 0.1f);

        // 음량 적용
        CarDriving.volume = currentVolume;

        Debug.Log("현재 소리 :" + currentVolume);

        // 엑셀을 누르지 않았을 때
        if (player.currentAccelerator == 0)
        {
            CarDriving.Stop();
            Debug.Log("엑셀 안 눌림");
        }
    }
}

