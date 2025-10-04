using UnityEngine;
using TMPro;
using System.Collections;

public class PawnInspector : MonoBehaviour
{
    public Transform Root;
    public Transform InspectRoot;
    public float RotationSpeed;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;

    public bool Inspecting { get; private set; }

    public IEnumerator Inspect(Pawn pawn)
    {
        Root.gameObject.SetActive(true);
        pawn.enabled = false;
        Title.text = pawn.Name;
        pawn.transform.SetParent(InspectRoot);
        pawn.transform.localPosition = Vector3.zero;
        pawn.transform.localRotation = Quaternion.identity;

        Destroy(pawn.GetComponent<Rigidbody>());
         
        while (!Input.GetMouseButtonDown(1))
        {
            InspectRoot.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
            yield return null;
        }        

        Destroy(pawn.gameObject);
        Root.gameObject.SetActive(false);
    }
}
