using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Audio/Event")]
public class AudioEvent: ScriptableObject
{
    public AudioClip[] Clips;
    public float MinVolume = 1f;
    public float MaxVolume = 1f;
    public float MinPitch = 1f;
    public float MaxPitch = 1f;
    public bool Looping;
    public AudioMixerGroup Group;
}
