using UnityEngine;

/// <summary>
/// Gives animation to a chracter
/// </summary>
public class CharacterAnimation : MonoBehaviour
{
    #region Private Members (consts)

    /// <summary>
    /// Animator parameter - Horizontal movement
    /// </summary>
    private readonly string AnimatorParameterHorizontal = "Horizontal";

    /// <summary>
    /// Animator parameter - Vertical movement
    /// </summary>
    private readonly string AnimatorParameterVertical = "Vertical";

    /// <summary>
    /// Animator parameter - Ranged Attack Trigger
    /// </summary>
    private readonly string AnimatorParameterAttackRangedTrigger = "AttackRangedTrigger";

    /// <summary>
    /// Animator parameter - Melee Attack Trigger
    /// </summary>
    private readonly string AnimatorParameterAttackMeleeTrigger = "AttackMeleeTrigger";

    /// <summary>
    /// Animator parameter - Jump Trigger
    /// </summary>
    private readonly string AnimatorParameterJumpTrigger = "JumpTrigger";

    /// <summary>
    /// Animator parameter - Jump
    /// </summary>
    private readonly string AnimatorParameterIsInFlight = "IsInFlight";

    /// <summary>
    /// Animator parameter - Death
    /// </summary>
    private readonly string AnimatorParameterIsDeath = "IsDeath";

    /// <summary>
    /// Animator parameter - Dodge
    /// </summary>
    private readonly string AnimatorParameterIsInDodge = "IsInDodge";

    /// <summary>
    /// Animator parameter - Crouching
    /// </summary>
    private readonly string AnimatorParameterIsCrouching = "IsCrouching";

    /// <summary>
    /// Animator parameter - IsInFire
    /// </summary>
    private readonly string AnimatorParameterIsPerformingFire = "IsPerformingFire";

    #endregion

    #region Public Members

    /// <summary>
    /// Animator reference
    /// </summary>
    [Header("Components")]
    public Animator animator;

    /// <summary>
    /// Character movement component reference
    /// </summary>
    public CharacterMovement characterMovement = null;

    /// <summary>
    /// Character mortality component reference
    /// </summary>
    public CharacterMortality characterMortality = null;

    /// <summary>
    /// Character fighting component reference
    /// </summary>
    public CharacterFighting characterFighting = null;

    /// <summary>
    /// Character evading component reference
    /// </summary>
    public CharacterEvading characterEvading = null;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        // Add trigger - Jump
        characterMovement.JumpTrigger += delegate { animator.SetTrigger(AnimatorParameterJumpTrigger); };

        // Add trigger - Ranged Fire
        if (characterFighting) characterFighting.RangedFireTrigger += delegate { animator.SetTrigger(AnimatorParameterAttackRangedTrigger); };

        // Add trigger - Melee Fire
        if (characterFighting) characterFighting.MeleeFireTrigger += delegate { animator.SetTrigger(AnimatorParameterAttackMeleeTrigger); };
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        // Set animation parameters...

        // Death
        animator.SetBool(AnimatorParameterIsDeath, characterMortality.IsDeath);

        // Horizontal movement
        animator.SetFloat(AnimatorParameterHorizontal, Mathf.Abs(characterMovement.MoveDirection.x));

        // Vertical movement
        animator.SetFloat(AnimatorParameterVertical, characterMovement.MoveDirection.y);

        // Jump
        animator.SetBool(AnimatorParameterIsInFlight, !characterMovement.IsGrounded);

        // Dodge
        if (characterEvading) animator.SetBool(AnimatorParameterIsInDodge, characterEvading.IsInDodge);

        // Crouch
        animator.SetBool(AnimatorParameterIsCrouching, characterMovement.HasCrouchPress || characterMovement.IsInSlide);

        // IsPerformingFire
        animator.SetBool(AnimatorParameterIsPerformingFire, characterFighting.IsPerformingFireFlag);
    }

    #endregion
}
