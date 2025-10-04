using System.Collections.Generic;
using UnityEngine;

public class ForceYeet : MonoBehaviour
{
    public float YeetForce = 100.0f;
    public float DistForMaxForce = 5.0f;

    private Pawn whoToYeet;
    private Vector3 lastYeetPoint;
    private Vector3 originalYeetPos;
    private Quaternion originalYeetRot;

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                whoToYeet = hit.transform.GetComponentInParent<Pawn>();

                if (whoToYeet)
                {
                    originalYeetPos = whoToYeet.transform.position;
                    originalYeetRot = whoToYeet.transform.rotation;
                }
            }
        }

        if (Input.GetMouseButton(0) && whoToYeet)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                lastYeetPoint = hit.point;
            }

            float dist = Vector3.Distance(whoToYeet.transform.position, lastYeetPoint);

            float chargeFactor = Mathf.Clamp01(dist / DistForMaxForce);

            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y);
            whoToYeet.transform.position = originalYeetPos + randomDir * chargeFactor * 0.2f;
        }

        if (Input.GetMouseButtonUp(0) && whoToYeet)
        {
            whoToYeet.transform.position = originalYeetPos;
            whoToYeet.transform.rotation = originalYeetRot;

            Vector3 yeetDirection = whoToYeet.transform.position - lastYeetPoint;
            float forceFactor = Mathf.Clamp01(yeetDirection.magnitude / DistForMaxForce);
            yeetDirection = new Vector3(yeetDirection.x, 0.0f, yeetDirection.z).normalized;
            whoToYeet.Yeet(yeetDirection * forceFactor * YeetForce);
        }
    }
}
