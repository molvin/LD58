using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Notepad Manager;
    public Button StartButton, QuitButton, Abandon;

    public GameObject InGame, InMenu;

    private void Awake()
    {
        StartButton.onClick.AddListener(StartGame);
        QuitButton.onClick.AddListener(QuitGame);
        Abandon.onClick.AddListener(Manager.BackToMenu);
    }

    public void StartGame()
    {
        Manager.StartGame();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
        
#endif
    }

    public void Show(bool inGame)
    {
        InGame.SetActive(inGame);
        InMenu.SetActive(!inGame);
    }

    public void SetCanPlay(bool canPlay)
    {
        StartButton.interactable = canPlay;
    }
}
