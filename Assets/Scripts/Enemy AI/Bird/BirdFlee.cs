using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFlee : State
{
    float fleeMovespeed = 15f;
    int potentialAngleOfFlee = 45;
    float swapDirectionCooldown = 0f;
    float maxSwapDirectionCooldown = 2f;
    System.Random rand = new System.Random();

    public BirdFlee(StateMachine parentMachine) : base(parentMachine)
    {

    }

    private BirdStateMachine GetParent()
    {
        return (BirdStateMachine)parent;
    }

    public override void Act()
    {
        // check exit condition(s)
        if (WanderExitCondition())
        {
            parent.UpdateCurrentState(typeof(BirdWander));
            return;
        }

        // check cooldowns
        if (swapDirectionCooldown >= 0f)
        {
            swapDirectionCooldown -= Time.deltaTime;


            return;
        }

        if (GetParent().Bird.AvoidingObstacle)
            swapDirectionCooldown = maxSwapDirectionCooldown;  // reset cooldown

        GetParent().Bird.RotateOverTime(Quaternion.Euler(0, GenerateFleeDirection(), 0));
        swapDirectionCooldown = maxSwapDirectionCooldown;  // reset cooldown
    }

    public virtual bool WanderExitCondition()
    {
        bool outwithBounds = (GetParent().SnowBuddy.position - GetParent().transform.position).magnitude > GetParent().ThreatDetectionRadius;
        return outwithBounds;
    }

    // this is used to reset a state when retrieving it from state machine's state pool
    public override void Reset()
    {
        GetParent().Bird.MoveSpeed = fleeMovespeed;
        GetParent().Bird.IsStanding = false;
    }

    private int GenerateFleeDirection()
    {
        float yRotf = Quaternion.LookRotation(
            GetParent().transform.position - GetParent().SnowBuddy.position,
            Vector3.up).eulerAngles.y;

        // return rotation with certain degree of randomness
        int temp = rand.Next((int)yRotf - potentialAngleOfFlee, (int)yRotf + potentialAngleOfFlee);
        return rand.Next((int)yRotf - potentialAngleOfFlee, (int)yRotf + potentialAngleOfFlee);
    }
}
