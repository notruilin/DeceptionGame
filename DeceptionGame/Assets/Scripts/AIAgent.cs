﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIAgent : MonoBehaviour
{

    private int FindChainCost(Vector3 start, Vector3 end, bool onlyRed)
    {
        List<Vector3> path = Methods.instance.FindPathInGrid(start, end, onlyRed);
        List<Vector3> emptyPos = Methods.instance.RemoveDepositedAndAnchor(path);
        return emptyPos.Count;
    }

    private Vector3[] FindCheapestChain(Vector3[] except, bool onlyRed)
    {
        Vector3[] anchors = new Vector3[2];
        int cheapestCost = int.MaxValue;
        foreach (Vector3 anchorCenter1 in GameManager.instance.anchorPositions)
        {
            foreach (Vector3 anchorCenter2 in GameManager.instance.anchorPositions)
            {
                if (anchorCenter1 == anchorCenter2) continue;
                Vector3 anchor1 = Methods.instance.TransAnchorPositionInGrid(anchorCenter1);
                Vector3 anchor2 = Methods.instance.TransAnchorPositionInGrid(anchorCenter2);
                int cost = FindChainCost(anchor1, anchor2, onlyRed);
                if (cost != 0 && cost < cheapestCost && !(except[0] == anchor1 && except[1] == anchor2) && !(except[0] == anchor2 && except[1] == anchor1))
                {
                    cheapestCost = cost;
                    anchors[0] = anchor1;
                    anchors[1] = anchor2;
                }
            }
        }
        return anchors;
    }

    private List<Vector3> RemovePositionsFromList(List<Vector3> list, List<Vector3> positions)
    {
        List<Vector3> validList = new List<Vector3>();
        for (int i = 0; i < list.Count; i++)
        {
            if (!positions.Contains(list[i]))
            {
                validList.Add(list[i]);
            }
        }
        return validList;
    }

    private Vector3 GetRandomEmptyGrid(Actions actions)
    {
        int x = Random.Range(0, GameParameters.instance.gridSize), y = Random.Range(0, GameParameters.instance.gridSize);
        List<Vector3> positions = actions.GetDepositPos();
        while (!Methods.instance.IsEmptyGrid(new Vector3(x, y, 0f)) || positions.Contains(new Vector3(x, y, 0f)))
        {
            x = Random.Range(0, GameParameters.instance.gridSize);
            y = Random.Range(0, GameParameters.instance.gridSize);
        }
        return new Vector3(x, y, 0f);
    }

    private Actions DepositAllRedCounters(Vector3[] anchors, Actions actions)
    {
        List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(anchors[0], anchors[1], true));
        foreach (Vector3 pos in positions)
        {
            actions.MoveTo(pos);
            actions.DepositAt(pos, 0);
        }
        return actions;
    }

    private List<int> RandomOrder(int num)
    {
        List<int> index = new List<int>();
        for (int i = 0; i < num; i++)
        {
            index.Add(i);
        }
        List<int> randomOrder = new List<int>();
        while (index.Count > 0)
        {
            int i = Random.Range(0, index.Count);
            randomOrder.Add(index[i]);
            index.RemoveAt(i);
        }
        return randomOrder;
    }

    private List<Vector3> GetAllRedPickups()
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < GameManager.instance.generators.Count; i++)
        {
            List<GameObject> pickups = GameManager.instance.generators[i].GetComponent<GeneratorManager>().GetPickupsInGn();
            foreach (GameObject pickup in pickups)
            {
                if (GameManager.instance.generators[i].GetComponent<GeneratorManager>().GetPickupsInGnColor(pickup.transform.position) == 0)
                {
                    positions.Add(pickup.transform.position);
                }
            }
        }
        return positions;
    }

    private void Start()
    {

    }

    public Actions MakeDecision()
    {
        Actions actions = new Actions();

        Vector3[] trueAnchors;
        Vector3[] closestAnchorsRed = FindCheapestChain(new Vector3[2], true);

        // Check if can win the game this turn
        List<Vector3> redPickups = GetAllRedPickups();
        int minCost = FindChainCost(closestAnchorsRed[0], closestAnchorsRed[1], true);
        if (minCost <= GameParameters.instance.carryLimit && redPickups.Count >= minCost)
        {
            for (int i = 0; i < minCost; i++)
            {
                actions.MoveTo(GameManager.instance.parkingPos[Methods.instance.OnPickup(redPickups[i])]);
                actions.CollectAt(redPickups[i]);
            }
            return DepositAllRedCounters(closestAnchorsRed, actions);
        }

        // Randomly choose true path
        Vector3[] closestAnchorsRed2 = FindCheapestChain(closestAnchorsRed, true);
        int tryNum = Random.Range(0, 3);
        if (tryNum <= 1)
        {
            trueAnchors = closestAnchorsRed;
        }
        else
        {
            trueAnchors = closestAnchorsRed2;
        }
        // Choose fake path
        Vector3[] fakeAnchors = FindCheapestChain(trueAnchors, false);
        Debug.Log("True anchors: " + trueAnchors[0] + "  " + trueAnchors[1]);
        Debug.Log("Fake anchors: " + fakeAnchors[0] + "  " + fakeAnchors[1]);

        // Randomly choose generator
        int generatorId = Random.Range(0, 4);
        tryNum = Random.Range(0, 3);
        if (tryNum <= 1)
        {
            generatorId = Methods.instance.MostRedGenerator();
        }
        actions.MoveTo(GameManager.instance.parkingPos[generatorId]);
        actions.CollectAt(Methods.instance.PickupsPosInGn(generatorId, GameParameters.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum()));

        // Randomly turn over counters
        List<int> randomOrder = RandomOrder(4);
        foreach (int i in randomOrder)
        {
            actions.TurnOverCounterInBagByIndex(i, 0.2f);
        }

        // Calculate current carry and bag
        int[] LastCarry = new int[3];
        LastCarry = GetComponent<AIBehavior>().GetCarryColor();
        int[] carry = new int[3];
        carry = Methods.instance.PickupColorInPos(actions.paras, GameParameters.instance.carryLimit - GetComponent<AIBehavior>().carry.Sum());
        for (int k = 0; k < 3; k++)
        {
            carry[k] += LastCarry[k];
        }
        int[] bag = actions.GetPickupColorBagPos();

        // Randomly deposit
        randomOrder = RandomOrder(4);
        Debug.Log("Deposit Order: " + randomOrder[0] + " " + randomOrder[1] + " " + randomOrder[2] + " " + randomOrder[3]);
        foreach (int i in randomOrder)
        {
            Debug.Log("index: " + i + "   " + "color: " + bag[i]);
            tryNum = Random.Range(0, 3);
            if (bag[i] == 0 && tryNum <= 1)
            {
                List <Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(trueAnchors[0], trueAnchors[1], true));
                positions = RemovePositionsFromList(positions, actions.GetDepositPos());
                Vector3 pos;
                if (positions.Count == 0)
                {
                    pos = GetRandomEmptyGrid(actions);
                }
                else
                {
                    pos = Methods.instance.RandomPosition(positions);
                }
                Debug.Log("Deposit At True Path: " + pos);
                actions.MoveTo(pos);
                actions.DepositIndexAt(pos, i, Random.Range(0.1f, 1f));
            }
            else
            {
                List<Vector3> positions = Methods.instance.RemoveDepositedAndAnchor(Methods.instance.FindPathInGrid(fakeAnchors[0], fakeAnchors[1], false));
                positions = RemovePositionsFromList(positions, actions.GetDepositPos());
                Vector3 pos;
                if (positions.Count == 0)
                {
                    pos = GetRandomEmptyGrid(actions);
                }
                else
                {
                    pos = Methods.instance.RandomPosition(positions);
                }
                Debug.Log("Deposit At Fake Path: " + pos);
                actions.MoveTo(pos);
                actions.DepositIndexAt(pos, i, Random.Range(0.1f, 3f));
            }
            bag[i] = -1;

            // Randomly turn over another counter when deposit
            int k = Random.Range(0, 4);
            if (bag[k] != -1)
            {
                actions.TurnOverCounterInBagByIndex(k, 0.1f);
            }
        }
        return actions;
    }
}
