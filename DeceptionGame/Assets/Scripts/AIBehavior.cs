/*
 * AIBehavior includes AI's properties such as information about carrying counters.
 */

using System.Linq;
using UnityEngine;

public class AIBehavior : MonoBehaviour
{
    public int[] carry = new int[3];
    // Turns over the counters how many seconds when the shuttle collides with them
    public float turnOverDelay = 0.5f;
    public int[] bagCounterColor = { -1, -1, -1, -1 };
    public GameObject[] counterInBag = new GameObject[4];

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (carry.Sum() == 0 && collision.gameObject.CompareTag("WhiteCounter"))
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

    // Returns the number of each counter
    public int[] GetCarryColor()
    {
        int[] copyCarry = new int[3];
        GetComponent<AIBehavior>().carry.CopyTo(copyCarry, 0);
        return copyCarry;
    }

    // Returns the color of each counter in the shuttle's bag
    public int[] GetBagCounterColor()
    {
        int[] copyBag = new int[4];
        GetComponent<AIBehavior>().bagCounterColor.CopyTo(copyBag, 0);
        return copyBag;
    }
}
