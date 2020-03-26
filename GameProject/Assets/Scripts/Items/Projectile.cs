using UnityEngine;

/// <summary>
/// Logic for character projectiles
/// </summary>
public class Projectile : MonoBehaviour
{
    #region Private Members

    /// <inheritdoc cref="Caster"/>
    private GameObject _caster;

    #endregion

    #region Public Members (Components)

    /// <summary>
    /// Rigid body of the projectile
    /// </summary>
    [Header("Components")]
    public Rigidbody2D rb;

    #endregion

    #region Public Members (Stats)

    /// <summary>
    /// Projectile speed
    /// </summary>
    [Header("Stats")]
    public float speed = 12f;

    #endregion

    #region Public Properties

    /// <summary>
    /// Caster of the projectile
    /// </summary>
    public GameObject Caster
    {
        get => _caster;
        set
        {
            if (_caster != null)
                return;
            _caster = value;
        }
    }

    /// <summary>
    /// Indicates, if the projectile is facing right (TRUE), otherwise left
    /// </summary>
    public bool IsFacingRight { get; set; } = true;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        rb.velocity = (IsFacingRight ? Vector2.right : Vector2.left) * speed;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {

    }

    #endregion

    #region Unity Triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player collision
        if (collision.gameObject.layer == LayerMask.NameToLayer(nameof(LayerName.Player)))
        {
            // If the collision is the caster...
            if (collision.gameObject.name.Equals(Caster.name))
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

        // TODO: Leave the projectile on
        Destroy(gameObject);
    }

    #endregion
}
