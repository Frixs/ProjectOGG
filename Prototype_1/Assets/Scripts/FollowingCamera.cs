using UnityEngine;

/// <summary>
/// Camera that follows an object
/// </summary>
public class FollowingCamera : MonoBehaviour
{
    #region Private Members

    private Vector2 threashold;

    private Rigidbody2D rb;

    #endregion

    #region Public Members

    public GameObject followObject;

    public float speed = 7f;

    public Vector2 followOffset;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        threashold = CalculateThreshold();
        rb = followObject.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        Vector2 follow = followObject.transform.position;
        float xDifference = Vector2.Distance(Vector2.right * transform.position.x, Vector2.right * follow.x);
        float yDifference = Vector2.Distance(Vector2.up * transform.position.y, Vector2.up * follow.y);

        Vector3 newPosition = transform.position;
        if (Mathf.Abs(xDifference) >= threashold.x)
            newPosition.x = follow.x;
        if (Mathf.Abs(yDifference) >= threashold.y)
            newPosition.y = follow.y;

        float moveSpeed = rb.velocity.magnitude > speed ? rb.velocity.magnitude : speed;

        transform.position = Vector3.MoveTowards(transform.position, newPosition, moveSpeed * Time.deltaTime);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Calculate threshold
    /// </summary>
    /// <returns>size of the threshold</returns>
    private Vector3 CalculateThreshold()
    {
        Rect aspect = Camera.main.pixelRect;
        Vector2 t = new Vector2(Camera.main.orthographicSize * aspect.width / aspect.height, Camera.main.orthographicSize);

        t.x -= followOffset.x;
        t.y -= followOffset.y;

        return t;
    }

    #endregion

    #region Gizmo

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector2 border = CalculateThreshold();
        Gizmos.DrawWireCube(transform.position, new Vector3(border.x * 2, border.y * 2, 1));
    } 

    #endregion
}
