using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpDisplay : MonoBehaviour
{
    public CharacterMovement movementScript;
    public GameObject jumpImage;
    public bool rightToLeft;
    private int jumps;
    // Start is called before the first frame update
    void Start()
    {
        jumps = movementScript.multiJump;
        GameObject previousObject = jumpImage;
        for (int i = 0; i < jumps - 1; i++)
        {
            GameObject clone = Instantiate(previousObject) as GameObject;
            clone.transform.SetParent(previousObject.transform.parent.transform);
            if (rightToLeft)
            {
                clone.transform.position = new Vector3(previousObject.transform.position.x - ((RectTransform)previousObject.transform).rect.width, previousObject.transform.position.y, previousObject.transform.position.z);
            }
            else
            {
                clone.transform.position = new Vector3(previousObject.transform.position.x + ((RectTransform)previousObject.transform).rect.width, previousObject.transform.position.y, previousObject.transform.position.z);
            }
            clone.name = previousObject.name;
            previousObject = clone;

        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < jumpImage.transform.parent.childCount; i++)
        {
            jumpImage.transform.parent.GetChild(i).gameObject.SetActive(true);
            if (i >= movementScript.getAvailableJumps())
            {
                jumpImage.transform.parent.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
