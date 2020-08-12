using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPointMirror : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int children = transform.childCount;
        for (int i = 0; i < children; ++i)
        {
            Transform child = transform.GetChild(i);
            GameObject c = Instantiate(child.gameObject);
            c.transform.SetParent(transform);
            c.transform.position = new Vector3(-child.position.x, child.position.y, child.position.z);
            c.name = child.name + "_mirror";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
