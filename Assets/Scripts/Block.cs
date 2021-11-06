using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector3 rotationPoint;
    private float previousTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.position += new Vector3(0, 1, 0);
            if (!ValidMove())
                transform.position -= new Vector3(0, 1, 0);
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.position += new Vector3(0, -1, 0);
            if (!ValidMove())
                transform.position -= new Vector3(0, -1, 0);
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            if (!ValidMove())
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
        }

        // fall
        if (Time.time - previousTime > (Input.GetKey(KeyCode.LeftArrow) ? (1.0f / (GameMaster.updatesPerSecond * GameMaster.boostFallMultiplier)) : (1.0f / GameMaster.updatesPerSecond)))
        {
            transform.position += new Vector3(-1, 0, 0);
            if (!ValidMove())
            {
                transform.position -= new Vector3(-1, 0, 0);
                this.enabled = false;
                AddToGrid();
                CheckForLines();
                FindObjectOfType<Spawner>().newBlock();
            }
            previousTime = Time.time;
        }
    }

    void CheckForLines()
    {
        for (int i = GameMaster.width-1; i >= 0; i--)
        {
            if (HasLine(i))
            {
                DeleteLine(i);
                Collapse(i);
            }
        }
    }

    bool HasLine(int i)
    {
        for (int j = 0; j < GameMaster.height; j++)
        {
            if (GameMaster.grid[i, j] == null)
                return false;
        }
        return true;
    }

    void DeleteLine(int i)
    {
        for (int j = 0; j < GameMaster.height; j++)
        {
            Destroy(GameMaster.grid[i, j].gameObject);
            GameMaster.grid[i, j] = null;
        }
    }

    void Collapse(int i)
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

    void AddToGrid()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            GameMaster.grid[roundedX, roundedY] = children;
        }
    }
    bool ValidMove()
    {
        foreach (Transform children in transform)
        {
            int roundedX = Mathf.RoundToInt(children.transform.position.x);
            int roundedY = Mathf.RoundToInt(children.transform.position.y);

            // stay within board
            if (roundedX < 0 || roundedX >= GameMaster.width || roundedY < 0 ||roundedY >= GameMaster.height)
                return false;

            // collide with other blocks
            if (GameMaster.grid[roundedX, roundedY] != null)
                return false;
        }
        return true;
    }
}
