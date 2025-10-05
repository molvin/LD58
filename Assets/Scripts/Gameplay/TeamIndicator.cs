using UnityEngine;

public class TeamIndicator : MonoBehaviour
{
    public float GroundHeight = 0.05f;
    public Color PlayerColor;
    public Color OpponentColor;
    public SpriteRenderer Renderer;

    private Pawn owner;
    private static Plane groundPlane;

    private void Start()
    {
        owner = GetComponentInParent<Pawn>();
        groundPlane = new Plane(Vector3.up, new Vector3(0, GroundHeight, 0));
        Renderer.color = owner.Team == 0 ? PlayerColor : OpponentColor;
        transform.SetParent(null);
    }

    private void Update()
    {
        if(!owner)
        {
            Destroy(gameObject);
        }
        else
        {
            Renderer.enabled = owner.enabled;
            transform.position = groundPlane.ClosestPointOnPlane(owner.transform.position);
            transform.rotation = Quaternion.identity;
        }
    }
}
