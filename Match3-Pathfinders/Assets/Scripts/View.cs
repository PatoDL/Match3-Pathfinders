using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{
    [SerializeField] private List<Sprite> spriteList;
    [SerializeField] private Vector2 colliderSize;
    [SerializeField] private Transform tokensParent;
    [SerializeField] private RuntimeAnimatorController animatorController;
    private LineRenderer lineRenderer;

    public void RenderGrid(int item, Vector3 position)
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
    }

    public void AddLinePosition(Vector3 position)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount-1, position);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
