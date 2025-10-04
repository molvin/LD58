using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public bool Paused { get; private set; }

    public Button ResumeButton, MainMenuButton;

    public void Toggle()
    {
        Paused = !Paused;
        gameObject.SetActive(Paused);
    }

    private void Update()
    {
        if (Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            Toggle();
        }
    }
}
