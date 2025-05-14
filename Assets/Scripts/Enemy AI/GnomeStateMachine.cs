using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GnomeStateMachine : StateMachine
{
    private void Awake()
    {
        states = new List<State>() { new Wander(this), new Flee(this) };
        currentState = states[0];
    }
}
