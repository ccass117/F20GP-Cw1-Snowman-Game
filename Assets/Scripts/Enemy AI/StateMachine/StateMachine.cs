using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Finite State Machine
/// </summary>
public class StateMachine : MonoBehaviour
{
    protected State currentState;

    // this acts as a list of states and as a state pool for the machine
    public List<State> states { get; protected set; }

    // used to set current state to a state from a state in the state pool
    public bool UpdateCurrentState(Type newStateType)
    {
        if (states == null)
        {
            Debug.Log($"{typeof(StateMachine)}.{nameof(states)} was null");
            return false;
        }

        // get state of matching type from pool
        State stateFromPool = states.FirstOrDefault(s => s.GetType() == newStateType);
        if (stateFromPool == null)
        {
            Debug.LogError($"Cannot find State of type {newStateType} in {typeof(StateMachine)}.{nameof(states)}");
            return false;
        }

        //if (currentState != null)
        //    print($"Swapping current state from {currentState.GetType()} to {newStateType}");
        //else
        //    print($"Setting current state from to {newStateType}");

        currentState = stateFromPool;
        currentState.Reset();
        return true;
    }

    private void FixedUpdate()
    {
       currentState.Act();
    }
}