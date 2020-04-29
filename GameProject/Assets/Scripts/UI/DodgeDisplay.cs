using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DodgeDisplay : MonoBehaviour
{
    public CharacterEvading evasionScript;
    public Image dodgeImage;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        dodgeImage.fillAmount = (Time.time - evasionScript.DodgeCooldownTimer)/evasionScript.dodgeCooldown;
    }
}
