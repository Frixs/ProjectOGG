using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main game loop/logic
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Private Members (Singleton)

    /// <summary>
    /// Static instance of GameManager which allows us toaccess it in any other script.
    /// </summary>
    public static GameManager Instance = null;

    #endregion

    #region Private Members

    /// <summary>
    /// <inheritdoc cref="CurrentRoom"/>
    /// </summary>
    public short _currentRoom = 0;

    #endregion

    #region Public Members (General Settings)

    /// <summary>
    /// Defualt respawn time
    /// </summary>
    [Header("General Settings")]
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

    #region Public Members (Load Settings)

    /// <summary>
    /// Game Object Name of <see cref="Player1"/> to search for while setting up the scene
    /// </summary>
    [Header("Load Settings")]
    public string ObjectNameP1 = "Player_1";

    /// <summary>
    /// Game Object Name of <see cref="Player2"/> to search for while setting up the scene
    /// </summary>
    public string ObjectNameP2 = "Player_2";

    /// <summary>
    /// Game Object Name of <see cref="Player1_SpawnPointList"/> to search for while setting up the scene
    /// Order matters - Index: 0 = the closest spawn point to house
    /// </summary>
    public string SpawnPointListObjectNameP1 = "SpawnPointsP1";

    /// <summary>
    /// Game Object Name of <see cref="Player2_SpawnPointList"/> to search for while setting up the scene
    /// Order matters - Index: 0 = the closest spawn point to house
    /// </summary>
    public string SpawnPointListObjectNameP2 = "SpawnPointsP2";

    #endregion

    #region Public Properties

    /// <summary>
    /// Player 1 GameObject
    /// </summary>
    public GameObject Player1 { get; private set; }

    /// <summary>
    /// Player 2 GameObject
    /// </summary>
    public GameObject Player2 { get; private set; }

    /// <summary>
    /// Spawn points for <see cref="Player1"/>
    /// </summary>
    public List<Transform> Player1_SpawnPointList { get; private set; }

    /// <summary>
    /// Spawn points for <see cref="Player2"/>
    /// </summary>
    public List<Transform> Player2_SpawnPointList { get; private set; }

    /// <summary>
    /// Player 1 statistics collection
    /// </summary>
    public GameStatistics Player1_Statistics { get; private set; } = new GameStatistics();

    /// <summary>
    /// Player 2 statistics collection
    /// </summary>
    public GameStatistics Player2_Statistics { get; private set; } = new GameStatistics();

    /// <summary>
    /// Indicates if Player 1 is in attack
    /// </summary>
    public bool IsAttackerPlayer1 => CurrentRoom > 0;

    /// <summary>
    /// Indicates if Player 2 is in attack
    /// </summary>
    public bool IsAttackerPlayer2 => CurrentRoom < 0;

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
    public short CurrentRoom 
    { 
        get => _currentRoom;
        set
        {
            PreviousRoom = _currentRoom;
            _currentRoom = value;
        }
    }

    /// <summary>
    /// Previous room for <see cref="CurrentRoom"/>
    /// </summary>
    public short PreviousRoom { get; set; } = 0;

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
    /// If any player is death, it returns FALSE
    /// </summary>
    public bool IsMeleeDistance => AnyPlayerDeath ? false : GetDistanceBetweenPlayers <= meleeDistance;

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
    /// On new scene load while this instance is active
    /// </summary>
    /// <param name="level"></param>
    private void OnLevelWasLoaded(int level)
    {
        // Init
        InitializeInScene();
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    private void Start()
    {
        // Init - this happens only once at the beginning of the game
        InitializeInScene();
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
        Player1_Statistics = new GameStatistics();
        Player2_Statistics = new GameStatistics();
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
        var playerStatistics = isItPlayer1 ? Player1_Statistics : Player2_Statistics;

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

    /// <summary>
    /// Check if the name belongs to player 1 (TRUE) or not (FALSE)
    /// </summary>
    /// <param name="playerName">The player name</param>
    /// <returns>The name belongs to player 1 (TRUE), otherwise FALSE</returns>
    public bool IsItPlayer1(string playerName) => playerName.Equals(Player1.name) ? true : false;

    /// <summary>
    /// Check if the gameobject is player 1 (TRUE) or not (FALSE)
    /// </summary>
    /// <param name="player">The player object</param>
    /// <returns>TRUE if the object is player 1, otherwise FALSE</returns>
    public bool IsItPlayer1(GameObject player) => IsItPlayer1(player.name);

    #endregion

    #region Private Methods (Initialization)

    /// <summary>
    /// Initialize game manager in scene
    /// </summary>
    private void InitializeInScene()
    {
        // TODO: Load players from inspector as prefab once the input controller will be set up.
        //       We need to control player controls in code before settings this up.

        // Load players
        Player1 = GameObject.Find(ObjectNameP1) ?? null;
        Player2 = GameObject.Find(ObjectNameP2) ?? null;

        // Init spawnpoint lists
        Player1_SpawnPointList = new List<Transform>();
        Player2_SpawnPointList = new List<Transform>();
        // Load spawn points
        foreach (Transform child in GameObject.Find(SpawnPointListObjectNameP1).transform)
            Player1_SpawnPointList.Add(child);
        foreach (Transform child in GameObject.Find(SpawnPointListObjectNameP2).transform)
            Player2_SpawnPointList.Add(child);

        // Reset temporrary death count
        Player1_Statistics.ResetTemporaryDeathCount();
        Player2_Statistics.ResetTemporaryDeathCount();

        // Set camera & player default positions
        if (CurrentRoom < PreviousRoom) // In P1 territory
        {
            var spawnPointP1 = Player1_SpawnPointList[Player1_SpawnPointList.Count - 2]; // Get 2nd farrest spawn point
            var spawnPointP2 = Player2_SpawnPointList[0]; // Get the closest spawn point to player's home
            Camera.main.transform.position = new Vector3(spawnPointP1.position.x, spawnPointP1.position.y, Camera.main.transform.position.z);
            Player1.transform.position = spawnPointP1.position;
            Player2.transform.position = spawnPointP2.position;
        }
        else if (CurrentRoom > PreviousRoom) // In P2 territory
        {
            var spawnPointP1 = Player1_SpawnPointList[0]; // Get the closest spawn point to player's home
            var spawnPointP2 = Player2_SpawnPointList[Player2_SpawnPointList.Count - 2]; // Get 2nd farrest spawn point
            Camera.main.transform.position = new Vector3(spawnPointP2.position.x, spawnPointP2.position.y, Camera.main.transform.position.z);
            Player1.transform.position = spawnPointP1.position;
            Player2.transform.position = spawnPointP2.position;
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Set respawn position
    /// </summary>
    /// <param name="player">The player to respawn</param>
    /// <param name="isDefending">Indicates if the spawnpoint pick should be preferred from the side (TRUE) or from the center (FALSE)</param>
    private void SetRespawnPosition(GameObject player, bool isDefending)
    {
        var isItPlayer1 = IsItPlayer1(player);

        // Get spawn point list
        var spawnPointList = isItPlayer1 ? Player1_SpawnPointList : Player2_SpawnPointList;
        // Get opposite player
        var oppositePlayer = isItPlayer1 ? Player2 : Player1;

        // Default return value
        Transform spawnPoint = null;

        for (int i = 0; i < spawnPointList.Count; i++)
        {
            // Get screen point of the spawn point
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(spawnPointList[i].position);
            // Check if the spawn point is on the user screen
            bool isOnScreen = screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;

            // Take the spawn point while on screen
            if (isOnScreen) 
            {
                // Do not spawn player behind the other one
                if (spawnPoint != null && (isItPlayer1 ? oppositePlayer.transform.position.x < spawnPointList[i].position.x : oppositePlayer.transform.position.x < spawnPointList[i].position.x))
                    break;

                // Save it
                spawnPoint = spawnPointList[i];
            }
            // If we want take defending spawn point pick the first one on screen
            if (isOnScreen && isDefending)
                break;
        }

        // Make sure we have spawn point
        if (spawnPoint == null)
            spawnPoint = spawnPointList[0];

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

        // Break out of the execution if the character instance does not exist anymore
        // Switching game rooms may trigger this...
        if (player == null)
            yield break;

        // Disable camera movement
        //Camera.main.GetComponent<FollowingCamera>().IsFollowingAllowed = false;

        // Set respawn position
        SetRespawnPosition(
            player, 
            IsItPlayer1(player) ? !IsAttackerPlayer1 : !IsAttackerPlayer2 // Is defending?
            );

        // Revive the character
        player.GetComponent<CharacterMortality>().Revive();
    }

    #endregion
}
