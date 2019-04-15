using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIManager : MonoBehaviour
{
    public float moveDelay = 0.2f;
    public float moveSpeed = 3f;

    public float trueDepositDelay = 0.1f;
    public float fakeDepositDelay = 1f;
    public float collectDelay = 0.5f;

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
        startPos = new Vector3(-3.5f, GameManager.instance.gridSize / 2f, 0f);
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

    IEnumerator CollectCounter(GameObject AI, int generatorId, Vector3 pos)
    {
        if (Methods.instance.IsInGn(pos, generatorId))
        {
            AI.GetComponent<AIBehavior>().carry[GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos)]++;
            AddToBag(AI, GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos));
            GameManager.instance.generators[generatorId].GetComponent<GeneratorManager>().AddToRegenerateList(pos);
            yield return new WaitForSeconds(collectDelay);
        }
    }

    IEnumerator ExecuteActions(GameObject AI, Actions actions)
    {
        for (int i = 0; i < actions.commands.Count; i++)
        {
            if (GameManager.instance.gameOver) break;
            switch (actions.commands[i])
            {
                case "Collect":
                    if (AI.GetComponent<AIBehavior>().carry.Sum() < GameManager.instance.carryLimit && Methods.instance.OnParkingPos(AI.transform.position))
                    {
                        int generatorId = Methods.instance.FindGenerator(AI.transform.position);
                        yield return StartCoroutine(CollectCounter(AI, generatorId, actions.paras[i]));
                    }
                    break;
                case "Move":
                    Debug.Log("Moving From" + AI.transform.position + " to " + actions.paras[i]);
                    yield return StartCoroutine(MoveToPosition(moveDelay, AI, actions.paras[i]));
                    break;
                case "Deposit":
                    Vector3 pos = new Vector3(actions.paras[i].x, actions.paras[i].y, 0f);
                    int color = (int)actions.paras[i].z;
                    if (AI.transform.position == pos)
                    {
                        Debug.Log("Start deposit at: " + actions.paras[0]);
                        yield return StartCoroutine(DepositCounter(AI, pos, color));
                    }
                    break;
            }
            Debug.Log("--------Check Game Over---------");
            if (GameManager.instance.CheckGameOver())
            {
                GameManager.instance.GameOverAIWin();
            }
        }
    }

    IEnumerator MoveToPosition(float delay, GameObject AI, Vector3 newPos)
    {
        while (AI.transform.position != newPos)
        {
            AI.transform.position = Vector3.MoveTowards(AI.transform.position, newPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(delay);
    }

    IEnumerator DepositCounter(GameObject AI, Vector3 pos, int color)
    {
        if (AI.GetComponent<AIBehavior>().carry[color] > 0 && Methods.instance.IsEmptyGrid(pos))
        {
            yield return StartCoroutine(MoveToBagPosition(AI, color));
            if (color == 0)
            {
                yield return new WaitForSeconds(trueDepositDelay);
            }
            else
            {
                yield return new WaitForSeconds(fakeDepositDelay);
            }
            Methods.instance.LayoutObject(GameManager.instance.counterTiles[3], pos.x, pos.y);
            AI.GetComponent<AIBehavior>().carry[color]--;
            DelFromBag(AI, color);
            GameManager.instance.deposited[(int)pos.x][(int)pos.y] = color;
        }
    }

    private IEnumerator MoveToBagPosition(GameObject AI, int color)
    {
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < 4; i++)
        {
            if (AI.GetComponent<AIBehavior>().bagCounterColor[i] == color)
            {
                pos = bagPos[i];
                break;
            }
        }
        Debug.Log("!!!!!!!!------BAG POS: " + pos);
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

    private void AddToBag(GameObject AI, int color)
    {
        for (int i = 0; i < 4; i++)
        {
            if (AI.GetComponent<AIBehavior>().bagCounterColor[i] == -1)
            {
                AI.GetComponent<AIBehavior>().bagCounterColor[i] = color;
                AI.GetComponent<AIBehavior>().counterInBag[i] = Methods.instance.LayoutObject(GameManager.instance.counterOnShuttleTiles[3], AI.transform.position.x + bagPos[i].x, AI.transform.position.y + bagPos[i].y);
                AI.GetComponent<AIBehavior>().counterInBag[i].transform.SetParent(AI.transform);
                StartCoroutine(TurnOverCounterInBag(AI, i, 0.5f));
                break;
            }
        }
    }

    private IEnumerator TurnOverCounterInBag(GameObject AI, int i, float turnOverDelay)
    {
        GameObject colorCounter = Methods.instance.LayoutObject(GameManager.instance.counterOnShuttleTiles[AI.GetComponent<AIBehavior>().bagCounterColor[i]], AI.GetComponent<AIBehavior>().counterInBag[i].transform.position.x, AI.GetComponent<AIBehavior>().counterInBag[i].transform.position.y);
        AI.GetComponent<AIBehavior>().counterInBag[i].SetActive(false);
        yield return new WaitForSeconds(turnOverDelay);
        Destroy(colorCounter);
        AI.GetComponent<AIBehavior>().counterInBag[i].SetActive(true);
    }
}
