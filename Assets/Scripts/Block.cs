using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector3 rotationPoint;

    private float previousTime;
    private GameObject ghost = null;

    // Start is called before the first frame update
    void Start()
    {
        ghost = Instantiate(gameObject);
        ghost.GetComponent<Block>().enabled = false;
        SetGhostPosition();
        SetGhostOpacity(0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.position += new Vector3(0, 1, 0);
            if (!GameMaster.ValidMove(transform))
                transform.position -= new Vector3(0, 1, 0);
            SetGhostPosition();
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.position += new Vector3(0, -1, 0);
            if (!GameMaster.ValidMove(transform))
                transform.position -= new Vector3(0, -1, 0);
            SetGhostPosition();
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            if (!GameMaster.ValidMove(transform))
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);

            ghost.transform.rotation = transform.rotation;
            SetGhostPosition();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GoToLowestPoint();
            previousTime -= (1/GameMaster.updatesPerSecond); // execute next game loop immediately
        }

        // fall
        if (Time.time - previousTime > (Input.GetKey(KeyCode.LeftArrow) ? (1.0f / (GameMaster.updatesPerSecond * GameMaster.boostFallMultiplier)) : (1.0f / GameMaster.updatesPerSecond)))
        {
            transform.position += new Vector3(-1, 0, 0);
            if (!GameMaster.ValidMove(transform))
            {
                transform.position -= new Vector3(-1, 0, 0);
                this.enabled = false;
                if (GameMaster.AddToGrid(transform))
                {
                    GameMaster.CheckForLines();
                    Destroy(ghost);
                    FindObjectOfType<Spawner>().newBlock();
                } else
                {
                    GameMaster.GameOver();
                }
            }
            previousTime = Time.time;
        }
    }

    void SetGhostPosition()
    {
        ghost.transform.position = transform.position;
        while (true)
        {
            ghost.transform.position += new Vector3(-1, 0, 0);
            if (!GameMaster.ValidMove(ghost.transform))
            {
                ghost.transform.position -= new Vector3(-1, 0, 0);
                break;
            }
        }
        ghost.transform.position = new Vector3(ghost.transform.position.x, Mathf.RoundToInt(transform.position.y), 0);
    }

    void SetGhostOpacity(float opacity)
    {
        foreach (Transform children in ghost.transform)
        {
            var sprite = children.GetComponent<SpriteRenderer>();
            var color = sprite.color;
            color.a = opacity;
            sprite.color = color;
        }
    }

    void GoToLowestPoint()
    {
        while (true)
        {
            transform.position += new Vector3(-1, 0, 0);
            if (!GameMaster.ValidMove(transform))
            {
                transform.position -= new Vector3(-1, 0, 0);
                break;
            }
        }
    }



    
}
