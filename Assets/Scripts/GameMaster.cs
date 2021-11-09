using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    // settings
    public static float updatesPerSecond = 1.0f;
    public static float boostFallMultiplier = 10.0f;

    // board
    public static int height = 10;
    public static int width = 24;
    public static Transform[,] grid = new Transform[width, height];

    // game state
    public static int clearedLines = 0;
    private const int numberOfLinesForLvlUp = 10;
    private const float LvlUpIncrement = 0.0f;
    
    // init grid
    private int numberOfColumns = 3;
    private int maximumNumberOfMudas = 3;

    // held block
    public static GameObject heldBlock;
    public static Vector3 heldBlockPos = new Vector3(2, height + 1, 0);

    public GameObject Cell;

    void Start()
    {
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

    public static void ClearedLine()
    {
        clearedLines++;
        if (clearedLines % numberOfLinesForLvlUp == 0)
        {
            updatesPerSecond += LvlUpIncrement;
        }
    }

    public static void CheckForLines()
    {
        for (int i = GameMaster.width - 1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                Collapse(i);
                GameMaster.ClearedLine();
            }
        }
    }

    private static bool HasLine(int i)
    {
        for (int j = 0; j < GameMaster.height; j++)
        {
            if (GameMaster.grid[i, j] == null)
                return false;
        }
        return true;
    }

    private static void DeleteLine(int i)
    {
        for (int j = 0; j < GameMaster.height; j++)
        {
            Destroy(GameMaster.grid[i, j].gameObject);
            GameMaster.grid[i, j] = null;
        }
    }

    private static void Collapse(int i)
    {
        for (int y = i; y < GameMaster.width; y++)
        {
            for (int j = 0; j < GameMaster.height; j++)
            {
                if (GameMaster.grid[y, j] != null)
                {
                    GameMaster.grid[y - 1, j] = GameMaster.grid[y, j];
                    GameMaster.grid[y, j] = null;
                    GameMaster.grid[y - 1, j].transform.position -= new Vector3(1, 0, 0);
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

            if (roundedX < GameMaster.width && roundedY < GameMaster.height)
            {
                GameMaster.grid[roundedX, roundedY] = children;
            }
            else
            {
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
            if (roundedX < 0 || roundedX >= GameMaster.width || roundedY < 0 || roundedY >= GameMaster.height)
                return false;

            // collide with other blocks
            if (GameMaster.grid[roundedX, roundedY] != null)
                return false;
        }
        return true;
    }

    public static void GameOver()
    {

    }
}
