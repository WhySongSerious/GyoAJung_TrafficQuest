using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    static string nextScene;

    [SerializeField]
    UnityEngine.UI.Image CurrentBar;

    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is invalid or null.");
            return;
        }
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    void Start()
    {
        if (CurrentBar == null)
        {
            Debug.LogError("CurrentBar is not assigned in the Inspector.");
            return;
        }

        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("nextScene is null or empty!");
            yield break;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;
        while (!op.isDone)
        {
            yield return null;

            if (op.progress < 0.9f)
            {
                CurrentBar.fillAmount = op.progress;
            }
            else
            {
                timer += Time.unscaledDeltaTime;
                CurrentBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer);

                if (CurrentBar.fillAmount >= 1f)
                {
                    yield return new WaitForSeconds(2f); // 2초 대기
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }
}
