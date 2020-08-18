using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParalax : MonoBehaviour
{
    public GameObject farBackground;
    public GameObject nearBackground;

    public float farBackgroundMovement;
    public float nearBackgroundMovement;

    public Camera mainCamera;
    private Vector2 screenBounds;

    private float position;
    private float lastPositions;


    void Start()
    {
        float height = mainCamera.orthographicSize * 2;
        float width = height * Screen.width / Screen.height;

        screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.transform.position.z));
        

        SpriteRenderer farBackgroundSpriteRenderer = farBackground.GetComponent<SpriteRenderer>();
        float farBackgroundGameObjectHeight = farBackgroundSpriteRenderer.sprite.rect.height / 3;
        float farBackgroundGameObjectWidth = farBackgroundSpriteRenderer.sprite.rect.width / 3;
        float farBackgroundScale = Screen.height / farBackgroundGameObjectHeight;
        farBackground.transform.localScale = new Vector3(farBackgroundScale, farBackgroundScale, farBackground.transform.localScale.z);
        farBackground.transform.position = new Vector3(mainCamera.transform.position.x * farBackgroundMovement, 0, farBackground.transform.position.z);

        SpriteRenderer nearBackgroundSpriteRenderer = nearBackground.GetComponent<SpriteRenderer>();
        float nearBackgroundGameObjectHeight = nearBackgroundSpriteRenderer.sprite.rect.height / 3;
        float nearBackgroundScale = Screen.height / nearBackgroundGameObjectHeight;
        nearBackground.transform.localScale = new Vector3(nearBackgroundScale, nearBackgroundScale, nearBackground.transform.localScale.z);
        nearBackground.transform.position = new Vector3(mainCamera.transform.position.x * nearBackgroundMovement, 0, nearBackground.transform.position.z);
        
        for (int i = 0; i <= 2; i++)
        {
            GameObject clone = Instantiate(farBackground) as GameObject;
            clone.transform.SetParent(farBackground.transform.parent);
            clone.transform.position = farBackground.transform.position + new Vector3(farBackgroundSpriteRenderer.bounds.size.x * (i + 1), 0,0);
        }

        for (int i = 0; i <= 2; i++)
        {
            GameObject clone = Instantiate(nearBackground) as GameObject;
            clone.transform.SetParent(nearBackground.transform.parent);
            clone.transform.position = nearBackground.transform.position + new Vector3(nearBackgroundSpriteRenderer.bounds.size.x * (i+1), 0, 0);
        }

    }

    private void Update()
    {
        SpriteRenderer farBackgroundSpriteRenderer = farBackground.GetComponent<SpriteRenderer>();
        SpriteRenderer nearBackgroundSpriteRenderer = nearBackground.GetComponent<SpriteRenderer>();
        farBackground.transform.position = new Vector3(mainCamera.transform.position.x * farBackgroundMovement, 0, farBackground.transform.position.z);
        nearBackground.transform.position = new Vector3(mainCamera.transform.position.x * nearBackgroundMovement, 0, nearBackground.transform.position.z);

        for (int i = 0; i < farBackground.transform.parent.childCount; i++)
        {
            farBackground.transform.parent.GetChild(i).transform.position = farBackground.transform.position + new Vector3(farBackgroundSpriteRenderer.bounds.size.x * i, 0, 0);
        }

        for (int i = 0; i < nearBackground.transform.parent.childCount; i++)
        {
            nearBackground.transform.parent.GetChild(i).transform.position = nearBackground.transform.position + new Vector3(nearBackgroundSpriteRenderer.bounds.size.x * i, 0, 0);
        }

        bool done = false;
        while (!done) 
        {
            for (int i = 0; i < farBackground.transform.parent.childCount; i++)
            { 
                Vector3 position = farBackground.transform.parent.GetChild(i).transform.position;

                if (position.x < mainCamera.transform.position.x - 2 * farBackgroundSpriteRenderer.bounds.size.x)
                {
                    positionLast(farBackground.transform.parent.GetChild(i));
                }
                else if (position.x > mainCamera.transform.position.x + 2 * farBackgroundSpriteRenderer.bounds.size.x)
                {
                    positionFirst(farBackground.transform.parent.GetChild(i));
                }
                else 
                {
                    done = true;
                }
                //    = farBackground.transform.position + new Vector3(farBackgroundSpriteRenderer.bounds.size.x * i, 0, 0);
            }
        }

        done = false;
        while (!done)
        {
            for (int i = 0; i < nearBackground.transform.parent.childCount; i++)
            {
                Vector3 position = nearBackground.transform.parent.GetChild(i).transform.position;

                if (position.x < mainCamera.transform.position.x - 2 * nearBackgroundSpriteRenderer.bounds.size.x)
                {
                    positionLast(nearBackground.transform.parent.GetChild(i));
                }
                else if (position.x > mainCamera.transform.position.x + 2 * nearBackgroundSpriteRenderer.bounds.size.x)
                {
                    positionFirst(nearBackground.transform.parent.GetChild(i));
                }
                else 
                {
                    done = true;
                }
            }
        }
    }

    private void positionLast(Transform transform)
    {
        SpriteRenderer nearBackgroundSpriteRenderer = nearBackground.GetComponent<SpriteRenderer>();
        Transform lastChild = transform;
        for (int i = 0; i < transform.parent.childCount; i++) 
        {
            Transform child = transform.parent.GetChild(i);
            if (child.position.x > lastChild.position.x) 
            {
                lastChild = child;
            }
        }

        transform.position = new Vector3(lastChild.position.x + nearBackgroundSpriteRenderer.bounds.size.x, 0, 0);
    }

    private void positionFirst(Transform transform)
    {
        SpriteRenderer nearBackgroundSpriteRenderer = nearBackground.GetComponent<SpriteRenderer>();
        Transform firstChild = transform;
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            Transform child = transform.parent.GetChild(i);
            if (child.position.x < firstChild.position.x)
            {
                firstChild = child;
            }
        }

        transform.position = new Vector3(firstChild.position.x - nearBackgroundSpriteRenderer.bounds.size.x, 0, 0);
    }
    /*
void loadChildObjects(int obj)
{
   int childsNeeded = 7;
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
       if (lastChild.transform.position.x < position + screenBounds.x)
       {
           firstChild.transform.SetAsLastSibling();
           firstChild.transform.position = new Vector3(lastChild.transform.position.x + choke[obj], lastChild.transform.position.y, lastChild.transform.position.z);
       }
       else if (firstChild.transform.position.x > position - screenBounds.x / 2)
       {
           lastChild.transform.SetAsFirstSibling();
           lastChild.transform.position = new Vector3(firstChild.transform.position.x - choke[obj], firstChild.transform.position.y, firstChild.transform.position.z);
       }
   }
}

void Update()
{
   lastPositions = position;
   position = mainCamera.transform.position.x - screenBounds.x / 2;
   for (int i = 0; i < levels.Length; i++)
   {
       Vector3 velocity = Vector3.zero;
       Vector3 desiredPosition = levels[i].transform.position;
       if (movementParalax)
       {
           desiredPosition.x += position * scrollSpeed[i] - lastPositions * scrollSpeed[i];
           levels[i].transform.position = desiredPosition;
       }
       else
       {
           desiredPosition += new Vector3(scrollSpeed[i], 0, 0);
           Vector3 smoothPosition = Vector3.SmoothDamp(levels[i].transform.position, desiredPosition, ref velocity, 0.3f);
           levels[i].transform.position = smoothPosition;
       }

   }

}
void LateUpdate()
{
   for (int i = 0; i < levels.Length; i++)
   {
       repositionChildObjects(i);
   }
}
*/
}
