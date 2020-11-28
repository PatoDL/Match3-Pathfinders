using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour
{
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;

    int[,] grid;

    [SerializeField] private int AmountOfTokens;

    // Start is called before the first frame update
    void Start()
    {
        grid = new int[sizeX, sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                SetGridValue(i,j, true);
            }
        }
    }

    void SetGridValue(int x, int y, bool random = false)
    {
        if (random)
        {
            grid[x, y] = Random.Range(0, AmountOfTokens);
        }
        else
        {
            grid[x, y] = grid[x, y + 1];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int[,] GetGrid(ref int maxX, ref int maxY)
    {
        maxX = sizeX;
        maxY = sizeY;

        return grid;
    }
}
