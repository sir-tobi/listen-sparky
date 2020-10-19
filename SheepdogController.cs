using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using FrostweepGames.Plugins.GoogleCloud.SpeechRecognition;

public class SheepdogController : MonoBehaviour
{
    public ParticleSystem barkParticleSystem;
    public ParticleSystem walkParticleSystem;
    public bool IsAgressive { get; private set; }
    public bool showDestination;
    public GameObject destinationIndicator;
    public Transform visualTransform;
    public GameObject CommandReceivedIndicator;
    public bool doesNotAcceptCommands;
    private NavMeshAgent navAgent;
    private SheepdogState state;
    private Transform closestSheep;
    private Transform mostLeftSheep;
    private Transform mostRightSheep;
    private bool hasBarked;
    private bool hasReachedDestination;
    private bool firstCommandSuccesfull;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        VoiceInteractionController.resultUpdateDelegate += onUpdateCommand;
        state = SheepdogState.Instance;
    }

    private void Update()
    {
        if (!Global.Instance.anyDialogueIsOpened && LevelController.Instance.currentLevel.levelIndex != 0)
        {
            if (Debug.isDebugBuild && Input.GetKeyUp(KeyCode.D))
            {
                onUpdateCommand("right");
            }

            if (Debug.isDebugBuild && Input.GetKeyUp(KeyCode.A))
            {
                onUpdateCommand("left");
            }

            if (Debug.isDebugBuild && Input.GetKeyUp(KeyCode.S))
            {
                onUpdateCommand("back");
            }

            if (Debug.isDebugBuild && Input.GetKeyUp(KeyCode.W))
            {
                onUpdateCommand("walk");
            }

            if (Debug.isDebugBuild && Input.GetKeyUp(KeyCode.R))
            {
                onUpdateCommand("bark");
            }
        }

        ShowDestination();

        if (!navAgent.pathPending)
        {
            if (navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                {
                    if (!hasReachedDestination)
                    {
                        StartCoroutine(ReturnToIdleAfterSeconds(0.5f));
                    }
                }
            }
        }

        if (!hasReachedDestination && navAgent.velocity.magnitude < 1f)
        {
            StartCoroutine(ReturnToIdleAfterSeconds(0.5f));
        }
    }

    private IEnumerator ReturnToIdleAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        IsAgressive = true;
        InputController.Instance.isProcessing = false;

        if (!hasBarked)
        {
            barkParticleSystem.Play();
            hasBarked = true;
        }

        hasReachedDestination = true;
        SheepdogState.Instance.SetState(SheepdogState.State.idle);

        if (LevelController.Instance.currentLevel.levelIndex == 1 && firstCommandSuccesfull)
        {
            if (LevelController.Instance.currentLevel.GroundTutorial)
            {
                LevelController.Instance.currentLevel.GroundTutorial.text = "Well done! Now repeat that!";
            }
        }
    }

    private IEnumerator SetBarkedToFalseAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        hasBarked = false;
        hasReachedDestination = false;
    }

    public void PreventCommand ()
    {
        CommandReceivedIndicator.SetActive(true);
        CommandReceivedIndicator.GetComponent<TextMesh>().text = "???";
        doesNotAcceptCommands = true;
        StartCoroutine(AcceptCommandsAfterSeconds(2f));
    }

    private IEnumerator AcceptCommandsAfterSeconds (float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Reset();
        CommandReceivedIndicator.GetComponent<TextMesh>().text = "";
        CommandReceivedIndicator.SetActive(false);
        doesNotAcceptCommands = false;
    }

    public void Reset ()
    {
        StartCoroutine(SetBarkedToFalseAfterSeconds(0f));
        IsAgressive = true;
        hasBarked = true;
        hasReachedDestination = true;
        SheepdogState.Instance.SetState(SheepdogState.State.idle);
    }

    private void onUpdateCommand(String command)
    {
        if (state.CurrentState != SheepdogState.State.idle || doesNotAcceptCommands)
        {
            return;
        }

        firstCommandSuccesfull = true;
        GetSheeps();
        StartCoroutine(SetBarkedToFalseAfterSeconds(0.5f));
        IsAgressive = false;
        LevelController.Instance.numberOfMoves++;
        ErrorController.Instance.TotalNumberOfMoves++;

        foreach (Transform t in Global.Instance.sheeps)
        {
            t.GetComponent<SheepController>().coolDownTimeStamp = Time.time;
        }

        command = command.ToLower();
        command = CorrectErrorRate(command);

        if (command.Contains("left") || command.Contains("franklin"))
        {
            CommandLeft();
            return;
        }

        if (command.Contains("right") || command.Contains("ride"))
        {
            CommandRight();
            return;
        }

        if (command.Contains("call off") || command.Contains("back"))
        {
            CommandBack();
            return;
        }

        if (command.Contains("wolf") || command.Contains("wulf") || command.Contains("bark") || command.Contains("scare") || command.Contains("scar") || command.Contains("wool") || command.Contains("bar"))
        {
            if (CommandScareWolf())
            {
                return;
            }
        }

        if (command.Contains("walk") || command.Contains("watts") || command.Contains("up") || command.Contains("woke") || command.Contains("towards") || command.Contains("ward") || command.Contains("too wards") || command.Contains("to wards") || command.Contains("worlds"))
        {
            CommandTowards();
            return;
        }

        // fall back
        LogController.Instance.CreateLogMessage("Command is an error.");
        ErrorController.Instance.TotalNumberOfUnrecognizedMoves++;
        if (Global.Instance.group == Group.worthyWolf)
        {
            CommandMagicBehavior();
        } else if (Global.Instance.group == Group.dancingDog)
        {
            CommandRandomBehavior();
        } else 
        {
            if (LevelController.Instance.currentLevel.levelIndex == 1)
            {
                firstCommandSuccesfull = false;
            }

            CommandReceivedIndicator.SetActive(true);
            CommandReceivedIndicator.GetComponent<TextMesh>().text = "???";
        }
    }

    private void CommandRandomBehavior ()
    {
        int max = LevelController.Instance.currentLevel.levelIndex - 1;
        max = Mathf.Clamp(max, 1, 4);
        int random = UnityEngine.Random.Range(0, max + 1);

        switch (random)
        {
            case 0: CommandTowards(); break;
            case 1: CommandLeft(); break;
            case 2: CommandRight(); break;
            case 4: CommandScareWolf(); break;
        }
    }

    private void CommandMagicBehavior ()
    {
        int level = LevelController.Instance.currentLevel.levelIndex;
        int move = LevelController.Instance.numberOfMoves;

        switch (level)
        {
            case 1:
                {
                    CommandTowards();
                    break;
                }
            case 2:
                {
                    if (move == 1)
                    {
                        CommandLeft();
                    } else
                    {
                        CommandTowards();
                    }
                    break;
                }
            case 3:
                {
                    if (move == 1)
                    {
                        CommandRight();
                    }
                    else if (move == 2)
                    {
                        CommandLeft();
                    }
                    else
                    {
                        CommandTowards();
                    }
                    break;
                }
            case 4:
                {
                    if (move == 1)
                    {
                        CommandScareWolf();
                    }
                    else if (move == 2)
                    {
                        CommandLeft();
                    }
                    else
                    {
                        CommandTowards();
                    }
                    break;
                }
            case 5:
                {
                    if (move == 1)
                    {
                        CommandScareWolf();
                    }
                    else if (move == 2 || move == 3)
                    {
                        CommandRight();
                    }
                    else
                    {
                        CommandTowards();
                    }
                    break;
                }
            case 8:
                {
                    if (move == 1)
                    {
                        CommandRight();
                    }
                    else
                    {
                        CommandTowards();
                    }
                    break;
                }
            default:
                {
                    CommandTowards();
                    break;
                }
        }
    }

    private void CommandTowards()
    {
        CommandReceivedIndicator.GetComponent<TextMesh>().text = "!";
        state.SetState(SheepdogState.State.busy);
        SetDestination(closestSheep.position - (closestSheep.position - transform.position).normalized * 10f);
    }

    private void CommandLeft ()
    {
        CommandReceivedIndicator.GetComponent<TextMesh>().text = "!";
        state.SetState(SheepdogState.State.busy);
        SetDestination(mostLeftSheep.position + ((mostLeftSheep.position - LevelController.Instance.currentLevel.goals[0].position).normalized * 10f));
    }

    private void CommandRight()
    {
        CommandReceivedIndicator.GetComponent<TextMesh>().text = "!";
        state.SetState(SheepdogState.State.busy);
        SetDestination(mostRightSheep.position + Vector3.Reflect((mostRightSheep.position - LevelController.Instance.currentLevel.goals[0].position).normalized * 10f, Vector3.right));
    }

    private void CommandBack ()
    {
        CommandReceivedIndicator.GetComponent<TextMesh>().text = "!";
        state.SetState(SheepdogState.State.busy);
        SetDestination(LevelController.Instance.currentLevel.levelStart.transform.position);
    }

    private bool CommandScareWolf ()
    {
        if (LevelController.Instance.currentLevel.wolfs[0])
        {
            CommandReceivedIndicator.GetComponent<TextMesh>().text = "!";
            state.SetState(SheepdogState.State.busy);
            SetDestination(LevelController.Instance.currentLevel.wolfs[0].position - (LevelController.Instance.currentLevel.wolfs[0].position - transform.position).normalized * 8f);
            StartCoroutine(SetWolfAsParalized(LevelController.Instance.currentLevel.wolfs[0].GetComponent<WolfController>()));
            return true;
        }

        return false;
    }

    private IEnumerator SetWolfAsParalized (WolfController wolf)
    {
        yield return new WaitForSeconds(1f);
        wolf.Paralize();
    }

    private void SetDestination (Vector3 destination)
    {
        destination = new Vector3(destination.x, destination.y, destination.z);
        navAgent.SetDestination(destination);
    }

    private String CorrectErrorRate (String input)
    {
        Debug.Log("Error rate is at: " + ErrorController.Instance.ErrorRate());

        if (!ErrorController.Instance.isCorrectingErrorRate)
        {
            LogController.Instance.CreateLogMessage("Understood: " + input);
            Debug.Log("Understood: " + input);
            return input;
        }

        String command = input;

        if (ErrorController.Instance.TotalNumberOfMoves > ErrorController.Instance.TurnCorrectionStarts)
        {
            if (ErrorController.Instance.ErrorRate() < ErrorController.Instance.TargetErrorRate)
            {
                LogController.Instance.CreateLogMessage("Understood: " + command + " , but refused because error rate was at " + ErrorController.Instance.ErrorRate());
                Debug.Log("Understood: " + command + " , but refused because error rate was at " + ErrorController.Instance.ErrorRate());
                command = "xxxxxxxxxxx";
            }
            else
            {
                LogController.Instance.CreateLogMessage("Understood: " + input);
                Debug.Log("Understood: " + command);
            }
        }
        else
        {
            Global.Instance.UI.GetComponent<DebugUI>().lastUnderstood.text = "Undestood: " + command;
            LogController.Instance.CreateLogMessage("Understood: " + input);
            Debug.Log("Understood: " + command);
        }

        return command;
    }

    private void GetSheeps()
    {
        closestSheep = Global.Instance.sheeps[0];
        mostLeftSheep = Global.Instance.sheeps[0];
        mostRightSheep = Global.Instance.sheeps[0];

        foreach (Transform sheep in Global.Instance.sheeps)
        {
            float currentDistance = Vector3.Distance(transform.position, closestSheep.position);
            float checkingDistance = Vector3.Distance(transform.position, sheep.position);

            if (checkingDistance < currentDistance)
            {
                closestSheep = sheep;
            }
        }

        foreach (Transform sheep in Global.Instance.sheeps)
        {
            if (sheep.position.x < mostLeftSheep.position.x)
            {
                mostLeftSheep = sheep;
            }
        }

        foreach (Transform sheep in Global.Instance.sheeps)
        {
            if (sheep.position.x > mostRightSheep.position.x)
            {
                mostRightSheep = sheep;
            }
        }
    }

    private void ShowDestination ()
    {
        destinationIndicator.transform.position = navAgent.destination;
        destinationIndicator.SetActive(showDestination && state.CurrentState != SheepdogState.State.idle);
    }
}
