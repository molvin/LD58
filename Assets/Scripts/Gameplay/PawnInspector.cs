using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PawnInspector : MonoBehaviour
{
    public Transform Root;
    public Transform InspectRoot;
    public float RotationSpeed;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI RarityText;
    public TextMeshProUGUI Description;

    public Image DamageFill, ForceFill, MassFill;

    public bool Inspecting { get; private set; }

    public async Awaitable Inspect(Pawn pawn, float maxDamage, float maxForce, float maxMass)
    {
        Root.gameObject.SetActive(true);
        Title.text = pawn.Name;
        RarityText.text = pawn.Rarity.ToString();
        for(int i = 0; i < (int) pawn.Rarity; i++)
        {
            RarityText.text += "!";
        }
        Description.text = pawn.Description;

        DamageFill.fillAmount = pawn.EffectiveAttackDamage / maxDamage;
        ForceFill.fillAmount = pawn.EffectiveAttackForce / maxForce;
        MassFill.fillAmount = pawn.EffectiveMass / maxMass;

        pawn.transform.SetParent(InspectRoot);
        pawn.transform.localPosition = Vector3.zero;
        pawn.transform.localRotation = Quaternion.identity;

        while (!Input.GetMouseButtonDown(1))
        {
            InspectRoot.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
            await Awaitable.NextFrameAsync();
        }        

        Destroy(pawn.gameObject);
        Root.gameObject.SetActive(false);
    }
}
