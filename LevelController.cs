using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using FrostweepGames.Plugins.GoogleCloud.SpeechRecognition;

public class LevelController : MonoBehaviour
{
    #region singleton
    public static LevelController Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public GameObject completedDialogue;
    public Text SecretWord;
    public int numberOfMoves;
    public Level currentLevel;
    public int numberOfDeadSheep;
    public int numberOfAttempt;

    private int numberOfSheepRemaining;
    private Object[] loadedLevels;
    private List<Level> levels;
    private int currentLevelIndex;
    private string groundTutorialInitialText;
    private VoiceInteractionController voiceCon;

    private void Start()
    {
        voiceCon = FrostweepGames.Plugins.GoogleCloud.SpeechRecognition.VoiceInteractionController.Instance;
        currentLevelIndex = -1;
        levels = new List<Level>();
        Object[] levelObjects = GameObject.FindGameObjectsWithTag("Level");
        foreach (Object o in levelObjects)
        {
            GameObject go = (GameObject)o;
            levels.Add(go.GetComponent<Level>());
        }

        Global.Instance.sheeps = new List<Transform>();
        CreateNewLevel(currentLevelIndex + 1);
    }

    private void Update()
    {
        if (Debug.isDebugBuild)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                // Skip Level
                CreateNewLevel(currentLevelIndex + 1);
            }
        }
    }

    public void CreateNewLevel (int index)
    {
        LogController.Instance.CreateLogMessage("Level " + index);
        numberOfAttempt = 0;
        currentLevelIndex = index;
        voiceCon.CancelRecording();
        Global.Instance.dog.GetComponent<SheepdogController>().Reset();
        Global.Instance.dog.GetComponent<SheepdogController>().walkParticleSystem.Stop();
        Global.Instance.UI.GetComponent<BlackScreen>().Show();
        numberOfMoves = 0;
        numberOfDeadSheep = 0;
        currentLevel = levels.Find(el => el.levelIndex == currentLevelIndex);
        Global.Instance.sheeps = currentLevel.sheeps;

        if (currentLevel.wolfs.Count > 0)
        {
            Global.Instance.wolf = currentLevel.wolfs[0];
        }

        RenderSettings.fogColor = currentLevel.backgroundColor;

        numberOfSheepRemaining = Global.Instance.sheeps.Count;
        Global.Instance.dog.GetComponent<NavMeshAgent>().Warp(currentLevel.levelStart.transform.position);
        Global.Instance.dog.GetComponent<SheepdogController>().walkParticleSystem.Play();

        if (currentLevel.GroundTutorial)
        {
            groundTutorialInitialText = currentLevel.GroundTutorial.text;
        }

        if (currentLevel.levelIndex == 1 && numberOfMoves == 0)
        {
            TutorialController.Instance.ShowTutorial(TutorialType.listen, 0f);
        }

        if (currentLevel.levelIndex == 2 && numberOfMoves == 0)
        {
            TutorialController.Instance.ShowTutorial(TutorialType.left, 1f);
        }

        if (currentLevel.levelIndex == 3 && numberOfMoves == 0)
        {
            TutorialController.Instance.ShowTutorial(TutorialType.wolf, 1f);
        }

        RestartLevel(false);
    }

    public void RestartLevel (bool withSound)
    {
        if (numberOfAttempt % 6 == 0 && numberOfAttempt > 0 && !completedDialogue.activeSelf)
        {
            Global.Instance.UI.GetComponent<SkipLevelUI>().Show();
            numberOfAttempt++;
            return;
        }

        numberOfAttempt++;

        voiceCon.CancelRecording();
        Global.Instance.dog.GetComponent<SheepdogController>().Reset();
        if (withSound)
        {
            LogController.Instance.CreateLogMessage("Repeats level");
            SoundController.Instance.PlaySound("failed", 0f, 0.4f, 1f);
        }
        Global.Instance.dog.GetComponent<SheepdogController>().walkParticleSystem.Stop();
        Global.Instance.UI.GetComponent<BlackScreen>().Show();
        numberOfMoves = 0;
        numberOfDeadSheep = 0;

        if (currentLevel.GroundTutorial) currentLevel.GroundTutorial.text = groundTutorialInitialText;
        if (currentLevel.wolfs.Count > 0)
        {
            Global.Instance.wolf.GetComponent<NavMeshAgent>().Warp(Global.Instance.wolf.GetComponent<WolfController>().originPosition);
            Global.Instance.wolf.GetComponent<WolfController>().EndParalization();
        }

        if (Global.Instance.sheeps.Count > 0)
        {
            foreach (Transform t in Global.Instance.sheeps)
            {
                t.gameObject.SetActive(true);
                t.GetComponent<NavMeshAgent>().Warp(t.GetComponent<SheepController>().originPosition);
            }        
        }

        numberOfSheepRemaining = Global.Instance.sheeps.Count;
        Global.Instance.dog.GetComponent<NavMeshAgent>().Warp(currentLevel.levelStart.transform.position);
        Global.Instance.dog.GetComponent<SheepdogController>().walkParticleSystem.Play();
    }

    public void SheepReachedGoal ()
    {
        SoundController.Instance.PlaySound("sheep", 0f, UnityEngine.Random.Range(0.5f, 0.7f), UnityEngine.Random.Range(1f, 1.3f));
        numberOfSheepRemaining--;
        CheckForLevelCompleted();
    }

    public void SheepDied()
    {
        SoundController.Instance.PlaySound("sheep", 0f, UnityEngine.Random.Range(0.5f, 0.7f), UnityEngine.Random.Range(1f, 1.3f));
        numberOfSheepRemaining--;
        numberOfDeadSheep++;

        if (numberOfSheepRemaining == 0)
        {
            if (numberOfDeadSheep >= currentLevel.sheeps.Count)
            {
                RestartLevel(true);

            } else
            {
                CheckForLevelCompleted();
            }
        }
    }

    private void CheckForLevelCompleted ()
    {
        if (numberOfSheepRemaining == 0 && numberOfDeadSheep < currentLevel.sheeps.Count)
        {
            CompleteLevel();

        }
    }

    public void CompleteLevel ()
    {
        if (currentLevelIndex == levels.Count - 1)
        {
            StartCoroutine(ShowProrotypeCompletedDialogueAfterSeconds(0.5f));
        }
        else
        {
            if (currentLevelIndex > 0)
            {
                StartCoroutine(ShowLevelDoneDialogueAfterSeconds(0.5f));
            }
        }
    }

    IEnumerator ShowLevelDoneDialogueAfterSeconds (float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SoundController.Instance.PlaySound("completed", 0f, 0.7f, 1f);
        Global.Instance.UI.GetComponent<LevelDoneUI>().ShowDialogue();
    }

    IEnumerator ShowProrotypeCompletedDialogueAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        SoundController.Instance.PlaySound("completed", 0f, 0.7f, 1f);
        float errorRateTwoDecimals = Mathf.Round(ErrorController.Instance.ErrorRate() * 100f) / 100f;
        string errorRateString = errorRateTwoDecimals.ToString();
        errorRateString = errorRateString.Replace(".", "");

        if (errorRateString.Length == 3)
        {
            errorRateString += "0";
        }
        else if (errorRateString.Length == 2)
        {
            errorRateString += "00";
        }
        else if (errorRateString.Length == 1)
        {
            errorRateString += "000";
        }

        System.String secretWordText = "";

        if (Global.Instance.group == Group.shySheep)
        {
            secretWordText = "SHY SHEEP " + errorRateString;
        }
        if (Global.Instance.group == Group.worthyWolf)
        {
            secretWordText = "WORTHY WOLF " + errorRateString;
        }
        if (Global.Instance.group == Group.dancingDog)
        {
            secretWordText = "DANCING DOG " + errorRateString;
        }

        SecretWord.text = secretWordText;
        secretWordText.CopyToClipboard();
        completedDialogue.SetActive(true);

        LogController lc = LogController.Instance;

        lc.CreateLogMessage("Session time: " + Time.time);
        lc.CreateLogMessage("Total moves: " + ErrorController.Instance.TotalNumberOfMoves);
        lc.CreateLogMessage("Unrecognized moves: " + ErrorController.Instance.TotalNumberOfUnrecognizedMoves);
        lc.CreateLogMessage("Error Rate: " + ErrorController.Instance.ErrorRate());
        lc.CreateLogMessage("Secret Word: " + secretWordText);
        lc.SaveLogs(secretWordText);
    }

}

public static class ClipboardExtension
{
    public static void CopyToClipboard(this string str)
    {
        GUIUtility.systemCopyBuffer = str;
    }
}