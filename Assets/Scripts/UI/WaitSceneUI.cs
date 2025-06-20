using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaitSceneUI : MonoBehaviour
{
    public Slider slider;
    public TextMeshProUGUI text;
    
    private void Start()
    {
        slider.value = 0;
        text.text = "Loading...";
        ABMgr.GetInstance().LoadAB("scene");
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        string nextScene = SceneLoadMgr.GetInstance().NextScene;
        
        AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(nextScene);
        asyncOperation.allowSceneActivation = false;

        while (asyncOperation.progress < 0.9f)
        {
            slider.value = asyncOperation.progress / 0.9f; // progress is between 0 and 0.9
            text.text = $"Loading... {slider.value * 100:F2}%";
            yield return null;
        }

        // 加载完成，允许场景激活
        slider.value = 1f;
        text.text = "Loading Complete!";
        yield return new WaitForSeconds(1f);
        asyncOperation.allowSceneActivation = true;

    }
}
