using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{
    [SerializeField] private List<Sprite> spriteList;
    [SerializeField] private Vector2 colliderSize;

    public void RenderGrid(int item, Vector3 position)
    {
        SpriteRenderer spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
        BoxCollider2D boxCollider2D = spriteRenderer.gameObject.AddComponent<BoxCollider2D>();
        boxCollider2D.size = new Vector2(colliderSize.x, colliderSize.y);
        spriteRenderer.sprite = item > -1 ? spriteList[item] : null;
        spriteRenderer.transform.position = position;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
