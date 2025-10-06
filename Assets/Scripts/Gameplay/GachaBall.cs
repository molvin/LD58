using UnityEngine;

public class GachaBall : MonoBehaviour
{
    public Pawn Prefab;
    public PawnRarity Rarity;
    public SphereCollider ClickCollider;
    public Rigidbody BallBody;
    public AudioEvent BonkNoise;
    public AudioEvent Rolling;

    private bool doSound = true;
    public AudioPlayer playah;

    public async void Start()
    {
//        await Awaitable.WaitForSecondsAsync(0.1f);
  //      if (BonkNoise != null)
    //        AudioManager.Play(BonkNoise, this.transform.position);
        await Awaitable.WaitForSecondsAsync(0.1f);
        if (Rolling != null)
            playah = AudioManager.Play(Rolling, this.transform.position);

        SetTimerForBody();
    }

    public void Update()
    {
        Debug.Log("Angular X : " + BallBody.angularVelocity.x);
        Debug.Log("Angular Y : " + BallBody.angularVelocity.y);
        Debug.Log("Angular X : " + BallBody.linearVelocity.x);
        Debug.Log("Angular Y : " + BallBody.linearVelocity.y);
        if (BallBody.angularVelocity.x == 0f && doSound == false)
        {
            playah.Source.Stop();
        }
    }

    public async void SetTimerForBody()
    {
        await Awaitable.WaitForSecondsAsync(2f);
        doSound = false;

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
