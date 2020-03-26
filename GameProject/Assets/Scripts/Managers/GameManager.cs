using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main game loop/logic
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Private Members

    /// <summary>
    /// Static instance of GameManager which allows us toaccess it in any other script.
    /// </summary>
    public static GameManager Instance = null;

    #endregion

    #region Public Members (Settings)

    /// <summary>
    /// Defualt respawn time
    /// </summary>
    [Header("Settings")]
    public float respawnTimeDefault = 3f;

    /// <summary>
    /// Respawn time value as additive as death penalty
    /// </summary>
    public float respawnTimeDeathPenalty = 0.2f;

    /// <summary>
    /// Melee distance to trigger melee attacks
    /// </summary>
    public float meleeDistance = 3.7f;

    #endregion

    #region Public Properties

    /// <summary>
    /// Player 1 GO
    /// </summary>
    public GameObject Player1 { get; private set; }

    /// <summary>
    /// Player 2 GO
    /// </summary>
    public GameObject Player2 { get; private set; }

    /// <summary>
    /// Spawn points for <see cref="Player1"/>
    /// </summary>
    public List<Transform> Player1SpawnPointList { get; private set; }

    /// <summary>
    /// Spawn points for <see cref="Player2"/>
    /// </summary>
    public List<Transform> Player2SpawnPointList { get; private set; }

    /// <summary>
    /// Player 1 statistics collection
    /// </summary>
    public GameStatistics Player1Statistics { get; private set; } = new GameStatistics();

    /// <summary>
    /// Player 2 statistics collection
    /// </summary>
    public GameStatistics Player2Statistics { get; private set; } = new GameStatistics();

    /// <summary>
    /// Indicates current room by number
    /// </summary>
    /// <remarks>
    ///     -2 Player1 home
    ///     -1
    ///      0 = start (middle)
    ///      1
    ///      2 = Player2 home
    /// </remarks>
    public short CurrentRoom { get; set; } = 0;

    /// <summary>
    /// Indicates any player death
    /// </summary>
    public bool AnyPlayerDeath => Player1.GetComponent<CharacterMortality>().IsDeath || Player2.GetComponent<CharacterMortality>().IsDeath; // TODO: load and save components in the manager

    /// <summary>
    /// Get distance between players
    /// </summary>
    public float GetDistanceBetweenPlayers => Vector3.Distance(Player1.transform.position, Player2.transform.position);

    /// <summary>
    /// Indicates melee distance between players
    /// </summary>
    public bool IsMeleeDistance => GetDistanceBetweenPlayers <= meleeDistance;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Awake is always called before any Start functions
    /// </summary>
    private void Awake()
    {
        // Check if instance already exists.
        if (Instance == null)
            // If not, set instance to this.
            Instance = this;
        // If instance already exists and it's not this.
        else if (Instance != this)
            // Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        // Sets this to not be destroyed when reloading scene.
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// TODO: this and <see cref="Start"/> has to be changed. This is just temporary solution!!!
    /// </summary>
    /// <param name="level"></param>
    private void OnLevelWasLoaded(int level)
    {
        // Reset players
        Player1 = null;
        Player2 = null;

        // Load players
        Player1 = GameObject.Find("Player_1");
        Player2 = GameObject.Find("Player_2");

        // Reset spawnpoints
        Player1SpawnPointList = new List<Transform>();
        Player2SpawnPointList = new List<Transform>();

        // Load spawn points
        foreach (Transform child in GameObject.Find("SpawnPointsP1").transform)
            Player1SpawnPointList.Add(child);
        foreach (Transform child in GameObject.Find("SpawnPointsP2").transform)
            Player2SpawnPointList.Add(child);
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        // Reset players
        Player1 = null;
        Player2 = null;

        // Load players
        Player1 = GameObject.Find("Player_1");
        Player2 = GameObject.Find("Player_2");
        
        // Reset spawnpoints
        Player1SpawnPointList = new List<Transform>();
        Player2SpawnPointList = new List<Transform>();

        // Load spawn points
        foreach (Transform child in GameObject.Find("SpawnPointsP1").transform)
            Player1SpawnPointList.Add(child);
        foreach (Transform child in GameObject.Find("SpawnPointsP2").transform)
            Player2SpawnPointList.Add(child);
    }

    /// <summary>
    /// Fixed update
    /// </summary>
    private void FixedUpdate()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starting new game and restart the game manager
    /// </summary>
    public void NewGame()
    {
        // Clear statistics
        Player1Statistics = new GameStatistics();
        Player2Statistics = new GameStatistics();
    }

    /// <summary>
    /// Rebirth the player
    /// </summary>
    /// <param name="playerName">Player name</param>
    public void Rebirth(string playerName)
    {
        // Get the player values
        var isItPlayer1 = IsItPlayer1(playerName);
        var player = isItPlayer1 ? Player1 : Player2;
        var playerStatistics = isItPlayer1 ? Player1Statistics : Player2Statistics;

        // Make the character die
        player.GetComponent<CharacterMortality>().Die();

        // Allow camera movement
        Camera.main.GetComponent<FollowingCamera>().IsFollowingAllowed = true;
        // Specify which player can move the camera (the one that is alive)
        Camera.main.GetComponent<FollowingCamera>().FollowFirstObject = !isItPlayer1;

        // Count death
        playerStatistics.IncrementPlayerDeathCount();

        // Calculate respawn time
        var respawnTime = respawnTimeDefault + (playerStatistics.TemporaryDeathCount * respawnTimeDeathPenalty);

        // Start respawn routine
        StartCoroutine(RespawnRoutine(player, respawnTime));
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Check if the name belongs to player 1 (TRUE) or not (FALSE)
    /// </summary>
    /// <param name="playerName">The player name</param>
    /// <returns>The name belongs to player 1 (TRUE), otherwise FALSE</returns>
    private bool IsItPlayer1(string playerName) => playerName.Equals(Player1.name) ? true : false;

    /// <summary>
    /// Check if the gameobject is player 1 (TRUE) or not (FALSE)
    /// </summary>
    /// <param name="player">The player object</param>
    /// <returns>TRUE if the object is player 1, otherwise FALSE</returns>
    private bool IsItPlayer1(GameObject player) => IsItPlayer1(player.name);

    /// <summary>
    /// Set respawn position
    /// </summary>
    /// <param name="player">The player to respawn</param>
    /// <param name="isDefending">Indicates if the spawnpoint pick should be preferred from the side (TRUE) or from the center (FALSE)</param>
    private void SetRespawnPosition(GameObject player, bool isDefending)
    {
        // Get spawn point list
        var spawnPointList = IsItPlayer1(player) ? Player1SpawnPointList : Player2SpawnPointList;

        // Default return value
        Transform spawnPoint = spawnPointList[0];

        for (int i = 0; i < spawnPointList.Count; i++)
        {
            // Get screen point of the spawn point
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(spawnPointList[i].position);
            // Check if the spawn point is on the user screen
            bool onScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            // Take the spawn point while on screen
            if (onScreen)
                spawnPoint = spawnPointList[i];
            // If we want take defending spawn point pick the first one on screen
            if (onScreen && isDefending)
                break;
        }

        // Set respawn position
        player.transform.position = spawnPoint.position;
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Respawn routine
    /// </summary>
    /// <param name="player">The player to respawn</param>
    /// <param name="respawnTime">Respawn time delay</param>
    /// <returns></returns>
    private IEnumerator RespawnRoutine(GameObject player, float respawnTime)
    {
        // Wait the respawn time
        yield return new WaitForSeconds(respawnTime);

        // Disable camera movement
        Camera.main.GetComponent<FollowingCamera>().IsFollowingAllowed = false;

        // Set respawn position
        SetRespawnPosition(player, true); // TODO: set defending (bool) by the map level

        // Revive the character
        player.GetComponent<CharacterMortality>().Revive();
    }

    #endregion
}
