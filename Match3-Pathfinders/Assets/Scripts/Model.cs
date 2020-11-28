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

    public void CheckForCombinations(char axis)
    {
        int lastToken = -1;
        int count = 1;

        if (axis == 'y')
        {
            for (int i = 0; i < sizeX; i++)
            {
                int j = 0;
                for (; j < sizeY; j++)
                {
                    if (grid[i, j] == -1)
                        continue;

                    if (grid[i, j] == lastToken)
                    {
                        count++;
                    }
                    else
                    {
                        if (count >= 3)
                        {
                            for (int k = 1; k <= count; k++)
                            {
                                grid[i, j - k] = -1;
                            }
                        }
                        count = 1;
                    }

                    lastToken = grid[i, j];
                }
                if (count >= 3)
                {
                    for (int k = 1; k <= count; k++)
                    {
                        grid[i, j - k] = -1;
                    }
                }
                count = 1;
                lastToken = -1;
            }
        }
        else if (axis == 'x')
        {
            for (int j = 0; j < sizeY; j++)
            {
                int i = 0;
                for (; i < sizeX; i++)
                {
                    if (grid[i, j] == -1)
                        continue;

                    if (grid[i, j] == lastToken)
                    {
                        count++;
                    }
                    else
                    { 
                        if (count >= 3)
                        {
                            for (int k = 1; k <= count; k++)
                            {
                                grid[i - k, j] = -1;
                            }
                        }
                        count = 1;
                    }

                    lastToken = grid[i, j];
                }
                if (count >= 3)
                {
                    for (int k = 1; k <= count; k++)
                    {
                        grid[i - k, j] = -1;
                    }
                }
                count = 1;
                lastToken = -1;
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
