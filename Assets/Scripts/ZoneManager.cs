using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.ComponentModel;

public enum WorldEnum
{
    [Description("Walnut Grove")]
    WalnutGrove,
    [Description("Vinebloom Pines")]
    VinebloomPines,
    [Description("Greymoss Bog")]
    GreyMossBog,
    [Description("Red Root Covert")]
    RedRootCovert,
    [Description("Thornberry Woods")]
    ThornberryWoods 
}

[System.Serializable]
public class ZoneAndObjectToBlurUnblur
{
    public GameObject[] planesToChangeFront;
    public GameObject[] planesToChangeMiddle;
    public SpriteRenderer[] BGToChange;

    public Material blurMat;
    public Material normalMat;
    public Sprite blurBGSprite;
    public Sprite normalBGSprite;
}

public class ZoneManager : MonoBehaviour
{
    [SerializeField] private string[] BGPaths;

    private Dictionary<WorldEnum, string> BGEnumToResource;


    private void Start()
    {
        BGEnumToResource = new Dictionary<WorldEnum, string>();

        for (int i = 0; i < BGPaths.Length; i++)
        {
            BGEnumToResource.Add((WorldEnum)i, BGPaths[i]);
        }
    }

    public string ReturnBGPathByType(WorldEnum world)
    {
        if(BGEnumToResource.ContainsKey(world))
        {
            return BGEnumToResource[world];
        }

        return "";
    }

}


/// create a context menu method that will go to resources folder and retrieve all BG's into
/// the BG Paths array