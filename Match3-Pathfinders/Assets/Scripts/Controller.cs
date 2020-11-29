using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] View view;
    [SerializeField] Model model;

    [SerializeField] private Vector2 tokenOffset;

    private GameObject actualSelectedToken;

    enum Game_Phase
    {
        WONDERING,
        CHAINING
    }

    Game_Phase game_phase;

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
        while (!model.CheckForCombinations('x') || !model.CheckForCombinations('y'))
        {
            model.PullDownTokens();
        }
        int maxX = 0, maxY = 0;
        int[,] grid = model.GetGrid(ref maxX, ref maxY);
        RenderGrid(grid, maxX, maxY);
        Vector3 cameraPosition = Camera.main.gameObject.transform.position;
        Camera.main.gameObject.transform.position = new Vector3(maxX * tokenOffset.x / 2, maxY * tokenOffset.y / 2, cameraPosition.z);
        game_phase = Game_Phase.WONDERING;
    }

    void ChangeMouseToken(GameObject newToken)
    {
        if(actualSelectedToken == null)
        {
            actualSelectedToken = newToken;
        }
    }

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

        RaycastHit2D raycastHit2D = Physics2D.Raycast(mousePosition2D, Vector2.zero);

        if (raycastHit2D.collider != null && raycastHit2D.collider.tag == "Token")
        {
            switch(game_phase)
            {
            case Game_Phase.WONDERING:
                {
                    if (actualSelectedToken != null && actualSelectedToken != raycastHit2D.collider.gameObject)
                    {
                        view.DeselectToken(actualSelectedToken);
                    }
                    actualSelectedToken = raycastHit2D.collider.gameObject;
                    view.SelectToken(actualSelectedToken);
                    if (Input.GetMouseButton(0))
                    {
                        game_phase = Game_Phase.CHAINING;
                    }
                }
                break;
            case Game_Phase.CHAINING:
                {
                     actualSelectedToken = raycastHit2D.collider.gameObject;
                     int xPos = 0, yPos = 0;
                     model.GetGameObjectGridPosition(actualSelectedToken, ref xPos, ref yPos, tokenOffset);
                     bool wasTokenSelected = model.SelectToken(xPos, yPos);
                     if (wasTokenSelected)
                     {
                         view.SelectToken(actualSelectedToken);
                         Vector3 position = raycastHit2D.transform.gameObject.transform.position;
                         position.z = -1;
                         view.AddLinePosition(position);
                     }
                }
                break;
            }
        }
    }
}
