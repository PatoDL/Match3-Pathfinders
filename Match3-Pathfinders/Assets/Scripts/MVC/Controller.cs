using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Controller : MonoBehaviour
{
    [SerializeField] View view = null;
    [SerializeField] Model model = null;

    [SerializeField] private Vector2 tokenOffset = Vector2.zero;

    [SerializeField] private int score = 0;

    [SerializeField] private int turnsLeft = 0;
    private int turnsLeftHandler = 0;

    private AudioSource audioSource = null;

    public delegate void OnIntValueUpdate(int newInt);

    public OnIntValueUpdate UpdateScore = null;
    public OnIntValueUpdate UpdateTurnsLeft = null;

    private GameObject actualSelectedToken = null;

    struct CoroutineCallbacks
    {
        public bool allTokensHaveSpawned;
        public bool tokensExploded;
        public bool tokensHaveFall;
        public bool combinationsRemaining;
    }

    private CoroutineCallbacks coroutineCallbacks;

    enum Game_Phase
    {
        WONDERING,
        CHAINING,
        EXPLODING
    }

    Game_Phase game_phase = Game_Phase.WONDERING;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        turnsLeftHandler = turnsLeft;

        RestartGame();

        coroutineCallbacks.allTokensHaveSpawned = false;
        coroutineCallbacks.tokensExploded = false;
        coroutineCallbacks.tokensHaveFall = false;
        coroutineCallbacks.combinationsRemaining = false;

        int maxX = 0, maxY = 0;
        int[,] grid = model.GetGrid(ref maxX, ref maxY);
        view.CreateGrid(grid, maxX, maxY, tokenOffset);
        UnityAction AllTokensAnimatedAction = () => { coroutineCallbacks.allTokensHaveSpawned = true; };
        StartCoroutine(view.SpawnAnimated(grid, maxX, maxY, AllTokensAnimatedAction));

        float tokensRes = Screen.currentResolution.width * Mathf.Max(maxX,maxY);

        Debug.Log(tokensRes);

        Camera.main.orthographicSize = 9/(115200f / tokensRes)*9;

        Vector3 cameraPosition = Camera.main.gameObject.transform.position;
        Camera.main.gameObject.transform.position = new Vector3((maxX-1) * tokenOffset.x / 2, (maxY - 1) * tokenOffset.y / 2, cameraPosition.z);
        game_phase = Game_Phase.WONDERING;
    }

    void PassDespawningTokensToView(UnityAction nextAction)
    {
        int maxX = 0, maxY = 0;
        int[,] grid = model.GetGrid(ref maxX, ref maxY);
        StartCoroutine(view.DespawnTokens(grid, maxX, maxY, nextAction));
    }

    void PassFallingTokensToView(List<Vector3> tokensToPass, UnityAction nextAction)
    {
        int maxX = 0, maxY = 0;
        int[,] grid = model.GetGrid(ref maxX, ref maxY);

        StartCoroutine(view.PullDownAnimatedTokens(grid, tokensToPass, tokenOffset, maxX, maxY, nextAction));
    }

    void CheckForNewCombinationsAndAddScore()
    {
        int scoreToAdd = 0;
        coroutineCallbacks.tokensHaveFall = true;

        //when tokens have fell, the update can advance to next step, checking for new combinations
        bool combinationsRemaining = (!model.CheckAndExplodeCombinations('x', ref scoreToAdd) ||
                                                    !model.CheckAndExplodeCombinations('y', ref scoreToAdd));

        score += scoreToAdd;
        UpdateScore(score);


        //------------------------------------------------------------------------


        int maxX = 0, maxY = 0;
        int[,] grid = model.GetGrid(ref maxX, ref maxY);

        if (combinationsRemaining)
        {
            UnityAction actionCallback = () =>
            {
                PassFallingTokensToView(model.PullDownTokens(), CheckForNewCombinationsAndAddScore);
            };

            PassDespawningTokensToView(actionCallback);
        }
        else
        {
            UpdateTurnsLeft(turnsLeft);

            if (turnsLeft <= turnsLeftHandler / 4)
                audioSource.pitch = 1.1f;

            bool stillGotMoves = model.CheckForAvailableMoves();
            if (turnsLeft <= 0 || !stillGotMoves)
            {
                RestartGame();

                
                view.SwitchGrid(grid, maxX, maxY);
            }

            game_phase = Game_Phase.WONDERING;
        }
    }

    void Update()
    {
        if (!coroutineCallbacks.allTokensHaveSpawned)
            return;

        Vector3 mousePosition = Vector3.zero;
        Vector2 mousePosition2D = Vector2.zero;
        RaycastHit2D raycastHit2D = default;

        Touch touch = default;

        bool selectionActioned = false;

        if (game_phase == Game_Phase.WONDERING || game_phase == Game_Phase.CHAINING)
        {


#if UNITY_EDITOR || UNITY_STANDALONE

            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
#endif

            mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

            raycastHit2D = Physics2D.Raycast(mousePosition2D, Vector2.zero);

            if (actualSelectedToken != null && raycastHit2D.collider == null)
            {
                view.DeselectToken(actualSelectedToken);
                actualSelectedToken = null;
            }
        }

        

        switch (game_phase)
        {
            case Game_Phase.WONDERING:
                {

#if UNITY_ANDROID && !UNITY_EDITOR
            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    selectionActioned = true;
                    mousePosition = Camera.main.ScreenToWorldPoint(touch.position);

                    mousePosition2D = new Vector2(mousePosition.x, mousePosition.y);

                    raycastHit2D = Physics2D.Raycast(mousePosition2D, Vector2.zero);

                    if (actualSelectedToken != null && raycastHit2D.collider == null)
                    {
                        view.DeselectToken(actualSelectedToken);
                        actualSelectedToken = null;
                    }
                }
            }
#endif

                    if (raycastHit2D.collider == null || raycastHit2D.collider.tag != "Token")
                        return;
                    
                    if (actualSelectedToken != null && actualSelectedToken != raycastHit2D.collider.gameObject)
                    {
                        view.DeselectToken(actualSelectedToken);
                    }

                    if (actualSelectedToken != raycastHit2D.collider.gameObject)
                    {
                        actualSelectedToken = raycastHit2D.collider.gameObject;
                        view.SelectToken(actualSelectedToken);
                    }



#if UNITY_EDITOR || UNITY_STANDALONE

                    selectionActioned = Input.GetMouseButton(0);
#endif
                    if (selectionActioned)
                    {
                        game_phase = Game_Phase.CHAINING;
                    }
                }
                break;
            case Game_Phase.CHAINING:
                {

                    raycastHit2D = Physics2D.Raycast(mousePosition2D, Vector2.zero);

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

                    bool explodeActioned = false;


#if UNITY_EDITOR || UNITY_STANDALONE

                    explodeActioned = Input.GetMouseButtonUp(0);
#endif


                    if (explodeActioned)
                    {
                        bool exploded = false;
                        score += model.ExplodeChain(ref exploded);
                        UpdateScore(score);

                        if(exploded)
                        {   
                            game_phase = Game_Phase.EXPLODING;
                            turnsLeft--;

                            UnityAction nextAction = () =>
                            {
                                PassFallingTokensToView(model.PullDownTokens(), CheckForNewCombinationsAndAddScore);
                            };

                            PassDespawningTokensToView(nextAction);
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

                }
                break;
        }
        
    }

    private void RestartGame(bool animated = false)
    {
        model.CreateNewGrid();
        score = 0;
        UpdateScore(score);
        turnsLeft = turnsLeftHandler;
        UpdateTurnsLeft(turnsLeft);

        audioSource.pitch = 1.0f;

        if (animated)
        {
            
        }
        else
        {
            int neededIntReference = 0;
            while (!model.CheckAndExplodeCombinations('x', ref neededIntReference) || !model.CheckAndExplodeCombinations('y', ref neededIntReference))
            {
                model.PullDownTokens();
            }
        }

        bool stillGotMoves = model.CheckForAvailableMoves();
        while (!stillGotMoves)
        {
            RestartGame(animated);
            stillGotMoves = model.CheckForAvailableMoves();
        }
    }
}
