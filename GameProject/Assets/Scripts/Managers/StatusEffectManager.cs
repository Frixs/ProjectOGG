using System.Linq;
using UnityEngine;

/// <summary>
/// Status effect manager to handle processing status effects
/// </summary>
public class StatusEffectManager
{
    #region Singleton

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static StatusEffectManager Instance { get; } = new StatusEffectManager();

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor - singleton
    /// </summary>
    private StatusEffectManager()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Process all target's status effects
    /// </summary>
    /// <param name="target">Object afflicted by this status effect<</param>
    public void Process(StatusEffectProcessor target)
    {
        if (target == null)
        {
            Debug.unityLogger.Log(LogType.Error, "Null reference while processing status effects!");
            return;
        }

        foreach (AStatusEffectBase se in target.StatusEffectList.ToArray())
        {
            se.Tick(Time.deltaTime);

            if (se.EndFlag)
                target.StatusEffectList.Remove(se);
        }
    }

    /// <summary>
    /// Apply new status effect to target's list of status effects.
    /// ---
    /// This should be the only entry point for adding new status effects!
    /// </summary>
    /// <param name="caster">Object that created this status effect</param>
    /// <param name="target">Object afflicted by this status effect</param>
    /// <param name="se">New status effect to apply</param>
    /// <returns></returns>
    public bool Apply(StatusEffectProcessor target, StatusEffectProcessor caster, AStatusEffectBase se)
    {
        if (target == null || se == null)
        {
            Debug.unityLogger.Log(LogType.Error, "Null reference while attempting to add new status effect!");
            return false;
        }

        // Create unique instance of the status effect
        AStatusEffectBase newStatusEffect = Object.Instantiate(se);
        // Setup status effect
        newStatusEffect.Setup(target, caster);

        // Check if the same type of status effect is already in the target's list
        bool typeOccurrence = target.StatusEffectList.Exists(item => item.IsSameType(newStatusEffect));
        // Check if the same effect is already in the taret's list
        bool effectOccurrence = target.StatusEffectList.Exists(item => item.IsSameEffect(newStatusEffect));

        // Overwrite the status effect if there are any status effects of the same type already in
        if (typeOccurrence && newStatusEffect.overwriteLevel == StatusEffectOverwriteLevel.Type)
            Remove(target, caster, newStatusEffect, StatusEffectOverwriteLevel.Type, false);
        // Overwrite the status effect if there are any status effects of the same effect already in
        else if (effectOccurrence && newStatusEffect.overwriteLevel == StatusEffectOverwriteLevel.Effect)
            Remove(target, caster, newStatusEffect, StatusEffectOverwriteLevel.Effect, false);
        // Don't let it create a new status effect of the same effect if the status effect is NOT stackable and there is already one in
        else if (!newStatusEffect.isStackable && effectOccurrence)
            return false;

        // Add new status effect
        Debug.unityLogger.LogFormat(LogType.Log, "[{0}] Status effect ({1}) applied!", target.name, newStatusEffect.name);
        target.StatusEffectList.Add(newStatusEffect);
        return true;
    }

    /// <summary>
    /// Remove status effect
    /// </summary>
    /// <param name="target">Target afflicted by the status effect</param>
    /// <param name="caster">Source of the status effect</param>
    /// <param name="se">The status effect to remove</param>
    /// <param name="includeCasterImportance">Is caster important for removing formula? Should I remove effect casted from partical caster?</param>
    /// <param name="removeLevel">Level of removing</param>
    public void Remove(StatusEffectProcessor target, StatusEffectProcessor caster, AStatusEffectBase se, StatusEffectOverwriteLevel removeLevel = StatusEffectOverwriteLevel.None, bool includeCasterImportance = false)
    {
        if (target == null || se == null)
        {
            Debug.unityLogger.Log(LogType.Error, "Null reference while attempting to remove status effect!");
            return;
        }

        // Remove all status effects with the same type
        if (removeLevel == StatusEffectOverwriteLevel.Type)
        {
            target.StatusEffectList.RemoveAll(delegate (AStatusEffectBase item)
            {
                // If the status effect is NOT of the same type...
                if (!item.IsSameType(se))
                    return false;

                // Include caster?
                if (includeCasterImportance)
                    if (caster == null || !item.Caster.Equals(caster))
                        return false;

                item.ForceEnd();
                Debug.unityLogger.LogFormat(LogType.Log, "[{0}] Status effect ({1}) removed!", target.name, item.name);
                return true;
            });
        }
        // Remove all status effects with the same effect
        else if (removeLevel == StatusEffectOverwriteLevel.Effect)
        {
            target.StatusEffectList.RemoveAll(delegate (AStatusEffectBase item)
            {
                // If the status effect is NOT of the same effect...
                if (!item.IsSameEffect(se))
                    return false;

                // Include caster?
                if (includeCasterImportance)
                    if (caster == null || !item.Caster.Equals(caster))
                        return false;

                // Force end it
                item.ForceEnd();
                Debug.unityLogger.LogFormat(LogType.Log, "[{0}] Status effect ({1}) removed!", target.name, item.name);
                return true;
            });
        }
        else // None
        {
            // Remove first status effect of the same effect
            var toRemove = target.StatusEffectList.FirstOrDefault(delegate (AStatusEffectBase item)
            {
                // If the status effect is NOT of the same effect...
                if (!item.IsSameEffect(se))
                    return false;

                // Include caster?
                if (includeCasterImportance)
                    if (caster == null || !item.Caster.Equals(caster))
                        return false;

                // Force end it
                item.ForceEnd();
                Debug.unityLogger.LogFormat(LogType.Log, "[{0}] Status effect ({1}) removed!", target.name, item.name);
                return true;
            });

            if (toRemove != null)
                target.StatusEffectList.Remove(toRemove);
        }
    }

    /// <summary>
    /// Remove all status effects which have to be removed on death
    /// </summary>
    /// <param name="target">Object afflicted by this status effect</param>
    public void RemoveOnDeath(StatusEffectProcessor target)
    {
        if (target == null)
        {
            Debug.unityLogger.Log(LogType.Error, "Null reference!");
            return;
        }

        foreach (AStatusEffectBase se in target.StatusEffectList.ToArray())
        {
            if (se.removeOnDeath)
            {
                se.ForceEnd();
                target.StatusEffectList.Remove(se);
            }
        }
    }

    #endregion
}
