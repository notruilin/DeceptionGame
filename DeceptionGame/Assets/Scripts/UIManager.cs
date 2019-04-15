﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public float showTurnDelay = 0.3f;
    public float delayBeforeAITurn = 0.3f;
    // seconds
    public float AITimeLimit = 180;

    private float startTime;
    // Set to true after show AI Turn panel 
    private bool AITurn;

    public Text timer;
    public GameObject AITurnPanel;
    public GameObject PlayerTurnPanel;
    public GameObject AIWinPanel;
    public GameObject PlayerWinPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        AITurnPanel.SetActive(false);
        PlayerTurnPanel.SetActive(false);
        AIWinPanel.SetActive(false);
        PlayerWinPanel.SetActive(false);
        string min = ((int)AITimeLimit / 60).ToString().PadLeft(2, '0');
        string sec = (AITimeLimit % 60).ToString("f0").PadLeft(2, '0');
        timer.text = "Remaining Time: " + min + ":" + sec;
        AITurn = false;
    }

    public void ShowAIWinText()
    {
        AIWinPanel.SetActive(true);
    }

    private void ShowPlayerWinText()
    {
        PlayerWinPanel.SetActive(true);
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

    // Update is called once per frame
    void FixedUpdate()
    {
        if (AITurn && !GameManager.instance.gameOver)
        {
            float d = AITimeLimit - (Time.time - startTime);
            string min = ((int)d / 60).ToString().PadLeft(2,'0');
            string sec = (d % 60).ToString("f0").PadLeft(2, '0');
            if ((int)d / 60 == 0 && d % 60 <= 15f)
            {
                timer.color = Color.red;
                if (d % 60 <= 0f)
                {
                    GameManager.instance.gameOver = true;
                    ShowPlayerWinText();
                }
            }
            if (d / 60 > 0 || d % 60 > 0f)
            {
                timer.text = "Remaining Time For AI: " + min + ":" + sec;
            }
        }
    }
}
