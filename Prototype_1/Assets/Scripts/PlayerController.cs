using System.Collections;
using UnityEngine;

/// <summary>
/// Option to give player control over an object
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Private Members

    /// <summary>
    /// Animator parameter
    /// </summary>
    private readonly string _animatorParameterHorizontal = "Horizontal";

    /// <summary>
    /// Animator parameter
    /// </summary>
    private readonly string _animatorParameterVertical = "Vertical";

    /// <summary>
    /// Sayss, if the character is facing right (TRUE), otherwise left
    /// </summary>
    private bool _facingRight = true;

    /// <summary>
    /// Trigger on what value the linear drag stop working
    /// </summary>
    private readonly float _linearDragTrigger = 0.1f;

    /// <summary>
    /// Direction of the movement
    /// </summary>
    private Vector2 _direction;

    /// <summary>
    /// Modifier to jump while holding jump button.
    /// Higgher number adds more gravity, less otherwise
    /// </summary>
    private readonly float _jumpModifierLong = 0.3f;

    /// <summary>
    /// Modifier to jump while NOT holding jump button.
    /// Higgher number adds more gravity, less otherwise
    /// </summary>
    private readonly float _jumpModifierShort = 0.5f;

    /// <summary>
    /// Jump counter for <see cref="multiJump"/> while jumping
    /// </summary>
    private short _multiJumpCounter = 0;

    /// <summary>
    /// How long it is since the last press jump button
    /// </summary>
    private float _jumpTimer;

    /// <summary>
    /// How long it is since the last press dodge button
    /// </summary>
    private float _dodgeTimer;

    /// <summary>
    /// Timer for fire speed (last fired)
    /// </summary>
    private float _fireSpeedTimer;

    /// <summary>
    /// Timer for dodge cooldown (last used)
    /// </summary>
    private float _dodgeCooldownTimer;

    /// <summary>
    /// Action delay for fixed update
    /// </summary>
    private float _actionDelay = 0.25f;

    /// <summary>
    /// Fire point offset
    /// </summary>
    private Vector3 _firePointOffset = new Vector3(0.5f, 0f, 0f);

    #endregion

    #region Public Members

    /// <summary>
    /// MOve to left - input
    /// </summary>
    [Header("Controls")]
    public KeyCode keyControlLeft = KeyCode.A;

    /// <summary>
    /// Move up - input
    /// </summary>
    public KeyCode keyControlUp = KeyCode.W;

    /// <summary>
    /// Move to right - input
    /// </summary>
    public KeyCode keyControlRight = KeyCode.D;

    /// <summary>
    /// Move down - input
    /// </summary>
    public KeyCode keyControlDown = KeyCode.S;

    /// <summary>
    /// Fire - input
    /// </summary>
    public KeyCode keyControlFire = KeyCode.LeftShift;

    /// <summary>
    /// Dodge - input
    /// </summary>
    public KeyCode keyControlDodge = KeyCode.LeftControl;

    /// <summary>
    /// Controllers animator
    /// </summary>
    [Header("Components")]
    public Animator animator;

    /// <summary>
    /// Controlelrs rigid body
    /// </summary>
    public Rigidbody2D rb;

    /// <summary>
    /// Layer of the ground enviroment
    /// </summary>
    public LayerMask groundLayer;

    /// <summary>
    /// Character holder object
    /// </summary>
    public GameObject characterHolder;

    /// <summary>
    /// The projectile
    /// </summary>
    public GameObject projectilePrefab;

    /// <summary>
    /// Fire speed
    /// </summary>
    [Header("Combat Stats")]
    public float fireSpeed = 1.0f;

    /// <summary>
    /// Dodge cooldown
    /// </summary>
    public float dodgeCooldown = 2.0f;

    /// <summary>
    /// Dodge duration
    /// </summary>
    public float dodgeDuration = 0.7f;

    /// <summary>
    /// Movement speed
    /// </summary>
    [Header("Horizontal Movement")]
    public float movementSpeed = 10f;

    /// <summary>
    /// Speed of the jump movement
    /// </summary>
    [Header("Vertical Movement")]
    public float jumpSpeed = 8f;

    /// <summary>
    /// How many times you can additionaly press jump in the air
    /// </summary>
    public short multiJump = 1;

    /// <summary>
    /// Maximal allowed speed
    /// </summary>
    [Header("Physics")]
    public float maxSpeed = 7f;

    /// <summary>
    /// Rigid linear drag
    /// </summary>
    public float linearDrag = 4f;

    /// <summary>
    /// Gravity definition
    /// </summary>
    public float gravity = 1f;

    /// <summary>
    /// Multiply gravity for falling
    /// </summary>
    public float fallMultiplier = 5f;

    /// <summary>
    /// Horizontal input acceleration
    /// </summary>
    public float accelerationHorizontal = 0.025f;

    /// <summary>
    /// Vertical input acceleration
    /// </summary>
    public float accelerationVertical = 0.025f;

    /// <summary>
    /// Says, if the object is grounded on <see cref="groundLayer"/>
    /// </summary>
    [Header("Collisions")]
    public bool isGrounded = false;

    /// <summary>
    /// Ground detection length trigger
    /// </summary>
    public float groundLength = 0.5f;

    /// <summary>
    /// Offset between raycast indicators of the ground
    /// </summary>
    public Vector3 colliderOffset;

    #endregion

    #region Public Properties

    public bool IsInDodge { get; private set; }

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {

    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        bool wasOnGround = isGrounded;
        isGrounded = Physics2D.Raycast(transform.position + colliderOffset, Vector2.down, groundLength, groundLayer) || Physics2D.Raycast(transform.position - colliderOffset, Vector2.down, groundLength, groundLayer);
        
        // If we were not in the ground but then detected ground collision (just landed)...
        if (!wasOnGround && isGrounded)
            StartCoroutine(JumpSqueeze(1.25f, 0.8f, 0.05f));

        // Jump
        if (Input.GetKey(keyControlUp))
            _jumpTimer = Time.time + _actionDelay;

        // Fire
        if (Input.GetKey(keyControlFire) && _fireSpeedTimer + fireSpeed < Time.time)
        {
            Fire();
            _fireSpeedTimer = Time.time;
        }

        // Dodge
        if (Input.GetKey(keyControlDodge))
            _dodgeTimer = Time.time + _actionDelay;

        // Handle movement directions
        HandlePlayerMovement();
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        // Move
        MoveCharacter(_direction.x);

        // Jump
        if (_jumpTimer > Time.time && (isGrounded || _multiJumpCounter < multiJump))
            Jump();

        // Dodge
        if (_dodgeTimer > Time.time && _dodgeCooldownTimer + dodgeCooldown < Time.time)
            Dodge();

        // Modify physics
        ModifyPhysics();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Move character
    /// </summary>
    /// <param name="horizontal">Horizontal movement</param>
    private void MoveCharacter(float horizontal)
    {
        // Add force into our rigid body
        rb.AddForce(Vector2.right * horizontal * movementSpeed);

        // Set animation parameters
        animator.SetFloat(_animatorParameterHorizontal, Mathf.Abs(_direction.x));
        animator.SetFloat(_animatorParameterVertical, _direction.y);

        // Solve correct direction
        if ((horizontal > 0 && !_facingRight) || (horizontal < 0 && _facingRight))
            FlipDirection();

        // Check maximal allowed speed...
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
    }

    /// <summary>
    /// Fire an projectile
    /// </summary>
    private void Fire()
    {
        var prefab = Instantiate(projectilePrefab, _facingRight ? transform.position + _firePointOffset : transform.position - _firePointOffset, Quaternion.identity);
        prefab.transform.rotation = Quaternion.Euler(0, _facingRight ? 0 : 180, 0);
        prefab.GetComponent<Projectile>().isFacingRight = _facingRight;
    }

    /// <summary>
    /// Fire an projectile
    /// </summary>
    private void Dodge()
    {
        // Update cooldown
        _dodgeCooldownTimer = Time.time;
        // Start it
        StartCoroutine(RunDodge());
    }

    /// <summary>
    /// Handle jump movement
    /// </summary>
    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        _multiJumpCounter++;
        _jumpTimer = 0;

        StartCoroutine(JumpSqueeze(0.5f, 0.9f, 0.1f));
    }

    /// <summary>
    /// Flip direction of the character
    /// </summary>
    private void FlipDirection()
    {
        _facingRight = !_facingRight;
        transform.rotation = Quaternion.Euler(0, _facingRight ? 0 : 180, 0);
    }

    /// <summary>
    /// MOdify default physics to our own
    /// </summary>
    private void ModifyPhysics()
    {
        bool changingDirections = (_direction.x > 0 && rb.velocity.x < 0) || (_direction.x < 0 && rb.velocity.x > 0);

        // Grounded state
        if (isGrounded)
        {
            if (Mathf.Abs(_direction.x) < _linearDragTrigger || changingDirections)
                rb.drag = linearDrag;
            else
                rb.drag = 0f;

            rb.gravityScale = 0;
            _multiJumpCounter = 0;
        }
        // Air state
        else
        {
            // Set up gravity
            rb.gravityScale = gravity;
            rb.drag = linearDrag * _jumpModifierLong;

            // If falling down...
            if (rb.velocity.y < 0)
                rb.gravityScale = gravity * fallMultiplier;
            // If moving up (in jump move) and player do NOT hold the jump key
            else if (rb.velocity.y > 0 && !Input.GetKey(keyControlUp))
                rb.gravityScale = gravity * (fallMultiplier / _jumpModifierShort);
        }
    }

    /// <summary>
    /// Squeeze character while jump
    /// </summary>
    /// <param name="xSqueeze">X modifier</param>
    /// <param name="ySqueeze">Y modifier</param>
    /// <param name="seconds">Squeeze time</param>
    /// <returns>Yield coroutine</returns>
    private IEnumerator JumpSqueeze(float xSqueeze, float ySqueeze, float seconds)
    {
        Vector3 originalSize = Vector3.one;
        Vector3 newSize = new Vector3(xSqueeze, ySqueeze, originalSize.z);
        float t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            characterHolder.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            yield return null;
        }
    }

    /// <summary>
    /// Perform a dodge
    /// </summary>
    /// <returns>Yield coroutine</returns>
    private IEnumerator RunDodge()
    {
        animator.GetComponent<SpriteRenderer>().color = Color.red;
        IsInDodge = true;

        float t = 0f;
        while (t <= dodgeDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        animator.GetComponent<SpriteRenderer>().color = Color.white;
        IsInDodge = false;
    }

    /// <summary>
    /// Handle movement of the player
    /// </summary>
    private void HandlePlayerMovement()
    {
        // Horizontal
        if (Input.GetKey(keyControlLeft))
            if (_direction.x > 0)
                _direction.x = -accelerationHorizontal;
            else
                _direction.x -= accelerationHorizontal;
        else if (Input.GetKey(keyControlRight))
            if (_direction.x < 0)
                _direction.x = accelerationHorizontal;
            else
                _direction.x += accelerationHorizontal;
        else
            _direction.x = 0f;

        // Vertical
        if (Input.GetKey(keyControlDown))
            if (_direction.y > 0)
                _direction.y = -accelerationVertical;
            else
                _direction.y -= accelerationVertical;
        else if (Input.GetKey(keyControlUp))
            if (_direction.y < 0)
                _direction.y = accelerationVertical;
            else
                _direction.y += accelerationVertical;
        else
            _direction.y = 0f;

        // Check we do not go over the limits...
        if (_direction.x > 1f)
            _direction.x = 1f;
        else if (_direction.x < -1f)
            _direction.x = -1f;
        if (_direction.y > 1f)
            _direction.y = 1f;
        else if (_direction.y < -1f)
            _direction.y = -1f;
    }

    #endregion

    #region Gizmo

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + colliderOffset, transform.position + colliderOffset + Vector3.down * groundLength);
        Gizmos.DrawLine(transform.position - colliderOffset, transform.position - colliderOffset + Vector3.down * groundLength);
    }

    #endregion
}
