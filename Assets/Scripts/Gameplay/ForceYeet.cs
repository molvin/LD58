using System.Collections.Generic;
using UnityEngine;

public class ForceYeet : MonoBehaviour
{
    public float YeetForce = 100.0f;

    private Pawn whoToYeet;

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                whoToYeet = hit.transform.GetComponentInParent<Pawn>();
            }
        }

        if (Input.GetMouseButtonDown(0) && whoToYeet)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 yeetDirection = hit.point - whoToYeet.transform.position;
                yeetDirection = new Vector3(yeetDirection.x, 0.0f, yeetDirection.z).normalized;

                whoToYeet.Yeet(yeetDirection * YeetForce);
            }
        }
    }
}
