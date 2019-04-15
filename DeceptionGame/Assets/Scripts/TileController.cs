using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileController : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (!GameManager.instance.gameOver && GameManager.instance.playerTurn)
        {
            if (Methods.instance.IsEmptyGrid(transform.position) && !Methods.instance.IsCorner(transform.position))
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                Color color = new Color(43f, 54f, 58f);
                color.a = 0.1f;
                sr.color = color;
                Methods.instance.BlockTile(transform.position);
                Debug.Log(transform.position + "clicked!");
                GameManager.instance.SetPlayerTurn(false);
                Debug.Log("PlayerTurn is False");
                StartCoroutine(GameManager.instance.TurnSwitch());
            }
        }
    }
}
