using UnityEngine;
using TMPro;

public class PawnInspector : MonoBehaviour
{
    public Transform Root;
    public Transform InspectRoot;
    public float RotationSpeed;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;

    public bool Inspecting { get; private set; }

    public async Awaitable Inspect(Pawn pawn)
    {
        Root.gameObject.SetActive(true);
        Title.text = pawn.Name;
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
