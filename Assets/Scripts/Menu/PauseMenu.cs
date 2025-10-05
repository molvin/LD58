using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public Slider MasterVolume;
    public Slider SfxVolume;
    public Slider MusicVolume;
    public AudioMixer Master;

    private void Start()
    {
        foreach((string group, Slider slider)in new[] {("Master", MasterVolume), ("Sfx", SfxVolume), ("Music", MusicVolume)})
        {
            Master.GetFloat(group, out float v);
            slider.value = Mathf.Pow(10, v);
            slider.onValueChanged.AddListener(x => SetVolume(x, group));
        }
    }

    private void SetVolume(float value, string group)
    {
        Master.SetFloat(group, Mathf.Log10(value) * 20);

    }
}
