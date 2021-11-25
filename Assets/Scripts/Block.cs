using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public Vector3 rotationPoint;
    public int xIdxAt;
    public int yIdxAt;
    public int numberOfColors;

    private float previousTime;
    private GameObject ghost = null;

    private float holdTime = 0;
    private bool spritesRotated = false;

    // Start is called before the first frame update
    void Start()
    {
        // set color

        // get max number of tiles in a line
        //Color primaryColor = GameMaster.TetrisColors[Random.Range(0, GameMaster.TetrisColors.Length)];
        //bool horizontal = Random.Range(0, 2) == 0;

        //List<int> usedIdc = new();
        //int idx = Random.Range(0, GameMaster.TetrisColors.Length);
        //usedIdc.Add(idx);

        //foreach (Transform children in transform)
        //{
        //    if (xIdxAt == -1 && yIdxAt == -1) // Exception for I_Block
        //    {
        //        if (horizontal)
        //        {
        //            while (usedIdc.Contains(idx))
        //            {
        //                idx = Random.Range(0, GameMaster.TetrisColors.Length);
        //            }
        //            usedIdc.Add(idx);
        //            children.GetComponent<SpriteRenderer>().color = GameMaster.TetrisColors[idx];
        //        }
        //        else
        //        {
        //            children.GetComponent<SpriteRenderer>().color = primaryColor;
        //        }
        //    }
        //    else
        //    {
        //        if (horizontal)
        //        {
        //            if (xIdxAt >= 0 && Mathf.RoundToInt(children.localPosition.x) == xIdxAt)
        //            {
        //                children.GetComponent<SpriteRenderer>().color = primaryColor;
        //            }
        //            else
        //            {
        //                if (usedIdc.Count < numberOfColors)
        //                {
        //                    while (usedIdc.Contains(idx))
        //                    {
        //                        idx = Random.Range(0, GameMaster.TetrisColors.Length);
        //                    }
        //                    usedIdc.Add(idx);
        //                }
        //                children.GetComponent<SpriteRenderer>().color = GameMaster.TetrisColors[idx];
        //            }
        //        }
        //        else
        //        {
        //            if (yIdxAt >= 0 && Mathf.RoundToInt(children.localPosition.y) == yIdxAt)
        //            {
        //                children.GetComponent<SpriteRenderer>().color = primaryColor;
        //            }
        //            else
        //            {
        //                if (usedIdc.Count < numberOfColors)
        //                {
        //                    while (usedIdc.Contains(idx))
        //                    {
        //                        idx = Random.Range(0, GameMaster.TetrisColors.Length);
        //                    }
        //                    usedIdc.Add(idx);
        //                }
        //                children.GetComponent<SpriteRenderer>().color = GameMaster.TetrisColors[idx];
        //            }
        //        }
        //    }
        //}

        ghost = Instantiate(gameObject);
        ghost.GetComponent<Block>().enabled = false;
        SetGhostPosition();
        SetGhostOpacity(0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        // move up or down
        if (!(Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow)))
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                if (holdTime == 0)
                {
                    transform.position += new Vector3(0, 1, 0);
                    if (!GameMaster.ValidMove(transform))
                        transform.position -= new Vector3(0, 1, 0);
                    SetGhostPosition();
                }

                holdTime += Time.deltaTime;
                if (holdTime > GameMaster.holdDuration)
                {
                    transform.position += new Vector3(0, 1, 0);
                    if (!GameMaster.ValidMove(transform))
                        transform.position -= new Vector3(0, 1, 0);
                    SetGhostPosition();
                    holdTime -= GameMaster.holdUpdateSpeed;
                }
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                if (holdTime == 0)
                {
                    transform.position -= new Vector3(0, 1, 0);
                    if (!GameMaster.ValidMove(transform))
                        transform.position += new Vector3(0, 1, 0);
                    SetGhostPosition();
                }

                holdTime += Time.deltaTime;
                if (holdTime > GameMaster.holdDuration)
                {
                    transform.position -= new Vector3(0, 1, 0);
                    if (!GameMaster.ValidMove(transform))
                        transform.position += new Vector3(0, 1, 0);
                    SetGhostPosition();
                    holdTime -= GameMaster.holdUpdateSpeed;
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            holdTime = 0;
        }

        // rotate
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);

            if (!GameMaster.ValidMove(transform))
            {
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
                CheckRotationAtEdges(1);
            }
            else
            {
                RotateSpriteOrientation();
            }

            ghost.transform.rotation = transform.rotation;
            SetGhostPosition();
        }

        // hold block
        if (Input.GetKeyDown(KeyCode.C) && !GameMaster.alreadySwitched)
        {
            if (GameMaster.heldBlock == null)
            {
                if (spritesRotated) RotateSpriteOrientation();

                GameMaster.heldBlock = Instantiate(gameObject, GameMaster.heldBlockPos, Quaternion.identity);
                GameMaster.heldBlock.GetComponent<Block>().enabled = false;
                FindObjectOfType<Spawner>().newBlock();
            }
            else
            {
                FindObjectOfType<Spawner>().newBlock(GameMaster.heldBlock.name.Replace("(Clone)", ""));
                Destroy(GameMaster.heldBlock);

                if (spritesRotated) RotateSpriteOrientation();
                GameMaster.heldBlock = Instantiate(gameObject, GameMaster.heldBlockPos, Quaternion.identity);
                GameMaster.heldBlock.GetComponent<Block>().enabled = false;
            }

            GameMaster.alreadySwitched = true;
            Destroy(gameObject);
            Destroy(ghost);
        }

        // hard drop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameMaster.AddPointsDrop((int)(transform.position.x - ghost.transform.position.x) * 2);
            GoToLowestPoint();
            previousTime -= (1 / GameMaster.updatesPerSecond); // execute next game loop immediately
        }
        // fall & soft drop
        if (Time.time - previousTime > (Input.GetKey(KeyCode.LeftArrow) ? (1.0f / (GameMaster.updatesPerSecond * GameMaster.boostFallMultiplier)) : (1.0f / GameMaster.updatesPerSecond)))
        {
            transform.position -= new Vector3(1, 0, 0);
            if (!GameMaster.ValidMove(transform))
            {
                transform.position += new Vector3(1, 0, 0);
                this.enabled = false;
                if (GameMaster.AddToGrid(transform))
                {
                    GameMaster.CheckForLines();
                    Destroy(ghost);
                    //FindObjectOfType<Spawner>().newBlock();
                    //GameMaster.alreadySwitched = false;
                } // else GAME OVER 
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    GameMaster.AddPointsDrop(1);
                }
            }
            previousTime = Time.time;
        }
    }

    void CheckRotationAtEdges(int n) // n = number to move away from edge
    {
        transform.position += new Vector3(0, n, 0);
        transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
        if (!GameMaster.ValidMove(transform))
        {
            transform.position -= new Vector3(0, n, 0);
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);
            transform.position -= new Vector3(0, n, 0);
            transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), 90);
            if (!GameMaster.ValidMove(transform))
            {
                transform.position += new Vector3(0, n, 0);
                transform.RotateAround(transform.TransformPoint(rotationPoint), new Vector3(0, 0, 1), -90);

                if (n == 1)
                    CheckRotationAtEdges(2);
            } else
            {
                RotateSpriteOrientation();
            }
        } else
        {
            RotateSpriteOrientation();
        }
    }


    void RotateSpriteOrientation()
    {
        spritesRotated = !spritesRotated;
        foreach (Transform children in transform)
        {
            var scale = children.transform.localScale;
            children.transform.localScale = new Vector3(scale.y, scale.x, scale.z);
        }

        foreach (Transform children in ghost.transform)
        {
            var scale = children.transform.localScale;
            children.transform.localScale = new Vector3(scale.y, scale.x, scale.z);
        }
    }



    public void SetGhostPosition()
    {
        ghost.transform.position = transform.position;
        while (true)
        {
            ghost.transform.position -= new Vector3(1, 0, 0);
            if (!GameMaster.ValidMove(ghost.transform))
            {
                ghost.transform.position += new Vector3(1, 0, 0);
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
            transform.position -= new Vector3(1, 0, 0);
            if (!GameMaster.ValidMove(transform))
            {
                transform.position += new Vector3(1, 0, 0);
                break;
            }
        }
    }



    
}
