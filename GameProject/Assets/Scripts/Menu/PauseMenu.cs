using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    private bool _paused = false;

    public GameObject menu;
    // Start is called before the first frame update
    void Start()
    {
        menu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (_paused)
            {
                resumeGame();
            }
            else 
            {
                pauseGame();
            }
        }
    }

    public void resumeGame() 
    {
        menu.SetActive(false);
        Time.timeScale = 1f;
        _paused = false;
    }

    private void pauseGame()
    {
        menu.SetActive(true);
        Time.timeScale = 0f;
        _paused = true;
    }

    public void returnToMenu() 
    {
        Time.timeScale = 1f;
        _paused = false;
        SceneManager.LoadScene(0);
    }
}
