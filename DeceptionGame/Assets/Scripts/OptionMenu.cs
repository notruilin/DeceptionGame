using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    private void SetGridSizeBig()
    {
        GameParameters.instance.gridSize = 30;
    }

    private void SetGridSizeMedium()
    {
        GameParameters.instance.gridSize = 25;
    }

    private void SetGridSizeSmall()
    {
        GameParameters.instance.gridSize = 20;
    }

    private void SetRandomAnchor()
    {
        GameParameters.instance.randomAnchor = true;
    }

    private void SetDefaultAnchor()
    {
        GameParameters.instance.randomAnchor = false;
    }

    private void SetAnchorDisLarge()
    {
        GameParameters.instance.minAnchorDis = 8;
    }

    private void SetAnchorDisSmall()
    {
        GameParameters.instance.minAnchorDis = 4;
    }

    // Gets numbers from more options panel
    public InputField shuttleNumIF;
    public InputField gridSizeIF;
    public InputField carryLimitIF;
    public InputField anchorNumIF;
    public InputField counterNumIF;
    public InputField minDisIF;
    public InputField cusAnchorPosIF;
    public Text errorText;
    public Text successText;

    private List<Vector3> cusAnchors = new List<Vector3>();

    public void SetMoreOptions()
    {
        errorText.text = "";
        successText.text = "";
        if (CheckInputValid())
        {
            GameParameters.instance.shuttleNum = StringToInt(shuttleNumIF.text);
            GameParameters.instance.gridSize = StringToInt(gridSizeIF.text);
            GameParameters.instance.carryLimit = StringToInt(carryLimitIF.text);
            GameParameters.instance.anchorCount = StringToInt(anchorNumIF.text);
            GameParameters.instance.counterNumInGenerator = StringToInt(counterNumIF.text);
            GameParameters.instance.minAnchorDis = StringToInt(minDisIF.text);
            if (cusAnchors.Count > 0)
            {
                GameParameters.instance.randomAnchor = false;
                foreach (Vector3 pos in cusAnchors)
                {
                    GameParameters.instance.defaultAnchorPos.Add(pos);
                }
            }
            StartCoroutine(ShowDoneMessage());
        }
        else
        {
            errorText.text = "Invalid Parameters! Please check and try again.";
            errorText.color = Color.red;
        }
    }

    IEnumerator ShowDoneMessage()
    {
        successText.text = "Done! Enjoy your game :)";
        yield return new WaitForSeconds(3);
        successText.text = "";
    }

    private bool CheckInputValid()
    {
        if (!(StringToInt(shuttleNumIF.text) >= 1 && StringToInt(shuttleNumIF.text) <= 4))
        {
            return false;
        }
        if (!(StringToInt(gridSizeIF.text) >= 20 && StringToInt(gridSizeIF.text) <= 40))
        {
            return false;
        }
        if (!(StringToInt(carryLimitIF.text) >= 1 && StringToInt(carryLimitIF.text) <= 4))
        {
            return false;
        }
        if (StringToInt(anchorNumIF.text) < 2)
        {
            return false;
        }
        if (!(StringToInt(counterNumIF.text) >= 1 && StringToInt(counterNumIF.text) <= 9))
        {
            return false;
        }
        if (StringToInt(minDisIF.text) < 3)
        {
            return false;
        }
        cusAnchors.Clear();
        if (cusAnchorPosIF.text.Length > 0)
        {
            string[] positions = cusAnchorPosIF.text.Split(' ');
            // Trans string to Vector3
            for (int i = 0; i < positions.Length; i++)
            {
                // Remove '(' and ')'
                positions[i] = positions[i].Substring(1, positions[i].Length - 2);
                string[] numbers = positions[i].Split(',');
                // Check if the numbers end by ".5"
                if (numbers.Length != 2 || numbers[0][numbers[0].Length - 1] != '5' || numbers[1][numbers[1].Length - 1] != '5' || numbers[0][numbers[0].Length - 2] != '.' || numbers[1][numbers[1].Length - 2] != '.')
                {
                    return false;
                }
                float x = StringToFloat(numbers[0]);
                float y = StringToFloat(numbers[1]);
                if (!Mathf.Approximately(x, -1) && !Mathf.Approximately(y, -1) && x >= 0.5f && y >= 0.5f && x <= StringToInt(gridSizeIF.text) - 1.5f && y <= StringToInt(gridSizeIF.text) - 1.5f)
                {
                    cusAnchors.Add(new Vector3(x, y, 0f));
                }
                else
                {
                    return false;
                }
            }

            // Check minimal distance between two anchors >= 2
            for (int i = 0; i < cusAnchors.Count; i++)
            {
                for (int j = i+1; j < cusAnchors.Count; j++)
                {
                    if (Vector3.Distance(cusAnchors[i], cusAnchors[j]) < 2)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private int StringToInt(string s)
    {
        int num = -1;
        try
        {
            num = Int32.Parse(s);
            return num;
        }
        catch (FormatException)
        {
            return -1;
        }
    }

    private float StringToFloat(string s)
    {
        float num = -1;
        try
        {
            num = float.Parse(s);
            return num;
        }
        catch (FormatException)
        {
            return -1;
        }
    }
}
