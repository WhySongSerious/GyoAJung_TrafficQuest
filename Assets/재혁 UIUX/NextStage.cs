using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextStage : MonoBehaviour
{
    public Transform camera1; // 목표 카메라 위치와 회전
    public float transitionDuration = 0.5f; // 전환 애니메이션의 지속 시간

    public void OnClickGameOneStart()
    {
        SceneManager.LoadScene("SampleScene"); 
        Debug.Log("게임 변환 완료");
    }

    public void NextGame()
    {
        if (Camera.main != null && camera1 != null)
        {
            StartCoroutine(SmoothTransition());
        }
        else
        {
            Debug.LogWarning("Main Camera 또는 camera1이 설정되지 않았습니다.");
        }
    }

    private IEnumerator SmoothTransition()
    {
        Vector3 startPosition = Camera.main.transform.position;
        Quaternion startRotation = Camera.main.transform.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            // 위치와 회전을 Lerp를 사용해 점진적으로 목표로 이동
            Camera.main.transform.position = Vector3.Lerp(startPosition, camera1.position, elapsedTime / transitionDuration);
            Camera.main.transform.rotation = Quaternion.Lerp(startRotation, camera1.rotation, elapsedTime / transitionDuration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 최종 위치 및 회전 값 적용
        Camera.main.transform.position = camera1.position;
        Camera.main.transform.rotation = camera1.rotation;
    }
}