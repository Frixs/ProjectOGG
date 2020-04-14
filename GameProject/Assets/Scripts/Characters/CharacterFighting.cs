using System.Collections;
using UnityEngine;

/// <summary>
/// Gives character ability to fight
/// It is optional component for character
/// </summary>
public class CharacterFighting : MonoBehaviour
{
    #region Private Members

    /// <summary>
    /// Character movement component reference
    /// </summary>
    private CharacterMovement _characterMovement;

    /// <summary>
    /// Character evading component reference
    /// </summary>
    private CharacterEvading _characterEvading;

    #endregion

    #region Public Members (Components)

    /// <summary>
    /// Projectile prefab
    /// </summary>
    [Header("Components")]
    public GameObject projectilePrefab;

    /// <summary>
    /// Collider to trigger melee attack
    /// </summary>
    public Collider2D meleeFireCollider;

    /// <summary>
    /// Status effect for ground smash
    /// </summary>
    public AStatusEffectBase groundSmashSE;

    #endregion

    #region Public Members (Stats)

    /// <summary>
    /// Speed of fire
    /// </summary>
    [Header("Stats")]
    public float fireSpeed = 1.0f;

    /// <summary>
    /// Number of projectiles in quiver
    /// </summary>
    public int quiver = 10;

    /// <summary>
    /// Minimal allowed height from the ground to activate air dive
    /// </summary>
    /// <remarks>
    ///     Drawn by gizmo.
    /// </remarks>
    public float airDiveMinimalAllowedGroundDetectionColliderHeight = 4.5f;

    /// <summary>
    /// Cooldown for Air Dive 
    /// </summary>
    public float airDiveCooldown = 0.9f;

    /// <summary>
    /// Delay of performing air dive
    /// </summary>
    public float airDiveDelay = 0.2f;

    /// <summary>
    /// Vertical strength of the air dive
    /// </summary>
    public float airDiveStrength = 30f;

    /// <summary>
    /// Strength of the air dive pre-jump
    /// </summary>
    public float airDiveJumpStrength = 15.0f;

    /// <summary>
    /// Duration of the ground smash animation
    /// </summary>
    public float groundSmashDuration = 0.6f;

    /// <summary>
    /// Range of the ground smash AoE
    /// </summary>
    public float groundSmashEffectRange = 7f;

    /// <summary>
    /// Offset to position ground of the effect - Y axis
    /// </summary>
    public float groundSmashGroundOffset = -1.5f;

    #endregion

    #region Public Members (Settings)

    /// <summary>
    /// Fire point offset
    /// </summary>
    [Header("Settings")]
    public Vector3 rangedFireOriginOffset = new Vector3(0.5f, 0f, 0f);

    /// <summary>
    /// Safe space to spawn projectile ahead. 
    /// Any obstacle in that range from <see cref="rangedFireOriginOffset"/> will not trigger the fire.
    /// </summary>
    /// <remarks>
    ///     Works only for X axis
    /// </remarks>
    public Vector3 rangedFireSafeSpaceToSpawn = new Vector3(0.75f, 0f, 0f);

    /// <summary>
    /// Delay to perform ranged fire
    /// </summary>
    public float rangedFireDelay = 0.4f;

    /// <summary>
    /// Delay to perform melee fire
    /// </summary>
    public float meleeFireDelay = 0.4f;

    #endregion

    #region Public Properties (Perms)

    /// <summary>
    /// Indicates if <see cref="RangedFire"/> is allowed...
    /// ---
    /// If character is not evading...
    /// If character is not in slide...
    /// If character is not performing air dive...
    /// If character is not performing ground smash...
    /// </summary>
    public bool IsFireAllowed =>
        (!_characterEvading || (_characterEvading && !_characterEvading.IsInDodge)) &&
        !_characterMovement.IsInSlide &&
        !IsPerformingAirDiveFlag &&
        !IsPerformingGroundSmashFlag;

