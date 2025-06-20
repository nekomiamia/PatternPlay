
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ABMgr: SingletonMono<ABMgr>
{

    // 主包
    private AssetBundle mainAB = null;
    // 配置信息 依赖信息
    private AssetBundleManifest manifest = null;
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    /// <summary>
    /// AB包存放路径
    /// </summary>
    private string PathURL
    {
        get
        {
            return Application.streamingAssetsPath + "/";
        }
    }
    
    private string GetFullPath(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    return "jar:file://" + path;
#elif UNITY_IOS && !UNITY_EDITOR
    return "file://" + path;
#else
        return "file://" + path;
#endif
    }

    
    /// <summary>
    /// 主包名称
    /// </summary>
    private string MainABName
    {
        get
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "PC";
#endif
        }
    }

    /// <summary>
    /// 加载AB包
    /// </summary>
    /// <param name="abName"></param>
    public void LoadAB(string abName)
    {
        // 加载主包
        if (mainAB == null)
        {
            mainAB = AssetBundle.LoadFromFile(PathURL + MainABName);
            manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
        // 加载主包中的关键配置信息 获取依赖包
        string[] strs = manifest.GetAllDependencies(abName);
        
        AssetBundle ab = null;
        for (int i = 0; i < strs.Length; i++)
        {
            if (!abDic.ContainsKey(strs[i]))
            {
                ab = AssetBundle.LoadFromFile(PathURL + strs[i]);
                abDic.Add(strs[i], ab);
            }
        }
        
        // 加载目标包
        if (!abDic.ContainsKey(abName))
        {
            ab = AssetBundle.LoadFromFile(PathURL + abName);
            abDic.Add(abName, ab);
        }
    }
    
    // 异步加载AB包和依赖信息
    private IEnumerator LoadABAsync(string abName, UnityAction onComplete = null)
    {
        if (mainAB == null)
        {
            string mainPath = GetFullPath(PathURL + MainABName);
            var mainRequest = UnityWebRequestAssetBundle.GetAssetBundle(mainPath);
            yield return mainRequest.SendWebRequest();

            if (mainRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("主包加载失败：" + mainRequest.error);
                yield break;
            }

            mainAB = DownloadHandlerAssetBundle.GetContent(mainRequest);
            manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        // 加载依赖包
        string[] dependencies = manifest.GetAllDependencies(abName);
        foreach (var dep in dependencies)
        {
            if (!abDic.ContainsKey(dep))
            {
                string depPath = GetFullPath(PathURL + dep);
                var depRequest = UnityWebRequestAssetBundle.GetAssetBundle(depPath);
                yield return depRequest.SendWebRequest();

                if (depRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"依赖包加载失败: {dep} - {depRequest.error}");
                    continue;
                }

                AssetBundle depAB = DownloadHandlerAssetBundle.GetContent(depRequest);
                abDic.Add(dep, depAB);
            }
        }

        // 加载目标包
        if (!abDic.ContainsKey(abName))
        {
            string abPath = GetFullPath(PathURL + abName);
            var abRequest = UnityWebRequestAssetBundle.GetAssetBundle(abPath);
            yield return abRequest.SendWebRequest();

            if (abRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("目标包加载失败：" + abRequest.error);
                yield break;
            }

            AssetBundle ab = DownloadHandlerAssetBundle.GetContent(abRequest);
            abDic.Add(abName, ab);
        }

        onComplete?.Invoke();
    }

    
    // 同步加载，不指定类型
    public Object LoadRes(string abName, string resName)
    {
        LoadAB(abName);

        Object obj = abDic[abName].LoadAsset(resName);
        if (obj is GameObject)
            return Instantiate(obj);
        else
            return obj;
    }

    // 同步加载，指定类型
    public Object LoadRes(string abName, string resName, System.Type type)
    {
        LoadAB(abName);

        Object obj = abDic[abName].LoadAsset(resName, type);
        if (obj is GameObject)
            return Instantiate(obj);
        else
            return obj;
    }

    // 同步加载，泛型
    public T LoadRes<T>(string abName, string resName) where T : Object
    {
        LoadAB(abName);

        T obj = abDic[abName].LoadAsset<T>(resName);
        if (obj is GameObject)
            return Instantiate(obj);
        else
            return obj;
    }
    
    
    // 异步加载
    // AB包的加载并没有使用异步
    // 只是从AB包中加载资源使用异步


    /// <summary>
    /// 根据名字异步加载资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public void LoadResAsync(string abName, string resName, UnityAction<Object> callback)
    {
        StartCoroutine(LoadABAsync(abName, () =>
        {
            StartCoroutine(ReallyLoadResAsync(abName, resName, callback));
        }));
    }
    private IEnumerator ReallyLoadResAsync(string abName, string resName, UnityAction<Object> callback)
    {
        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName);
        yield return abr;
        // 异步加载结束后 通过委托传递给外部
        if (abr.asset is GameObject)
            callback(Instantiate(abr.asset));
        else
            callback(abr.asset);
    }
    
    /// <summary>
    /// 根据Type异步加载资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callback)
    {
        StartCoroutine(LoadABAsync(abName, () =>
        {
            StartCoroutine(ReallyLoadResAsync(abName, resName, type, callback));
        }));
    }
    private IEnumerator ReallyLoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callback)
    {
        LoadAB(abName);

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync(resName, type);
        yield return abr;
        // 异步加载结束后 通过委托传递给外部
        if (abr.asset is GameObject)
            callback(Instantiate(abr.asset));
        else
            callback(abr.asset);
    }
    
    /// <summary>
    /// 根据泛型异步加载资源
    /// </summary>
    /// <param name="abName"></param>
    /// <param name="resName"></param>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callback) where T: Object
    {
        StartCoroutine(LoadABAsync(abName, () =>
        {
            StartCoroutine(ReallyLoadResAsync<T>(abName, resName, callback));
        }));
    }
    private IEnumerator ReallyLoadResAsync<T>(string abName, string resName, UnityAction<T> callback) where T: Object
    {
        LoadAB(abName);

        AssetBundleRequest abr = abDic[abName].LoadAssetAsync<T>(resName);
        yield return abr;
        // 异步加载结束后 通过委托传递给外部
        if (abr.asset is GameObject)
            callback(Instantiate(abr.asset) as T);
        else
            callback(abr.asset as T);
    }
    
    
    // 单个包卸载
    public void UnLoad(string abName)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(false);
            abDic.Remove(abName);
        }
    }
    
    
    // 卸载所有包
    public void ClearAB()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        abDic.Clear();
        mainAB = null;
        manifest = null;
    }
}