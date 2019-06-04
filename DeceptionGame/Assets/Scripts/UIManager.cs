/*
 * The UIManager holds the timer, displays tips when switching turns and the end of the game.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public float showTurnDelay = 5f;
    public float delayBeforeAITurn = 0.3f;
    public float AITimeLimit;
    public float startTime;

    // Set to true after show AI Turn panel 
    private bool AITurn;

    public Text timer;
    public GameObject AITurnPanel;
    public GameObject PlayerTurnPanel;
    public GameObject AIWinPanel;
    public GameObject PlayerWinPanel;
    public GameObject RestartButton;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        AITurnPanel.SetActive(false);
        PlayerTurnPanel.SetActive(false);
        AIWinPanel.SetActive(false);
        PlayerWinPanel.SetActive(false);
        RestartButton.SetActive(false);
        AITimeLimit = GameParameters.instance.timeLimitForAI;
        string min = ((int)AITimeLimit / 60).ToString().PadLeft(2, '0');
        string sec = (AITimeLimit % 60).ToString("f0").PadLeft(2, '0');
        timer.color = new Color32(83, 89, 113, 255);
        timer.text = "Remaining Time For AI: " + min + ":" + sec;
        AITurn = false;
    }

    public void ShowAIWinText()
    {
        AIWinPanel.SetActive(true);
        RestartButton.SetActive(true);
    }

    public void ShowPlayerWinText()
    {
        PlayerWinPanel.SetActive(true);
        RestartButton.SetActive(true);
    }

    public IEnumerator ShowPlayerTurn()
    {
        AITurn = false;
        AITimeLimit -= Time.time - startTime;
        PlayerTurnPanel.SetActive(true);
        yield return new WaitForSeconds(showTurnDelay);
        PlayerTurnPanel.SetActive(false);
    }

    public IEnumerator ShowAITurn()
    {

        yield return new WaitForSeconds(delayBeforeAITurn);
        AITurnPanel.SetActive(true);
        yield return new WaitForSeconds(showTurnDelay);
        AITurnPanel.SetActive(false);
        startTime = Time.time;
        AITurn = true;
    }

    void FixedUpdate()
    {
        if (AITurn && !GameManager.instance.gameOver)
        {
            float d = AITimeLimit - (Time.time - startTime);
            string min = ((int)d / 60).ToString().PadLeft(2,'0');
            string sec = ((int)d % 60).ToString("f0").PadLeft(2, '0');
            if ((int)d / 60 == 0 && d % 60 <= 15f)
            {
                timer.color = Color.red;
                if (d % 60 <= 0f)
                {
                    GameManager.instance.GameOverPlayerWin();
                }
            }
            if (d / 60 > 0 || d % 60 > 0f)
            {
                timer.text = "Remaining Time For AI: " + min + ":" + sec;
            }
        }
    }
}
