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
        commands.Add("Deposit");
        // (x,y), z == color
        paras.Add(new Vector3(pos.x, pos.y, color));
    }

    public List<Vector3> GetDepositPos()
    {
        List<Vector3> depositList = new List<Vector3>();
        for (int i = 0; i < commands.Count; i++)
        {
            if (commands[i].Equals("Deposit"))
            {
                depositList.Add(paras[i]);
            }
        }
        return depositList;
    }
}
