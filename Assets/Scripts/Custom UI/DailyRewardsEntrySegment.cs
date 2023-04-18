using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardsEntrySegment : UIElementDisplayerSegment
{
    static private readonly int GIVE_DAILY = Animator.StringToHash("Give Daily");

    [SerializeField] private GameObject currentRewardParent;
    [SerializeField] private float timeDeactivateTodayVFX;
    [SerializeField] private Image connectedImage;
    [SerializeField] private Animator connectedAnimator;

    
    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, Action[] actions)
    {
        base.SetMyElement(texts, sprites);
    }

    public void SetDisplayAsTodaysReward(bool isTodaysReward)
    {
        currentRewardParent.gameObject.SetActive(isTodaysReward);

        connectedAnimator.enabled = isTodaysReward;
    }

    public void SetAsAlreadyGiven()
    {
        connectedImage.color = new Color(connectedImage.color.r, connectedImage.color.g, connectedImage.color.b, 0.5f);
        connectedAnimator.enabled = false;

    }
    public IEnumerator AfterRecievedReward()
    {
        
        connectedAnimator.SetTrigger(GIVE_DAILY);
        yield return new WaitForSeconds(timeDeactivateTodayVFX);

        SetDisplayAsTodaysReward(false);
    }
}
