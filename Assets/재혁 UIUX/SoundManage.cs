using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource bgm; // 인스펙터에서 직접 할당

    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬을 이동해도 오브젝트가 파괴되지 않음
        }
        else
        {
            Destroy(gameObject); // 이미 존재하면 새로 생성된 오브젝트 파괴
        }
    }

    void Update()
    {
        // 현재 씬이 "SampleScene"이면 배경 음악을 멈춤
        if (SceneManager.GetActiveScene().name == "SampleScene" && bgm.isPlaying)
        {
            bgm.Stop();
        }
    }

    void Start()
    {
        if (bgm != null)
        {
            bgm.Play();
        }
        else
        {
            Debug.LogWarning("AudioSource가 할당되지 않았습니다.");
        }
    }
}
