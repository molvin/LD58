using UnityEngine;
using System.Collections.Generic;

public class PlaceableAreas : MonoBehaviour
{
    public List<Collider> Colliders;
    
    public (bool, Vector3) Valid(Vector3 pointAboveArea)
    {
        foreach(Collider collider in Colliders)
        {
            Ray ray = new Ray(pointAboveArea, Vector3.down);
            bool valid = collider.Raycast(ray, out RaycastHit hitInfo, 10000.0f);
            if(valid)
            {
                return (true, hitInfo.point);
            }
        }
        return (false, Vector3.zero);
    }
}
