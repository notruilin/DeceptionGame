/*
 * The BoardGenerator to setup the board.
 */

using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    private Transform boardHolder;

    private List<Vector3> gridPositions = new List<Vector3>();

    public void SetupScene()
    {
        InitialiseCamera();
        BoardSetup();
        InitialiseList();
        if (GameParameters.instance.randomAnchor)
        {
            AddRandomAnchorPos(GameParameters.instance.anchorCount);
        }
        else
        {
            SetDefaultAnchorPos();
            AddDefaultAnchorPos();
        }
        LayoutAnchors();
        SetCounterGenerator();
    }

    private void InitialiseCamera()
    {
        Camera.main.orthographicSize = GameParameters.instance.gridSize / 1.75f;
        Camera.main.transform.position = new Vector3(((float)(GameParameters.instance.gridSize / 2f - 0.5)), (float)(GameParameters.instance.gridSize / 2f - 0.5), -10f);
    }

    private void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;
        boardHolder.tag = "Board";

        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                GameObject tile = Methods.instance.LayoutObject(GameManager.instance.gridTile, x, y);
                tile.transform.SetParent(boardHolder);
            }
        }
    }

    // gridPositions includes all position in the grid
    private void InitialiseList()
    {
        gridPositions.Clear();
        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    public bool OutOfBoundForAnchor(Vector3 position)
    {
        if (position.x < 0.5 || position.x >= GameParameters.instance.gridSize - 1 || position.y < 0.5 || position.y >= GameParameters.instance.gridSize - 1)
        {
            return true;
        }
        return false;
    }

    private void AddRandomAnchorPos(int count)
    {
        GameManager.instance.anchorPositions.Clear();
        for (int i = 0; i < count; i++)
        {
            bool valid = false;
            Vector3 randomPosition = Vector3.zero;
            while (!valid && gridPositions.Count > 0)
            {
                valid = true;
                randomPosition = Methods.instance.RandomPosition(gridPositions);
                gridPositions.Remove(randomPosition);
                randomPosition += new Vector3(0.5f, 0.5f, 0f);
                if (OutOfBoundForAnchor(randomPosition))
                {
                    valid = false;
                    continue;
                }
                foreach (Vector3 position in GameManager.instance.anchorPositions)
                {
                    float dist = Vector3.Distance(randomPosition, position);
                    if (dist < GameParameters.instance.minAnchorDis)
                    {
                        valid = false;
                        break;
                    }
                }
            }
            // Avoid to add the last random position when gridPositions is empty
            if (valid)
            {
                GameManager.instance.anchorPositions.Add(randomPosition);
            }
            else
            {
                Debug.LogError("No valid space for more Anchors!");
            }
        }
    }

    private void SetDefaultAnchorPos()
    {
        if (GameParameters.instance.defaultAnchorPos.Count > 0) return;
        float fouth = GameParameters.instance.gridSize / 4 + 0.5f;
        GameParameters.instance.defaultAnchorPos.Add(new Vector3(GameParameters.instance.gridSize - fouth, fouth, 0f));
        GameParameters.instance.defaultAnchorPos.Add(new Vector3(fouth, GameParameters.instance.gridSize - fouth, 0f));
        GameParameters.instance.defaultAnchorPos.Add(new Vector3(GameParameters.instance.gridSize - fouth, GameParameters.instance.gridSize - fouth, 0f));
        GameParameters.instance.defaultAnchorPos.Add(new Vector3(fouth, fouth, 0f));
    }

    private void AddDefaultAnchorPos()
    {
        foreach (Vector3 pos in GameParameters.instance.defaultAnchorPos)
        {
            GameManager.instance.anchorPositions.Add(pos);
        }
    }

    private void LayoutAnchors()
    {
        foreach (Vector3 pos in GameManager.instance.anchorPositions)
        {
            Methods.instance.LayoutObject(GameManager.instance.Anchor, pos.x, pos.y);
            Methods.instance.LayoutObject(GameManager.instance.OnAnchor, pos.x, pos.y);
        }
    }

    private void SetCounterGenerator()
    {
        GameManager.instance.generators.Clear();
        GameManager.instance.parkingPos.Clear();
        GameManager.instance.generators.Add(Methods.instance.LayoutObject(GameManager.instance.GeneratorsImages[0], -1.5f, GameParameters.instance.gridSize - 2f));
        GameManager.instance.parkingPos.Add(new Vector3(-1.5f - 2.2f, GameParameters.instance.gridSize - 2f - 2f, 0f));
        GameManager.instance.generators.Add(Methods.instance.LayoutObject(GameManager.instance.GeneratorsImages[1], -1.5f, 2f));
        GameManager.instance.parkingPos.Add(new Vector3(-1.5f - 2.2f, 2f + 1.5f, 0f));
        GameManager.instance.generators.Add(Methods.instance.LayoutObject(GameManager.instance.GeneratorsImages[2], GameParameters.instance.gridSize + 0.5f, GameParameters.instance.gridSize - 2f));
        GameManager.instance.parkingPos.Add(new Vector3(GameParameters.instance.gridSize + 0.5f + 2.2f, GameParameters.instance.gridSize - 2f - 2f, 0f));
        GameManager.instance.generators.Add(Methods.instance.LayoutObject(GameManager.instance.GeneratorsImages[3], GameParameters.instance.gridSize + 0.5f, 2f));
        GameManager.instance.parkingPos.Add(new Vector3(GameParameters.instance.gridSize + 0.5f + 2.2f, 2f + 1.5f, 0f));
    }
}
