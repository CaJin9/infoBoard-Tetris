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
