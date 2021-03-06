﻿using UnityEngine;

/// <summary>
/// Defines character life relation
/// It is mandatory component for character
/// 
/// TODO: Make interface for all character components and make better flow between them - recode for future project
/// 
/// </summary>
public class CharacterMortality : MonoBehaviour
{
    #region Private Members

    /// <summary>
    /// Character evading component reference
    /// </summary>
    private CharacterEvading _characterEvading;

    #endregion

    #region Public Members (Components)

    /// <summary>
    /// Status effect for respawn
    /// </summary>
    [Header("Components")]
    public AStatusEffectBase respawnInvulnerabilitySE;

    /// <summary>
    /// Character upper body collider component reference
    /// </summary>
    public GameObject upperBodyCollider;

    /// <summary>
    /// Character lower body collider component reference
    /// </summary>
    public GameObject lowerBodyCollider;

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates characters death
    /// </summary>
    public bool IsDeath { get; set; }

    /// <summary>
    /// Indicates if the character is invincible (TRUE), or not (FALSE)
    /// </summary>
    public bool IsInvincible => (_characterEvading && (_characterEvading.IsInDodge || _characterEvading.IsInEvade));

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        _characterEvading = GetComponent<CharacterEvading>();

        // Make sure to always have player alived on creation
        Revive();
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Make the character die
    /// </summary>
    public void Die()
    {
        // Make it death
        IsDeath = true;

        // Temporary disable player collisions
        upperBodyCollider.SetActive(false);
        lowerBodyCollider.SetActive(false);
    }

    /// <summary>
    /// Revive the character
    /// </summary>
    public void Revive()
    {
        // Revive
        IsDeath = false;

        // Re-enable player collisions
        upperBodyCollider.SetActive(true);
        lowerBodyCollider.SetActive(true);

        // Apply invulnerability on respawn
        StatusEffectManager.Instance.Apply(GetComponent<StatusEffectProcessor>(), null, respawnInvulnerabilitySE);
    }

    #endregion
}
