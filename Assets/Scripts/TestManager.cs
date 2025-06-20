using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject obj = ABMgr.GetInstance().LoadRes<GameObject>("model", "Sphere");
        obj.transform.position = Vector3.up;
        
        ABMgr.GetInstance().LoadResAsync<GameObject>("model", "Sphere", (obj) =>
        {
            (obj as GameObject).transform.position = Vector3.down;
        });
        
        ABMgr.GetInstance().LoadResAsync("model", "Sphere", (obj) =>
        {
            (obj as GameObject).transform.position = Vector3.right;
        });
        
        ABMgr.GetInstance().LoadResAsync("model", "Sphere", typeof(GameObject), (obj) =>
        {
            (obj as GameObject).transform.position = Vector3.left;
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            SceneLoadMgr.GetInstance().Load("FlightScene");
        }
    }
}
