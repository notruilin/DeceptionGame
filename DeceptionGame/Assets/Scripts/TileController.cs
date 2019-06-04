/*
 * The TileController monitors human blocking on human’s turn
 */

using UnityEngine;

public class TileController : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (!GameManager.instance.gameOver && GameManager.instance.playerTurn)
        {
            if (Methods.instance.IsEmptyGrid(transform.position))
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                Color color = new Color(43f, 54f, 58f);
                color.a = 0.1f;
                sr.color = color;
                Methods.instance.BlockTile(transform.position);
                Debug.Log(transform.position + "clicked!");
                GameManager.instance.gameLog += "Player blocks " + transform.position + "\n";
                GameManager.instance.SetPlayerTurn(false);
                StartCoroutine(GameManager.instance.TurnSwitch());
            }
        }
    }
}
