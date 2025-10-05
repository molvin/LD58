using UnityEngine;
using TMPro;

public class PawnInfo : MonoBehaviour
{
    public TextMeshProUGUI DamageText;
    public TextMeshProUGUI InfoText;

    private Transform cameraTransform;
    private Pawn owner;
    private Vector3 offset;

    private void Start()
    {
        owner = GetComponentInParent<Pawn>();
        offset = transform.localPosition;
        transform.SetParent(null);

        // DamageText.enabled = false;
        InfoText.enabled = false;

        cameraTransform = Camera.main.transform;
        owner.OnDamageTaken += UpdateDamage;
        UpdateDamage(0);
    }
    
    private void Update()
    {
        if(owner == null)
        {
            Destroy(gameObject);
        }
        else
        {
            transform.position = owner.transform.position + offset;
            transform.LookAt(cameraTransform.position, cameraTransform.up);
        }
    }

    private void UpdateDamage(float factor)
    {
        DamageText.text = $"{factor.ToString("P0")}";
    }
}
