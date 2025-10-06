using UnityEngine;

public class Sticker : MonoBehaviour
{
    public bool IsSpawner;
    public Collider Collider;
    public int Index;

    public bool Hovering()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Collider.Raycast(ray, out RaycastHit _, 10000.0f);
    }
}
