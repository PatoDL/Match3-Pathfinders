using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] View view;
    [SerializeField] Model model;

    [SerializeField] private Vector2 tokenOffset;

    void RenderGrid(int[,] grid, int xMax, int yMax)
    {
        for(int i=0; i<xMax; i++)
        {
            for (int j = 0; j < yMax; j++)
            {
                view.RenderGrid(grid[i, j], new Vector3(i * tokenOffset.x, j * tokenOffset.y));
            }
        }
    }

    void Start()
    {
        int maxX = 0, maxY = 0;
        while (!model.CheckForCombinations('x') || !model.CheckForCombinations('y'))
        {
            model.PullDownTokens();
        }
        
        int[,] grid = model.GetGrid(ref maxX, ref maxY);
        RenderGrid(grid, maxX, maxY);
        Vector3 cameraPosition = Camera.main.gameObject.transform.position;
        Camera.main.gameObject.transform.position = new Vector3(maxX * tokenOffset.x / 2, maxY * tokenOffset.y / 2, cameraPosition.z);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

            RaycastHit2D raycastHit2D = Physics2D.Raycast(mousePosition2D, Vector2.zero);

            if (raycastHit2D.collider != null)
            {
                Debug.Log(raycastHit2D.transform.position);
            }
        }
    }
}
