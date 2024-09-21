using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void SwitchToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log("Switching to scene: " + sceneName);
        Time.timeScale = 1;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
