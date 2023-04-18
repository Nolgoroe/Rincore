using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ZoneMaterialData : MonoBehaviour
{
    [SerializeField] private ZoneAndObjectToBlurUnblur blurUnblurPerZone;

    public void ChangeZoneToBlurryZoneDisplay()
    {
        foreach (GameObject go in blurUnblurPerZone.planesToChangeFront)
        {
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();

            renderer.material = blurUnblurPerZone.blurMat;
        }

        foreach (GameObject go in blurUnblurPerZone.planesToChangeMiddle)
        {
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();

            renderer.material = blurUnblurPerZone.blurMat;
        }

        foreach (SpriteRenderer sr in blurUnblurPerZone.BGToChange)
        {
            sr.sprite = blurUnblurPerZone.blurBGSprite;
        }
    }
    public void ChangeZoneToNormalZoneDisplay()
    {
        foreach (GameObject go in blurUnblurPerZone.planesToChangeFront)
        {
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();

            renderer.material = blurUnblurPerZone.normalMat;
        }

        foreach (GameObject go in blurUnblurPerZone.planesToChangeMiddle)
        {
            MeshRenderer renderer = go.GetComponent<MeshRenderer>();

            renderer.material = blurUnblurPerZone.normalMat;
        }

        foreach (SpriteRenderer sr in blurUnblurPerZone.BGToChange)
        {
            sr.sprite = blurUnblurPerZone.normalBGSprite;
        }
    }
}
