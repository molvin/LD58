using UnityEngine;

public class SplashScreen : MonoBehaviour
{
    public GameObject Splash;

    private void Update()
    {
        if(Input.anyKeyDown)
        {
            Splash.SetActive(false);
        }
    }
}
