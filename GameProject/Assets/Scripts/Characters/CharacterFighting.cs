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

    #endregion

    #region Public Members (Stats)

    /// <summary>
    /// Speed of fire
    /// </summary>
    [Header("Stats")]
    public float fireSpeed = 1.0f;

    #endregion

    #region Public Members (Settings)

    /// <summary>
    /// Fire point offset
    /// </summary>
    [Header("Settings")]
    public Vector3 fireOriginOffset = new Vector3(0.5f, 0f, 0f);

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
    /// </summary>
    public bool IsFireAllowed =>
        (!_characterEvading || (_characterEvading && !_characterEvading.IsInDodge)) &&
        !_characterMovement.IsInSlide;

    #endregion

    #region Public Properties (Cooldown Timers)

    /// <summary>
    /// Timer for fire cooldown (last used)
    /// </summary>
    public float FireCooldownTimer { get; private set; }

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
        // Fire
        if (FireTime > Time.time && FireCooldownTimer + fireSpeed < Time.time)
            if (IsFireAllowed)
                if (GameManager.Instance.IsMeleeDistance) // TODO: 0 arrow - switch to melee only
                    StartCoroutine(MeleeFire());
                else
                    StartCoroutine(RangedFire());

    }

    #endregion

    #region Private Method

    /// <summary>
    /// Handled fire of the character
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
    /// Handled fire of the character
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
        PerformRangedFire();

        // Put the flag down
        IsPerformingFireFlag = false;
    }

    /// <summary>
    /// Perform fire
    /// </summary>
    private void PerformRangedFire()
    {
        var offset = transform.position + fireOriginOffset;
        offset.x = _characterMovement.isFacingRight ? transform.position.x + fireOriginOffset.x : transform.position.x - fireOriginOffset.x;

        // Instantiate prefab
        var prefab = Instantiate(projectilePrefab, offset, Quaternion.identity);

        // define caster
        prefab.GetComponent<Projectile>().Caster = gameObject;

        // Set correct facing direction
        prefab.transform.rotation = Quaternion.Euler(0, _characterMovement.isFacingRight ? 0 : 180, 0);
        prefab.GetComponent<Projectile>().IsFacingRight = _characterMovement.isFacingRight;
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
}
