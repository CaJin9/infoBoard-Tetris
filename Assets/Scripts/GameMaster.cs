using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

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
    public static float timeUntilCollapse = 0.1f;
    private static float collapseDuration = 0.05f;
    private static GameMaster instance;

    // vfx
    public Material glowMat;
    private static Material glowMatStatic;

    // Effect Bar
    private static int effectBarValue;
    public const int effectBarValueMax = 100;
    public static EffectBar effectBar;

    public static bool pause = false; // rotating & moving is still possible in pause .. pause to finish the animation

    public GameObject Cell;
    public static GameObject CellStatic;

    public Color[] TetrisColors;

    public static List<Color> TetrisColorsStatic = new();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene(); 
            SceneManager.LoadScene(scene.name);
        }
    }

    void Start()
    {
        for (int i = 0; i < TetrisColors.Length; i++)
        {
            TetrisColorsStatic.Add(TetrisColors[i]);
        }
        LeanTween.init(width * height * 10);
        CellStatic = Cell;
        glowMatStatic = glowMat;
        instance = this;
        effectBar = GameObject.Find("EffectBar").GetComponent<EffectBar>(); 
        scoreText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();
        gameOverText = GameObject.Find("GameOverText").GetComponent<TextMeshProUGUI>();
        gameOverText.enabled = false;
        levelText = GameObject.Find("LevelText").GetComponent<TextMeshProUGUI>();
        levelText.text = LEVELSTRING + currentLvl;
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
            default: 
                points += 3000 * numberOfClearedLines * currentLvl * (combo + 1); 
                break;
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
        DeleteLine(i, sec);
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

    public static void Execute(Transform transform)
    {
        pause = true;
        if (CheckIfColorsAlign(transform))
        {
            //instance.StartCoroutine(FillHoles());
            instance.StartCoroutine(CollapseHoles());
        } else
        {
            CheckForLines();
        }
        pause = false;
    }

    private static void CheckForLines()
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
                instance.StartCoroutine(wait(timeUntilCollapse + (0.01f * numberOfClearedlines), i, numberOfLinesClearedPerRow));
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

    private static void DeleteLine(int i, float sec)
    {
        for (int j = 0; j < height; j++)
        {
            //grid[i, j].GetComponent<SpriteRenderer>().material = glowMatStatic;
            LeanTween.color(grid[i, j], new Color(1, 1, 1, 1), sec);
            Destroy(grid[i, j].gameObject, sec);
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

    private static bool CheckIfColorsAlign(Transform transform)
    {

        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if (roundedX <= 0) break;

            Color color = children.GetComponent<SpriteRenderer>().color;

            if (CheckIfSmallestX(transform, roundedX, roundedY))
            {
                if (grid[roundedX - 1, roundedY] != null && grid[roundedX - 1, roundedY].GetComponent<SpriteRenderer>().color == color)
                {
                    effectBarValue += 20;
                    effectBar.SetValue(effectBarValue);
                    if (effectBarValue >= 100)
                    {
                        effectBarValue = 0;
                        effectBar.SetValue(effectBarValue);
                        return true; // return true if 100% was reached
                    }
                } else
                {
                    effectBarValue -= 5;
                    effectBarValue = effectBarValue < 0 ? 0 : effectBarValue;
                    effectBar.SetValue(effectBarValue);
                }
            }
        }
        return false;
    }

    private static IEnumerator FillHoles()
    {
        for (int x = 0; x < width; x++)
        {
            bool ColIsEmpty = true;
            for (int y = 0; ColIsEmpty && y < height; y++)
            {
                if (grid[x,y])
                    ColIsEmpty = false;
            }

            if (ColIsEmpty) break;

            for (int y = 0; y < height; y++)
            {
                
                if (grid[x, y] == null)
                {
                    grid[x, y] = Instantiate(CellStatic, new Vector3(x, y, 0), Quaternion.identity);
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }
        CheckForLines();
    }

    private static IEnumerator CollapseHoles()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == null)
                {
                    // look for next block if there is any
                    int numberOfCells = 0; // how far away is the next block
                    int numberOfConsecutiveBlocks = 1; // how big is the chunk
                    bool found = false;
                    for (int xx = x; xx < width; xx++)
                    {
                        if (grid[xx, y] != null)
                        {
                            found = true;
                            if (xx + 1 < width && grid[xx + 1, y] != null)
                            {
                                numberOfConsecutiveBlocks++;
                            }
                        } else
                        {
                            if (!found)
                            {
                                numberOfCells++;
                            } else
                            {
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        for (int k = 0; k < numberOfConsecutiveBlocks; k++)
                        {
                            grid[x + numberOfCells + k, y].transform.position -= Vector3.right * numberOfCells;
                            grid[x + k, y] = grid[x + numberOfCells + k, y];
                            grid[x + numberOfCells + k, y] = null;
                        }
                        x = 0;
                        yield return new WaitForSeconds(0.2f);
                    }
                }
            }
        }
        CheckForLines();
    }

    private static bool CheckIfSmallestX(Transform transform, int x, int y)
    {
        int smallestX = int.MaxValue;

        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            if (roundedY == y)
            {
                smallestX = roundedX < smallestX ? roundedX : smallestX; 
            }
        }

        return smallestX == x;
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
