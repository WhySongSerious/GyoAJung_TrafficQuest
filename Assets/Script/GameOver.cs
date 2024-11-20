using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class GameOver : MonoBehaviour
{
    [SerializeField] GameObject GameOverPanel;
    [SerializeField] Image TrafficLight;
    [SerializeField] Image Crash;

    static LogitechGSDK.DIJOYSTATE2ENGINES rec;

    public bool restartButtonPressed = false;

    private void Start()
    {
        GameOverPanel.SetActive(false);
        TrafficLight.enabled = false;
        Crash.enabled = false;
    }

    void Update()
    {
        restartButtonPressed = Player.instance.reStartButtonPressed;
    }
    public IEnumerator LimitGameOver()
    {
        GameOverPanel.SetActive(true);
        TrafficLight.enabled = true;
        StartCoroutine(GamePuase());
        yield return new WaitUntil(() => restartButtonPressed);
        StartCoroutine(ReStart());
    }
    public IEnumerator TrafficLightGameOver()
    {
        GameOverPanel.SetActive(true);
        TrafficLight.enabled = true;
        StartCoroutine(GamePuase());
        yield return new WaitUntil(() => restartButtonPressed);
        StartCoroutine(ReStart());
    }
    public IEnumerator CrashGameOver()
    {
        GameOverPanel.SetActive(true);
        Crash.enabled = true;
        StartCoroutine(GamePuase());
        yield return new WaitUntil(() => restartButtonPressed);
        StartCoroutine(ReStart());
    }

    IEnumerator ReStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
        yield return null;
    }

    IEnumerator GamePuase()
    {
        Time.timeScale = 0f;
        yield return null;
    }
}
