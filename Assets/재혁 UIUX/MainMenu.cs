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

<<<<<<< Updated upstream
    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickNewGame()
    {
        Debug.Log("새 게임")
    }

    public void OnClickLoad()
    {
        Debug.Log("불러오기")
    }

    public void OnClickOption()
    {
        Debug.Log("옵션")
    }

    public void OnclickQuit()
=======
    public void OnCLickQuit()  // 종료버튼 
>>>>>>> Stashed changes
    {
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying= false;
#else 
    Application.Quit();
#endif 

    }
}
