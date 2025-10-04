using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button StartButton, OptionsButton, QuitButton;
    public PauseMenu PauseMenu;

    private void Start()
    {
        StartButton.onClick.AddListener(StartGame);
        OptionsButton.onClick.AddListener(Options);
        QuitButton.onClick.AddListener(QuitGame);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void Options()
    {
        PauseMenu.Toggle();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
        
#endif
    }

}
