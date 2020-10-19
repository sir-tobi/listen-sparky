using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FrostweepGames.Plugins.GoogleCloud.SpeechRecognition;

public class DebugUI : MonoBehaviour
{
    public GameObject debugUI;
    public Text level;
    public Text moves;
    public Text totalMoves;
    public Text errorRateCorrectionEnabled;
    public Text movesUntilErrorRateCorrection;
    public Text errors;
    public Text errorRate;
    public Text lastUnderstood;

    private void Update()
    {
        if (debugUI)
        {
            if (debugUI.activeSelf)
            {
                LevelController lc = LevelController.Instance;
                ErrorController ec = ErrorController.Instance;

                level.text = "Level: " + lc.currentLevel.levelIndex;
                moves.text = "Moves in this level: " + lc.numberOfMoves;
                totalMoves.text = "Total moves: " + ec.TotalNumberOfMoves;
                errorRateCorrectionEnabled.text = "Error correction enabled: " + ec.isCorrectingErrorRate;
                movesUntilErrorRateCorrection.text = "Error correction starts at turn: " + ec.TurnCorrectionStarts;
                errors.text = "Errors: " + ec.TotalNumberOfUnrecognizedMoves;
                errorRate.text = "Errorrate: " + Mathf.Round(ec.ErrorRate() * 100f) / 100f;
            }
        }
    }
}
