using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestLogic : MonoBehaviour
{
    [SerializeField] private Animator anim;


    private void Start()
    {
        GameManager.instance.summonedChest = this;
    }
    public void OnPressedChest()
    {
        StartCoroutine(InitiateLootGive()); //go over this with Lior
    }

    private IEnumerator InitiateLootGive()
    {
        anim.SetTrigger("TappedChest");

        yield return new WaitForSeconds(1.2f);

        GameManager.instance.AdvanceGiveLootFromManager(); //go over this with Lior
    }
    public IEnumerator AfterGiveLoot() //go over this with Lior
    {
        anim.SetTrigger("FinishedLootDisplay");

        UIManager.instance.ContinueAfterChest();

        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}
