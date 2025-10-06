using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshRenderer))]  
public class RecolorScript : MonoBehaviour
{
    public List<Material> Materials;
    void Awake()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = Materials[Random.Range(0, Materials.Count)];
        
    }
}
