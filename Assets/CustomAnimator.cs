using UnityEngine;

public class CustomAnimator : MonoBehaviour
{
    public Animator Anim;

    public void Update()
    {
        Anim.SetBool("HitAir", !Input.GetKey(KeyCode.Space));
        if (Input.GetKeyDown(KeyCode.Mouse0))
            Anim.SetTrigger("Swing");
    }
}