    /// <summary>
    /// Indicates if <see cref="AirDive"/> is allowed...
    /// ---
    /// If allowed height for air dive is OK...
    /// If character is not evading...
    /// If character is not grounded...
    /// If character is not performing fire...
    /// If character is not in slide...
    /// If character is not performing air dive...
    /// </summary>
    public bool IsAirDiveAllowed =>
        HasAirDiveAllowedHeight &&
        (!_characterEvading || (_characterEvading && !_characterEvading.IsInDodge)) &&
        !_characterMovement.IsGrounded &&
        !IsPerformingFireFlag &&
        !_characterMovement.IsInSlide &&
        !IsPerformingAirDiveFlag;

    #endregion

    #region Public Properties (Cooldown Timers)

    /// <summary>
    /// Timer for fire cooldown (last used)
    /// </summary>
    public float FireCooldownTimer { get; private set; }

    /// <summary>
    /// Timer for air dive cooldown (last used)
    /// </summary>
    public float AirDiveCooldownTimer { get; private set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// How long it is since the last fire press
    /// </summary>
    public float FireTime { get; set; }

    /// <summary>
    /// Indicates if fire is currently being performed
    /// </summary>
    public bool IsPerformingFireFlag { get; private set; }

    /// <summary>
    /// Indicates if air dive is in allowed height to trigger
    /// </summary>
    public bool HasAirDiveAllowedHeight { get; private set; }

    /// <summary>
    /// How long it is since the last air dive press
    /// </summary>
    public float AirDiveTime { get; set; }

    /// <summary>
    /// Indicates if air dive is currently being performed
    /// </summary>
    public bool IsPerformingAirDiveFlag { get; private set; }

    /// <summary>
    /// Indicates if ground smash is currently being performed
    /// </summary>
    public bool IsPerformingGroundSmashFlag { get; private set; }

    #endregion

    #region Events (Triggers)

    /// <summary>
    /// Event trigger - Ranged Fire
    /// </summary>
    public event RangedFireTriggerDelegate RangedFireTrigger;

    /// <summary>
    /// Delegate event <see cref="RangedFireTrigger"/>
    /// </summary>
    public delegate void RangedFireTriggerDelegate();

    /// <summary>
    /// Event trigger - Melee Fire
    /// </summary>
    public event MeleeFireTriggerDelegate MeleeFireTrigger;

    /// <summary>
    /// Delegate event <see cref="MeleeFireTrigger"/>
    /// </summary>
    public delegate void MeleeFireTriggerDelegate();

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _characterEvading = GetComponent<CharacterEvading>();

        // Make sure, we start with disabled melee collider
        meleeFireCollider.enabled = false;
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        // Calculate allowed height for air dive
        // TODO: Create helper static class with all these collider & physics modifiers
        HasAirDiveAllowedHeight = !Physics2D.Raycast(
            transform.position + _characterMovement.groundDetectionColliderOffset,
            Vector2.down, airDiveMinimalAllowedGroundDetectionColliderHeight, _characterMovement.groundLayer) && !Physics2D.Raycast(transform.position - _characterMovement.groundDetectionColliderOffset, Vector2.down,
            airDiveMinimalAllowedGroundDetectionColliderHeight,
            _characterMovement.groundLayer
            );

        // Fire
        if (FireTime > Time.time && FireCooldownTimer + fireSpeed < Time.time)
            if (IsFireAllowed)
                if (!GameManager.Instance.IsMeleeDistance && quiver > 0)
                    StartCoroutine(RangedFire());
                else
                    StartCoroutine(MeleeFire());

        // AirDive
        if (AirDiveTime > Time.time && AirDiveCooldownTimer + airDiveCooldown < Time.time)
            if (IsAirDiveAllowed)
                AirDive();
        if (IsPerformingAirDiveFlag)
        {
            // If crouch is not pressed and character is not performing ground smash...
            if (!_characterMovement.HasCrouchPress && !IsPerformingGroundSmashFlag)
                // Turn off indication of performing air dive
                IsPerformingAirDiveFlag = false;

            // if is already grounded and character is not performing ground smash...
            else if (_characterMovement.IsGrounded && !IsPerformingGroundSmashFlag)
                // Perform ground smash
                PerformGroundSmash();
        }
    }

