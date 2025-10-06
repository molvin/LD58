using UnityEngine;

public class DropShadow : MonoBehaviour
{
    public SpriteRenderer Renderer;

    private void Update()
    {
        if (Renderer.enabled)
        {
            transform.rotation = Quaternion.identity;
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0, 0.1f, 0));
            Ray ray = new Ray(transform.position, Vector3.down);
            bool hit = groundPlane.Raycast(ray, out float enter);
            if (hit)
            {
                Renderer.transform.position = ray.GetPoint(enter);
            }
        }
    }
}
