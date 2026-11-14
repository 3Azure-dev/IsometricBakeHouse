using UnityEngine;

public class CreatureAnimationDriver : MonoBehaviour
{
    Animator anim;
    public void PlaySpecial() { if (anim) anim.SetTrigger("Special"); }
    void Awake() => anim = GetComponentInChildren<Animator>();

    public void SetRunning(bool v) { if (anim) anim.SetBool("IsRunning", v); }
    public void SetStunned(bool v) { if (anim) anim.SetBool("Stunned", v); }
    public void PlayCaught()
    {
        if (!anim) return;
        anim.SetBool("Stunned", false);
        anim.SetTrigger("Caught");
    }
}