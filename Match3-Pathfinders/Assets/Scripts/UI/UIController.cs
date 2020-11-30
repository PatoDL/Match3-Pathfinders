using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text turnsLeftText;
    [SerializeField] GameObject RestartImagePanel;

    [SerializeField] GameObject NoMoreMovesGO;
    [SerializeField] GameObject NoTurnsLeftGO;
    
    [SerializeField] Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        controller.UpdateScore = UpdateScoreText;
        controller.UpdateTurnsLeft = UpdateTurnsLeftText;
        Color c = RestartImagePanel.GetComponent<Image>().material.color;
        c.a = 0.0f;
        RestartImagePanel.GetComponent<Image>().material.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ImageFadeTo(float aValue, float aTime, GameObject g)
    {
        Material mat = g.GetComponent<Image>().material;
        float alpha = mat.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            mat.color = newColor;
            yield return null;
        }

    }

    IEnumerator ShowAndHideImage(float secondsBetweenActions, GameObject ImageGO)
    {
        ImageGO.SetActive(true);
        StartCoroutine(ImageFadeTo(1.0f, 0.3f, ImageGO));
        yield return new WaitForSeconds(secondsBetweenActions);
        StartCoroutine(ImageFadeTo(0.0f, 0.3f, ImageGO));
        yield return new WaitForSeconds(0.3f);
        ImageGO.SetActive(false);
    }

    void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score: " + newScore.ToString();
    }

    void ShowNoMoreMovesAvailable(int i)
    {
        StartCoroutine(ShowAndHideImage(4, NoMoreMovesGO));
    }

    void UpdateTurnsLeftText(int newTurnsLeft)
    {
        turnsLeftText.text = "Turns left: " + newTurnsLeft.ToString();
        if(newTurnsLeft <= 0)
        {
            StartCoroutine(ShowAndHideImage(4, NoTurnsLeftGO));
        }
    }
}
