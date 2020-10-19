using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepdogState : MonoBehaviour
{
    #region singleton
    public static SheepdogState Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public bool shouldShowState = true;

    public GameObject IdleIndicator;
    public GameObject BusyIndicator;
    public GameObject ProcessingIndicator;
    public GameObject ReceivingIndicator;

    public State CurrentState { get; private set; }

    private void Start()
    {
        CurrentState = State.idle;    
    }

    public void SetState (State state)
    {
        CurrentState = state;
    }

    private void Update()
    {
        if(shouldShowState)
        {
            IdleIndicator.SetActive(CurrentState == State.idle);
            BusyIndicator.SetActive(CurrentState == State.busy);
            ProcessingIndicator.SetActive(CurrentState == State.processing);
            ReceivingIndicator.SetActive(CurrentState == State.receiving);
        } else
        {
            IdleIndicator.SetActive(false);
            BusyIndicator.SetActive(false);
            ProcessingIndicator.SetActive(false);
            ReceivingIndicator.SetActive(false);
        }
    }

    public enum State
    {
        idle,
        busy,
        processing,
        receiving
    }
}
