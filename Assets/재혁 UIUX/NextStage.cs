using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextStage : MonoBehaviour
{
    public Transform Camera1;
    public Transform Camera2;
    public Transform Camera0;
    public float transitionDuration = 0.3f; // 전환 애니메이션의 지속 시간

    public void OnClickGameOneStart()
    {
        LoadingSceneController.LoadScene("Stage1");
        Debug.Log("게임 변환 완료");
    }

    public void OnClickGameTwoStart()
    {
        SceneManager.LoadScene("Stage2");
        Debug.Log("게임 변환 완료");
    }

    public void OnClickGameThreeStart()
    {
        SceneManager.LoadScene("Stage3");
        Debug.Log("게임 변환 완료");
    }

    // Camera0에서 Camera1로 이동
    public void ZeroToOne()
    {
        if (Camera0 != null && Camera1 != null)
        {
            Debug.Log("됐는데 왜!!!");
            StartCoroutine(SmoothTransitionTo(Camera1));
        }
        else
        {
            Debug.LogWarning("Camera0 또는 Camera1이 설정되지 않았습니다.");
        }
    }

    // Camera1에서 Camera2로 이동
    public void OneToTwo()
    {
        if (Camera1 != null && Camera2 != null)
        {
            Debug.Log("원투투");
            StartCoroutine(SmoothTransitionTo(Camera2));
        }
        else
        {
            Debug.LogWarning("Camera1 또는 Camera2가 설정되지 않았습니다.");
        }
    }

    // Camera1에서 Camera0로 이동
    public void OneToZero()
    {
        if (Camera1 != null && Camera0 != null)
        {
            StartCoroutine(SmoothTransitionTo(Camera0));
        }
        else
        {
            Debug.LogWarning("Camera1 또는 Camera0이 설정되지 않았습니다.");
        }
    }

    private IEnumerator SmoothTransitionTo(Transform targetCamera)
    {
        Vector3 startPosition = Camera.main.transform.position;
        Quaternion startRotation = Camera.main.transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            Camera.main.transform.position = Vector3.Lerp(startPosition, targetCamera.position, elapsedTime / transitionDuration);
            Camera.main.transform.rotation = Quaternion.Lerp(startRotation, targetCamera.rotation, elapsedTime / transitionDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 위치 및 회전 값 적용
        Camera.main.transform.position = targetCamera.position;
        Camera.main.transform.rotation = targetCamera.rotation;
    }
}
