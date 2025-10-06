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

    public float MaxDamage;
    public float MaxForce;
    public float MaxMass;
    
    public async Awaitable Inspect(Pawn orig, bool disableOrig = true)
    {
        Pawn pawn = Instantiate(orig);
        orig.gameObject.SetActive(false);
        pawn.rigidbody.isKinematic = true;

        Root.gameObject.SetActive(true);
        Title.text = pawn.Name;
        RarityText.text = pawn.Rarity.ToString();
        for(int i = 0; i < (int) pawn.Rarity; i++)
        {
            RarityText.text += "!";
        }
        Description.text = pawn.Description;

        DamageFill.fillAmount = pawn.EffectiveAttackDamage / MaxDamage;
        ForceFill.fillAmount = pawn.EffectiveAttackForce / MaxForce;
        MassFill.fillAmount = pawn.EffectiveMass / MaxMass;

        pawn.transform.SetParent(InspectRoot);
        pawn.transform.localPosition = Vector3.zero;
        pawn.transform.localRotation = Quaternion.identity;

        InspectRoot.localRotation = Quaternion.Euler(0, 180, 0);
        await Awaitable.NextFrameAsync();
        while (!Input.GetMouseButtonDown(0))
        {
            InspectRoot.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
            await Awaitable.NextFrameAsync();
        }

        Destroy(pawn.gameObject);
        Root.gameObject.SetActive(false);
        orig.gameObject.SetActive(true);
    }
}
