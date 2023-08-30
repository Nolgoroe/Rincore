using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class autoScaler : MonoBehaviour
{
    public int defaultWidth = 1080, defaultHeight = 1920;
    public Vector3 scale;
    private void Update()
    {


        scale = new Vector3((float)defaultWidth / (float)Display.main.systemWidth, (float)defaultHeight / (float)Display.main.systemHeight, 1f);

        Vector3 newScale = Vector3.Scale(transform.localScale, scale);

        transform.localScale = new Vector3(newScale.x, newScale.y, 1);
    }
}
