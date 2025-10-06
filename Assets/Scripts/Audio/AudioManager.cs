using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    public AudioPlayer PlayerPrefab;

    private AudioPlayer player;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("Multiple AuiodManagers in scene");
            return;
        }
    }
    private void Update()
    {
        // For hot-reloading
        if(instance == null)
        {
            instance = this;
        }
    }

    public static AudioPlayer Play(AudioEvent settings, Vector3 position)
    {

        AudioPlayer player;
        player = Instantiate(instance.PlayerPrefab, instance.transform);
        player.transform.position = position;
        player.Play(settings);
        return player;
    }

}
