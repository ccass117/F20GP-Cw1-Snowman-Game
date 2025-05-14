using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public abstract class State
{
    protected StateMachine parent;

    public State(StateMachine parentMachine)
    {
        parent = parentMachine;
    }


    // do action associated with state (called from FixedUpdate)
    public abstract void Act();

    // called when returning to a state (does nothing by default)
    public virtual void Reset() { }
}
