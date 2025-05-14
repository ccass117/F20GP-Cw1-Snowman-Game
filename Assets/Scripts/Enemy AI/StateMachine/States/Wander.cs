using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wander : State
{
    float counter = 0;

    public Wander(StateMachine parentMachine ) : base(parentMachine)
    {

    }


    public override void Act()
    {
        counter += Time.deltaTime;

        if (FleeExitCondition())
            parent.UpdateCurrentState(typeof(Flee));
    }

    private bool FleeExitCondition()
    {
        return counter > 2;
    }

    public override void Reset()
    {
        counter = 0;
    }
}
