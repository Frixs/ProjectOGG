/// <summary>
/// Level that defines overwrite rules of status effects
/// </summary>
public enum StatusEffectOverwriteLevel
{
    /// <summary>
    /// No overwrite rules
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates if the status effect should rewrite all status effects of the same particular effect on the same object
    /// e.g. When one of the scriptable object (SO) status effects created from <see cref="CharacterInvulnerability"/> is applied... 
    /// ... It rewrites (removes) all status effects of the type (specific SO) of the currently applying status effect created from <see cref="CharacterInvulnerability"/>
    /// --- 
    /// e.g. 2: Let's have... Invulnerability : AStatusEffectBase. 
    /// We created 3 specific Invulnerabilities from the Invulnerability - Invul1, Invul2, Invul3. 
    /// When we apply new status effect Invul2, all status effects Invul2 will be removed, if any applied.
    /// </summary>
    Effect = 1,

    /// <summary>
    /// Indicates if the status effect should rewrite all status effects of the same type on the same object when applied.
    /// e.g. When one of the scriptable object status effects created from <see cref="CharacterInvulnerability"/> is applied... 
    /// ... It rewrites (removes) all status effects created from <see cref="CharacterInvulnerability"/> that are currently on affected object.
    /// --- 
    /// e.g. 2: Let's have... Invulnerability : AStatusEffectBase. 
    /// We created 3 specific Invulnerabilities from the Invulnerability - Invul1, Invul2, Invul3. 
    /// When we apply new status effect Invul2, all status effects Invul1, Invul2 and Invul3 will be removed, if any applied.
    /// </summary>
    Type = 2,
}
