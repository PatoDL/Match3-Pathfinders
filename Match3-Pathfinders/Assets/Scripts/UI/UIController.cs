using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] Text scoreText;
    [SerializeField] Text turnsLeftText;
    [SerializeField] GameObject restartImagePanel;

    [SerializeField] GameObject noMoreMovesGO;
    [SerializeField] GameObject noTurnsLeftGO;
    
    [SerializeField] Controller controller;

    // Start is called before the first frame update
    void Start()
    {
        controller.UpdateScore = UpdateScoreText;
        controller.UpdateTurnsLeft = UpdateTurnsLeftText;
        Color c = restartImagePanel.GetComponent<Image>().material.color;
        c.a = 0.0f;
        restartImagePanel.GetComponent<Image>().material.color = c;
    }

    IEnumerator ImageFadeTo(float aValue, float aTime, GameObject g, UnityAction Callback)
    {
        Material mat = g.GetComponent<Image>().material;
        float alpha = mat.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            mat.color = newColor;
            yield return null;
        }

        Callback?.Invoke();
    }

    IEnumerator ShowAndHideImage(float secondsBetweenActions, GameObject ImageGO)
    {
        ImageGO.SetActive(true);
        StartCoroutine(ImageFadeTo(1.0f, 0.3f, ImageGO, null));
        yield return new WaitForSeconds(secondsBetweenActions);
        StartCoroutine(ImageFadeTo(0.0f, 0.3f, ImageGO, () => { ImageGO.SetActive(false); }));
    }

    void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score: " + newScore.ToString();
    }

    void ShowNoMoreMovesAvailable(int i)
    {
        StartCoroutine(ShowAndHideImage(4, noMoreMovesGO));
    }

    void UpdateTurnsLeftText(int newTurnsLeft)
    {
        turnsLeftText.text = "Turns left: " + newTurnsLeft.ToString();
        if(newTurnsLeft <= 0)
        {
            StartCoroutine(ShowAndHideImage(4, noTurnsLeftGO));
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
