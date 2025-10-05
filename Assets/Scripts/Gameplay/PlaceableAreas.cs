using UnityEngine;
using System.Collections.Generic;

public class PlaceableAreas : MonoBehaviour
{
    public List<Collider> PlayerSide;
    public List<Collider> OpponentSide;
    
    public (bool, Vector3) Valid(Vector3 pointAboveArea)
    {
        foreach(Collider collider in PlayerSide)
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

    public Vector3 GetRandomPointInPlaceableArea()
    {
        int index = Random.Range(0, PlayerSide.Count);
        Collider coll = PlayerSide[index];
        return coll.bounds.RandomPointInBounds();
    }

    public Vector3 GetCenter(bool player)
    {
        return player ? PlayerSide[0].bounds.center : OpponentSide[0].bounds.center;
    }
}
