using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameParameters : MonoBehaviour
{
    public static GameParameters instance;
    public static int red = 0, yellow = 1, blue = 2;

    public int shuttleNum = 1;
    public int gridSize = 25;
    public float minAnchorDis = 4;
    public int counterNumInGenerator = 6;
    public int carryLimit = 4;
    public int anchorCount = 8;
    public bool randomAnchor = true;
    public List<Vector3> defaultAnchorPos;

    // Uses to control the proportion of each color of apples, default is equal proportion
    [HideInInspector] public List<int> colorBag;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        SetCustomAnchorPos();
        InitializeColorProportion();
        SetDefaultColorProportion();
    }

    private void SetCustomAnchorPos()
    {
        defaultAnchorPos.Clear();
        // Add Postions to defaultAnchorPos here to customize Anchors' Positions
        // Keep empty if you don't need to customize Anchors' Positions
        // You need to select "Default" in Options Menu 
        /* Example : Anchor Positions are (1.5, 1.5), (4.5, 4.5), (6.5, 6.5), (8.5, 8.5)
        defaultAnchorPos.Add(new Vector3(1.5f, 1.5f, 0f));
        defaultAnchorPos.Add(new Vector3(4.5f, 4.5f, 0f));
        defaultAnchorPos.Add(new Vector3(6.5f, 6.5f, 0f));
        defaultAnchorPos.Add(new Vector3(8.5f, 8.5f, 0f));
        */
        // Your code BEGINS HERE


        // Your code ENDS HERE
        if (defaultAnchorPos.Count > 0)
        {
            anchorCount = defaultAnchorPos.Count;
        }
    }

    private void InitializeColorProportion()
    {
        colorBag.Clear();
        // Initialize colors' proportions for generators here
        // Keep empty if you don't need to customize
        /* Example : 100% red, 0% yellow and 0% blue
        SetColorProportion(100, 0, 0);
        */
        // Your code BEGINS HERE


        // Your code ENDS HERE
    }

    public void SetColorProportion(int redP, int yellowP, int blueP)
    {
        colorBag.Clear();
        int i;
        if (redP + yellowP + blueP == 100)
        {
            for (i = 0; i < redP; i++)
            {
                colorBag.Add(red);
            }
            for (i = 0; i < yellowP; i++)
            {
                colorBag.Add(yellow);
            }
            for (i = 0; i < blueP; i++)
            {
                colorBag.Add(blue);
            }
        }
        else
        {
            Debug.LogError("Invalid Proportion!");
        }
    }

    private void SetDefaultColorProportion()
    {
        if (colorBag.Count > 0) return;
        colorBag = new List<int> { red, yellow, blue };
    }
}
