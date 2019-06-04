/*
 * The AIManager is responsible to control the movements of shuttles. It gets decisions from the AIAgent and executes these actions on AI’s turn.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class AIManager : MonoBehaviour
{
    public int turnCount;

    public float moveDelay = 0.2f;
    public float moveSpeed = 3f;

    public float collectDelay = 0.3f;
    public float defaultDepositDelay = 0.1f;

    // Records actions for each shuttle
    public List<Actions> AIactions = new List<Actions>();

    private List<GameObject> AIs = new List<GameObject>();
    private List<Vector3> bagPos = new List<Vector3>();
    private List<bool> AIMoving = new List<bool>();

    private void Awake()
    {
        bagPos.Clear();
        bagPos.Add(new Vector3(-1.6f, 0.02f, 0f));
        bagPos.Add(new Vector3(-0.5f, 0.02f, 0f));
        bagPos.Add(new Vector3(0.55f, 0.02f, 0f));
        bagPos.Add(new Vector3(1.65f, 0.02f, 0f));
    }

    public void InitialiseAIs()
    {
        turnCount = 0;
        AIs.Clear();
        AIMoving.Clear();
        for (int i = 0; i < GameParameters.instance.shuttleNum; i++)
        {
            AIs.Add(Methods.instance.LayoutObject(GameManager.instance.AI, 0f, 0f));
            AIs[i].transform.position = new Vector3(-3.5f, GameParameters.instance.gridSize / 2f + i * 1.5f, 0f);
            AIMoving.Add(true);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < GameParameters.instance.shuttleNum; i++)
        {
            if (AIMoving[i]) return;
        }
        if (!GameManager.instance.gameOver)
        {
            GameManager.instance.SetPlayerTurn(true);
            StartCoroutine(GetComponent<UIManager>().ShowPlayerTurn());
            for (int i = 0; i < GameParameters.instance.shuttleNum; i++)
            {
                AIMoving[i] = true;
            }
        }
        else
        {
            Vector3 axis = new Vector3(0, 0, 1);
            if (GameManager.instance.AICelebrate)
            {
                for (int i = 0; i < GameParameters.instance.shuttleNum; i++)
                {
                    AIs[i].transform.RotateAround(AIs[i].transform.position, axis, 130 * Time.deltaTime);
                }
            }
        }
    }

    public void AITurn()
    {
        AIactions.Clear();
        turnCount++;
        for (int i = 0; i < AIs.Count; i++)
        {
            Debug.Log("Shuttle " + i + "Decisions: -------------------");
            AIMoving[i] = true;
            AIactions.Add(AIs[i].GetComponent<AIAgent>().MakeDecision(AIactions));
            AIactions[AIactions.Count - 1].MoveTo(new Vector3(-3.5f, GameParameters.instance.gridSize / 2f + i * 1.5f, 0f));
        }
        for (int i = 0; i < AIs.Count; i++)
        {
            StartCoroutine(ExecuteActions(AIs[i], AIactions[i], i));
        }
    }

    IEnumerator ExecuteActions(GameObject AI, Actions actions, int AIindex)
    {
        for (int i = 0; i < actions.commands.Count; i++)
        {
            if (GameManager.instance.gameOver) break;
            string[] commands = actions.commands[i].Split('#');
            switch (commands[0])
            {
                case "Collect":
                    if (AI.GetComponent<AIBehavior>().carry.Sum() < GameParameters.instance.carryLimit && Methods.instance.OnParkingPos(AI.transform.position))
                    {
                        int generatorId = Methods.instance.FindGenerator(AI.transform.position);
                        GameManager.instance.gameLog += "Shuttle " + AIindex + " collects in Generator " + generatorId + " ";
                        yield return StartCoroutine(CollectCounter(AI, generatorId, actions.paras[i]));
                    }
                    break;
                case "Move":
                    yield return StartCoroutine(MoveToPosition(moveDelay, AI, actions.paras[i]));
                    GameManager.instance.gameLog += "Shuttle " + AIindex + " moves to " + "(" + actions.paras[i].x + ", " + actions.paras[i].y + ")" + "\n";
                    break;
                case "Deposit":
                    Vector3 pos = new Vector3(actions.paras[i].x, actions.paras[i].y, 0f);
                    int num = Int32.Parse(commands[2]);
                    if (commands[1].Equals("Color"))
                    {
                        if (AI.transform.position == pos)
                        {
                            yield return StartCoroutine(DepositCounter(AI, pos, num, actions.paras[i].z));
                            GameManager.instance.gameLog += "Shuttle " + AIindex + " deposits at " + "(" + pos.x + ", " + pos.y + ")" + ", color: " + num + "\n";
                        }
                    }
                    else
                    {
                        if (AI.transform.position == pos)
                        {
                            yield return StartCoroutine(DepositCounterByIndex(AI, pos, num, actions.paras[i].z));
                            GameManager.instance.gameLog += "Shuttle " + AIindex + " deposits at " + "(" + pos.x + ", " + pos.y + ")" + ", index: " + num + "\n";
                        }
                    }
                    break;
                case "TurnOver":
                    int index = Int32.Parse(commands[1]);
                    TurnOverCounterInBag(AI, index);
                    GameManager.instance.gameLog += "Shuttle " + AIindex + " turns over bag " + index + "\n";
                    break;
                case "CollectFromBoard":
                    yield return StartCoroutine(CollectCounterFromGrid(AI, actions.paras[i]));
                    GameManager.instance.gameLog += "Shuttle " + AIindex + " collects from " + "(" + actions.paras[i].x + ", " + actions.paras[i].y + ")" + "\n";
                    break;
            }
            if (GameManager.instance.CheckGameOver())
            {
                GameManager.instance.GameOverAIWin();
            }
        }
        AIMoving[AIindex] = false;
    }

    private IEnumerator MoveToPosition(float delay, GameObject AI, Vector3 newPos)
    {
        while (AI.transform.position != newPos)
        {
            AI.transform.position = Vector3.MoveTowards(AI.transform.position, newPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(delay);
    }

    private IEnumerator DepositCounter(GameObject AI, Vector3 pos, int color, float delay)
    {
        if (AI.GetComponent<AIBehavior>().carry[color] > 0 && Methods.instance.IsEmptyGrid(pos))
        {
            yield return StartCoroutine(MoveToBagPosition(AI, GetBagPosByColor(AI, color)));
            if (Mathf.Approximately(delay, 0f))
            {
                yield return new WaitForSeconds(defaultDepositDelay);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
            GameManager.instance.countersOnBoard[(int)pos.x][(int)pos.y] = Methods.instance.LayoutObject(GameManager.instance.counterTiles[3], pos.x, pos.y);
            AI.GetComponent<AIBehavior>().carry[color]--;
            GameManager.instance.deposited[(int)pos.x][(int)pos.y] = color;
            DelFromBag(AI, color);
        }
    }

    private IEnumerator DepositCounterByIndex(GameObject AI, Vector3 pos, int index, float delay)
    {
        if (AI.GetComponent<AIBehavior>().bagCounterColor[index] != -1 && Methods.instance.IsEmptyGrid(pos))
        {
            yield return StartCoroutine(MoveToBagPosition(AI, index));
            if (Mathf.Approximately(delay, 0f))
            {
                yield return new WaitForSeconds(defaultDepositDelay);
            }
            else
            {
                yield return new WaitForSeconds(delay);
            }
            GameManager.instance.countersOnBoard[(int)pos.x][(int)pos.y] = Methods.instance.LayoutObject(GameManager.instance.counterTiles[3], pos.x, pos.y);
            AI.GetComponent<AIBehavior>().carry[AI.GetComponent<AIBehavior>().bagCounterColor[index]]--;
            GameManager.instance.deposited[(int)pos.x][(int)pos.y] = AI.GetComponent<AIBehavior>().bagCounterColor[index];
            DelFromBagByIndex(AI, index);
        }
    }

    IEnumerator CollectCounter(GameObject AI, int generatorId, Vector3 pos)
    {
        if (Methods.instance.IsInGn(pos, generatorId) && GetEmptyBagPosIndex(AI) != -1)
        {
            GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().visitThisTurn = true;
            AI.GetComponent<AIBehavior>().carry[GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos)]++;
            GameManager.instance.gameLog += "color: " + GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos) + "\n";
            AddToBag(AI, GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos));
            GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().AddToRegenerateList(pos);
            yield return new WaitForSeconds(collectDelay);
        }
    }

    private IEnumerator CollectCounterFromGrid(GameObject AI, Vector3 pos)
    {
        int color = Methods.instance.OnCounter(pos);
        int bagPosIndex = GetEmptyBagPosIndex(AI);
        yield return StartCoroutine(MoveToBagPosition(AI, bagPosIndex));
        if (color != -1 && bagPosIndex != -1)
        {
            AI.GetComponent<AIBehavior>().carry[color]++;
            // Hide the turned over color counter
            GameObject[] counters;
            counters = GameObject.FindGameObjectsWithTag("Counter");
            foreach (GameObject counter in counters)
            {
                if (counter.transform.position == pos)
                {
                    counter.SetActive(false);
                }
            }
            AddToBag(AI, color);
            GameManager.instance.deposited[(int)pos.x][(int)pos.y] = -1;
            GameManager.instance.countersOnBoard[(int)pos.x][(int)pos.y].SetActive(false);
            GameManager.instance.countersOnBoard[(int)pos.x][(int)pos.y] = null;
            yield return new WaitForSeconds(collectDelay);
        }
    }

    private int GetBagPosByColor(GameObject AI, int color)
    {
        for (int i = 0; i < 4; i++)
        {
            if (AI.GetComponent<AIBehavior>().bagCounterColor[i] == color)
            {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator MoveToBagPosition(GameObject AI, int i)
    {
        Vector3 pos = bagPos[i];
        pos = new Vector3(AI.transform.position.x + (pos.x * (-1f)), AI.transform.position.y, 0f);
        while (AI.transform.position != pos)
        {
            AI.transform.position = Vector3.MoveTowards(AI.transform.position, pos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private void DelFromBag(GameObject AI, int color)
    {
        for (int i = 0; i < 4; i++)
        {
            if (AI.GetComponent<AIBehavior>().bagCounterColor[i] == color)
            {
                AI.GetComponent<AIBehavior>().bagCounterColor[i] = -1;
                Destroy(AI.GetComponent<AIBehavior>().counterInBag[i]);
                break;
            }
        }
    }

    private void DelFromBagByIndex(GameObject AI, int index)
    {
        AI.GetComponent<AIBehavior>().bagCounterColor[index] = -1;
        Destroy(AI.GetComponent<AIBehavior>().counterInBag[index]);
    }

    private int GetEmptyBagPosIndex(GameObject AI)
    {
        for (int i = 0; i < 4; i++)
        {
            if (AI.GetComponent<AIBehavior>().bagCounterColor[i] == -1)
            {
                return i;
            }
        }
        return -1;
    }

    private void AddToBag(GameObject AI, int color)
    {
        int i = GetEmptyBagPosIndex(AI);
        AI.GetComponent<AIBehavior>().bagCounterColor[i] = color;
        AI.GetComponent<AIBehavior>().counterInBag[i] = Methods.instance.LayoutObject(GameManager.instance.counterOnShuttleTiles[color], AI.transform.position.x + bagPos[i].x, AI.transform.position.y + bagPos[i].y);
        AI.GetComponent<AIBehavior>().counterInBag[i].transform.SetParent(AI.transform);
    }

    private void TurnOverCounterInBag(GameObject AI, int i)
    {
        GameObject anotherCounter;
        if (AI.GetComponent<AIBehavior>().bagCounterColor[i] != -1)
        {
            anotherCounter = AI.GetComponent<AIBehavior>().counterInBag[i];
            if (AI.GetComponent<AIBehavior>().counterInBag[i].CompareTag("WhiteOnShuttle"))
            {
                AI.GetComponent<AIBehavior>().counterInBag[i] = Methods.instance.LayoutObject(GameManager.instance.counterOnShuttleTiles[AI.GetComponent<AIBehavior>().bagCounterColor[i]], AI.GetComponent<AIBehavior>().counterInBag[i].transform.position.x, AI.GetComponent<AIBehavior>().counterInBag[i].transform.position.y);
            }
            else
            {
                AI.GetComponent<AIBehavior>().counterInBag[i] = Methods.instance.LayoutObject(GameManager.instance.counterOnShuttleTiles[3], AI.GetComponent<AIBehavior>().counterInBag[i].transform.position.x, AI.GetComponent<AIBehavior>().counterInBag[i].transform.position.y);
            }
            AI.GetComponent<AIBehavior>().counterInBag[i].transform.SetParent(AI.transform);
            Destroy(anotherCounter);
        }
    }
}
