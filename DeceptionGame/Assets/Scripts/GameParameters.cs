/*
 * GameParameters control the parameters of the game. 
 * If you make selections in the "Options" menu, it will cover the values in GameParameters.
 */

using System.Collections.Generic;
using UnityEngine;

public class GameParameters : MonoBehaviour
{
    public static GameParameters instance;
    public static int red = 0, yellow = 1, blue = 2;

    // The number of the shuttles
    public int shuttleNum = 1;
    // The default size of the board
    public int gridSize = 25;
    // The minimal distance between two anchors
    public float minAnchorDis = 7;
    // The number of counters in each generator
    public int counterNumInGenerator = 6;
    // The number of counters each shuttle can carry
    public int carryLimit = 4;
    // The number of anchors
    public int anchorCount = 8;
    // The positions of the anchors are random or default
    public bool randomAnchor = true;
    // The time limit for AI
    public float timeLimitForAI = 180;
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

    // Edit this function to set customized anchor positions
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
    }

    // Edit this function to initialize the proportions of each colored counter in the generators
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

    // Sets the proportions of each colored counter in the generators during the game
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
