
using UnityEngine.SceneManagement;

public class SceneLoadMgr : SingletonMono<SceneLoadMgr>
{
    public string NextScene;
    
    public void Load(string scene)
    {
        NextScene = scene;
        SceneManager.LoadScene("WaitScene");
    }
}