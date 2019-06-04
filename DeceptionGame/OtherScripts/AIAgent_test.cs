/*
 * This script is used to test the APIs.
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    // Called before each AI's turn
    public Actions MakeDecision(List<Actions> AIactions)
    {
        Actions actions = new Actions();

        // Gets 1 position of pickup from generator 0
        List<Vector3> collectList = Methods.instance.PickupsPosInGn(0, 1);
        // Moves to the parking position of generator 0
        actions.MoveTo(GameManager.instance.parkingPos[0]);
        // Collect 1 pickup from generator 0
        actions.CollectAt(collectList);
        // Gets 1 position of pickup from generator 1
        collectList = Methods.instance.PickupsPosInGn(1, 1);
        // Moves to the parking position of generator 1
        actions.MoveTo(GameManager.instance.parkingPos[1]);
        // Collect 1 pickup from generator 1
        actions.CollectAt(collectList);
        // Gets 2 positions of pickups from generator 3
        collectList = Methods.instance.PickupsPosInGn(3, 2);
        // Moves to the parking position of generator 3
        actions.MoveTo(GameManager.instance.parkingPos[3]);
        // Collect 2 pickups from generator 3
        actions.CollectAt(collectList);
        // Moves to (0, 0)
        actions.MoveTo(new Vector3(0, 0, 0));
        // Gets the number of red counters on the shuttle now
        int redNum = actions.GetPickupColor()[0];
        // Deposits a red counter at (0, 0) if has one
        if (redNum > 0)
        {
            actions.DepositAt(new Vector3(0, 0, 0), 0);
        }
        // Moves to (2, 3)
        actions.MoveTo(new Vector3(2, 3, 0));
        // Gets the color of each bag on the shuttle now
        int[] bag = actions.GetPickupColorBagPos();
        // Deposit the first counter at (2, 3) if the counter is available
        if (bag[0] != -1)
        {
            actions.DepositIndexAt(new Vector3(2, 3, 0), 0);
        }
        // Moves to (0, 0)
        actions.MoveTo(new Vector3(0, 0, 0));
        // Collect the counter at (0, 0)
        actions.CollectFromBoard(new Vector3(0, 0, 0));
        // Gets the color of each bag on the shuttle now
        bag = actions.GetPickupColorBagPos();
        // Turns over the first counter on the shuttle if the counter is available
        actions.TurnOverCounterInBagByIndex(0);
        // Turns over the second counter on the shuttle if the counter is available
        actions.TurnOverCounterInBagByIndex(1);
        return actions;
    }
}
