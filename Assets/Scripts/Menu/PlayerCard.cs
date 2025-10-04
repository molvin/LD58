using UnityEngine;
using TMPro;

public class PlayerCard : MonoBehaviour
{
    public TMP_InputField NameInput;

    public string Name;
    public System.Action<bool> OnNameChanged;

    private void Start()
    {
        // TODO read name from playerprefs, or server
        UpdateName("");
        NameInput.onValueChanged.AddListener(UpdateName);
    }

    private void UpdateName(string name)
    {
        Name = name;
        bool valid = Name.Length >= 3;

        // TODO: update in server or something

        OnNameChanged?.Invoke(valid);
    }

    public void SetInteractable(bool interactable)
    {
        NameInput.interactable = interactable;
    }
}
