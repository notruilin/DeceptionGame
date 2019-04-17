using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    // trueStart, trueEnd, fakeStart, fakeEnd
    private List<Vector3> anchor = new List<Vector3>();
    private int trueStart = 0, trueEnd = 1, fakeStart = 2, fakeEnd = 3;
    private int neighborRange = 3;

    private float trueDepositDelay = 0.1f;
    private float fakeDepositDelay = 1f;

    void Awake()
    {
        anchor.Add(Methods.instance.SearchClosestAnchor(anchor));
        anchor.Add(Methods.instance.SearchClosestAnchor(anchor));
        //anchor.Add(Methods.instance.RandomAnchor(anchor));
        //anchor.Add(Methods.instance.RandomAnchor(anchor));
        anchor.Add(Methods.instance.RandomAnchor(anchor));
        anchor.Add(Methods.instance.RandomAnchor(anchor));
        for (int i = 0; i < anchor.Count; i++)
        {
            anchor[i] = Methods.instance.TransAnchorPositionInGrid(anchor[i]);
        }
        Debug.Log("Anchor: ");
        foreach (Vector3 pos in anchor)
        {
            Debug.Log("anchor: " + pos);
        }
        Debug.Log("End Anchor");
    }

    public Actions MakeDecision()
    {
        Actions actions = new Actions();
        int generatorId = Methods.instance.MostRedGenerator();
        GameObject generator = GameManager.instance.generators[generatorId];
        //actions.TurnOverCounterInBagByIndex(0, 0.5f);
        actions.MoveTo(GameManager.instance.parkingPos[generatorId]);
        actions.CollectAt(Methods.instance.PickupsPosInGn(generatorId, GameManager.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum())); 
        int[] LastCarry = new int[3];
        GetComponent<AIBehavior>().carry.CopyTo(LastCarry, 0);
        int[] carry = new int[3];
        carry = Methods.instance.PickupColorInPos(actions.paras, GameManager.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum());
        for (int k = 0; k < 3; k++)
        {
            carry[k] += LastCarry[k];
        }
        Vector3 tmp = Vector3.zero;
        List<Vector3> pathList;
        List<Vector3> truePath;
        pathList = Methods.instance.FindPathInGrid(anchor[fakeStart], anchor[fakeEnd], false);
        int otherCounterNum = carry[1] + carry[2];
        int i = 0;
        int fakingNum = Random.Range(0, Mathf.Min(otherCounterNum, 2));
        while (otherCounterNum - fakingNum > 0 && i < pathList.Count - 1)
        {
            if (GameManager.instance.deposited[(int)pathList[i].x][(int)pathList[i].y] == -1 && Methods.instance.IsOnAnAnchor(pathList[i]) == Vector3.zero)
            {
                actions.MoveTo(pathList[i]);
                //actions.TurnOverCounterInBagByIndex(1, 0.5f);
                int randomColor = Methods.instance.RandomCarryCounter(carry);
                if (tmp == Vector3.zero)
                {
                    tmp = pathList[i];
                }
                actions.DepositAt(pathList[i], randomColor, fakeDepositDelay);
                carry[randomColor]--;
                Debug.Log("Deposit on fake path AddTarget:  " + pathList[i]);
                anchor[fakeStart] = pathList[i];
                otherCounterNum--;
            }
            i++;
        }
        //actions.TurnOverCounterInBagByIndex(2, 0.5f);
        truePath = Methods.instance.FindPathInGrid(anchor[trueStart], anchor[trueEnd], true);
        pathList = Methods.instance.RemoveDepositedAndAnchor(truePath);
        int depositTrueNum = 0;
        while (carry[0] > 0 && pathList.Count > depositTrueNum)
        {
            Vector3 randomPos = pathList[Random.Range(0, pathList.Count)];
            if (!actions.GetDepositPos().Contains(randomPos))
            {
                actions.MoveTo(randomPos);
                actions.DepositAt(randomPos, 0, trueDepositDelay);
                carry[0]--;
                depositTrueNum++;
                Debug.Log("Deposit on TRUE path AddTarget:  " + randomPos);
            }
        }
        List<Vector3> neighbor = new List<Vector3>();
        if (otherCounterNum > 0)
        {
            neighbor = Methods.instance.FindEmptyNeighbor(truePath, otherCounterNum, actions.GetDepositPos(), neighborRange);
            // Cannot find enough neighbors in range
            while (neighbor.Count == 0)
            {
                neighborRange++;
                neighbor = Methods.instance.FindEmptyNeighbor(truePath, otherCounterNum, actions.GetDepositPos(), neighborRange);
            }
        }
        for (i = 0; i < neighbor.Count; i++)
        {
            actions.MoveTo(neighbor[i]);
            int randomColor = Methods.instance.RandomCarryCounter(carry);
            actions.DepositAt(neighbor[i], randomColor, fakeDepositDelay);
            carry[randomColor]--;
            Debug.Log("Deposit around true path to confuse AddTarget:  " + neighbor[i]);
        }
        //actions.MoveTo(tmp);
        //Debug.Log("Want to move to : " + tmp);
        //actions.CollectFromBoard(tmp);
        return actions;
    }
}
