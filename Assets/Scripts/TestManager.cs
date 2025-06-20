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
    }
    
}
