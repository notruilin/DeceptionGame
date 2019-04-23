using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorManager : MonoBehaviour
{
    public float regenerateDelay = 1.5f;

    private List<GameObject> pickupsInGn = new List<GameObject>();
    private List<int> pickupsInGnColor = new List<int>();

    // The positions which need to regenerate counters
    private List<Vector3> reGeneratePickup = new List<Vector3>();

    private int RandomColor()
    {
        int randomIndex = Random.Range(0, GameParameters.instance.colorBag.Count);
        return GameParameters.instance.colorBag[randomIndex];
    }

    private GameObject GeneratePickup(Vector3 pos, int color)
    {
        GameObject pickup = Methods.instance.LayoutObject(GameManager.instance.pickupTiles[color], pos.x, pos.y);
        pickup.transform.SetParent(transform);
        return pickup;
    }

    private void Awake()
    {
        // Adjusts generator scale by counterNumInGenerator
        transform.localScale += new Vector3((GameParameters.instance.counterNumInGenerator - 4) * 0.2f, 0f, 0f);
        float d = -0.7f, reverse = -1;
        // generator on right side
        if (transform.position.x > GameParameters.instance.gridSize)
        {
            d = 0.7f;
            reverse = 1;
        }
        for (int i = 0; i < GameParameters.instance.counterNumInGenerator; i++)
        {
            int randomColor = RandomColor();
            pickupsInGn.Add(GeneratePickup(new Vector3(transform.position.x + d, transform.position.y, 0f), randomColor));
            pickupsInGnColor.Add(randomColor);
            d += 1f * reverse;
        }
    }

    private IEnumerator RegenerateCounter(Vector3 position)
    {
        yield return new WaitForSeconds(regenerateDelay);
        int randomColor = RandomColor();
        pickupsInGn.Add(GeneratePickup(position, randomColor));
        pickupsInGnColor.Add(randomColor);
    }

    private void FixedUpdate()
    {
        if (reGeneratePickup.Count > 0)
        {
            StartCoroutine(RegenerateCounter(reGeneratePickup[0]));
            reGeneratePickup.RemoveAt(0);
        }
    }

    private int GetPickupIndex(Vector3 pos)
    {
        int index = -1;
        for (int i = 0; i < pickupsInGn.Count; i++)
        {
            if (pickupsInGn[i].transform.position == pos)
            {
                index = i;
            }
        }
        return index;
    }

    public void AddToRegenerateList(Vector3 position)
    {
        reGeneratePickup.Add(position);
        int index = GetPickupIndex(position);
        Destroy(pickupsInGn[index]);
        pickupsInGn.RemoveAt(index);
        pickupsInGnColor.RemoveAt(index);
    }

    public List<GameObject> GetPickupsInGn()
    {
        return pickupsInGn;
    }

    public int GetRedPickupsNumber()
    {
        int count = 0;
        foreach (int color in pickupsInGnColor)
        {
            if (color == GameParameters.red)
            {
                count += 1;
            }
        }
        return count;
    }

    public int GetPickupsInGnColor(Vector3 pos)
    {
        int index = GetPickupIndex(pos);
        if (index == -1)
        {
            return -1;
        }
        return pickupsInGnColor[index];
    }
}
