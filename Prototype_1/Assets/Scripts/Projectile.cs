using UnityEngine;

public class Projectile : MonoBehaviour
{
    #region Public Members

    public bool isFacingRight = true;

    public float speed = 12f;

    public Rigidbody2D rb;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        rb.velocity = (isFacingRight ? Vector2.right : Vector2.left) * speed;
    }

    #endregion

    #region Trigger Methods

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (collision.gameObject.GetComponent<PlayerController>().IsInDodge)
                return;

            GameObject.Find("GameManager").GetComponent<GameManager>().RespawnPlayersOne();
            GameObject.Find("GameManager").GetComponent<GameManager>().RespawnPlayersTwo();
        }

        Destroy(gameObject);
    }

    #endregion
}
