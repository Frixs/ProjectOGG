using UnityEngine;

/// <summary>
/// Unarm character
/// </summary>
[CreateAssetMenu(menuName = "StatusEffect/Character/Unarm")]
public class CharacterUnarm : AStatusEffectBase
{
    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="caster">Object that created this status effect</param>
    /// <param name="target">Object afflicted by this status effect</param>
    public CharacterUnarm(StatusEffectProcessor target, StatusEffectProcessor caster) : base(target, caster)
    {
    }

    #endregion

    #region Control Methods

    /// <inheritdoc/>
    protected override void Activate()
    {
        Target.GetComponent<CharacterFighting>().enabled = false;
    }

    /// <inheritdoc/>
    protected override void Delay()
    {
    }

    /// <inheritdoc/>
    protected override void End()
    {
        Target.GetComponent<CharacterFighting>().enabled = true;
    }

    /// <inheritdoc/>
    protected override void Repeat()
    {
    } 

    #endregion
}
