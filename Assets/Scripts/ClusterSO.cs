using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cluster", menuName = "ScriptableObjects/Create Cluster")]
public class ClusterSO : ScriptableObject
{
    public int clusterID;
    public LevelSO[] clusterLevels;

    public bool isEndOfZone;


    //[ContextMenu("Num Levels")]
    //private void InspectorNumLevel()
    //{
    //    //int num = -1;

    //    //string tempName = name;

    //    //string[] splitArray = tempName.Split(" ");

    //    //int.TryParse(splitArray[1], out num);

    //    //levelNumInZone = num;
    //}

    [ContextMenu("Num Levels")]
    private void InspectorNumLevel()
    {
        for (int i = 0; i < clusterLevels.Length; i++)
        {
            clusterLevels[i].levelNumInZone = i + 1;
        }
    }
}
