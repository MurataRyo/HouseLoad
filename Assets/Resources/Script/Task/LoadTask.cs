using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadTask : MonoBehaviour
{
    public enum SceneName
    {
        Title,
        Main
    }

    public static SceneName nextScene = SceneName.Main;

    AsyncOperation loadScene;      //ロード先
    const float LoadTimeMin = 1f;  //最低のロード時間

    public static void NextScene(SceneName sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("Load");
    }

    // Start is called before the first frame update
    void Start()
    {
        loadScene = SceneManager.LoadSceneAsync(nextScene.ToString());
        loadScene.allowSceneActivation = false;
        StartCoroutine(load());
    }

    IEnumerator load()
    {
        yield return new WaitForSeconds(LoadTimeMin);
        loadScene.allowSceneActivation = true;
        yield break;
    }
}
