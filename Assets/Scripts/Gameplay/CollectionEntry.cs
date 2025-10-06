using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CollectionEntry : MonoBehaviour
{
    public Image[] CheckMarks;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;

    public void Init(Pawn pawn, bool[] discovered)
    {
        if (pawn != null)
        {
            Name.text = pawn.Name;
            Description.text = pawn.Description;
        }
        else
        {
            Name.text = "";
            Description.text = "";
        }

        for (int i = 0; i < discovered.Length; i++)
        {
            bool d = discovered[i];
            CheckMarks[i].enabled = d;
        }
    }
}
