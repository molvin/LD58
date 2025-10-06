using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    public GameObject Splash;

    private void Awake()
    {
        Splash.SetActive(true);
    }

    private void Update()
    {
        if(Input.anyKeyDown)
        {
            Splash.SetActive(false);
        }
    }
}
