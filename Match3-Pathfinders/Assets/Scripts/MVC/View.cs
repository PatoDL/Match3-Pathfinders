using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.Events;

public class View : MonoBehaviour
{
    [SerializeField] private List<Sprite> spriteList;
    [SerializeField] private Vector2 colliderSize;
    [SerializeField] private float tokenFallSpeed;
    [SerializeField] private float tokenDistanceTreshold;
    [SerializeField] private Transform tokensParent;
    [SerializeField] private AudioClip bubbleSound;
    [SerializeField] private RuntimeAnimatorController animatorController;
    private LineRenderer lineRenderer;

    private List<GameObject> fallingObjects;

    private GameObject[,] viewGrid;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        fallingObjects = new List<GameObject>();
    }

    #region AnimationRegion

    /// <summary>
    /// Data saves the x position of the falling token, y the actual y position of the falling token and z the y destiny position of it.
    /// </summary>
    public IEnumerator PullDownAnimatedTokens(int[,] grid, List<Vector3> data, Vector2 offset, int xMax, int yMax, UnityAction Callback)
    {
        foreach (Vector3 d in data)
        {
            GameObject g = null;
            if (d.y >= yMax)
            {
                int newObjectsCount = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i] == d)
                        break;

                    if (data[i].x == d.x && data[i].y >= yMax)
                        newObjectsCount++;
                }
                g = Instantiate(viewGrid[0, 0], new Vector3(d.x * offset.x, (yMax + newObjectsCount) * offset.y), Quaternion.identity);
                if(grid[(int)d.x, (int)d.z] == -1)
                    Debug.Break();
                else
                    g.GetComponent<SpriteRenderer>().sprite = spriteList[grid[(int)d.x, (int)d.z]];
            }
            else
            {
                GameObject gAux = viewGrid[(int)d.x, (int)d.y];
                g = Instantiate(gAux, gAux.transform.position, Quaternion.identity);
                gAux.GetComponent<SpriteRenderer>().sprite = null;
            }

            fallingObjects.Add(g);
        }

        int amountOfFallenTokens = 0;

        while (amountOfFallenTokens < fallingObjects.Count)
        {
            for (int i = 0; i < fallingObjects.Count; i++)
            {
                if (Mathf.Abs(fallingObjects[i].transform.position.y - data[i].z * offset.y) < tokenDistanceTreshold && fallingObjects[i].activeSelf)
                {
                    amountOfFallenTokens++;
                    fallingObjects[i].SetActive(false);
                    viewGrid[(int)data[i].x, (int)data[i].z].GetComponent<SpriteRenderer>().sprite = spriteList[grid[(int)data[i].x, (int)data[i].z]];

                    viewGrid[(int)data[i].x, (int)data[i].z].transform.localScale = Vector3.one;

                }

                else
                {
                    fallingObjects[i].transform.position += Vector3.down * tokenFallSpeed * Time.deltaTime;
                }
            }

            yield return null;
        }

        for (int i = fallingObjects.Count - 1; i >= 0; i--)
        {
            GameObject g = fallingObjects[i];
            fallingObjects.Remove(g);
            Destroy(g);
        }

        Callback?.Invoke();
    }

    public bool PullDownAnimatedSingleToken(GameObject aux, int x, int y, int destinyY, float yOffset, int yMax)
    {
        if (y >= yMax)
            return false;

        aux = Instantiate(viewGrid[x, y], viewGrid[x, y].transform.position,
            viewGrid[x, y].transform.rotation);

        viewGrid[x, y].GetComponent<SpriteRenderer>().sprite = null;

        while (Mathf.Abs(aux.transform.position.y - destinyY * yOffset) > tokenDistanceTreshold)
        {
            aux.transform.position += Vector3.down * tokenFallSpeed * Time.deltaTime;


            return false;

        }

        viewGrid[x, destinyY].GetComponent<SpriteRenderer>().sprite = aux.GetComponent<SpriteRenderer>().sprite;
        return true;
    }

    public IEnumerator PullDownAnimatedSingleToken(int x, int y, int destinyY, float yOffset, int yMax, UnityAction Callback)
    {
        if (y >= yMax)
            yield break;

        GameObject g = Instantiate(viewGrid[x, y], viewGrid[x, y].transform.position,
            viewGrid[x, y].transform.rotation);

        viewGrid[x, y].GetComponent<SpriteRenderer>().sprite = null;

        while (Mathf.Abs(g.transform.position.y - destinyY * yOffset) > tokenDistanceTreshold)
        {
            g.transform.position += Vector3.down * tokenFallSpeed * Time.deltaTime;

            Debug.Log(Mathf.Abs(g.transform.position.y - destinyY*yOffset));

            yield return null;
            
        }

        viewGrid[x, destinyY].GetComponent<SpriteRenderer>().sprite = g.GetComponent<SpriteRenderer>().sprite;
        Destroy(g);

        Callback?.Invoke();
    }

    public IEnumerator DespawnTokens(int[,] grid, int xMax, int yMax, UnityAction Callback)
    {
        List<GameObject> tokensToDespawn = new List<GameObject>();
        for (int i = 0; i < xMax; i++)
        {
            for (int j = 0; j < yMax; j++)
            {
                if (grid[i, j] == -1)
                {
                    tokensToDespawn.Add(viewGrid[i, j]);
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

        Callback();
    }

    public IEnumerator SpawnAnimated(int[,] grid, int xMax, int yMax, UnityAction Callback)
    {
        for (int i = 0; i < xMax; i++)
        {
            for (int j = 0; j < yMax; j++)
            {
                viewGrid[i, j].SetActive(true);
                Animator animator = viewGrid[i, j].GetComponent<Animator>();
                animator.SetTrigger("Spawn");
                AudioSource audioSource = animator.GetComponent<AudioSource>();
                audioSource.pitch = 1.0f;
                audioSource.Play();
                yield return null;
            }
        }

        Callback();
    }

    #endregion

    #region GridStuffRegion

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

    public GameObject CreateGridItem(int item, Vector3 position)
    {
        SpriteRenderer spriteRenderer = new GameObject().AddComponent<SpriteRenderer>();
        BoxCollider2D boxCollider2D = spriteRenderer.gameObject.AddComponent<BoxCollider2D>();
        Animator animator = spriteRenderer.gameObject.AddComponent<Animator>();
        AudioSource audioSource =  spriteRenderer.gameObject.AddComponent<AudioSource>();
        audioSource.clip = bubbleSound;
        audioSource.volume = 0.7f;

        animator.runtimeAnimatorController = animatorController;

        boxCollider2D.size = new Vector2(colliderSize.x, colliderSize.y);
        spriteRenderer.sprite = item > -1 ? spriteList[item] : null;
        spriteRenderer.transform.position = position;

        spriteRenderer.gameObject.tag = "Token";
        spriteRenderer.gameObject.transform.parent = tokensParent;
        spriteRenderer.gameObject.SetActive(false);
        return spriteRenderer.gameObject;
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

    #endregion

    #region LineRendererRegion

    public void AddLinePosition(Vector3 position)
    {
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, position);
    }

    public void RemoveLinePosition()
    {
        lineRenderer.positionCount--;
    }

    public void ResetLineRenderer()
    {
        lineRenderer.positionCount = 0;
    }

    #endregion//LINE RENDERER STUFF

    #region TokenSelectionRegion

    public void SelectToken(GameObject token)
    {
        Animator animator = token.GetComponent<Animator>();
        if (!animator.GetBool("MouseEnter"))
            animator.SetBool("MouseEnter", true);

        AudioSource audioSource = animator.GetComponent<AudioSource>();
        audioSource.pitch = 1.5f;
        if(!audioSource.isPlaying)
            audioSource.Play();
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

    #endregion
}
