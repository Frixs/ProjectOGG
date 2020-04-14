using UnityEngine;

/// <summary>
/// Defines character life relation
/// It is mandatory component for character
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

        // temporary ignore player to player collisions while one of the player is death
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(LayerName.Player), LayerMask.NameToLayer(LayerName.Player), true);
    }

    /// <summary>
    /// Revive the character
    /// </summary>
    public void Revive()
    {
        // Revive
        IsDeath = false;

        // Re-enable player to player collisions
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer(LayerName.Player), LayerMask.NameToLayer(LayerName.Player), false);

        // Apply invulnerability on respawn
        StatusEffectManager.Instance.Apply(GetComponent<StatusEffectProcessor>(), null, respawnInvulnerabilitySE);
    }

    #endregion
}
