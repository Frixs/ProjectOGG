using UnityEngine;

/// <summary>
/// Handles collider and its movement for the camera
/// </summary>
public class CameraCollider : MonoBehaviour
{
    #region Public Members (Components)

    /// <summary>
    /// Left collider
    /// </summary>
    [Header("Components")]
    public EdgeCollider2D leftCollider;

    /// <summary>
    /// Right collider
    /// </summary>
    public EdgeCollider2D rightCollider;

    #endregion

    #region Public Members (Settings)

    /// <summary>
    /// Thickness of the collider
    /// </summary>
    public float colliderThickness = 2f;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        // Set thickness
        leftCollider.offset = new Vector2(-colliderThickness, leftCollider.offset.y);
        rightCollider.offset = new Vector2(+colliderThickness, rightCollider.offset.y);
        leftCollider.edgeRadius = colliderThickness;
        rightCollider.edgeRadius = colliderThickness;
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        CalculateColliderPosition();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// ¨Calculate collider position by the camera
    /// </summary>
    void CalculateColliderPosition()
    {
        Vector2[] colliderpoints;

        colliderpoints = leftCollider.points;
        colliderpoints[0] = Camera.main.ViewportToWorldPoint(new Vector2(0f, 0f));
        colliderpoints[1] = Camera.main.ViewportToWorldPoint(new Vector2(0f, 1f));
        leftCollider.points = colliderpoints;

        colliderpoints = rightCollider.points;
        colliderpoints[0] = Camera.main.ViewportToWorldPoint(new Vector2(1f, 0f));
        colliderpoints[1] = Camera.main.ViewportToWorldPoint(new Vector2(1f, 1f));
        rightCollider.points = colliderpoints;
    }

    #endregion

    #region Gizmo

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(leftCollider.points[0], leftCollider.points[1]);
        Gizmos.DrawLine(rightCollider.points[0], rightCollider.points[1]);
    }

    #endregion
}
