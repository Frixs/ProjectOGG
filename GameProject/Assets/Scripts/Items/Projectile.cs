using System.Collections;
using UnityEngine;

/// <summary>
/// Logic for character projectiles
/// </summary>
public class Projectile : MonoBehaviour
{
    #region Private Members

    /// <inheritdoc cref="Caster"/>
    private GameObject _caster;

    /// <summary>
    /// Rigid body of the projectile
    /// </summary>
    private Rigidbody2D _rb;

    #endregion

    #region Public Members (Components)

    /// <summary>
    /// Reference to projectile item prefab
    /// </summary>
    [Header("Components")]
    public GameObject projectileItemPrefab;

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
        _rb = GetComponent<Rigidbody2D>();
        _rb.velocity = (IsFacingRight ? Vector2.right : Vector2.left) * speed;
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {

    }

    #endregion

    #region Unity Triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Name of the trigger event
        string triggerEvent = "Hit";

        // Player collision
        if (collision.gameObject.layer == LayerMask.NameToLayer(nameof(LayerName.Player)))
        {
            // Get player root GO
            GameObject playerRoot = collision.gameObject.GetTopParent(LayerMask.NameToLayer(nameof(LayerName.Player)));

            // If the collision is the caster...
            if (playerRoot.name.Equals(Caster.name))
                return;

            // Ignoring, character is invincible
            if (playerRoot.GetComponent<CharacterMortality>().IsInvincible)
                return;

            // Ignoring, character is death (corpse)
            if (playerRoot.GetComponent<CharacterMortality>().IsDeath)
                return;

            // Trigger hit event
            GetComponent<Animator>().SetTrigger(triggerEvent);

            // Rebirth the player
            GameManager.Instance.Rebirth(playerRoot.name);

            // Destroy (faster)
            StartCoroutine(DelayedProjectileDestroy(0.1f, false));
        }
        // Otherwise
        {
            // Trigger hit event
            GetComponent<Animator>().SetTrigger(triggerEvent);
        }

        // Destroy
        StartCoroutine(DelayedProjectileDestroy(0.35f, true));
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Projectile destroy with a delay
    /// </summary>
    /// <param name="delay">The delay</param>
    /// <param name="instantiateItem">Indicates if we should instantiate item on destroy (TRUE) or not (FALSE)</param>
    /// <returns></returns>
    private IEnumerator DelayedProjectileDestroy(float delay, bool instantiateItem)
    {
        // Stop projectile
        _rb.velocity = Vector2.zero;

        // Delay
        yield return new WaitForSeconds(delay);

        // If item instantiation is requested...
        if (instantiateItem)
            InstantiateItem(true);

        // Destroy projectile
        Destroy(gameObject);
    }

    /// <summary>
    /// Instantiate projectile item
    /// </summary>
    private void InstantiateItem(bool rotate)
    {
        var item = Instantiate(projectileItemPrefab, gameObject.transform.position, Quaternion.identity);
        item.transform.Rotate(0, 0, -90);
    }

    #endregion
}
