using UnityEngine;

/// <summary>
/// Handles camera orthographic size to screen size
/// </summary>
public class CameraScreenScaling : MonoBehaviour
{
    #region Public Members

    /// <summary>
    /// Ingame units to always seen on width (approximately)
    /// </summary>
    [Header("Settings")]
    public short UnitsToBeSeen = 20;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
        Camera.main.orthographicSize = UnitsToBeSeen * Screen.height / Screen.width * 0.5f;
    }

    #endregion
}
