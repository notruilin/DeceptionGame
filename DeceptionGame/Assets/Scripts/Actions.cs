using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actions
{
    public List<string> commands = new List<string>();
    public List<Vector3> paras = new List<Vector3>();

    public void CollectAt(List<Vector3> positions)
    {
        foreach (Vector3 pos in positions)
        {
            commands.Add("Collect");
            paras.Add(pos);
        }
    }

    public void MoveTo(Vector3 pos)
    {
        commands.Add("Move");
        paras.Add(pos);
    }

    public void MoveTo(List<Vector3> positions)
    {
        foreach (Vector3 pos in positions)
        {
            commands.Add("Move");
            paras.Add(pos);
        }
    }

    public void DepositAt(Vector3 pos, int color)
    {
        commands.Add("Deposit#" + color.ToString());
        // (x,y), z == delay
        paras.Add(new Vector3(pos.x, pos.y, 0f));
    }

    public void DepositAt(Vector3 pos, int color, float delay)
    {
        commands.Add("Deposit#" + color.ToString());
        // (x,y), z == delay
        paras.Add(new Vector3(pos.x, pos.y, delay));
    }

    public List<Vector3> GetDepositPos()
    {
        List<Vector3> depositList = new List<Vector3>();
        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i].Length > 7 && commands[i].Substring(0, 7).Equals("Deposit"))
            {
                depositList.Add(new Vector3(paras[i].x, paras[i].y, 0f));
            }
        }
        return depositList;
    }

    public void TurnOverCounterInBagByIndex(int index, float delay)
    {
        commands.Add("TurnOver#" + index.ToString());
        paras.Add(new Vector3(delay, 0f, 0f));
    }

    public void CollectFromBoard(Vector3 pos)
    {
        commands.Add("CollectFromBoard");
        paras.Add(pos);
    }
}
