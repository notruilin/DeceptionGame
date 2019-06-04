/*
 * Before each turn, the AI agent needs to create an Actions class which includes all actions the shuttle(s) will take sequentially in this turn. 
 */

using System;
using System.Collections.Generic;
using UnityEngine;

public class Actions
{
    public List<string> commands = new List<string>();
    public List<Vector3> paras = new List<Vector3>();

    private List<int[]> carry = new List<int[]>();
    private List<int[]> bagCounterColor = new List<int[]>();

    // Collects pickups from a list of positions
    public void CollectAt(List<Vector3> positions)
    {
        foreach (Vector3 pos in positions)
        {
            commands.Add("Collect");
            paras.Add(pos);
            int id = Methods.instance.OnPickup(pos);
            int color = GameManager.instance.generators[id].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos);
            UpdateCarryAndBagForCollect(color);
        }
    }

    // Collects a pickup from a position
    public void CollectAt(Vector3 pos)
    {
        commands.Add("Collect");
        paras.Add(pos);
        int id = Methods.instance.OnPickup(pos);
        int color = GameManager.instance.generators[id].GetComponent<GeneratorManager>().GetPickupsInGnColor(pos);
        UpdateCarryAndBagForCollect(color);
    }

    // Moves to a position
    public void MoveTo(Vector3 pos)
    {
        commands.Add("Move");
        paras.Add(pos);
    }

    // Moves to a list of positions
    public void MoveTo(List<Vector3> positions)
    {
        foreach (Vector3 pos in positions)
        {
            commands.Add("Move");
            paras.Add(pos);
        }
    }

    // Deposits a specified colored counter at the position
    public void DepositAt(Vector3 pos, int color)
    {
        commands.Add("Deposit#Color#" + color.ToString());
        // (x,y), z == delay
        paras.Add(new Vector3(pos.x, pos.y, 0f));
        UpdateCarryAndBagForDeposit(color);
    }

    // Deposits a specified colored counter at the position with a delay
    public void DepositAt(Vector3 pos, int color, float delay)
    {
        commands.Add("Deposit#Color#" + color.ToString());
        // (x,y), z == delay
        paras.Add(new Vector3(pos.x, pos.y, delay));
        UpdateCarryAndBagForDeposit(color);
    }

    // Deposits a specified indexed counter at the position
    public void DepositIndexAt(Vector3 pos, int index)
    {
        commands.Add("Deposit#Index#" + index.ToString());
        // (x,y), z == delay
        paras.Add(new Vector3(pos.x, pos.y, 0f));
        AddNewCarryAndBag();
        carry[carry.Count - 1][bagCounterColor[bagCounterColor.Count - 1][index]]--;
        bagCounterColor[bagCounterColor.Count - 1][index] = -1;
    }

    // Deposits a specified indexed counter at the position with a delay
    public void DepositIndexAt(Vector3 pos, int index, float delay)
    {
        commands.Add("Deposit#Index#" + index.ToString());
        // (x,y), z == delay
        paras.Add(new Vector3(pos.x, pos.y, delay));
        carry[carry.Count - 1][bagCounterColor[bagCounterColor.Count - 1][index]]--;
        bagCounterColor[bagCounterColor.Count - 1][index] = -1;
    }

    // Returns a list of positions that have been added deposit commands in this actions
    public List<Vector3> GetDepositPosFromActions(Actions acs)
    {
        List<Vector3> depositList = new List<Vector3>();
        for (int i = 0; i < acs.commands.Count; i++)
        {
            if (acs.commands[i].Length > 7 && acs.commands[i].Substring(0, 7).Equals("Deposit"))
            {
                depositList.Add(new Vector3(acs.paras[i].x, acs.paras[i].y, 0f));
            }
        }
        return depositList;
    }

    // Returns a list of positions that have been added collect commands in this actions
    private List<Vector3> GetCollectPosFromActions(Actions acs)
    {
        List<Vector3> collectList = new List<Vector3>();
        for (int i = 0; i < acs.commands.Count; i++)
        {
            if (acs.commands[i].Length >= 7 && acs.commands[i].Substring(0, 7).Equals("Collect"))
            {
                collectList.Add(new Vector3(acs.paras[i].x, acs.paras[i].y, 0f));
            }
        }
        return collectList;
    }

    // Returns a list of positions that have been added deposit commands in this actions as well as all other shuttles' actions.
    public List<Vector3> GetDepositPos(List<Actions> AIactions)
    {
        List<Vector3> depositList = new List<Vector3>();
        depositList.AddRange(GetDepositPosFromActions(this));
        for (int i = 0; i < AIactions.Count; i++)
        {
            depositList.AddRange(GetDepositPosFromActions(AIactions[i]));
        }
        return depositList;
    }

    // Returns a list of positions that have been added collect commands in this actions as well as all other shuttles' actions.
    public List<Vector3> GetCollectPos(List<Actions> AIactions)
    {
        List<Vector3> collectList = new List<Vector3>();
        collectList.AddRange(GetCollectPosFromActions(this));
        for (int i = 0; i < AIactions.Count; i++)
        {
            collectList.AddRange(GetCollectPosFromActions(AIactions[i]));
        }
        return collectList;
    }

    // Turns over the counter with the given index
    public void TurnOverCounterInBagByIndex(int index)
    {
        commands.Add("TurnOver#" + index.ToString());
        paras.Add(new Vector3(0f, 0f, 0f));
    }

    // Collects a counter from the board
    public void CollectFromBoard(Vector3 pos)
    {
        commands.Add("CollectFromBoard");
        paras.Add(pos);
        int color = -1;
        for (int i = paras.Count - 2; i >= 0; i--)
        {
            string[] splitCommands = commands[i].Split('#');
            if (new Vector3(paras[i].x, paras[i].y, 0f) == pos)
            {
                if (splitCommands[0].Equals("Deposit"))
                {
                    if (splitCommands[1].Equals("Color"))
                    {
                        color = Int32.Parse(splitCommands[2]);
                    }
                    else
                    {
                        int index = Int32.Parse(splitCommands[2]);
                        color = bagCounterColor[i][index];
                    }
                    break;
                }
                if (splitCommands[0].Equals("CollectFromBoard"))
                {
                    Debug.LogError("You cannot collect from " + pos);
                    break;
                }
            }
        }
        if (color == -1)
        {
            if (Methods.instance.OnCounter(pos) == -1)
            {
                Debug.LogError("You cannot collect from " + pos);
            }
            else
            {
                color = Methods.instance.OnCounter(pos);
            }
        }
        UpdateCarryAndBagForCollect(color);
    }

    // Gets how many red, yellow and blue counters are carried according to the action
    public int[] GetPickupColor()
    {
        if (carry.Count == 0)
        {
            return new int[] { 0, 0, 0};
        }
        return carry[carry.Count - 1];
    }

    // Gets the color of counter on each bag position
    public int[] GetPickupColorBagPos()
    {
        if (bagCounterColor.Count == 0)
        {
            return new int[] { -1, -1, -1, -1 };
        }
        return bagCounterColor[bagCounterColor.Count - 1];
    }

    private void InitializeCarryAndBag()
    {
        carry.Add(new int[3]);
        bagCounterColor.Add(new int[] { -1, -1, -1, -1 });
    }

    private void CopyTheLastCarryAndBag()
    {
        InitializeCarryAndBag();
        carry[carry.Count - 2].CopyTo(carry[carry.Count - 1], 0);
        bagCounterColor[bagCounterColor.Count - 2].CopyTo(bagCounterColor[bagCounterColor.Count - 1], 0);
    }

    private void AddNewCarryAndBag()
    {
        if (carry.Count == 0)
        {
            InitializeCarryAndBag();
        }
        else
        {
            CopyTheLastCarryAndBag();
        }
    }

    private void UpdateCarryAndBagForCollect(int color)
    {
        AddNewCarryAndBag();
        carry[carry.Count - 1][color]++;
        for (int i = 0; i < bagCounterColor[bagCounterColor.Count - 1].Length; i++)
        {
            if (bagCounterColor[bagCounterColor.Count - 1][i] == -1)
            {
                bagCounterColor[bagCounterColor.Count - 1][i] = color;
                break;
            }
        }
    }

    private void UpdateCarryAndBagForDeposit(int color)
    {
        AddNewCarryAndBag();
        carry[carry.Count - 1][color]--;
        for (int i = 0; i < bagCounterColor[bagCounterColor.Count - 1].Length; i++)
        {
            if (bagCounterColor[bagCounterColor.Count - 1][i] == color)
            {
                bagCounterColor[bagCounterColor.Count - 1][i] = -1;
                break;
            }
        }
    }
}
