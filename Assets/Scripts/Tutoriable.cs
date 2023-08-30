using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutoriable : MonoBehaviour
{
    [SerializeField] private SpriteRenderer renderer;

    public void ToggleConnectedHighlight(bool _on)
    {
        renderer.gameObject.SetActive(_on);
    }
}
