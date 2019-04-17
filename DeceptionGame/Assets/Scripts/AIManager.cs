using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class AIManager : MonoBehaviour
{
    public float moveDelay = 0.2f;
    public float moveSpeed = 3f;

    public float collectDelay = 0.5f;
    public float defaultDepositDelay = 0.1f;

    private List<GameObject> AIs = new List<GameObject>();
    private List<Vector3> bagPos = new List<Vector3>();

    private Vector3 startPos;

    private void Awake()
    {
        bagPos.Clear();
        bagPos.Add(new Vector3(-1.6f, 0.02f, 0f));
        bagPos.Add(new Vector3(-0.5f, 0.02f, 0f));
        bagPos.Add(new Vector3(0.55f, 0.02f, 0f));
        bagPos.Add(new Vector3(1.65f, 0.02f, 0f));
        startPos = new Vector3(-3.5f, MainMenu.instance.gridSize / 2f, 0f);
    }

    public void InitialiseAIs()
    {
        AIs.Clear();
        AIs.Add(Methods.instance.LayoutObject(GameManager.instance.AI, 0f, 0f));
    }

    public IEnumerator AITurn()
    {
        Actions actions;
        for (int i = 0; i < AIs.Count; i++)
        {
            actions = AIs[i].GetComponent<AIAgent>().MakeDecision();
            actions.MoveTo(startPos);
            yield return StartCoroutine(ExecuteActions(AIs[i], actions));
        }
        if (!GameManager.instance.gameOver)
        {
            GameManager.instance.SetPlayerTurn(true);
            yield return StartCoroutine(UIManager.instance.ShowPlayerTurn());
        }
    }

    IEnumerator ExecuteActions(GameObject AI, Actions actions)
    {
        for (int i = 0; i < actions.commands.Count; i++)
        {
            if (GameManager.instance.gameOver) break;
            string[] commands = actions.commands[i].Split('#');
            switch (commands[0])
            {
                case "Collect":
                    if (AI.GetComponent<AIBehavior>().carry.Sum() < GameManager.instance.carryLimit && Methods.instance.OnParkingPos(AI.transform.position))
                    {
                        int generatorId = Methods.instance.FindGenerator(AI.transform.position);
                        GameManager.instance.gameLog += "Shuttle collects in Generator " + generatorId + " ";
                        yield return StartCoroutine(CollectCounter(AI, generatorId, actions.paras[i]));
                    }
                    break;
                case "Move":
                    Debug.Log("Moving From" + AI.transform.position + " to " + actions.paras[i]);
                    yield return StartCoroutine(MoveToPosition(moveDelay, AI, actions.paras[i]));
                    GameManager.instance.gameLog += "Shuttle moves to " + "(" + actions.paras[i].x + ", " + actions.paras[i].y + ")" + "\n";
                    break;
                case "Deposit":
                    Vector3 pos = new Vector3(actions.paras[i].x, actions.paras[i].y, 0f);
                    int color = Int32.Parse(commands[1]);
                    if (AI.transform.position == pos)
                    {
                        Debug.Log("Start deposit at: " + actions.paras[i]);
                        yield return StartCoroutine(DepositCounter(AI, pos, color, actions.paras[i].z));
                        GameManager.instance.gameLog += "Shuttle deposits at " + "(" + pos.x + ", " + pos.y + ")" + ", color: " + color + "\n";
                    }
                    break;
                case "TurnOver":
                    int index = Int32.Parse(commands[1]);
                    StartCoroutine(TurnOverCounterInBag(AI, index, actions.paras[i].x));
                    GameManager.instance.gameLog += "Shuttle turns over bag " + index + " for " + actions.paras[i].x + "seconds" + "\n";
                    break;
                case "CollectFromBoard":
                    yield return StartCoroutine(CollectCounterFromGrid(AI, actions.paras[i]));
                    GameManager.instance.gameLog += "Shuttle collects from " + "(" + actions.paras[i].x + ", " + actions.paras[i].y + ")" + "\n";
                    break;
            }
            if (GameManager.instance.CheckGameOver())
            {
                GameManager.instance.GameOverAIWin();
            }
        }
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
            DelFromBag(AI, color);
            GameManager.instance.deposited[(int)pos.x][(int)pos.y] = color;
        }
    }

    IEnumerator CollectCounter(GameObject AI, int generatorId, Vector3 pos)
    {
        if (Methods.instance.IsInGn(pos, generatorId) && GetEmptyBagPosIndex(AI) != -1)
        {
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
        Debug.Log("CollectCounterFromGrid  color =  " + color);
        int bagPosIndex = GetEmptyBagPosIndex(AI);
        Debug.Log("CollectCounterFromGrid  bagPosIndex =  " + bagPosIndex);
        yield return StartCoroutine(MoveToBagPosition(AI, bagPosIndex));
        Debug.Log("After MoveToBagPosition");
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
        AI.GetComponent<AIBehavior>().counterInBag[i] = Methods.instance.LayoutObject(GameManager.instance.counterOnShuttleTiles[3], AI.transform.position.x + bagPos[i].x, AI.transform.position.y + bagPos[i].y);
        AI.GetComponent<AIBehavior>().counterInBag[i].transform.SetParent(AI.transform);
        StartCoroutine(TurnOverCounterInBag(AI, i, 0.5f));
    }

    private IEnumerator TurnOverCounterInBag(GameObject AI, int i, float turnOverDelay)
    {
        if (AI.GetComponent<AIBehavior>().bagCounterColor[i] != -1)
        {
            GameObject colorCounter = Methods.instance.LayoutObject(GameManager.instance.counterOnShuttleTiles[AI.GetComponent<AIBehavior>().bagCounterColor[i]], AI.GetComponent<AIBehavior>().counterInBag[i].transform.position.x, AI.GetComponent<AIBehavior>().counterInBag[i].transform.position.y);
            colorCounter.transform.SetParent(AI.transform);
            AI.GetComponent<AIBehavior>().counterInBag[i].SetActive(false);
            yield return new WaitForSeconds(turnOverDelay);
            Destroy(colorCounter);
            AI.GetComponent<AIBehavior>().counterInBag[i].SetActive(true);
        }
    }
}
