using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Current turn is player's turn or AI's turn
    [HideInInspector] public bool playerTurn;
    [HideInInspector] public bool gameOver;

    public GameObject gridTile;
    public GameObject Anchor;
    public GameObject OnAnchor;
    public GameObject[] GeneratorsImages;
    public GameObject[] pickupTiles;
    public GameObject[] counterTiles;
    public GameObject[] counterOnShuttleTiles;
    public GameObject AI;

    public int gridSize;
    public int anchorCount;
    public int counterNumInGenerator;
    public float minAnchorDis;
    public int carryLimit = 4;

    // Red, White, Blue, Yellow Generators
    [HideInInspector] public List<GameObject> generators = new List<GameObject>();
    // Parking position of each generator above
    [HideInInspector] public List<Vector3> parkingPos = new List<Vector3>();
    // The center position of each anchor
    [HideInInspector] public List<Vector3> anchorPositions = new List<Vector3>();
    // The colors of deposited counters, deposited[x][y] == -1 means empty grid
    [HideInInspector] public List<List<int>> deposited = new List<List<int>>();
    // blocked[x][y] == false: (x, y) has been blocked
    [HideInInspector] public List<List<bool>> blocked = new List<List<bool>>();
    // use to prevent counter turned over immediately after deposit
    [HideInInspector] public List<List<bool>> readyToTurnOver = new List<List<bool>>();

    private BoardGenerator boardScript;
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
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
        StartCoroutine(TurnSwitch());
    }

    private void Initialize()
    {
        gameOver = false;
        SetPlayerTurn(false);
        boardScript = GetComponent<BoardGenerator>();
        boardScript.SetupScene();
        aiScript = GetComponent<AIManager>();
        aiScript.InitialiseAIs();
        InitialiseDeposited();
    }

    public IEnumerator TurnSwitch()
    {
        if (!gameOver)
        {
            yield return StartCoroutine(UIManager.instance.ShowAITurn());
            StartCoroutine(aiScript.AITurn());
        }
    }

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

    public void GameOverAIWin()
    {
        gameOver = true;
        StartCoroutine(UIManager.instance.ShowAIWinText());
        Methods.instance.TurnAllWhiteCounterOver();
    }

    public void GameOverPlayerWin()
    {
        gameOver = true;
        StartCoroutine(UIManager.instance.ShowPlayerWinText());
        Methods.instance.TurnAllWhiteCounterOver();
    }

    private void InitialiseDeposited()
    {
        deposited.Clear();
        readyToTurnOver.Clear();
        blocked.Clear();
        for (int x = 0; x < gridSize; x++)
        {
            deposited.Add(new List<int>());
            readyToTurnOver.Add(new List<bool>());
            blocked.Add(new List<bool>());
            for (int y = 0; y < gridSize; y++)
            {
                deposited[x].Add(-1);
                readyToTurnOver[x].Add(false);
                blocked[x].Add(true);
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
        objects = GameObject.FindGameObjectsWithTag("Floor");
        DestroyObjects(objects);
        objects = GameObject.FindGameObjectsWithTag("Anchor");
        DestroyObjects(objects);
        Methods.instance.InitializeMethods();
        Initialize();
        UIManager.instance.Initialize();
        StartCoroutine(TurnSwitch());
    }
}
