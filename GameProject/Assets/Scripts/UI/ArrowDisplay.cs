using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowDisplay : MonoBehaviour
{
    public CharacterFighting fightingScript;
    public GameObject arrowImage;
    public bool rightToLeft;
    private int maxQuiver;
    // Start is called before the first frame update
    void Start()
    {
        maxQuiver = fightingScript.quiver;
        GameObject previousObject = arrowImage;
        for (int i = 0; i < maxQuiver - 1; i++)
        {
            GameObject clone = Instantiate(previousObject) as GameObject;
            clone.transform.SetParent(previousObject.transform.parent.transform);
            if (rightToLeft)
            {
                clone.transform.position = new Vector3(previousObject.transform.position.x - ((RectTransform)previousObject.transform).rect.width, previousObject.transform.position.y, previousObject.transform.position.z);
            }
            else {
                clone.transform.position = new Vector3(previousObject.transform.position.x + ((RectTransform)previousObject.transform).rect.width, previousObject.transform.position.y, previousObject.transform.position.z);
            }
            clone.name = previousObject.name;
            previousObject = clone;

        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < arrowImage.transform.parent.childCount; i++)
        {
            arrowImage.transform.parent.GetChild(i).gameObject.SetActive(true);
            if (i >= fightingScript.quiver) 
            {
                arrowImage.transform.parent.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
