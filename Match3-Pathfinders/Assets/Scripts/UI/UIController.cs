using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Text scoreText;
    public Text turnsLeftText;

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

    void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score: " + newScore.ToString();
    }

    void UpdateTurnsLeftText(int newTurnsLeft)
    {
        turnsLeftText.text = "Turns left: " + newTurnsLeft.ToString();
    }
}
