using UnityEngine;

/// <summary>
/// Logic for collectibles - Projectile
/// </summary>
public class ProjectileItem : MonoBehaviour
{
    #region Private Members

    /// <summary>
    /// Rigid body of the projectile
    /// </summary>
    private Rigidbody2D _rb;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        _rb.velocity = new Vector2(0, _rb.velocity.y);
    }

    #endregion

    #region Unity Triggers

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player collision
        if (collision.gameObject.layer == LayerMask.NameToLayer(nameof(LayerName.Player)))
        {
            // Get player root GO
            GameObject playerRoot = collision.gameObject.GetTopParent(LayerMask.NameToLayer(nameof(LayerName.Player)));

            // Ignoring, character is death (corpse)
            if (playerRoot.GetComponent<CharacterMortality>().IsDeath)
                return;

            // Increment quiver if fighting component exists...
            var component = playerRoot.GetComponent<CharacterFighting>();
            if (component != null) 
            {
                // Increment
                component.quiver++;
                // Destroy item on the ground
                Destroy(gameObject);
            }
        }
    }

    #endregion
}
