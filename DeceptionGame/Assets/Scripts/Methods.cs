/*
 * This class contains several general methods and algorithms that may help you develop your agent.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Methods : MonoBehaviour
{
    public static Methods instance;

    private List<List<Vector3>> path = new List<List<Vector3>>();
    private List<List<bool>> visited = new List<List<bool>>();
    private int[] dx = { -1, 1, 0, 0 };
    private int[] dy = { 0, 0, -1, 1 };

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
    }

    // Returns a random position from the list
    public Vector3 RandomPosition(List<Vector3> list)
    {
        int randomIndex = Random.Range(0, list.Count);
        return list[randomIndex];
    }

    //If the pos is on one anchor, return the position of the anchor's center, else return Vector3.zero
    public Vector3 IsOnAnAnchor(Vector3 pos)
    {
        foreach (Vector3 position in GameManager.instance.anchorPositions)
        {
            if (Vector3.Distance(position, pos) < 1f)
            {
                return position;
            }
        }
        return Vector3.zero;
    }

    // Checks if the position is valid on the board
    public bool IsOnBoard(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < GameParameters.instance.gridSize && pos.y >= 0 && pos.y < GameParameters.instance.gridSize)
        {
            return true;
        }
        return false;
    }

    // Checks if the position is empty grid
    public bool IsEmptyGrid(Vector3 pos)
    {
        if (GameManager.instance.deposited[(int)pos.x][(int)pos.y] == -1 && IsOnAnAnchor(pos) == Vector3.zero && TileExist(pos))
        {
            return true;
        }
        return false;
    }

    // Checks if two positions are adjacent
    public bool IsAdjGrid(Vector3 pos1, Vector3 pos2)
    {
        if (Mathf.Approximately(Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y), 1f))
        {
            return true;
        }
        return false;
    }

    // Returns the generator ID the parking position belongs
    public int FindGenerator(Vector3 pos)
    {
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            if (GameManager.instance.parkingPos[i] == pos)
            {
                return i;
            }
        }
        return -1;
    }

    // Checks if the position belongs to the generator
    public bool IsInGn(Vector3 pos, int generatorId)
    {
        GameObject generator = GameManager.instance.generators[generatorId];
        foreach (GameObject pickups in generator.GetComponent<GeneratorManager>().GetPickupsInGn())
        {
            if (pickups.transform.position == pos)
            {
                return true;
            }
        }
        return false;
    }

    // Returns the generator ID if the pos has a pickup
    public int OnPickup(Vector3 pos)
    {
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            List<GameObject> pickups = GameManager.instance.generators[i].GetComponent<GeneratorManager>().GetPickupsInGn();
            for (int j = 0; j < pickups.Count; j++)
            {
                if (pickups[j].transform.position == pos)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    // Returns counter's color at pos, if no counter, return -1
    public int OnCounter(Vector3 pos)
    {
        if (IsOnBoard(pos) && GameManager.instance.deposited[(int)pos.x][(int)pos.y] > -1)
        {
            return GameManager.instance.deposited[(int)pos.x][(int)pos.y];
        }
        return -1;
    }

    public bool ReadyToTurnOver(Vector3 pos)
    {
        if (IsOnBoard(pos))
        {
            return GameManager.instance.readyToTurnOver[(int)pos.x][(int)pos.y];
        }
        return false;
    }


    public void SetReadyToTurnOver(Vector3 pos, bool ready)
    {
        if (IsOnBoard(pos))
        {
            GameManager.instance.readyToTurnOver[(int)pos.x][(int)pos.y] = ready;
        }
    }

    List<GameObject> waitToDestoryCounter = new List<GameObject>();
    List<GameObject> waitToActiveCounter = new List<GameObject>();
    List<GameObject> shuttles = new List<GameObject>();

    public void InitializeMethods()
    {
        waitToDestoryCounter.Clear();
        waitToActiveCounter.Clear();
        shuttles.Clear();
    }

    public IEnumerator TurnWhiteCounterOver(Vector3 pos, float turnOverDelay, GameObject shuttle)
    {
        GameObject[] counters;
        counters = GameObject.FindGameObjectsWithTag("WhiteCounter");
        foreach (GameObject counter in counters)
        {
            if (counter != null && counter.transform.position == pos)
            {
                counter.SetActive(false);
                if (OnCounter(pos) == -1) continue;
                GameObject colorCounter = LayoutObject(GameManager.instance.counterTiles[OnCounter(pos)], pos.x, pos.y);
                yield return new WaitForSeconds(turnOverDelay);
                // Do not turn back if game over or keep collision with shuttle
                if (!GameManager.instance.gameOver && !colorCounter.GetComponent<BoxCollider2D>().IsTouching(shuttle.GetComponent<BoxCollider2D>()))
                {
                    Destroy(colorCounter);
                    counter.SetActive(true);
                }
                else
                {
                    waitToDestoryCounter.Add(colorCounter);
                    waitToActiveCounter.Add(counter);
                    shuttles.Add(shuttle);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (waitToDestoryCounter.Count == 0) return;
        if (!GameManager.instance.gameOver && !waitToDestoryCounter[0].GetComponent<BoxCollider2D>().IsTouching(shuttles[0].GetComponent<BoxCollider2D>()))
        {
            Destroy(waitToDestoryCounter[0]);
            if (GameManager.instance.deposited[(int)waitToActiveCounter[0].transform.position.x][(int)waitToActiveCounter[0].transform.position.y] == -1)
            {
                Destroy(waitToActiveCounter[0]);
            }
            else
            {
                waitToActiveCounter[0].SetActive(true);
            }
            waitToDestoryCounter.RemoveAt(0);
            waitToActiveCounter.RemoveAt(0);
            shuttles.RemoveAt(0);
        }
    }

    public void TurnAllWhiteCounterOver()
    {
        GameObject[] counters;
        counters = GameObject.FindGameObjectsWithTag("WhiteCounter");
        foreach (GameObject counter in counters)
        {
            if (OnCounter(counter.transform.position) != -1)
            {
                LayoutObject(GameManager.instance.counterTiles[OnCounter(counter.transform.position)], counter.transform.position.x, counter.transform.position.y);
            }
            Destroy(counter);
        }
    }

    // Checks if the position hasn't been blocked
    public bool TileExist(Vector3 pos)
    {
        return !GameManager.instance.blocked[(int)pos.x][(int)pos.y];
    }

    public void BlockTile(Vector3 pos)
    {
        GameManager.instance.blocked[(int)pos.x][(int)pos.y] = true;
    }

    // Randomly chooses from yellow and blue
    public int RandomCarryCounter(int[] carry)
    {
        List<int> bag = new List<int>();
        for (int i = 1; i < 3; i ++)
        {
            for (int j = 0; j < carry[i]; j++)
            {
                bag.Add(i);
            }
        }
        return bag[Random.Range(0, bag.Count)];
    }

    // Removes all deposited grids and anchors in the given list
    public List<Vector3> RemoveDepositedAndAnchor(List<Vector3> list)
    {
        List<Vector3> validList = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
        {
            if (IsEmptyGrid(list[i]))
            {
                validList.Add(list[i]);
            }
        }
        return validList;
    }

    // Finds num neighbors around list, return empty list if it is hard to find
    public List<Vector3> FindEmptyNeighbor(List<Vector3> list, int num, List<Vector3> addedToDeposit, int neighborRange)
    {
        List<Vector3> neighbor = new List<Vector3>();
        int randomCount = 0;
        while (num > 0)
        {
            Vector3 pos = list[Random.Range(0, list.Count)];
            Vector3 newNeighbor = new Vector3(pos.x + Random.Range(0, neighborRange), pos.y + Random.Range(0, neighborRange), 0f);
            if (IsEmptyGrid(newNeighbor) && !addedToDeposit.Contains(newNeighbor) && !list.Contains(newNeighbor) && !neighbor.Contains(newNeighbor))
            {
                neighbor.Add(newNeighbor);
                num--;
            }
            // Hard to find neighbor in range
            randomCount++;
            if (randomCount > 50)
            {
                return new List<Vector3>();
            }
        }
        return neighbor;
    }

    // Returns how many red, yellow and blue counters can be picked up if move according to list, carryLimit is n
    public int[] PickupColorInPos(List<Vector3> list, int n)
    {
        int[] carry = new int[3];
        foreach (Vector3 pos in list)
        {
            if (carry.Sum() == n) break;
            int id = OnPickup(pos);
            if (id > -1)
            {
                carry[GameManager.instance.generators[id].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos)]++;
            }
        }
        return carry;
    }

    // Returns the generator which has the most number of red pickups, if no red counter in any generator, return the first generator
    public int MostRedGenerator()
    {
        int mostCount = -1;
        int bestGenerator = 0;
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            int nowCount = GameManager.instance.generators[i].GetComponent<GeneratorManager>().GetRedPickupsNumber();
            if (nowCount > mostCount)
            {
                mostCount = nowCount;
                bestGenerator = i;
            }
        }
        return bestGenerator;
    }

    // Returns Anchor with the shortest linear distance except pos in list
    public Vector3 SearchClosestAnchor(List<Vector3> list)
    {
        Vector3 anchor = Vector3.zero;
        float dist = Mathf.Infinity;
        foreach (Vector3 position in GameManager.instance.anchorPositions)
        {
            if (Vector3.Distance(transform.position, position) < dist && !list.Contains(position))
            {
                dist = Vector3.Distance(transform.position, position);
                anchor = position;
            }
        }
        return anchor;
    }

    // Returns random Anchor except pos in list
    public Vector3 RandomAnchor(List<Vector3> list)
    {
        int index = Random.Range(0, GameManager.instance.anchorPositions.Count);
        while (list.Contains(GameManager.instance.anchorPositions[index]))
        {
            index = Random.Range(0, GameManager.instance.anchorPositions.Count);
        }
        return GameManager.instance.anchorPositions[index];
    }

    // Transfer anchor's center position to a valid grid
    public Vector3 TransAnchorPositionInGrid(Vector3 position)
    {
        return new Vector3(position.x - 0.5f, position.y - 0.5f, 0f);
    }

    // Returns num pickups' positions in the generator
    public List<Vector3> PickupsPosInGn(int generatorId, int num)
    {
        List<Vector3> pickupsPos = new List<Vector3>();
        List<GameObject> pickups = GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().GetPickupsInGn();
        for (int i = 0; i < pickups.Count; i++)
        {
            //if (generator.GetComponent<GeneratorManager>().GetPickupsInGnColor(pickups[i].transform.position) == 0)
            pickupsPos.Add(pickups[i].transform.position);
            num--;
            if (num == 0) break;
        }
        return pickupsPos;
    }

    public bool OnParkingPos(Vector3 pos)
    {
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            if (GameManager.instance.parkingPos[i] == pos)
            {
                return true;
            }
        }
        return false;
    }

    // Returns a path in grid from start to end
    // if onlyRedCounter == true, the path only throughs red counters
    public List<Vector3> FindPathInGrid(Vector3 start, Vector3 end, bool onlyRedCounter)
    {
        InitialisePath();
        path[(int)start.x][(int)start.y] = new Vector3(-1, -1, 0f);   //Unvisited positions are -2
        Queue queue = new Queue();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector3 now = (Vector3)queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(now.x + dx[i], now.y + dy[i], 0f);
                if (IsOnBoard(pos) && TileExist(pos) && path[(int)pos.x][(int)pos.y].x < -1)
                {
                    if (!onlyRedCounter || IsOnAnAnchor(pos) != Vector3.zero || GameManager.instance.deposited[(int)pos.x][(int)pos.y] == -1 || GameManager.instance.deposited[(int)pos.x][(int)pos.y] == 0)
                    {
                        queue.Enqueue(pos);
                        path[(int)pos.x][(int)pos.y] = now;
                        if (Mathf.Abs(pos.x - end.x) < 1f && Mathf.Abs(pos.y - end.y) < 1f)
                        {
                            return FindPath(pos);
                        }
                    }
                }
            }
        }
        return new List<Vector3>();
    }

    public bool BFStoAnotherAnchor(Vector3 start)
    {
        Vector3 startAnchorCenter = IsOnAnAnchor(start);
        InitialiseVisited();
        Queue queue = new Queue();
        queue.Enqueue(start);
        visited[(int)start.x][(int)start.y] = true;
        while (queue.Count > 0)
        {
            Vector3 now = (Vector3)queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(now.x + dx[i], now.y + dy[i], 0f);
                if (IsOnBoard(pos))
                {
                    if (!visited[(int)pos.x][(int)pos.y] && (GameManager.instance.deposited[(int)pos.x][(int)pos.y] == 0 || IsOnAnAnchor(pos) != Vector3.zero))
                    {
                        queue.Enqueue(pos);
                        visited[(int)pos.x][(int)pos.y] = true;
                        if (IsOnAnAnchor(pos) != Vector3.zero && IsOnAnAnchor(pos) != startAnchorCenter)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    private void InitialisePath()
    {
        path.Clear();
        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            path.Add(new List<Vector3>());
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                path[x].Add(new Vector3(-2, -2, 0f));
            }
        }
    }

    private void InitialiseVisited()
    {
        visited.Clear();
        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            visited.Add(new List<bool>());
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                visited[x].Add(false);
            }
        }
    }

    private List<Vector3> FindPath(Vector3 pos)
    {
        List<Vector3> pathList = new List<Vector3>();
        pathList.Clear();
        while (path[(int)pos.x][(int)pos.y].x >= 0)
        {
            pathList.Add(pos);
            pos = path[(int)pos.x][(int)pos.y];
        }
        pathList.Reverse();
        return pathList;
    }

    public GameObject LayoutObject(GameObject prefab, float x, float y)
    {
        Vector3 position = new Vector3(x, y, 0f);
        return Instantiate(prefab, position, Quaternion.identity);
    }
}
