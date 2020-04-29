using System.Collections;
using UnityEngine;

/// <summary>
/// Definition of character movement
/// It is mandatory component for character
/// </summary>
public class CharacterMovement : MonoBehaviour
{
    #region Private Members

    /// <summary>
    /// Jump counter for <see cref="multiJump"/> while jumping
    /// </summary>
    private short _multiJumpCounter = 0;

    /// <summary>
    /// Indicates if jump is currently being performed (jump delay animation)
    /// </summary>
    private bool _isPerformingJumpFlag = false;

    /// <summary>
    /// Character fighting component reference
    /// </summary>
    private CharacterFighting _characterFighting;

    /// <summary>
    /// Character evading component reference
    /// </summary>
    private CharacterEvading _characterEvading;

    /// <summary>
    /// Character mortality component reference
    /// </summary>
    private CharacterMortality _characterMortality;

    #endregion

    #region Private Members (Properties)

    /// <inheritdoc cref="HasCrouchPress"/>
    private bool _hasCrouchPress;

    /// <inheritdoc cref="IsInSlide"/>
    private bool _isInSlide;

    #endregion

    #region Public Members (Components)

    /// <summary>
    /// Controlelrs rigid body
    /// </summary>
    [Header("Components")]
    public Rigidbody2D rb;

    /// <summary>
    /// Layer of the ground enviroment
    /// </summary>
    public LayerMask groundLayer;

    /// <summary>
    /// Character holder object reference
    /// </summary>
    public GameObject characterHolderObject;

    #endregion

    #region Public Members (Stats)

    /// <summary>
    /// Speed of horizontal movement
    /// </summary>
    [Header("Stats")]
    public float moveSpeed = 10f;

    /// <summary>
    /// Speed/strength of vertical movement
    /// </summary>
    public float jumpStrength = 8f;

    /// <summary>
    /// Speed/strength of horizontal movement - slide
    /// </summary>
    public float slideStrength = 20f;

    /// <summary>
    /// Slide cooldown
    /// </summary>
    public float slideCooldown = 1.1f;

    /// <summary>
    /// Time during which you cannot interrupt slide from the activation
    /// </summary>
    public float slideForceDuration = 0.3f;

    /// <summary>
    /// How many times you can additionaly press jump in the air
    /// </summary>
    public short multiJump = 1;

    /// <summary>
    /// Modifier to jump while holding jump button.
    /// Higgher number adds more gravity, less otherwise
    /// </summary>
    public readonly float greaterJumpGravityModifier = 0.3f;

    /// <summary>
    /// Modifier to jump while NOT holding jump button.
    /// Higgher number adds more gravity, less otherwise
    /// </summary>
    public readonly float lessJumpGravityModifier = 0.5f;

    #endregion

    #region Public Members (Physics)

    /// <summary>
    /// Maximal allowed speed horizontal speed
    /// </summary>
    [Header("Physics")]
    public float maxMoveSpeed = 7f;

    /// <summary>
    /// Maximal allowed speed horizontal speed in the air
    /// </summary>
    public float maxAirMoveSpeed = 7f;

    /// <summary>
    /// Multiply gravity for falling
    /// </summary>
    public float fallMultiplier = 5f;

    /// <summary>
    /// Gravity definition
    /// </summary>
    public float rbGravity = 1f;

    /// <summary>
    /// Rigid linear drag constant
    /// </summary>
    public float rbLinearDrag = 4f;

    /// <summary>
    /// Horizontal acceleration per unit
    /// </summary>
    public float accelerationHorizontal = 0.025f;

    /// <summary>
    /// Vertical acceleration per unit
    /// </summary>
    public float accelerationVertical = 0.025f;

    #endregion

    #region Public Members (Collisions)

    /// <summary>
    /// Ground detection length trigger.
    /// Draws lines from transform.position down. The end of the lines detects collision with ground.
    /// E.g. it should slightly greater than character collider, in perfect scenario
    /// </summary>
    /// <remarks>
    ///     Drawn by gizmo.
    /// </remarks>
    [Header("Collisions")]
    public float groundDetectionColliderHeight = 0.5f;

