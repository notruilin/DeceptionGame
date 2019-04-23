using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior : MonoBehaviour
{
    public int[] carry = new int[3];
    public float turnOverDelay = 0.5f;
    public int[] bagCounterColor = { -1, -1, -1, -1 };
    public GameObject[] counterInBag = new GameObject[4];

    private void Start()
    {
        transform.position = new Vector3(-3.5f, GameParameters.instance.gridSize / 2f, 0f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WhiteCounter"))
        {
            Vector3 pos = collision.gameObject.transform.position;
            if (Methods.instance.ReadyToTurnOver(pos))
            {
                Methods.instance.SetReadyToTurnOver(pos, false);
                StartCoroutine(Methods.instance.TurnWhiteCounterOver(pos, turnOverDelay, this.gameObject));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // The shuttle may exit the counter before it turns back to white
        if (collision.gameObject.CompareTag("WhiteCounter") || collision.gameObject.CompareTag("Counter"))
        {
            Vector3 pos = collision.gameObject.transform.position;
            Methods.instance.SetReadyToTurnOver(pos, true);
        }
    }

    public int[] GetCarryColor()
    {
        int[] copyCarry = new int[3];
        GetComponent<AIBehavior>().carry.CopyTo(copyCarry, 0);
        return copyCarry;
    }

    public int[] GetBagCounterColor()
    {
        int[] copyBag = new int[4];
        GetComponent<AIBehavior>().bagCounterColor.CopyTo(copyBag, 0);
        return copyBag;
    }
}
