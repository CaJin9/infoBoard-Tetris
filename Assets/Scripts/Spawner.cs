using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private List<GameObject> preview = new();
    public int NumberOfPreviews = 1;
    public Transform PreviewLocation;
    public float Distance = 1;
    int prevR = -1;

    public GameObject[] Blocks;
    public void newBlock()
    {
        preview[0].GetComponent<Block>().enabled = true;
        preview[0].transform.position = transform.position;
        preview.Remove(preview[0]);
        for (int i = 0; i < preview.Count; i++)
        {
            preview[i].transform.position = PreviewLocation.position + i * Distance * Vector3.right; // aufruecken
        }
        SpawnPreview();
    }

    void SpawnPreview()
    {
        int r = prevR;

        for (int i = preview.Count; i < NumberOfPreviews; i++)
        {
            while (r == prevR)
            {
                r = Random.Range(0, Blocks.Length); // never get same block twice in a row
            }
            prevR = r;
            preview.Add(Instantiate(Blocks[r], PreviewLocation.position + i * Distance * Vector3.right, Quaternion.identity));
            preview[preview.Count - 1].GetComponent<Block>().enabled = false;
        }
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
        SpawnPreview();
        newBlock();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
