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

    void Awake()
    {
        //anchor.Add(Methods.instance.SearchClosestAnchor(anchor));
        anchor.Add(Methods.instance.RandomAnchor(anchor));
        anchor.Add(Methods.instance.RandomAnchor(anchor));
        anchor.Add(Methods.instance.RandomAnchor(anchor));
        anchor.Add(Methods.instance.RandomAnchor(anchor));
        for (int i = 0; i < anchor.Count; i++)
        {
            anchor[i] = Methods.instance.TransAnchorPositionInGrid(anchor[i]);
        }
        Debug.Log("!!!!!!!!Anchor: ");
        foreach (Vector3 pos in anchor)
        {
            Debug.Log("anchor: " + pos);
        }
        Debug.Log("!!!!!!!!End Anchor");
    }

    public Actions MakeDecision()
    {
        Actions actions = new Actions();
        //Debug.Log("NeedCounterNum: " + (GameManager.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum()));
        int generatorId = Methods.instance.MostRedGenerator();
        GameObject generator = GameManager.instance.generators[generatorId];
        actions.MoveTo(GameManager.instance.parkingPos[generatorId]);
        actions.CollectAt(Methods.instance.PickupsPosInGn(generator, GameManager.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum())); 
        int[] carry = new int[3];
        carry = Methods.instance.PickupColorInPos(actions.paras, GameManager.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum());
        Debug.Log("-----WillBag-----");
        for (int j = 0; j < 3; j++)
        {
            Debug.Log("bag color = " + j + " : " + carry[j]);
        }
        Debug.Log("-----WillBagEnd-----");
        List<Vector3> pathList;
        List<Vector3> truePath;
        Debug.Log("FakeStart: " + anchor[fakeStart]);
        Debug.Log("FakeEnd: " + anchor[fakeEnd]);
        pathList = Methods.instance.FindPathInGrid(anchor[fakeStart], anchor[fakeEnd], false);
        Debug.Log("fakePathxxxxxxxx");
        foreach (Vector3 pos in pathList)
        {
            Debug.Log(pos);
        }
        Debug.Log("End FakePath");
        int otherCounterNum = carry[1] + carry[2];
        int i = 0;
        int fakingNum = Random.Range(0, Mathf.Min(otherCounterNum, 2));
        Debug.Log("fakingNum: " + fakingNum);
        while (otherCounterNum - fakingNum > 0 && i < pathList.Count - 1)
        {
            if (GameManager.instance.deposited[(int)pathList[i].x][(int)pathList[i].y] == -1 && Methods.instance.IsOnAnAnchor(pathList[i]) == Vector3.zero)
            {
                actions.MoveTo(pathList[i]);
                int randomColor = Methods.instance.RandomCarryCounter(carry);
                actions.DepositAt(pathList[i], randomColor);
                carry[randomColor]--;
                Debug.Log("Deposit on fake path AddTarget:  " + pathList[i]);
                anchor[fakeStart] = pathList[i];
                otherCounterNum--;
            }
            i++;
        }
        truePath = Methods.instance.FindPathInGrid(anchor[trueStart], anchor[trueEnd], true);
        pathList = Methods.instance.RemoveDepositedAndAnchor(truePath);
        int depositTrueNum = 0;
        while (carry[0] > 0 && pathList.Count > depositTrueNum)
        {
            Vector3 randomPos = pathList[Random.Range(0, pathList.Count)];
            if (!actions.GetDepositPos().Contains(randomPos))
            {
                actions.MoveTo(randomPos);
                actions.DepositAt(randomPos, 0);
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
            Debug.Log("Neighbor: " + neighbor[i]);
            actions.MoveTo(neighbor[i]);
            int randomColor = Methods.instance.RandomCarryCounter(carry);
            actions.DepositAt(neighbor[i], randomColor);
            carry[randomColor]--;
            Debug.Log("Deposit around true path to confuse AddTarget:  " + neighbor[i]);
        }
        return actions;
    }
}
