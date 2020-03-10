using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Members

    public GameObject playerOnePrefab;
    public GameObject playerTwoPrefab;
    public Transform spawnPoint_1;
    public Transform spawnPoint_2;

    #endregion

    #region Unity Methods

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        RespawnPlayersOne();
        RespawnPlayersTwo();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {

    }

    #endregion

    #region Public Methods

    public void RespawnPlayersOne()
    {
        playerOnePrefab.transform.position = spawnPoint_1.position;
    }

    public void RespawnPlayersTwo()
    {
        playerTwoPrefab.transform.position = spawnPoint_2.position;
    }

    #endregion
}
