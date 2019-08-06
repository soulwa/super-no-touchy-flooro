using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public static Menu instance; //needed for button functions, easier music/sounds bc gm wouldn't play when transition to load state

    public AudioClip menuMusic;
    public AudioClip cursorScroll;
    public AudioClip cursorSelect;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Vertical"))
        {
            AudioPlayer.instance.PlaySound(cursorScroll);
        }
        if (Input.GetButtonDown("Submit"))
        {
            AudioPlayer.instance.PlaySound(cursorSelect);
        }
    }

    public void NewGame()
    {
        GameManager.instance.NewGame();
    }

    public void ContinueGame()
    {
        GameManager.instance.ContinueGame();
    }

    public void QuitGame()
    {
        GameManager.instance.QuitGame();
    }

    public void ToggleClassicMode()
    {
        GameManager.instance.ToggleClassicMode();
    }

    public void RestartGame()
    {
        GameManager.instance.RestartGame();
    }

    public void ResetSave()
    {
        GameManager.instance.ResetSave();
    }
}
