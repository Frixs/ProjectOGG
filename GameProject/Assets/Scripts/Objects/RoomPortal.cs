using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handle logic to teleport players between rooms
/// </summary>
public class RoomPortal : MonoBehaviour
{
    #region Public Members (Settings)

    /// <summary>
    /// Indicates right portal (TRUE), otherwise it is left (FALSE)
    /// </summary>
    [Header("Settings")]
    public bool isItRightPortal;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {

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
        // Player collision
        if (collision.gameObject.layer == LayerMask.NameToLayer(nameof(LayerName.Player)))
        {
            // At least 1 player has to be death to open the portal
            if (!GameManager.Instance.AnyPlayerDeath)
                return;

            // Set new room number
            if (isItRightPortal)
                GameManager.Instance.CurrentRoom++;
            else
                GameManager.Instance.CurrentRoom--;

            // Log it
            Debug.unityLogger.Log($"Loading new room ({GameManager.Instance.CurrentRoom})...");

            // Load new room
            SceneManager.LoadScene($"GameRoom_{GameManager.Instance.CurrentRoom}");
        }
    }

    #endregion
}
