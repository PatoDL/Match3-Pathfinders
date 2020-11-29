using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] View view;
    [SerializeField] Model model;

    [SerializeField] private Vector2 tokenOffset;

    [SerializeField] private int score;

    [SerializeField] private int turnsLeft;
    private int turnsLeftHandler;

    public delegate void OnIntValueUpdate(int newInt);

    public OnIntValueUpdate UpdateScore;
    public OnIntValueUpdate UpdateTurnsLeft;

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
        turnsLeftHandler = turnsLeft;

        RestartGame();

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

                            bool exploded = false;
                            score += model.ExplodeChain(ref exploded);
                            UpdateScore(score);

                            if(exploded)
                            {   
                                game_phase = Game_Phase.EXPLODING;
                                turnsLeft--;
                                UpdateTurnsLeft(turnsLeft);
                            }
                            else
                            {
                                foreach (Vector2 tokenPosition in model.selectedTokens)
                                {
                                    view.MarkError((int)tokenPosition.x, (int)tokenPosition.y);
                                }

                                model.selectedTokens.Clear();
                                game_phase = Game_Phase.WONDERING;
                            }
                            view.ResetLineRenderer();
                        }
                    }
                    break;
                case Game_Phase.EXPLODING:
                    {
                        while (!model.CheckThereAreNoEmptyTokens())
                        {
                            model.PullDownTokens();
                        }

                        int scoreToAdd = 0;
                        while (!model.CheckForCombinations('x', ref scoreToAdd) || !model.CheckForCombinations('y', ref scoreToAdd))
                        {
                            model.PullDownTokens();
                        }
                        score += scoreToAdd;
                        UpdateScore(score);

                        if (turnsLeft <= 0)
                            RestartGame();

                        int maxX = 0, maxY = 0;
                        int[,] grid = model.GetGrid(ref maxX, ref maxY);

                        view.SwitchGrid(grid, maxX, maxY);


                        game_phase = Game_Phase.WONDERING;
                    }
                    break;
            }
        }
    }

    private void RestartGame()
    {
        model.CreateNewGrid();
        score = 0;
        UpdateScore(score);
        turnsLeft = turnsLeftHandler;
        UpdateTurnsLeft(turnsLeft);

        int neededIntReference = 0;
        while (!model.CheckForCombinations('x', ref neededIntReference) || !model.CheckForCombinations('y', ref neededIntReference))
        {
            model.PullDownTokens();
        }
    }
}
