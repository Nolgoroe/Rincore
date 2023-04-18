using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class RewardStruct
{
    public ScriptableObject rewardData;
    public Sprite rewardSprite;
    public int rewardAmount;
}

[CreateAssetMenu(fileName = "Daily Reward", menuName = "ScriptableObjects/Create Daily Reward")]
public class DailyRewardsSO : ScriptableObject
{
    public RewardStruct[] rewards;

    private void OnValidate() // go over this with Lior!
    {
        Ingredients ingredientVersion;
        PowerupScriptableObject powerupVersion;

        foreach (RewardStruct element in rewards)
        {
            ingredientVersion = element.rewardData as Ingredients;

            if (ingredientVersion != null)
            {
                element.rewardSprite = ingredientVersion.ingredientSprite;

                continue;
            }

            powerupVersion = element.rewardData as PowerupScriptableObject;

            if (powerupVersion != null)
            {
                element.rewardSprite = powerupVersion.potionSprite;

                continue;
            }

        }
    }
}
