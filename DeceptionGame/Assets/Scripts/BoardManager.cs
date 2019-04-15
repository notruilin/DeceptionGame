using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private Transform boardHolder;

    private List<Vector3> gridPositions = new List<Vector3>();

    public void SetupScene()
    {
        InitialiseCamera();
        BoardSetup();
        InitialiseList();
        LayoutObjectAtRandom(GameManager.instance.Anchor, GameManager.instance.anchorCount);
        SetCounterGenerator();
    }

    private void InitialiseCamera()
    {
        Camera.main.orthographicSize = GameManager.instance.gridSize / 1.8f;
        Camera.main.transform.position = new Vector3(((float)(GameManager.instance.gridSize / 2f - 0.5)), (float)(GameManager.instance.gridSize / 2f - 0.5), -10f);
    }

    private void BoardSetup()
    {
        boardHolder = new GameObject("Board").transform;

        for (int x = 0; x < GameManager.instance.gridSize; x++)
        {
            for (int y = 0; y < GameManager.instance.gridSize; y++)
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
        for (int x = 0; x < GameManager.instance.gridSize; x++)
        {
            for (int y = 0; y < GameManager.instance.gridSize; y++)
            {
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    public bool OutOfBoundForAnchor(Vector3 position)
    {
        if (position.x < 0.5 || position.x >= GameManager.instance.gridSize - 1 || position.y < 0.5 || position.y >= GameManager.instance.gridSize - 1)
        {
            return true;
        }
        return false;
    }

    private void LayoutObjectAtRandom(GameObject prefab, int count)
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
                    if (dist < GameManager.instance.minAnchorDis)
                    {
                        valid = false;
                        break;
                    }
                }
            }
            // Avoid to add the last random position when gridPositions is empty
            if (valid)
            {
                Methods.instance.LayoutObject(prefab, randomPosition.x, randomPosition.y);
                Methods.instance.LayoutObject(GameManager.instance.OnAnchor, randomPosition.x, randomPosition.y);
                GameManager.instance.anchorPositions.Add(randomPosition);
            }
            else
            {
                Debug.LogError("No valid space for more Anchors!");
            }
        }
    }

    private void SetCounterGenerator()
    {
        GameManager.instance.generators.Clear();
        GameManager.instance.parkingPos.Clear();
        GameManager.instance.generators.Add(Methods.instance.LayoutObject(GameManager.instance.GeneratorsImages[0], -4f, GameManager.instance.gridSize - 2f));
        GameManager.instance.parkingPos.Add(new Vector3(-4f + 0.5f, GameManager.instance.gridSize - 2f - 2f, 0f));
        GameManager.instance.generators.Add(Methods.instance.LayoutObject(GameManager.instance.GeneratorsImages[1], -4f, 2f));
        GameManager.instance.parkingPos.Add(new Vector3(-4f + 0.5f, 2f + 1.5f, 0f));
        GameManager.instance.generators.Add(Methods.instance.LayoutObject(GameManager.instance.GeneratorsImages[2], GameManager.instance.gridSize + 3f, GameManager.instance.gridSize - 2f));
        GameManager.instance.parkingPos.Add(new Vector3(GameManager.instance.gridSize + 3f - 0.5f, GameManager.instance.gridSize - 2f - 2f, 0f));
        GameManager.instance.generators.Add(Methods.instance.LayoutObject(GameManager.instance.GeneratorsImages[3], GameManager.instance.gridSize + 3f, 2f));
        GameManager.instance.parkingPos.Add(new Vector3(GameManager.instance.gridSize + 3f - 0.5f, 2f + 1.5f, 0f));
    }
}
