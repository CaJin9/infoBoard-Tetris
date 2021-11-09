using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] Blocks;
    public void newBlock()
    {
        Instantiate(Blocks[Random.Range(0, Blocks.Length)], transform.position, Quaternion.identity);
    }

    public void newBlock(string name)
    {
        for (int i = 0; i < Blocks.Length; i++)
        {
            if (Blocks[i].name == name)
            {
                Instantiate(Blocks[i], transform.position, Quaternion.identity);
                break;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        newBlock();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
