﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundLoop : MonoBehaviour
{
    public GameObject[] levels;
    public Camera mainCamera;
    private Vector2 screenBounds;
    public float[] choke;
    public float[] scrollSpeed;

    void Start()
    {
        screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
        for (int i = 0; i < levels.Length; i++)
        {
            loadChildObjects(i);
        }
    }
    void loadChildObjects(int obj)
    {
        float objectWidth = levels[obj].GetComponent<SpriteRenderer>().bounds.size.x;
        int childsNeeded = 4;
        GameObject clone = Instantiate(levels[obj]) as GameObject;
        for (int i = 0; i <= childsNeeded; i++)
        {
            GameObject c = Instantiate(clone) as GameObject;
            c.transform.SetParent(levels[obj].transform);
            c.transform.position = new Vector3(levels[obj].transform.position.x + choke[obj] * i, levels[obj].transform.position.y, levels[obj].transform.position.z);
            c.name = levels[obj].name + i;
        }
        Destroy(clone);
        Destroy(levels[obj].GetComponent<SpriteRenderer>());
    }
    void repositionChildObjects(int obj)
    {
        Transform[] children = levels[obj].GetComponentsInChildren<Transform>();
        if (children.Length > 1)
        {
            GameObject firstChild = children[1].gameObject;
            GameObject lastChild = children[children.Length - 1].gameObject;
            if (screenBounds.x*2 > lastChild.transform.position.x)
            {
                firstChild.transform.SetAsLastSibling();
                firstChild.transform.position = new Vector3(lastChild.transform.position.x + choke[obj], lastChild.transform.position.y, lastChild.transform.position.z);
            }
            else if (-screenBounds.x < firstChild.transform.position.x)
            {
                lastChild.transform.SetAsFirstSibling();
                lastChild.transform.position = new Vector3(firstChild.transform.position.x - choke[obj], firstChild.transform.position.y, firstChild.transform.position.z);
            }
        }
    }
    void Update()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            Vector3 velocity = Vector3.zero;
            Vector3 desiredPosition = levels[i].transform.position + new Vector3(scrollSpeed[i], 0, 0);
            Vector3 smoothPosition = Vector3.SmoothDamp(levels[i].transform.position, desiredPosition, ref velocity, 0.3f);

            levels[i].transform.position = smoothPosition;
        }

    }
    void LateUpdate()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            repositionChildObjects(i);
        }
    }
}