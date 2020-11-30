using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

    struct CoroutineCallbacks
    {
        public bool allTokensHaveSpawned;
        public bool tokensExploded;
        public bool tokensHaveFall;
    }

    private CoroutineCallbacks coroutineCallbacks;

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

        coroutineCallbacks.allTokensHaveSpawned = false;
        coroutineCallbacks.tokensExploded = false;
        coroutineCallbacks.tokensHaveFall = false;

        int maxX = 0, maxY = 0;
        int[,] grid = model.GetGrid(ref maxX, ref maxY);
        view.CreateGrid(grid, maxX, maxY, tokenOffset);
        UnityAction AllTokensAnimatedAction = () => { coroutineCallbacks.allTokensHaveSpawned = true; };
        StartCoroutine(view.SpawnAnimated(grid, maxX, maxY, AllTokensAnimatedAction));
        Camera.main.orthographicSize *= Mathf.Max(maxX, maxY);
        Camera.main.orthographicSize /= 10;
        Vector3 cameraPosition = Camera.main.gameObject.transform.position;
        Camera.main.gameObject.transform.position = new Vector3((maxX-1) * tokenOffset.x / 2, (maxY - 1) * tokenOffset.y / 2, cameraPosition.z);
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
        if (!coroutineCallbacks.allTokensHaveSpawned)
            return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

        RaycastHit2D raycastHit2D = Physics2D.Raycast(mousePosition2D, Vector2.zero);

        if(actualSelectedToken != null && raycastHit2D.collider == null)
        {
            view.DeselectToken(actualSelectedToken);
            actualSelectedToken = null;
        }

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
                        else
                        {
                            Vector2 toRemove = Vector2.zero;
                            bool goingBackwards = model.CheckTokenSelectionGoingBackwards(xPos, yPos, ref toRemove);

                            if (goingBackwards)
                            {
                                view.DeselectToken((int)toRemove.x, (int)toRemove.y);
                                view.RemoveLinePosition();
                            }
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


                        bool stillGotMoves = model.CheckForAvailableMoves();
                        if (turnsLeft <= 0 || !stillGotMoves)
                        {
                            RestartGame();

                            while (!stillGotMoves)
                            {
                                RestartGame();
                                stillGotMoves = model.CheckForAvailableMoves();
                            }
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

        bool stillGotMoves = model.CheckForAvailableMoves();
        while(!stillGotMoves)
        {
            RestartGame();
            stillGotMoves = model.CheckForAvailableMoves();
        }
    }
}
