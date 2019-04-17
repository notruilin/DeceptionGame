using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    public void SetGridSizeBig()
    {
        MainMenu.instance.gridSize = 30;
    }

    public void SetGridSizeMedium()
    {
        MainMenu.instance.gridSize = 25;
    }

    public void SetGridSizeSmall()
    {
        MainMenu.instance.gridSize = 20;
    }

    public void SetRandomAnchor()
    {
        MainMenu.instance.randomAnchor = true;
    }

    public void SetDefaultAnchor()
    {
        MainMenu.instance.randomAnchor = false;
    }

    public void SetAnchorDisLarge()
    {
        MainMenu.instance.minAnchorDis = 8;
    }

    public void SetAnchorDisSmall()
    {
        MainMenu.instance.minAnchorDis = 4;
    }
}
