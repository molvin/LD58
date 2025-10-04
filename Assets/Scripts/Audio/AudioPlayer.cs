using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public AudioSource Source;

    private bool destroyWhenDone;

    public void Play(AudioEvent settings)
    {
        name = $"AudioPlayer: {settings.name}";

        Source.clip = settings.Clips[Random.Range(0, settings.Clips.Length)];

        Source.volume = Random.Range(settings.MinVolume, settings.MaxVolume);
        Source.pitch = Random.Range(settings.MinPitch, settings.MaxPitch);
        Source.loop = settings.Looping;
        destroyWhenDone = !settings.Looping;

        Source.Play();
    }

    private void Update()
    {
        if(destroyWhenDone && !Source.isPlaying)
        {
            Destroy(gameObject);
        }
    }
}
