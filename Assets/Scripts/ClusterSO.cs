using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Cluster", menuName = "ScriptableObjects/Create Cluster")]
public class ClusterSO : ScriptableObject
{
    public int clusterID;
    public LevelSO[] clusterLevels;

    public bool isEndOfZone;
}
