using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/Event")]
public class AudioEvent: ScriptableObject
{
    public AudioClip[] Clips;
    public float MinVolume;
    public float MaxVolume;
    public float MinPitch;
    public float MaxPitch;
    public bool Looping;
    public AudioMixerGroup Group;
}