    #endregion

    #region Private Method (AirDive)

    /// <summary>
    /// Handled character air dive
    /// </summary>
    private void AirDive()
    {
        // Update cooldown
        AirDiveCooldownTimer = Time.time;

        // Turn on indication of performing air dive
        IsPerformingAirDiveFlag = true;

        // Stop character
        _characterMovement.rb.velocity = new Vector2(0, _characterMovement.rb.velocity.y);

        // Perform jump
        _characterMovement.PerformJump(airDiveJumpStrength);

        // Perform air dive
        StartCoroutine(PerformAirDive());
    }

    /// <summary>
    /// Perform air dive
    /// </summary>
    /// <returns></returns>
    private IEnumerator PerformAirDive()
    {
        // Delay air dive
        yield return new WaitForSeconds(airDiveDelay);

        // Dive character
        _characterMovement.rb.velocity = new Vector2(0, -airDiveStrength);
    }

    #endregion

    #region Private Method (GroundSmash)

    /// <summary>
    /// Perform ground smash
    /// </summary>
    private void PerformGroundSmash()
    {
        // Turn on indication of performing ground smash
        IsPerformingGroundSmashFlag = true;

        // Change collider to crouch
        _characterMovement.ColliderChangeToCrouch();

        // Delay recovery from ground smash
        StartCoroutine(RecoverFromGroundSmash());

        // AoE debuff - unarm
        HandleGroundSmashEffect();
    }

    /// <summary>
    /// Delayed recovery from ground smash
    /// </summary>
    /// <returns></returns>
    private IEnumerator RecoverFromGroundSmash()
    {
        // Delay air dive
        yield return new WaitForSeconds(groundSmashDuration);

        // Change collider back to original
        _characterMovement.ColliderChangeToOriginal();

        // Turn off indication of performing ground smash
        IsPerformingGroundSmashFlag = false;
        // Turn off indication of performing air dive
        IsPerformingAirDiveFlag = false;
    }

    /// <summary>
    /// Handle action of ground smash effect
    /// </summary>
    private void HandleGroundSmashEffect()
    {
        // Get opponent player
        var opponentPlayer = GameManager.Instance.IsItPlayer1(gameObject) ? GameManager.Instance.Player2 : GameManager.Instance.Player1;
        
        // If the opponent is under AoE...
        if (transform.position.y + groundSmashGroundOffset > opponentPlayer.transform.position.y)
            // Do nothing
            return;

        // If players are not in range of effect...
        if (Vector3.Distance(transform.position + new Vector3(0, groundSmashGroundOffset, 0), opponentPlayer.transform.position + new Vector3(0, groundSmashGroundOffset, 0)) > groundSmashEffectRange)
            // Do nothing
            return;

        // Apply debuff on the opponent player
        StatusEffectManager.Instance.Apply(opponentPlayer.GetComponent<StatusEffectProcessor>(), GetComponent<StatusEffectProcessor>(), groundSmashSE);
    }

    #endregion

    #region Private Method (Attack Melee/Ranged)

    /// <summary>
    /// Handled character fire (melee)
    /// </summary>
    private IEnumerator MeleeFire()
    {
        // Flag up
        IsPerformingFireFlag = true;

        // Update cooldown
        FireCooldownTimer = Time.time;

        // Trigger fire events
        MeleeFireTrigger();

        // Set collider to original if not
        _characterMovement.ColliderChangeToOriginal();

        // Delay - half of time to activate DMG collider
        yield return new WaitForSeconds(meleeFireDelay / 2);

        // Enable DMG collider
        meleeFireCollider.enabled = true;

        // Delay - rest of time to finish
        yield return new WaitForSeconds(meleeFireDelay / 2);

        // Disable DMG collider
        meleeFireCollider.enabled = false;

        // Put the flag down
        IsPerformingFireFlag = false;
    }

