using UnityEngine;

/// <summary>
/// Camera that follows an object
/// </summary>
public class FollowingCamera : MonoBehaviour
{
    #region Private Members

    /// <summary>
    /// Threshold
    /// </summary>
    private Vector2 _threashold;

    /// <summary>
    /// Rigid body of the object 1
    /// </summary>
    private Rigidbody2D _rbObj1;

    /// <summary>
    /// Rigid body of the object 2
    /// </summary>
    private Rigidbody2D _rbObj2;

    #endregion

    #region Public Members (Components)

    /// <summary>
    /// Followable object 1
    /// </summary>
    [Header("Components")]
    public GameObject obj1;

    /// <summary>
    /// Followable object 2
    /// </summary>
    public GameObject obj2;

    #endregion

    #region Public Members (Settings)

    /// <summary>
    /// Camera move speed
    /// </summary>
    [Header("Settings")]
    public float speed = 7f;

    /// <summary>
    /// Offset of the camera edges (triggers to move)
    /// </summary>
    public Vector2 followOffset;

    public Vector2 cameraOffset;

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the following mechanism is turned on (TRUE) or not (FALSE)
    /// </summary>
    public bool IsFollowingAllowed { get; set; }

    /// <summary>
    /// Indicates if the camere should follow the first object <see cref="obj1"/> (TRUE) otherwise <see cref="obj2"/> (FALSE)
    /// </summary>
    public bool FollowFirstObject { get; set; }

    /// <summary>
    /// Followed object by the camera
    /// </summary>
    public GameObject FollowedObject => FollowFirstObject ? obj1 : obj2;

    /// <summary>
    /// Rigid body of the followed object
    /// </summary>
    private Rigidbody2D FollowedObjectRigidBody => FollowFirstObject ? _rbObj1 : _rbObj2;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        _threashold = CalculateThreshold();
        _rbObj1 = obj1.GetComponent<Rigidbody2D>();
        _rbObj2 = obj2.GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        if (!IsFollowingAllowed)
            return;

        // Calculate threshold
        _threashold = CalculateThreshold();

        // Get follow position
        Vector2 follow = FollowedObject.transform.position;
        if (FollowFirstObject)
        {
            follow += cameraOffset;
        }
        else {
            follow -= cameraOffset;
        }

        float xDifference = Vector2.Distance(Vector2.right * transform.position.x, Vector2.right * follow.x);
        float yDifference = Vector2.Distance(Vector2.up * transform.position.y, Vector2.up * follow.y);

        Vector3 newPosition = transform.position;
        //if (Mathf.Abs(xDifference) >= _threashold.x)
        
        newPosition.x = follow.x;
        if (Mathf.Abs(yDifference) >= _threashold.y)
            newPosition.y = follow.y;

        float moveSpeed = FollowedObjectRigidBody.velocity.magnitude > speed ? FollowedObjectRigidBody.velocity.magnitude : speed;

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
