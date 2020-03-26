using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles processing status effects
/// Put this on object you want to add status effects om
/// </summary>
public class StatusEffectProcessor : MonoBehaviour
{
    #region Public Properties

    /// <summary>
    /// List of all status effects currently active
    /// </summary>
    public List<AStatusEffectBase> StatusEffectList { get; } = new List<AStatusEffectBase>();

    #endregion

    #region Unity Engine

    /// <summary>
    /// Fixed update
    /// </summary>
    void FixedUpdate()
    {
        // Process all character's status effects
        StatusEffectManager.Instance.Process(this);
    } 

    #endregion
}
