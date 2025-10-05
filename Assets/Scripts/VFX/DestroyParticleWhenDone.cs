using UnityEngine;

public class DestroyParticleWhenDone : MonoBehaviour
{
    private ParticleSystem particleToBeDestroyed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        particleToBeDestroyed = this.gameObject.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(" TIME : " + particleToBeDestroyed.time)
        if(!particleToBeDestroyed.isPlaying)
        {
            Destroy(this.gameObject);
        }
        if (particleToBeDestroyed.time > particleToBeDestroyed.main.duration)
        {
            Destroy(this.gameObject);
        }
    }
}
