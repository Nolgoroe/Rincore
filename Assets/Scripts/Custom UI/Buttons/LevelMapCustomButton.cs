using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Linq;
using System;

[Serializable]
public class LevelPresetData
{
    public LevelSO connectedLevelSO;
    public ClusterSO connectedCluster;
    public Ring connectedRing;
    public int indexInCluster;
}
public class LevelMapCustomButton : CustomButtonParent
{
    public LevelPresetData data;
    public override void OnClickButton()
    {
        SoundManager.instance.CallPlaySound(sounds.ButtonClick);

        buttonEventsInspector?.Invoke();
    }

    //called from button
    public void AutomatiTransferLevel ()
    {
        GameManager.instance.ClickOnLevelIconMapSetData(data);
    }

    public void ActionsOnClickLevel()
    {
        GameManager.instance.ClickOnLevelIconMapSetData(data);
    }

    public override void OverrideSetMyElement(string[] texts, Sprite[] sprites, System.Action[] actions = null)
    {
        base.SetMyElement(texts, sprites);
    }

    [ContextMenu("Populate cluster SO")]
    public void PopulateCluster()
    {
        GameManager gm = GameObject.FindObjectOfType<GameManager>();
        foreach (var cluster in gm.allClusters)
        {
            for (int i = 0; i < cluster.clusterLevels.Length; i++)
            {
                if(cluster.clusterLevels[i] == data.connectedLevelSO)
                {
                    data.connectedCluster = cluster;
                    data.indexInCluster = i;
                }
            }
        }
    }
}
