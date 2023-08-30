using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyMainCam : MonoBehaviour
{
    [SerializeField] private Camera camToCopy;
    [SerializeField] private Camera thisCam;

    private void OnValidate()
    {
        thisCam = GetComponent<Camera>();
    }
    void Update()
    {
        thisCam.orthographicSize = camToCopy.orthographicSize;
    }
}
