using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Everything that is on the Tile inserting layer AND also wants to be grabbable, has to have this interface
/// </summary>
public interface IGrabTileFrom
{
    void GrabTileFrom();
}
