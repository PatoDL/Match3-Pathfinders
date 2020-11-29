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
        CHAINING,
        EXPLODING
    }

    Game_Phase game_phase;

    

    void Start()
    {
        while (!model.CheckForCombinations('x') || !model.CheckForCombinations('y'))
        {
            model.PullDownTokens();
        }
        int maxX = 0, maxY = 0;
        int[,] grid = model.GetGrid(ref maxX, ref maxY);
        view.CreateGrid(grid, maxX, maxY, tokenOffset);
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
            switch (game_phase)
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

                        //if(Input.GetKeyDown(KeyCode.Z))
                        //{
                        //    model.PullDownTokens();

                        //    int maxX = 0, maxY = 0;
                        //    int[,] grid = model.GetGrid(ref maxX, ref maxY);

                        //    view.SwitchGrid(grid, maxX, maxY);
                        //}
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

                        if (Input.GetMouseButtonUp(0))
                        {
                            foreach(Vector2 tokenPosition in model.selectedTokens)
                            {
                                view.DeselectToken((int)tokenPosition.x, (int)tokenPosition.y);
                            }
                            model.ExplodeChain();
                            view.ResetLineRenderer();
                            game_phase = Game_Phase.EXPLODING;
                        }
                    }
                    break;
                case Game_Phase.EXPLODING:
                    {
                        while (!model.CheckThereAreNoEmptyTokens())
                        {
                            model.PullDownTokens();
                        }

                        while (!model.CheckForCombinations('x') || !model.CheckForCombinations('y'))
                        {
                            model.PullDownTokens();
                        }

                        int maxX = 0, maxY = 0;
                        int[,] grid = model.GetGrid(ref maxX, ref maxY);

                        view.SwitchGrid(grid, maxX, maxY);

                        game_phase = Game_Phase.WONDERING;
                    }
                    break;
            }
        }
    }
}