    /// <summary>
    /// Offset for the left-side raycast indicator of the ground
    /// TODO: Make spceial wrapper for ground detectors
    /// </summary>
    public Transform groundDetectionLeftColliderOffset;

    /// <summary>
    /// Offset for the right-side raycast indicator of the ground
    /// </summary>
    public Transform groundDetectionRightColliderOffset;

    #endregion

    #region Public Members (Settings)

    /// <summary>
    /// Delay to perform jump
    /// </summary>
    [Header("Settings")]
    public float groundJumpDelay = 0.2f;

    /// <summary>
    /// Indicates, if the character is facing right (TRUE), otherwise left
    /// </summary>
    public bool isFacingRight = true;

    #endregion

    #region Public Properties (Perms)

    /// <summary>
    /// Indicates if rigidbody force is allowed to apply
    /// ---
    /// If character is not fighting currently...
    /// If character is not ground smashing currently...
    /// If character is out of crouch...
    /// If character is not in slide...
    /// </summary>
    /// <remarks>
    ///     You can only flip character, you cannot move if it is NOT allowed.
    /// </remarks>
    public bool IsMovementForceAllowed =>
        (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingFireFlag)) &&
        (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingGroundSmashFlag)) &&
        !HasCrouchPress &&
        !IsInSlide;

    /// <summary>
    /// Indicates if crouch and slide are allowed to perform
    /// ---
    /// If character is not fighting currently...
    /// If character is not air-diving currently...
    /// If character is not ground smashing currently...
    /// If character is not in dodge...
    /// If character is grounded...
    /// </summary>
    public bool IsCrouchSlideAllowed =>
        (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingFireFlag)) &&
        (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingAirDiveFlag)) &&
        (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingGroundSmashFlag)) &&
        (!_characterEvading || (_characterEvading && !_characterEvading.IsInDodge)) &&
        IsGrounded;

    /// <summary>
    /// Indicates if ground jump is alloowed to perform
    /// ---
    /// If not performing ground jump
    /// If character is grounded
    /// If character is not in dodge
    /// </summary>
    public bool IsGroundJumpAllowed =>
        !_isPerformingJumpFlag &&
        IsGrounded &&
        (!_characterEvading || (_characterEvading && !_characterEvading.IsInDodge));

    /// <summary>
    /// Indicates if air jump is alloowed to perform
    /// ---
    /// If not performing ground jump (can happen due to ground jump delay)
    /// If character is not grounded
    /// If character still have multi-jump
    /// </summary>
    public bool IsAirJumpAllowed =>
        !_isPerformingJumpFlag &&
        !IsGrounded &&
        _multiJumpCounter < multiJump;

    #endregion

    #region Public Properties (Cooldown Timers)

    /// <summary>
    /// Timer for slide cooldown (last used)
    /// </summary>
    public float SlideCooldownTimer { get; private set; }

    #endregion

    #region Public Properties

    /// <summary>
    /// Says, if the object is grounded on <see cref="groundLayer"/>
    /// </summary>
    public bool IsGrounded { get; private set; } = false;

    /// <summary>
    /// How long it is since the last jump press
    /// </summary>
    public float JumpTime { get; set; }

    /// <summary>
    /// How long it is since the last slide press
    /// </summary>
    public float SlideTime { get; set; }

    /// <summary>
    /// How long it is since the last crouch press
    /// </summary>
    public float CrouchTime { get; set; }

    /// <summary>
    /// Indicates if jump is pressed
    /// </summary>
    public bool HasJumpPress { get; set; }

    /// <summary>
    /// Indicates if crouch is pressed
    /// </summary>
    public bool HasCrouchPress
    {
        get => _hasCrouchPress;
        set
        {
            if (!value && !IsInSlide && (!_characterFighting || (_characterFighting && !_characterFighting.IsPerformingGroundSmashFlag))) // Wants to return to orig size and character is out of slide too
                ColliderChangeToStand();
            else if (value && IsGrounded && IsCrouchSlideAllowed)
                ColliderChangeToCrouch();

            // Set value
            _hasCrouchPress = value;
        }
    }

    /// <summary>
    /// Indicates if character is currently in slide (TRUE) or not (FALSE)
    /// </summary>
    public bool IsInSlide
    {
        get => _isInSlide;
        private set
        {
            // On value change only...
            if (value != _isInSlide)
            {
                if (!value && !HasCrouchPress) // Wants to return to orig size and character is out of crouch too
                    ColliderChangeToStand();
                else if (value && IsGrounded && IsCrouchSlideAllowed)
                    ColliderChangeToCrouch();
            }

            // Set value
            _isInSlide = value;
        }
    }

    /// <summary>
    /// Movement direction
    /// </summary>
    /// <remarks>
    ///     Modifying this property cause character make a move.
    ///     All values are scaled in range <-1; 1>,
    ///     default 0 = idle
    ///     ---
    ///     X = Horizontal movement
    ///     Y = Vertical movement
    /// </remarks>
    public Vector2 MoveDirection { get; set; }

    #endregion

    #region Events (Triggers)

    /// <summary>
    /// Event trigger - Jump
    /// </summary>
    public event JumpTriggerDelegate JumpTrigger;

    /// <summary>
    /// Delegate event <see cref="JumpTrigger"/>
    /// </summary>
    public delegate void JumpTriggerDelegate();

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        _characterMortality = GetComponent<CharacterMortality>();
        _characterFighting = GetComponent<CharacterFighting>();
        _characterEvading = GetComponent<CharacterEvading>();

        // Initial direction flip
        if (!isFacingRight)
        {
            isFacingRight = !isFacingRight; // Reflip
            FlipDirection();
        }
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        // Save previous grounded state
        bool wasGrounded = IsGrounded;

        // Check grounded state...
        IsGrounded = Physics2D.Raycast(groundDetectionLeftColliderOffset.position + Vector3.down * (groundDetectionColliderHeight / 2f), Vector2.down, groundDetectionColliderHeight / 2f, groundLayer) ||
            Physics2D.Raycast(groundDetectionRightColliderOffset.position + Vector3.down * (groundDetectionColliderHeight / 2f), Vector2.down, groundDetectionColliderHeight / 2f, groundLayer);

        // If we were not in the ground but then detected ground collision (just landed)...
        if (!wasGrounded && IsGrounded)
            // Squezee animation - landing
            StartCoroutine(JumpSqueeze(1.25f, 0.8f, 0.05f));
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        // Make character move
        MoveCharacter();

        // Jump
        if (JumpTime > Time.time) // Elseif cuz we dont want to go into the issue where both jumpts are triggered at the same time (edge of the cliff)
        {
            // Jump - Ground
            if (IsGroundJumpAllowed)
                DoGroundJump();

            // Jump - Air
            else if (IsAirJumpAllowed) // Elseif cuz we dont want to go into the issue where both jumpts are triggered at the same time (edge of the cliff)
                DoAirJump();
        }

        // Slide (crouch)
        if (SlideTime > Time.time && SlideCooldownTimer + slideCooldown < Time.time)
            if (IsCrouchSlideAllowed)
                DoSlide();

        // Crouch
        if (CrouchTime > Time.time)
            if (IsCrouchSlideAllowed)
                DoCrouch();

        // Modify physics
        ModifyPhysics();

        if (IsGrounded) {
            _multiJumpCounter = 0;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Perform ground jump movement
    /// </summary>
    public void DoGroundJump()
    {
        StartCoroutine(GroundJump_Perform());
    }

    /// <summary>
    /// Perform air jump movement
    /// </summary>
    public void DoAirJump()
    {
        AirJump_Perform();
    }

    /// <summary>
    /// Perform slide movement
    /// </summary>
    public void DoSlide()
    {
        Slide_Perform();
    }

    /// <summary>
    /// Perform crouch movement
    /// </summary>
    public void DoCrouch()
    {
        Crouch_Perform();
    }

    /// <summary>
    /// Perform jump movement
    /// </summary>
    /// <param name="jumpVelocity">Custom constant velocity</param>
    public void PerformJump(float jumpVelocity = 0f)
    {
        rb.velocity = new Vector2(jumpVelocity > 0 ? jumpVelocity / 2 * rb.velocity.x : rb.velocity.x, 0);
        rb.AddForce(Vector2.up * (jumpVelocity > 0 ? jumpVelocity : jumpStrength), ForceMode2D.Impulse);
        JumpTime = 0;
        
        // Squezee animation - bounce off
        //StartCoroutine(JumpSqueeze(0.5f, 0.9f, 0.1f));
    }

    public int getAvailableJumps() 
    {
        return multiJump - _multiJumpCounter;
    }

    #endregion

    #region Private Methods (Jump)

    /// <summary>
    /// Handle jump movement - from the ground
    /// </summary>
    private IEnumerator GroundJump_Perform()
    {
        // Trigger jump events at trigger time only
        if (!_isPerformingJumpFlag)
            JumpTrigger();

        // Flag up performing jmup
        _isPerformingJumpFlag = true;

        // Delay
        yield return new WaitForSeconds(groundJumpDelay);

        // Reset multi-jump for air jumps
        _multiJumpCounter = 0;

        // Put the flag down
        _isPerformingJumpFlag = false;

        // Perform jump
        PerformJump();
    }

    /// <summary>
    /// Handle jump movement - above the ground
    /// </summary>
    private void AirJump_Perform()
    {
        // Trigger jump events
        JumpTrigger();

        // Increment multi-jump counter
        _multiJumpCounter++;

        // Perform jump
        PerformJump();
    }

    #endregion

    #region Private Methods (Slide/Crouch)

    /// <summary>
    /// Handle slide movement
    /// </summary>
    private void Slide_Perform()
    {
        // Update cooldown
        SlideCooldownTimer = Time.time;

        rb.velocity = Vector2.zero;
        rb.AddForce((isFacingRight ? Vector2.right : Vector2.left) * slideStrength, ForceMode2D.Impulse);
        SlideTime = 0;

        // Force the slide animation
        StartCoroutine(ForceSlide());
    }

    /// <summary>
    /// Handle crouch
    /// </summary>
    private void Crouch_Perform()
    {
        CrouchTime = 0;
    }

    #endregion

    #region Private Methods (Collider change)

    /// <summary>
    /// Update collider to fit crouch position
    /// TODO: Move character collider updates into separate component (remove it from mortality too)
    /// </summary>
    public void ColliderChangeToCrouch()
    {
        // If character is already in crouch...
        if (!_characterMortality.upperBodyCollider.activeSelf)
            // Ignore
            return;

        // Update collider
        _characterMortality.upperBodyCollider.SetActive(false);
    }

    /// <summary>
    /// Update collider back to original
    /// </summary>
    public void ColliderChangeToStand()
    {
        // If character is already in stand...
        if (_characterMortality.upperBodyCollider.activeSelf)
            // Ignore
            return;

        // Get collider
        var collider = _characterMortality.upperBodyCollider.GetComponent<BoxCollider2D>();
        // Check if there is ground obstacle...
        bool isGround = Physics2D.Raycast(groundDetectionLeftColliderOffset.position + Vector3.down * (collider.size.y), Vector2.up, collider.offset.y + collider.size.y * 1.5f, groundLayer) ||
            Physics2D.Raycast(groundDetectionRightColliderOffset.position + Vector3.down * (collider.size.y), Vector2.up, collider.offset.y + collider.size.y * 1.5f, groundLayer);

        // If there is obstacle...
        if (isGround)
            // Prevent to return into stand
            return;

        // Update collider back to original
        _characterMortality.upperBodyCollider.SetActive(true);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Move character
    /// </summary>
    private void MoveCharacter()
    {
        // Solve correct direction
        if ((MoveDirection.x > 0 && !isFacingRight) || (MoveDirection.x < 0 && isFacingRight))
            FlipDirection();

        // Ignore rest, if movement is not allowed...
        if (!IsMovementForceAllowed)
            return;

        // Add force into our rigid body
        rb.AddForce(Vector2.right * MoveDirection.x * moveSpeed);

        // Check maximal allowed speed...
        if (Mathf.Abs(rb.velocity.x) > (IsGrounded ? maxMoveSpeed : maxAirMoveSpeed))
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * (IsGrounded ? maxMoveSpeed : maxAirMoveSpeed), rb.velocity.y);
    }

    /// <summary>
    /// Flip direction of the character
    /// </summary>
    private void FlipDirection()
    {
        isFacingRight = !isFacingRight;

        // If facing RIGHT...
        if (isFacingRight)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        // Otherwise, facing LEFT
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    /// <summary>
    /// MOdify default physics to our own
    /// </summary>
    private void ModifyPhysics()
    {
        bool changingDirections = (MoveDirection.x > 0 && rb.velocity.x < 0) || (MoveDirection.x < 0 && rb.velocity.x > 0);

        // Grounded state
        if (IsGrounded)
        {
            if (Mathf.Abs(MoveDirection.x) < rbLinearDrag || changingDirections)
                rb.drag = rbLinearDrag;
            else
                rb.drag = 0f;

            rb.gravityScale = 0;
        }
        // Air state
        else
        {
            // Set up gravity
            rb.gravityScale = rbGravity;
            rb.drag = rbLinearDrag * greaterJumpGravityModifier;

            // If falling down...
            if (rb.velocity.y < 0)
                rb.gravityScale = rbGravity * fallMultiplier;
            // If moving up (in jump move) and player do NOT hold the jump key
            else if (rb.velocity.y > 0 && !HasJumpPress)
                rb.gravityScale = rbGravity * (fallMultiplier / lessJumpGravityModifier);
        }
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Force slide to stay in the animation
    /// </summary>
    /// <returns></returns>
    private IEnumerator ForceSlide()
    {
        IsInSlide = true;

        // Delay
        yield return new WaitForSeconds(slideForceDuration);

        IsInSlide = false;
    }

    /// <summary>
    /// Squeeze <see cref="characterHolderObject"/> while jump
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
            characterHolderObject.transform.localScale = Vector3.Lerp(originalSize, newSize, t);
            yield return null;
        }
        t = 0f;
        while (t <= 1.0)
        {
            t += Time.deltaTime / seconds;
            characterHolderObject.transform.localScale = Vector3.Lerp(newSize, originalSize, t);
            yield return null;
        }
    }

    #endregion

    #region Gizmo

    private void OnDrawGizmos()
    {
        // Draw crouch-ground detection
        Gizmos.color = Color.magenta;
        var collider = GetComponent<CharacterMortality>().upperBodyCollider.GetComponent<BoxCollider2D>();
        Gizmos.DrawLine(
            groundDetectionLeftColliderOffset.position + Vector3.down * collider.size.y,
            groundDetectionLeftColliderOffset.position + Vector3.down * collider.size.y + Vector3.up * (collider.offset.y + collider.size.y * 1.5f));
        Gizmos.DrawLine(
            groundDetectionRightColliderOffset.position + Vector3.down * collider.size.y,
            groundDetectionRightColliderOffset.position + Vector3.down * collider.size.y + Vector3.up * (collider.offset.y + collider.size.y * 1.5f));

        // Draw jump collider triggers
        Gizmos.color = Color.red;
        Gizmos.DrawLine(groundDetectionLeftColliderOffset.position + Vector3.down * (groundDetectionColliderHeight / 2f), groundDetectionLeftColliderOffset.position + Vector3.down * groundDetectionColliderHeight);
        Gizmos.DrawLine(groundDetectionRightColliderOffset.position + Vector3.down * (groundDetectionColliderHeight / 2f), groundDetectionRightColliderOffset.position + Vector3.down * groundDetectionColliderHeight);
    }

    #endregion
}
