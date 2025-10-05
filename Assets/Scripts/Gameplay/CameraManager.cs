using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Animator Anim;

    public async Awaitable Gacha()
    {
        Anim.SetBool("Placing", false);
        Anim.SetBool("Gacha", true);
        await Awaitable.WaitForSecondsAsync(0.5f);
    }

    public async Awaitable Idle()
    {
        Anim.SetBool("Gacha", false);
        Anim.SetBool("Placing", false);
        await Awaitable.WaitForSecondsAsync(0.5f);
    }

    public async Awaitable Placing()
    {
        Anim.SetBool("Gacha", false);
        Anim.SetBool("Placing", true);
        await Awaitable.WaitForSecondsAsync(0.5f);
    }
}
