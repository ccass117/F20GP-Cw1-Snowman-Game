using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Flee : State
{
    float counter = 0;

    public Flee(StateMachine parentMachine) : base(parentMachine)
    {

    }


    public override void Act()
    {
        counter += Time.deltaTime;

        if (WanderExitCondition())
            parent.UpdateCurrentState(typeof(Wander));
    }

    public virtual bool WanderExitCondition()
    {
        return counter > 3.5;
    }

    // this is used to reset a state when retrieving it from state machine's state pool
    public override void Reset()
    {
        counter = 0;
    }
}
