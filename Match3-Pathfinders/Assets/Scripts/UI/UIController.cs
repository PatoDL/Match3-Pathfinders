using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Text scoreText;
    public Text turnsLeftText;
    public GameObject RestartImagePanel;

    public Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        controller.UpdateScore = UpdateScoreText;
        controller.UpdateTurnsLeft = UpdateTurnsLeftText;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RestartImageFadeTo(float aValue, float aTime, GameObject g)
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

    IEnumerator ShowAndHideRestartImage(float secondsBetweenActions)
    {
        StartCoroutine(RestartImageFadeTo(1.0f, 0.3f, RestartImagePanel));

        yield return new WaitForSeconds(secondsBetweenActions);

        StartCoroutine(RestartImageFadeTo(0.0f, 0.3f, RestartImagePanel));
    }

    void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score: " + newScore.ToString();
    }

    void UpdateTurnsLeftText(int newTurnsLeft)
    {
        turnsLeftText.text = "Turns left: " + newTurnsLeft.ToString();
        if(newTurnsLeft <= 0)
        {
            StartCoroutine(ShowAndHideRestartImage(4));
        }
    }
}
