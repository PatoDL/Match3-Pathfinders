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
        CreateNewGrid();
    }

    public void CreateNewGrid()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                SetRandomGridValue(i, j);
            }
        }
    }

    public bool CheckForAvailableMoves()
    {
        for(int i = 0; i < sizeX; i++)
        {
            for(int j = 0; j < sizeY; j++)
            {
                int adjacentCount = 0;

                if(i > 0)
                {
                    bool hasLeftAdjacent = grid[i, j] == grid[i - 1, j];
                    if (hasLeftAdjacent)
                        adjacentCount++;
                }

                if(i < sizeX - 1)
                {
                    bool hasRightAdjacent = grid[i, j] == grid[i + 1, j];
                    if (hasRightAdjacent)
                        adjacentCount++;
                }

                if(j > 0)
                {
                    bool hasDownAdjacent = grid[i, j] == grid[i, j - 1];
                    if (hasDownAdjacent)
                        adjacentCount++;
                }

                if(j < sizeY - 1)
                {
                    bool hasUpAdjacent = grid[i, j] == grid[i, j + 1];
                    if (hasUpAdjacent)
                        adjacentCount++;
                }

                if (adjacentCount > 1)
                    return true;
            }
        }

        return false;
    }

     /// <summary>
     /// Returns true if there are combinations
     /// </summary>
    public bool CheckForCombinations(char axis, ref int scoreToAdd)
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
                        if (count >= minTokenAmountCombination)
                        {
                            for (int k = 1; k <= count; k++)
                            {
                                grid[i, j - k] = -1;
                                scoreToAdd++;
                            }
                        }
                        count = 1;
                    }

                    lastToken = grid[i, j];
                }
                if (count >= minTokenAmountCombination)
                {
                    for (int k = 1; k <= count; k++)
                    {
                        grid[i, j - k] = -1;
                        scoreToAdd++;
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
                        if (count >= minTokenAmountCombination)
                        {
                            for (int k = 1; k <= count; k++)
                            {
                                grid[i - k, j] = -1;
                                scoreToAdd++;
                            }
                        }
                        count = 1;
                    }

                    lastToken = grid[i, j];
                }
                if (count >= minTokenAmountCombination)
                {
                    for (int k = 1; k <= count; k++)
                    {
                        grid[i - k, j] = -1;
                        scoreToAdd++;
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
    public bool CheckThereAreNoEmptyTokens()
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

    public int GetGridValueByPosition(int x, int y)
    {
        return grid[x, y];
    }

    public void PullDownTokens()
    {
        for(int i=0;i<sizeX;i++)
        {
            for(int j=0;j<sizeY;j++)
            {
                if(grid[i,j] == -1)
                {
                    int thisToken = grid[i, j];
                    int k = j;
                    while (thisToken == -1)
                    {
                        k++;
                        if (k == sizeY)
                            break;
                        thisToken = grid[i, k];
                    }

                    if (k ==sizeY)
                    {
                        for (k = j; k < sizeY; k++)
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

    public bool CheckTokenSelectionGoingBackwards(int x, int y, ref Vector2 toRemove)
    {
        if (selectedTokens.Count <= 1)
            return false;

        Vector2 positionAux = selectedTokens[selectedTokens.Count - 2];

        if (positionAux.x == x && positionAux.y == y)
        {
            toRemove = selectedTokens[selectedTokens.Count - 1];
            selectedTokens.RemoveAt(selectedTokens.Count - 1);
            return true;
        }
        return false;
    }

    /// <summary>
    /// returns true if token wasn't selected yet
    /// </summary>
    public bool CheckTokenAlreadySelected(int x, int y)
    {
        if (selectedTokens.Count <= 0)
            return true;

        

        foreach(Vector2 tokenPosition in selectedTokens)
        {
            if (tokenPosition.x == x && tokenPosition.y == y)
                return false;
        }

        return true;
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
    public bool SelectToken(int x, int y)
    {
        if (CheckValidTokenPosition(x, y) && CheckValidTokenValue(x, y) && CheckTokenAlreadySelected(x,y))
        {
            selectedTokens.Add(new Vector2(x, y));
            return true;
        }

        return false;
    }


    /// <summary>
    /// Returns the score that should be added to the actual game score. 
    /// Exploded returns true if the combination chain was up to 2 tokens
    public int ExplodeChain(ref bool exploded)
    {
        int scoreToAdd = 0;
        if(selectedTokens.Count >= minTokenAmountCombination)
        {
            foreach (Vector2 tokenPosition in selectedTokens)
            {
                grid[(int)tokenPosition.x, (int)tokenPosition.y] = -1;
                scoreToAdd++;
            }
            exploded = true;
            selectedTokens.Clear();
        }

        return scoreToAdd;
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
