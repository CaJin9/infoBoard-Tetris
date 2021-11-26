using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class GameMaster : MonoBehaviour
{
    // settings
    public static float updatesPerSecond = 1.0f;
    public static float boostFallMultiplier = 10.0f;

    // board
    public static int height = 10;
    public static int width = 24;
    public static GameObject[,] grid = new GameObject[width, height];

    // controls
    public static float holdDuration = 0.3f;
    public static float holdUpdateSpeed = 0.05f;

    // game state
    public static int clearedLines = 0;
    private const int numberOfLinesForLvlUp = 10;
    private const float LvlUpIncrement = 0.2f;
    private const string LEVELSTRING = "Level: ";
    private static TextMeshProUGUI levelText;
    private static TextMeshProUGUI gameOverText;
    private static int currentLvl = 1;

    // init grid
    private int numberOfColumns = 3;
    private int maximumNumberOfMudas = 3;

    // held block
    public static GameObject heldBlock;
    public static Vector3 heldBlockPos = new Vector3(width - 3, height + 1, 0);
    public static bool alreadySwitched = false;
    public static Color heldBlockColor;

    // Scoring System
    public static int points = 0;
    private static TextMeshProUGUI scoreText;
    private static int combo;

    // Animation
    public static float timeUntilCollapse = 0.2f;
    private static float collapseDuration = 0.05f;
    private static GameMaster instance;

    // vfx
    public Material glowMat;
    private static Material glowMatStatic;

    public static bool pause = true; // rotating & moving is still possible in pause
    private static float pauseTimer = 0;
    private static float pauseDuration = 0;

    public GameObject Cell;


    public static Color[] TetrisColors = new Color[] {
        new Color(20 / 255f, 45 / 255f, 85 / 255f),
        new Color(121 / 255f, 185 / 255f, 231 / 255f),
        new Color(109 / 255f, 115 / 255f, 126 / 255f),
        new Color(77 / 255f, 178 / 255f, 107 / 255f),
        new Color(218 / 255f, 190 / 255f, 91 / 255f),
        new Color(233 / 255f, 70/ 255f, 100 / 255f),
        new Color(195 / 255f, 130/ 255f, 255 / 255f)
    };

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene(); 
            SceneManager.LoadScene(scene.name);
        }

        if (pause)
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer > pauseDuration)
            {
                pauseTimer = 0;
                pause = false;
            }
        }
    }

    void Start()
    {
        glowMatStatic = glowMat;
        instance = this;
        scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
        gameOverText = GameObject.Find("GameOverText").GetComponent<TextMeshProUGUI>();
        gameOverText.enabled = false;
        levelText = GameObject.Find("LevelText").GetComponent<TextMeshProUGUI>();
        levelText.text = LEVELSTRING + currentLvl;
        /*
        for (int i = 0; i < numberOfColumns; i++)
        {
            int numberOfMudas = Random.Range(1, maximumNumberOfMudas + 1);
            List<int> idc = new();
            for (int m = 0; m < numberOfMudas; m++)
            {
                while (true)
                {
                    int temp = Random.Range(0, height);
                    if (!idc.Contains(temp))
                    {
                        idc.Add(temp);
                        break;
                    }
                }
            }

            for (int j = 0; j < height; j++)
            {
                if (!idc.Contains(j))
                {
                    var c = Instantiate(Cell, new Vector3(i, j, 0), Quaternion.identity);
                    grid[i, j] = c.transform;
                }
            }
        } */
    }

    private static void AddPoints(int numberOfClearedLines)
    {
        switch (numberOfClearedLines)
        {
            case 0: break;
            case 1:
                points += 40 * currentLvl * (combo + 1);
                break;
            case 2:
                points += 100 * currentLvl * (combo + 1);
                break;
            case 3:
                points += 300 * currentLvl * (combo + 1);
                break;
            case 4:
                points += 1200 * currentLvl * (combo + 1);
                break;
            default: break;
        }
        scoreText.text = points.ToString();
    }

    public static void AddPointsDrop(int numOfDrops)
    {
        points += (2 * numOfDrops);
        scoreText.text = points.ToString();
    }

    static IEnumerator wait(float sec, int i, int[] numberOfLinesClearedPerRow)
    {
        pause = true;
        pauseDuration = sec + numberOfLinesClearedPerRow[numberOfLinesClearedPerRow.Length - 1] * collapseDuration;
        DeleteLine(i);
        yield return new WaitForSeconds(sec);
        Collapse(i, numberOfLinesClearedPerRow);
        ClearedLine();
        
        var objects = FindObjectsOfType<Block>();
        yield return new WaitForSeconds(numberOfLinesClearedPerRow[numberOfLinesClearedPerRow.Length-1] * collapseDuration);
        for (int j = 0; j < objects.Length; j++)
        {
            var b = objects[j].GetComponent<Block>();
            if (b.enabled)
            {
                b.SetGhostPosition();
            }
            // clear empty gameobjects
            if (objects[j].gameObject.activeInHierarchy && objects[j].gameObject.transform.childCount == 0 && objects[j].gameObject.name.Contains("(Clone)"))
            {
                Destroy(objects[j].gameObject);
            }
        }
    }

    public static void CheckForLines()
    {
        int numberOfClearedlines = 0;
        int[] numberOfLinesClearedPerRow = new int[width];


        for (int i = 0; i < width; i++)
        {
            numberOfLinesClearedPerRow[i] = numberOfClearedlines;
            if (HasLine(i))
            {
                numberOfClearedlines++;
            }
        }

        for (int i = width - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                instance.StartCoroutine(wait(timeUntilCollapse, i, numberOfLinesClearedPerRow));
            }
        }

        if (numberOfClearedlines > 0)
        {
            combo++;
        } else
        {
            combo = 0;
        }

        AddPoints(numberOfClearedlines);
        FindObjectOfType<Spawner>().newBlock();
        alreadySwitched = false;
    }

    private static void ClearedLine()
    {
        clearedLines++;
        if (clearedLines % numberOfLinesForLvlUp == 0)
        {
            updatesPerSecond += LvlUpIncrement;
            currentLvl++;
            levelText.text = LEVELSTRING + currentLvl;
        }
    }

    private static bool HasLine(int i)
    {
        for (int j = 0; j < height; j++)
        {
            if (grid[i, j] == null)
                return false;
        }
        return true;
    }

    private static void DeleteLine(int i)
    {
        for (int j = 0; j < height; j++)
        {
            //grid[i, j].GetComponent<SpriteRenderer>().material = glowMatStatic;
            LeanTween.color(grid[i, j], new Color(1, 1, 1, 1), timeUntilCollapse);
            Destroy(grid[i, j].gameObject, timeUntilCollapse);
            grid[i, j] = null;
        }
    }

    private static void Collapse(int i, int[] numberOfLinesClearedPerRow)
    {
        for (int y = i; y < width; y++)
        {
            for (int j = 0; j < height; j++)
            {
                if (grid[y, j] != null)
                {
                    grid[y - 1, j] = grid[y, j];
                    grid[y, j] = null;
                    LeanTween.move(grid[y - 1, j], grid[y - 1, j].transform.position - new Vector3(numberOfLinesClearedPerRow[y], 0, 0), collapseDuration * numberOfLinesClearedPerRow[y]);
                }
            }
        }
    }

    public static bool AddToGrid(Transform transform)
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if (roundedX < width && roundedY < height)
            {
                grid[roundedX, roundedY] = children.gameObject;
            }
            else
            {
                GameOver();
                return false; // GAME OVER
            }
        }
        return true;
    }

    public static bool ValidMove(Transform transform)
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            // stay within board
            if (roundedX < 0 || roundedX >= width || roundedY < 0 || roundedY >= height)
                return false;

            // collide with other blocks
            if (grid[roundedX, roundedY] != null)
                return false;
        }
        return true;
    }

    public static void GameOver()
    {
        gameOverText.enabled = true;
    }
}
