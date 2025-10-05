using UnityEngine;
using TMPro;
using System.Collections;
using static Database;

public class PawnInspector : MonoBehaviour
{
    public Transform Root;
    public Transform InspectRoot;
    public float RotationSpeed;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;

    public bool Inspecting { get; private set; }

    public IEnumerator Inspect(GameObject go, Pawn prefab)
    {
        Root.gameObject.SetActive(true);
        Title.text = prefab.Name;
        go.transform.SetParent(InspectRoot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        while (!Input.GetMouseButtonDown(1))
        {
            InspectRoot.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
            yield return null;
        }        

        Destroy(go);
        Root.gameObject.SetActive(false);
    }
}
