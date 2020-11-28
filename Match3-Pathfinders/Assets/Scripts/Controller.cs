using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] View view;
    [SerializeField] Model model;


    void RenderGrid(int[,] grid, int xMax, int yMax)
    {
        for(int i=0; i<xMax; i++)
        {
            for (int j = 0; j < yMax; j++)
            {
                view.RenderGrid(grid[i, j], new Vector3(i,j));
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("hola");
            int maxX = 0, maxY = 0;
            int[,] grid = model.GetGrid( ref maxX, ref maxY);
            RenderGrid(grid,maxX,maxY);
        }
    }
}
