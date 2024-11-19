using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource bgm; // 인스펙터에서 직접 할당
    public AudioClip bgmClip; // 배경 음악 클립을 할당할 변수

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
        // 현재 씬이 "Stage1"이면 배경 음악을 멈춤
        if (SceneManager.GetActiveScene().name == "Stage1" && bgm.isPlaying)
        {
            bgm.Stop();
        }
    }

    void Start()
    {
        // bgm이 null이 아니라면 배경음악을 재생
        if (bgm != null)
        {
            if (bgmClip != null)
            {
                bgm.clip = bgmClip; // 클립 할당
                bgm.loop = true; // 배경음악 반복 재생
                bgm.Play(); // 배경음악 시작
            }
            else
            {
                Debug.LogWarning("BGM 클립이 할당되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning("AudioSource가 할당되지 않았습니다.");
        }
    }
}
