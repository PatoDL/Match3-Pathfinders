using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class View : MonoBehaviour
{
    [SerializeField] private List<Sprite> spriteList;
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    public void RenderGrid(int item, Vector3 position)
    {
        SpriteRenderer spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteList[item];
        position.x *= offsetX;
        position.y *= offsetY;
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
