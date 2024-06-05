using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu_buttons : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject PlayMenu;
    public GameObject CreditsMenu;
    public GameObject SettingsMenu;

    // Start is called before the first frame update
    void Start()
    {
        MainMenuButton();
    }

    public void PlayNowButton()
    {
        MainMenu.SetActive(false);
        PlayMenu.SetActive(true);
    }

    public void CreditsButton()
    {
        // Show Credits Menu
        MainMenu.SetActive(false);
        CreditsMenu.SetActive(true);
    }
    public void SettingsButton()
    {
        MainMenu.SetActive(false);
        SettingsMenu.SetActive(true);
    }

    public void MainMenuButton()
    {
        // Show Main Menu
        MainMenu.SetActive(true);
        CreditsMenu.SetActive(false);
        SettingsMenu.SetActive(false);
        PlayMenu.SetActive(false);
    }
    public void Tutorial()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("gametutorial");
    }
    public void Mission1()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }

    public void QuitButton()
    {
        // Quit Game
        Application.Quit();
    }
}