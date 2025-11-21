using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    #region Editor Fields
    #endregion

    #region Properties
    #endregion

    #region Methods
    public void LoadLevel(string levelName)
    {
        Time.timeScale = 1.0f;
        StartCoroutine(LoadLevelAsync(levelName));
    }

    public void RestartLevel(string levelName)
    {
        AudioManager.Instance.RestartMusic();
        Time.timeScale = 1.0f;
        StartCoroutine(LoadLevelAsync(levelName));
    }

    private IEnumerator LoadLevelAsync(string levelName)
    {
        AsyncOperation loadLevel = SceneManager.LoadSceneAsync(levelName);
        yield return null;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
}
