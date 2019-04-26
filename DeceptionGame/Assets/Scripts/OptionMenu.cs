using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    private void SetGridSizeBig()
    {
        GameParameters.instance.gridSize = 30;
    }

    private void SetGridSizeMedium()
    {
        GameParameters.instance.gridSize = 25;
    }

    private void SetGridSizeSmall()
    {
        GameParameters.instance.gridSize = 20;
    }

    private void SetRandomAnchor()
    {
        GameParameters.instance.randomAnchor = true;
    }

    private void SetDefaultAnchor()
    {
        GameParameters.instance.randomAnchor = false;
    }

    private void SetAnchorDisLarge()
    {
        GameParameters.instance.minAnchorDis = 8;
    }

    private void SetAnchorDisSmall()
    {
        GameParameters.instance.minAnchorDis = 4;
    }
}
