using UnityEngine;

/// <summary>
/// Base logic for status effects
/// </summary>
public abstract class AStatusEffectBase : ScriptableObject
{
    #region Private Members

    /// <summary>
    /// Says TRUE if the status effect is already activated - <see cref="Activate"/>
    /// </summary>
    private bool _isActivated = false;

    /// <summary>
    /// Indicates TRUE if <see cref="Delay"/> is already activated
    /// </summary>
    private bool _isDelayActivated = false;

    /// <summary>
    /// Countdown timer for delay of the status effect
    /// </summary>
    private float _delayTimer; //;

    /// <summary>
    /// Main duration timer for the status effect
    /// </summary>
    private float _durationTimer; //;

    /// <summary>
    /// Timer for repeating status effect (DoT)
    /// </summary>
    private float _repeatTimer; //;

    #endregion

    #region Public Members (Basic Settings)

    /// <summary>
    /// Type of the status effect
    /// Used to distinguish status effects
    /// </summary>
    [Header("Basic Settings")]
    [Tooltip("Used to distinguish status effects")]
    public StatusEffectType type = StatusEffectType.Buff;

    #endregion

    #region Public Members (Effect Duration)

    /// <summary>
    ///Indicates if the status effect lasts forever (TRUE) or not (FALSE)
    /// </summary>
    [Header("Effect Duration")]
    [Tooltip("Indicates if the status effect lasts forever (TRUE) or not (FALSE)")]
    public bool isPermanent = false;

    /// <summary>
    /// Indicates if the status effect should be removed at the death of an object
    /// </summary>
    [Tooltip("Indicates if the status effect should be removed at the death of an object")]
    public bool removeOnDeath = false;

    /// <summary>
    /// Duration of the status effett.
    /// If <see cref="isPermanent"/> is TRUE, this values has no effect
    /// </summary>
    [Tooltip("Duration of the status effett\nIf isPermanent is TRUE, this values has no effect")]
    public float duration = 0;

    #endregion

    #region Public Members (Effect Connections)

    /// <summary>
    /// Indicates, how status effects can be rewriten by other status effects
    /// </summary>
    [Header("Effect Connections")]
    [Tooltip("Indicates, how status effects can be rewriten by other status effects")]
    public StatusEffectOverwriteLevel overwriteLevel = StatusEffectOverwriteLevel.None;

    /// <summary>
    /// Indicates if the status effect of specific effect is stackable (TRUE) or not (FALSE)
    /// </summary>
    [Tooltip("Indicates if the status effect of specific effect is stackable (TRUE) or not (FALSE)")]
    public bool isStackable = false;

    #endregion

    #region Public Members (Effect Timing)

    /// <summary>
    /// Set delay before activation (0 = no delay)
    /// </summary>
    [Header("Effect Timing")]
    [Tooltip("Set delay before activation (0 = no delay)")]
    public float startDelay = 0;

    /// <summary>
    /// Set repeat interval for repeating event (DoT) - (0 = no repeat effect)
    /// </summary>
    [Tooltip("Set repeat interval for repeating event (DoT) - (0 = no repeat effect)")]
    public float repeatTime = 0;

    #endregion

    #region Public Properties

    /// <summary>
    /// Indicates if the status effect should be removed from an object
    /// </summary>
    public bool EndFlag { get; private set; } = false;

    /// <summary>
    /// Object afflicted by this status effect
    /// </summary>
    public StatusEffectProcessor Target { get; protected set; } = null;

    /// <summary>
    /// Object that created this status effect
    /// </summary>
    public StatusEffectProcessor Caster { get; protected set; } = null;

    #endregion

    #region Constructor

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="target">Object affected by this status effect</param>
    /// <param name="caster">Object that created this status effect</param>
    public AStatusEffectBase(StatusEffectProcessor target, StatusEffectProcessor caster)
    {
        Setup(target, caster);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Intialize status effect with default values
    /// </summary>
    /// <param name="target">Object affected by this status effect</param>
    /// <param name="caster">Object that created this status effect</param>
    public void Setup(StatusEffectProcessor target, StatusEffectProcessor caster)
    {
        // Ignoring, if target is already set...
        if (Target != null)
            return;

        // Set Target/Caster
        Target = target;
        Caster = caster;

        // Make sure these values are set with default value
        EndFlag = false;
        _isActivated = false;
        _isDelayActivated = false;

        // Set default values
        _durationTimer = duration;
        _delayTimer = startDelay;
        _repeatTimer = repeatTime;
    }

    /// <summary>
    /// Tick event for update method
    /// </summary>
    /// <param name="delta"></param>
    public void Tick(float delta)
    {
        // Delay
        if (_delayTimer > 0f)
        {
            if (!_isDelayActivated)
            {
                _isDelayActivated = true;
                Delay();
            }

            _delayTimer -= delta;

            return;
        }
        
        // Activate status effect
        if (!_isActivated)
        {
            _isActivated = true;
            Activate();
        }

        // Check if status effect is NOT permanent, i.e. timed
        if (!isPermanent)
        {
            if (_durationTimer <= 0f)
            {
                EndFlag = true;
                End();
                Debug.unityLogger.LogFormat(LogType.Log, "[{0}] Status effect ({1}) ended!", Target.name, this.name);
                return;
            }

            _durationTimer -= delta;
        }

        // Repeating process for permanent status effect
        if (repeatTime > 0f)
        {
            if (_repeatTimer <= 0f)
            {
                _repeatTimer = repeatTime;
                Repeat();
            }

            _repeatTimer -= delta;
        }
    }

    /// <summary>
    /// Force end the status effect
    /// </summary>
    public void ForceEnd()
    {
        EndFlag = true;
        End();
    }

    #endregion

    #region Control Methods (Abstract)

    /// <summary>
    /// Delay procedure
    /// Runs if the status effect has set delay
    /// </summary>
    protected abstract void Delay();

    /// <summary>
    /// Activate procedure
    /// Runs on each start of status effects
    /// </summary>
    protected abstract void Activate();

    /// <summary>
    /// End procedure
    /// Runs on each end of status effects
    /// </summary>
    protected abstract void End();

    /// <summary>
    /// Repeat procedure
    /// Runs for repeating status effects (DoT)
    /// </summary>
    protected abstract void Repeat();

    #endregion

    #region Public Helper Methods

    /// <summary>
    /// Compare 2 status effects and say if the status effects are of the same type or not
    /// </summary>
    /// <param name="se">The status effect to compare</param>
    /// <returns>TRUE same, FALSE otherwise</returns>
    public bool IsSameType(AStatusEffectBase se)
    {
        return GetType().Equals(se.GetType());
    }

    /// <summary>
    /// Compare 2 status effects and say if the status effects are of the same effect or not
    /// </summary>
    /// <param name="se">The status effect to compare</param>
    /// <returns>TRUE same, FALSE otherwise</returns>
    public bool IsSameEffect(AStatusEffectBase se)
    {
        return name.Equals(se.name);
    }

    #endregion
}
