using UnityEngine;

/// <summary>
/// Gives character ability to dodge
/// It is optional component for character
/// </summary>
public class CharacterEvading : MonoBehaviour
{
    #region Private Members

    /// <summary>
    /// Character movement component reference
    /// </summary>
    private CharacterMovement _characterMovement;

    /// <summary>
    /// Character fighting component reference
    /// </summary>
    private CharacterFighting _characterFighting;

    #endregion

    #region Public Members (Components)

    /// <summary>
    /// Status effect for dodge
    /// </summary>
    [Header("Components")]
    public AStatusEffectBase dodgeInvulnerabilitySE;

    #endregion

    #region Public Members (Stats)

    /// <summary>
    /// Cooldown for dodge
    /// </summary>
    [Header("Stats")]
    public float dodgeCooldown = 1.7f;

    /// <summary>
    /// Strength of the dodge jump
    /// </summary>
    public float dodgeJumpStrength = 10.0f;

    #endregion

    #region Public Properties (Perms)

    /// <summary>
    /// Indicates if <see cref="Fire"/> is allowed...
    /// ---
    /// If character is not in fire...
    /// If character is not in air dive...
    /// If character is not in ground smash...
    /// If character is not in slide...
    /// </summary>
    public bool IsDodgeAllowed => 
        (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingFireFlag)) &&
        (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingAirDiveFlag)) &&
        (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingGroundSmashFlag)) &&
        !_characterMovement.IsInSlide;

    #endregion

    #region Public Properties (Cooldown Timers)

    /// <summary>
    /// Timer for dodge cooldown (last used)
    /// </summary>
    public float DodgeCooldownTimer { get; private set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// How long it is since the last dodge press
    /// </summary>
    public float DodgeTime { get; set; }

    /// <summary>
    /// Indicates if the character is currently in dodge (TRUE) or not (FALSE)
    /// </summary>
    public bool IsInDodge { get; set; }

    /// <summary>
    /// Indicates if the character is currently different form of evade
    /// </summary>
    /// <remarks>
    ///     Mark for <see cref="CharacterMortality"/>
    /// </remarks>
    public bool IsInEvade { get; set; }

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _characterFighting = GetComponent<CharacterFighting>();
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        // Dodge
        if (DodgeTime > Time.time && DodgeCooldownTimer + dodgeCooldown < Time.time)
            if (IsDodgeAllowed)
                DoDodge();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Perform Dodge ability
    /// </summary>
    public void DoDodge()
    {
        Dodge_Apply();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Apply dodge ability
    /// </summary>
    private void Dodge_Apply()
    {
        // Update cooldown
        DodgeCooldownTimer = Time.time;

        // Apply status effect (invulnerability)
        StatusEffectManager.Instance.Apply(GetComponent<StatusEffectProcessor>(), null, dodgeInvulnerabilitySE);

        // Perform jump
        _characterMovement.PerformJump(dodgeJumpStrength);
    }

    #endregion
}