    /// <summary>
    /// Handled character fire (ranged)
    /// </summary>
    private IEnumerator RangedFire()
    {
        // Flag up
        IsPerformingFireFlag = true;

        // Update cooldown
        FireCooldownTimer = Time.time;

        // Trigger fire events
        RangedFireTrigger();

        // Set collider to original if not
        _characterMovement.ColliderChangeToOriginal();

        // Delay
        yield return new WaitForSeconds(rangedFireDelay);

        // Perform fire
        if (PerformRangedFire())
            // Lower the quiver content
            quiver--;

        // Put the flag down
        IsPerformingFireFlag = false;
    }

    /// <summary>
    /// Perform fire
    /// </summary>
    /// <returns>Successfully performed (TRUE), otherwise (FALSE)</returns>
    private bool PerformRangedFire()
    {
        var offset = transform.position + rangedFireOriginOffset;
        offset.x = _characterMovement.isFacingRight ? transform.position.x + rangedFireOriginOffset.x : transform.position.x - rangedFireOriginOffset.x;

        // Check for safe location to spawn projectile...
        if (Physics2D.Raycast(
            transform.position,
            _characterMovement.isFacingRight ? Vector2.right : Vector2.left,
            rangedFireOriginOffset.x + rangedFireSafeSpaceToSpawn.x,
            _characterMovement.groundLayer)
            )
            // Ignore, we dont want to spawn projectile inside ground
            return false;

        // Instantiate prefab
        var prefab = Instantiate(projectilePrefab, offset, Quaternion.identity);

        // define caster
        prefab.GetComponent<Projectile>().Caster = gameObject;

        // Set correct facing direction
        prefab.transform.rotation = Quaternion.Euler(0, _characterMovement.isFacingRight ? 0 : 180, 0);
        prefab.GetComponent<Projectile>().IsFacingRight = _characterMovement.isFacingRight;

        return true;
    }

    #endregion

    #region Unity Triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player collision
        if (collision.gameObject.layer == LayerMask.NameToLayer(nameof(LayerName.Player)))
        {
            // If the collision is the caster...
            if (collision.gameObject.name.Equals(gameObject.name))
                return;

            // Ignoring, character is invincible
            if (collision.gameObject.GetComponent<CharacterMortality>().IsInvincible)
                return;

            // Ignoring, character is death (corpse)
            if (collision.gameObject.GetComponent<CharacterMortality>().IsDeath)
                return;

            // Rebirth the player
            GameManager.Instance.Rebirth(collision.gameObject.name);
        }
    }

    #endregion

    #region Gizmo

    private void OnDrawGizmos()
    {
        // Draw jump collider triggers
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + rangedFireOriginOffset, transform.position + rangedFireOriginOffset + rangedFireSafeSpaceToSpawn);

        // Draw air dive collider triggers
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * airDiveMinimalAllowedGroundDetectionColliderHeight);

        // Draw ground smash AoE
        UnityEditor.Handles.color = Color.cyan;
        //Gizmos.DrawSphere(transform.position - new Vector3(0, -groundSmashGroundOffset, 0), groundSmashEffectRange);
        UnityEditor.Handles.DrawLine(transform.position - new Vector3(groundSmashEffectRange, -groundSmashGroundOffset, 0), transform.position + new Vector3(groundSmashEffectRange, groundSmashGroundOffset, 0));
        UnityEditor.Handles.DrawWireArc(transform.position - new Vector3(0, -groundSmashGroundOffset, 0), new Vector3(1, 0, 10f), new Vector3(1, 0, 0), 180, groundSmashEffectRange);
    }

    #endregion
}
