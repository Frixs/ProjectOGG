using UnityEngine;

/// <summary>
/// Gives player ability to control character
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Private Constants

    /// <summary>
    /// Action delay for fixed update (update method delay - anti-lag)
    /// </summary>
    private readonly float _actionDelay = 0.25f;

    #endregion

    #region Private Members

    /// <summary>
    /// Character movement reference
    /// </summary>
    private CharacterMovement _characterMovement;

    /// <summary>
    /// Character fighting reference
    /// </summary>
    private CharacterFighting _characterFighting;

    /// <summary>
    /// Character evading reference
    /// </summary>
    private CharacterEvading _characterEvading;

    /// <summary>
    /// Character mortality reference
    /// </summary>
    private CharacterMortality _characterMortality;

    #endregion

    #region Public Members (Controls)

    /// <summary>
    /// Input: Move TO LEFT
    /// </summary>
    [Header("Controls")]
    public KeyCode keyControlLeft = KeyCode.A;

    /// <summary>
    /// Input: Move UP
    /// </summary>
    public KeyCode keyControlUp = KeyCode.W;

    /// <summary>
    /// Input: Move TO RIGHT
    /// </summary>
    public KeyCode keyControlRight = KeyCode.D;

    /// <summary>
    /// Input: Move DOWN
    /// </summary>
    public KeyCode keyControlDown = KeyCode.S;

    /// <summary>
    /// Input: Fire
    /// </summary>
    public KeyCode keyControlFire = KeyCode.LeftShift;

    /// <summary>
    /// Input: Dodge
    /// </summary>
    public KeyCode keyControlDodge = KeyCode.LeftControl;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        _characterMovement = GetComponent<CharacterMovement>();
        _characterFighting = GetComponent<CharacterFighting>();
        _characterEvading = GetComponent<CharacterEvading>();
        _characterMortality = GetComponent<CharacterMortality>();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    private void Update()
    {
        // If character is death...
        if (_characterMortality.IsDeath)
        {
            // Stop moving
            _characterMovement.MoveDirection = new Vector2(0, 0);
            // Interrupt all other actions
            return;
        }

        // Handle movement directions
        HandlePlayerMovement();

        // Jump
        if (Input.GetKeyDown(keyControlUp))
            _characterMovement.JumpTime = Time.time + _actionDelay;
        // Jump - Pressed
        if (Input.GetKey(keyControlUp))
            _characterMovement.HasJumpPress = true;
        else
            _characterMovement.HasJumpPress = false;

        // Slide (crouch)
        if (Input.GetKeyDown(keyControlDown) && (Input.GetKey(keyControlLeft) || Input.GetKey(keyControlRight)))
            _characterMovement.SlideTime = Time.time + _actionDelay;
        // Crouch
        else if (Input.GetKey(keyControlDown))
            _characterMovement.CrouchTime = Time.time + _actionDelay;
        // Crouch - Pressed
        if (Input.GetKey(keyControlDown))
            _characterMovement.HasCrouchPress = true;
        else
            _characterMovement.HasCrouchPress = false;

        // Fire (+ check if fighting component is available)
        if (Input.GetKey(keyControlFire) && _characterFighting != null)
            _characterFighting.FireTime = Time.time + _actionDelay;

        // Air Dive (+ check if fighting component is available)
        if (Input.GetKey(keyControlDown) && _characterFighting != null)
            _characterFighting.AirDiveTime = Time.time + _actionDelay;

        // Dodge (+ check if evading component is available)
        if (Input.GetKey(keyControlDodge) && _characterEvading != null)
            _characterEvading.DodgeTime = Time.time + _actionDelay;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Handle movement of the player
    /// </summary>
    private void HandlePlayerMovement()
    {
        // Horizontal movement
        // Left
        if (Input.GetKey(keyControlLeft))
            if (_characterMovement.MoveDirection.x > 0)
                _characterMovement.MoveDirection = new Vector2(
                    _characterMovement.accelerationHorizontal * (-1),
                    _characterMovement.MoveDirection.y);
            else
                _characterMovement.MoveDirection = new Vector2(
                    _characterMovement.MoveDirection.x - _characterMovement.accelerationHorizontal,
                    _characterMovement.MoveDirection.y);
        // Right
        else if (Input.GetKey(keyControlRight))
            if (_characterMovement.MoveDirection.x < 0)
                _characterMovement.MoveDirection = new Vector2(
                    _characterMovement.accelerationHorizontal,
                    _characterMovement.MoveDirection.y);
            else
                _characterMovement.MoveDirection = new Vector2(
                    _characterMovement.MoveDirection.x + _characterMovement.accelerationHorizontal,
                    _characterMovement.MoveDirection.y);
        // None
        else
            _characterMovement.MoveDirection = new Vector2(0, _characterMovement.MoveDirection.y);

        // Vertical movement
        // Down
        if (Input.GetKey(keyControlDown))
            if (_characterMovement.MoveDirection.y > 0)
                _characterMovement.MoveDirection = new Vector2(
                    _characterMovement.MoveDirection.x,
                    _characterMovement.accelerationVertical * (-1));
            else
                _characterMovement.MoveDirection = new Vector2(
                    _characterMovement.MoveDirection.x,
                    _characterMovement.MoveDirection.y - _characterMovement.accelerationVertical);
        // Up
        else if (Input.GetKey(keyControlUp))
            if (_characterMovement.MoveDirection.y < 0)
                _characterMovement.MoveDirection = new Vector2(
                    _characterMovement.MoveDirection.x,
                    _characterMovement.accelerationVertical);
            else
                _characterMovement.MoveDirection = new Vector2(
                    _characterMovement.MoveDirection.x,
                    _characterMovement.MoveDirection.y + _characterMovement.accelerationVertical);
        // None
        else
            _characterMovement.MoveDirection = new Vector2(_characterMovement.MoveDirection.x, 0);

        // Check we do not go over the limits...
        if (_characterMovement.MoveDirection.x > 1f)
            _characterMovement.MoveDirection = new Vector2(1f, _characterMovement.MoveDirection.y);
        else if (_characterMovement.MoveDirection.x < -1f)
            _characterMovement.MoveDirection = new Vector2(-1f, _characterMovement.MoveDirection.y);
        if (_characterMovement.MoveDirection.y > 1f)
            _characterMovement.MoveDirection = new Vector2(_characterMovement.MoveDirection.x, 1f);
        else if (_characterMovement.MoveDirection.y < -1f)
            _characterMovement.MoveDirection = new Vector2(_characterMovement.MoveDirection.x, -1f);
    }

    #endregion
}
