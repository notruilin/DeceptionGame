/*
 * The GameManager controls logic of the game, singleton
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Current turn is player's turn or AI's turn
    [HideInInspector] public bool playerTurn;
    // Game over signal
    [HideInInspector] public bool gameOver;
    // AI gonna rotation when it's true
    [HideInInspector] public bool AICelebrate;

    // Game sprites
    public GameObject gridTile;
    public GameObject Anchor;
    public GameObject OnAnchor;
    public GameObject[] GeneratorsImages;
    public GameObject[] pickupTiles;
    public GameObject[] counterTiles;
    public GameObject[] counterOnShuttleTiles;
    public GameObject AI;

    // Stores what happened in the game
    [HideInInspector] public string gameLog;

    // Red, White, Blue, Yellow Generators
    [HideInInspector] public List<GameObject> generators = new List<GameObject>();
    // Parking position of each generator above
    [HideInInspector] public List<Vector3> parkingPos = new List<Vector3>();
    // The center position of each anchor
    [HideInInspector] public List<Vector3> anchorPositions = new List<Vector3>();
    // The colors of deposited counters, deposited[x][y] == -1 means empty grid
    [HideInInspector] public List<List<int>> deposited = new List<List<int>>();
    // blocked[x][y] == true: (x, y) has been blocked
    [HideInInspector] public List<List<bool>> blocked = new List<List<bool>>();
    // Use to prevent counter turned over immediately after deposit
    [HideInInspector] public List<List<bool>> readyToTurnOver = new List<List<bool>>();
    // Store GameObjects of counters deposited on the board
    [HideInInspector] public List<List<GameObject>> countersOnBoard = new List<List<GameObject>>();

    // Board Generator, creates the board and generators
    private BoardGenerator boardScript;
    // AI Manager, controls AI moves
    private AIManager aiScript;

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
    }

    // Initialize the whole game
    private void Initialize()
    {
        gameLog = "";
        gameOver = false;
        AICelebrate = false;
        SetPlayerTurn(false);
        boardScript = GetComponent<BoardGenerator>();
        boardScript.SetupScene();
        aiScript = GetComponent<AIManager>();
        aiScript.InitialiseAIs();
        InitialiseDeposited();
        gameLog += "--- Game Start ---\n";
    }

    // Start AI's turn
    public IEnumerator TurnSwitch()
    {
        if (!gameOver)
        {
            for (int i = 0; i < generators.Count; i++)
            {
                if (!generators[i].GetComponent<GeneratorManager>().visitThisTurn)
                {
                    generators[i].GetComponent<GeneratorManager>().idleTurnCount += 1;
                }
            }
            for (int i = 0; i < generators.Count; i++)
            {
                generators[i].GetComponent<GeneratorManager>().visitThisTurn = false;
            }
            yield return StartCoroutine(GetComponent<UIManager>().ShowAITurn());
            aiScript.AITurn();
        }
    }

    // Check if there is a red path between two anchors
    public bool CheckGameOver()
    {
        foreach (Vector3 position in anchorPositions)
        {
            if (Methods.instance.BFStoAnotherAnchor(new Vector3(position.x - 0.5f, position.y - 0.5f, 0f)))
            {
                gameOver = true;
                return true;
            }
        }
        return false;
    }

    // Show AI Win and write the result to gameLog
    public void GameOverAIWin()
    {
        gameLog += "--- AI Win ---\n";
        gameLog += "AI Turn Count: " + aiScript.turnCount + "\n";
        gameLog += "Remaining Time For AI: " + (GetComponent<UIManager>().AITimeLimit - (Time.time - GetComponent<UIManager>().startTime)) + " seconds\n";
        gameOver = true;
        AICelebrate = true;
        GetComponent<UIManager>().ShowAIWinText();
        Methods.instance.TurnAllWhiteCounterOver();
        SendToServer();
    }

    // Show Player Win and write the result to gameLog
    public void GameOverPlayerWin()
    {
        gameLog += "Time Out!\n";
        gameLog += "--- Player Win ---\n";
        gameLog += "AI Turn Count: " + aiScript.turnCount + "\n";
        gameOver = true;
        GetComponent<UIManager>().ShowPlayerWinText();
        Methods.instance.TurnAllWhiteCounterOver();
        SendToServer();
    }

    // Initializes lists which use to record the game state
    private void InitialiseDeposited()
    {
        deposited.Clear();
        readyToTurnOver.Clear();
        blocked.Clear();
        countersOnBoard.Clear();
        for (int x = 0; x < GameParameters.instance.gridSize; x++)
        {
            deposited.Add(new List<int>());
            readyToTurnOver.Add(new List<bool>());
            blocked.Add(new List<bool>());
            countersOnBoard.Add(new List<GameObject>());
            for (int y = 0; y < GameParameters.instance.gridSize; y++)
            {
                deposited[x].Add(-1);
                readyToTurnOver[x].Add(false);
                blocked[x].Add(false);
                countersOnBoard[x].Add(null);
            }
        }
    }

    public void SetPlayerTurn(bool turn)
    {
        playerTurn = turn;
    }

    private void DestroyObjects(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            Destroy(obj);
        }
    }

    // Clear objects and initialize a new game
    public void RestartGame()
    {
        foreach (GameObject obj in generators)
        {
            Destroy(obj);
        }
        GameObject[] objects;
        objects = GameObject.FindGameObjectsWithTag("PickUp");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Counter");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("WhiteCounter");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Shuttle");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("OnShuttle");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("WhiteOnShuttle");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Floor");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Anchor");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Board");
        DestroyObjects(objects);
        Methods.instance.InitializeMethods();
        Initialize();
        GetComponent<UIManager>().Initialize();
        StartCoroutine(TurnSwitch());
    }

    // Sends the gameLog to Server after gameover
    private void SendToServer()
    {
        StartCoroutine(SendLogToServer());
    }

    IEnumerator SendLogToServer()
    {
        WWWForm form = new WWWForm();
        form.AddField("time", DateTime.Now.ToString() + "\n");
        form.AddField("log", gameLog);    
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost:1234/logFile.php", form))
        {
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Upload complete!");
            }
        }
    }

    // Called when the scene loaded
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            RestartGame();
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
