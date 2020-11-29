using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Model : MonoBehaviour
{
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;

    int[,] grid;

    [SerializeField] private int AmountOfTokens;

    [HideInInspector] public List<Vector2> selectedTokens;
    [HideInInspector] public GameObject onMouseToken;

    [SerializeField] private int minTokenAmountCombination;

    // Start is called before the first frame update
    void Start()
    {
        selectedTokens = new List<Vector2>();
        grid = new int[sizeX, sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                SetRandomGridValue(i,j);
            }
        }
    }

     /// <summary>
     /// Returns true if there are combinations
     /// </summary>
     /// <param name="axis"></param>
     /// <returns></returns>
    public bool CheckForCombinations(char axis)
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

        return CheckThereAreNoEmptyTokens();
    }

    /// <summary>
    /// Returns true if there are no empty tokens
    /// </summary>
    /// <returns></returns>
    bool CheckThereAreNoEmptyTokens()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                if (grid[i, j] == -1)
                    return false;
            }
        }

        return true;
    }

    public void PullDownTokens()
    {
        for (int i = sizeX - 1; i >= 0; i--)
        {
            for (int j = sizeY - 1; j >= 0; j--)
            {
                if (grid[i, j] == -1)
                {
                    int thisToken = grid[i, j];
                    int k = j;
                    while (thisToken == -1)
                    {
                        k--;
                        if (k < 0)
                            break;
                        thisToken = grid[i, k];
                    }

                    if (k < 0)
                    {
                        for (k = j; k >= 0; k--)
                        {
                            SetRandomGridValue(i, k);
                        }

                        continue;
                    }

                    grid[i, j] = grid[i, k];
                    grid[i, k] = -1;
                }
            }
        }
    }

    public void GetGameObjectGridPosition(GameObject tokenGameObject, ref int x, ref int y, Vector2 offset)
    {
        Vector2 position = tokenGameObject.transform.position;
        x = (int)(position.x / offset.x);
        y = (int)(position.y / offset.y);
    }

    bool CheckValidTokenPosition(int x, int y)
    {
        if (selectedTokens.Count <= 0)
            return true;

        Vector2 lastTokenPosition = selectedTokens[selectedTokens.Count - 1];

        bool xAdjacent = (y == lastTokenPosition.y && (x == lastTokenPosition.x + 1 || x == lastTokenPosition.x - 1));
        bool yAdjacent = (x == lastTokenPosition.x && (y == lastTokenPosition.y + 1 || y == lastTokenPosition.y - 1));

        if ((xAdjacent && !yAdjacent) || (yAdjacent && !xAdjacent))
            return true;

        return false;
    }

    bool CheckValidTokenValue(int x, int y)
    {
        if (selectedTokens.Count <= 0)
            return true;

        Vector2 lastTokenPosition = selectedTokens[selectedTokens.Count - 1];

        if(grid[x, y] == grid[(int)lastTokenPosition.x, (int)lastTokenPosition.y])
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if the token could be selected
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool SelectToken(int x, int y)
    {
        if (CheckValidTokenPosition(x, y) && CheckValidTokenValue(x, y))
        {
            selectedTokens.Add(new Vector2(x, y));
            return true;
        }

        return false;
    }

    void SetRandomGridValue(int x, int y)
    {
        grid[x, y] = Random.Range(0, AmountOfTokens);
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
