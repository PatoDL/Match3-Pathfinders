﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class View : MonoBehaviour
{
    [SerializeField] private List<Sprite> spriteList;
    [SerializeField] private Vector2 colliderSize;
    [SerializeField] private Transform tokensParent;
    [SerializeField] private RuntimeAnimatorController animatorController;
    private LineRenderer lineRenderer;

    GameObject[,] viewGrid;

    public GameObject CreateGridItem(int item, Vector3 position)
    {
        SpriteRenderer spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
        BoxCollider2D boxCollider2D = spriteRenderer.gameObject.AddComponent<BoxCollider2D>();
        Animator animator = spriteRenderer.gameObject.AddComponent<Animator>();

        animator.runtimeAnimatorController = animatorController;

        boxCollider2D.size = new Vector2(colliderSize.x, colliderSize.y);
        spriteRenderer.sprite = item > -1 ? spriteList[item] : null;
        spriteRenderer.transform.position = position;

        spriteRenderer.gameObject.tag = "Token";
        spriteRenderer.gameObject.transform.parent = tokensParent;
        spriteRenderer.gameObject.SetActive(false);
        return spriteRenderer.gameObject;
    }

    public IEnumerator PullDownAnimatedTokens(int[,] grid, int xMax, int yMax, UnityAction StartCallback)
    {
        for(int i=0;i<xMax;i++)
        {
            for(int j=0;j<yMax;j++)
            {

            }
        }
        yield return null;
    }

    public IEnumerator DespawnTokens(int[,] grid, int xMax, int yMax, UnityAction CallBack)
    {
        List<GameObject> tokensToDespawn = new List<GameObject>();
        for (int i = 0; i < xMax; i++)
        {
            for (int j = 0; j < yMax; j++)
            {
                if (grid[i, j] == -1)
                {
                    tokensToDespawn.Add(viewGrid[i,j]);
                }
            }
        }

        foreach (GameObject g in tokensToDespawn)
        {
            Animator animator = g.GetComponent<Animator>();

            if (!animator.GetBool("Despawning"))
            {
                animator.SetBool("Despawning", true);
                animator.SetBool("MouseEnter", false);
            }
        }

        int despawnedTokenAmount = 0;
        while (despawnedTokenAmount < tokensToDespawn.Count)
        {
            foreach (GameObject g in tokensToDespawn)
            {
                Animator animator = g.GetComponent<Animator>();

                if (!animator.GetBool("Despawning"))
                {
                    despawnedTokenAmount++;
                }
            }

            if (despawnedTokenAmount < tokensToDespawn.Count)
                despawnedTokenAmount = 0;

            yield return null;
        }

        Debug.Log("coroutine end");
        CallBack();
    }

    public IEnumerator SpawnAnimated(int[,] grid, int xMax, int yMax, UnityAction Callback)
    {
        for(int i=0;i<xMax;i++)
        {
            for(int j=0;j<yMax;j++)
            {
                viewGrid[i, j].SetActive(true);
                Animator animator = viewGrid[i, j].GetComponent<Animator>();
                animator.SetTrigger("Spawn");
                yield return null;
            }
        }

        Callback();
    }

    public void CreateGrid(int[,] grid, int xMax, int yMax, Vector2 tokenOffset)
    {
        viewGrid = new GameObject[xMax, yMax];
        for (int i = 0; i < xMax; i++)
        {
            for (int j = 0; j < yMax; j++)
            {
                viewGrid[i, j] = CreateGridItem(grid[i, j], new Vector3(i * tokenOffset.x, j * tokenOffset.y));
            }
        }
    }

    public void SwitchGridItem(int x, int y, int item)
    {
        if (item > -1)
            viewGrid[x, y].GetComponent<SpriteRenderer>().sprite = spriteList[item];
        else
            viewGrid[x, y].GetComponent<SpriteRenderer>().sprite = null;
    }

    public void SwitchGrid(int[,] grid, int xMax, int yMax)
    {
        for (int i = 0; i < xMax; i++)
        {
            for (int j = 0; j < yMax; j++)
            {
                SwitchGridItem(i, j, grid[i, j]);
            }
        }
    }

    public void AddLinePosition(Vector3 position)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount-1, position);
    }

    public void RemoveLinePosition()
    {
        lineRenderer.positionCount--;
    }

    public void SelectToken(GameObject token)
    {
        Animator animator = token.GetComponent<Animator>();
        if (!animator.GetBool("MouseEnter"))
            animator.SetBool("MouseEnter", true);
    }

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    internal void DeselectToken(GameObject token)
    {
        Animator animator = token.GetComponent<Animator>();
        if (animator.GetBool("MouseEnter"))
            animator.SetBool("MouseEnter", false);
    }

    internal void DeselectToken(int x, int y)
    {
        Animator animator = viewGrid[x, y].GetComponent<Animator>();
        if (animator.GetBool("MouseEnter"))
            animator.SetBool("MouseEnter", false);
    }

    public void MarkError(int x, int y)
    {
        viewGrid[x, y].GetComponent<SpriteRenderer>().material.color = Color.red;
        StartCoroutine(BackToItsOriginalColor(x, y));
       
    }

    IEnumerator BackToItsOriginalColor(int x, int y)
    {
        yield return new WaitForSeconds(0.3f);
        viewGrid[x, y].GetComponent<SpriteRenderer>().material.color = Color.white;
    }

    public void ResetLineRenderer()
    {
        lineRenderer.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}
