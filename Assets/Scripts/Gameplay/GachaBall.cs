using UnityEngine;

public class GachaBall : MonoBehaviour
{
    public Pawn Prefab;
    public PawnRarity Rarity;
    public SphereCollider ClickCollider;
    public AudioEvent BonkNoise;
    public AudioEvent Rolling;

    private bool doSound;
    public AudioPlayer playah;

    public async void Start()
    {
        await Awaitable.WaitForSecondsAsync(0.1f);
        if (BonkNoise != null)
            AudioManager.Play(BonkNoise, this.transform.position);
        await Awaitable.WaitForSecondsAsync(0.1f);
        if (Rolling != null)
            playah = AudioManager.Play(Rolling, this.transform.position);

        
    }

    public void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject != null)
        {
            if (BonkNoise != null)
                AudioManager.Play(BonkNoise, this.transform.position);
        }
    }
}
