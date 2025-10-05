using UnityEngine;

public class Boundaries : MonoBehaviour
{
    public Collider[] BoundaryColliders;

    public float CheckBoundary(Vector3 point, Vector3 dir)
    {
        Ray ray = new Ray(point, dir);
        foreach (Collider coll in BoundaryColliders)
        {
            bool hit = coll.Raycast(ray, out RaycastHit hitInfo, 100000.0f);
            if(hit)
            {
                return hitInfo.distance;
            }
        }
        return 0.0f;
    }
}
