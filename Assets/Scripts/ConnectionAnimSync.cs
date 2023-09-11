using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionAnimSync : MonoBehaviour
{
    public static Animator masterAnim;

    [SerializeField] private Animator anim;

    private void Start()
    {
        if (masterAnim == null)// there has to already be an active master anim in the scene for this to work!
        {
            masterAnim = anim;
        }
    }

    private void OnValidate()
    {
        TryGetComponent<Animator>(out anim);
    }

    private void OnEnable()
    {
        if (masterAnim != null)
        {
            anim.Play(0, -1, masterAnim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }
    }
}
