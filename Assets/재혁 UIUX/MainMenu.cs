using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnClickGameStart()
    {
        SceneManager.LoadScene("GameSelect"); //게임 선택창으로 이동
        Debug.Log("변환 완료");
    }

    public void OnCLickQuit()  // 종료버튼 
    {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying= false;
#else 
    Application.Quit();
#endif 

    }
}
