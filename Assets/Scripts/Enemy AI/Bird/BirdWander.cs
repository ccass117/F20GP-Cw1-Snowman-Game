using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BirdWander : State
{
    float swapDirectionCooldown = 0f;
    float maxSwapDirectionCooldown = 6f;
    System.Random rand = new System.Random();


    public BirdWander(StateMachine parentMachine) : base(parentMachine)
    {

    }

    private BirdStateMachine GetParent()
    {
        return (BirdStateMachine)parent;
    }

    public override void Act()
    {
        // check exit condition(s)
        if (FleeExitCondition())
        {
            parent.UpdateCurrentState(typeof(BirdFlee));
            return;
        }

        if (GroupSteeringExitCondition())
        {

            parent.UpdateCurrentState(typeof(BirdGroupSteering));
            return;
        }

        // check cooldowns
        if (swapDirectionCooldown >= 0f)
        {
            swapDirectionCooldown -= Time.deltaTime;
            
            if (GetParent().Bird.Rotating)
                swapDirectionCooldown = maxSwapDirectionCooldown;  // reset cooldown

            return;
        }

        // rotate bird randomly on the y axis
        GetParent().Bird.RotateOverTime(Quaternion.Euler(0, rand.Next(360), 0));
        swapDirectionCooldown = maxSwapDirectionCooldown;  // reset cooldown
    }

    private bool FleeExitCondition()
    {
        bool withinBounds = (GetParent().SnowBuddy.position - GetParent().transform.position).magnitude < GetParent().ThreatDetectionRadius;
        return withinBounds;
    }

    private bool GroupSteeringExitCondition()
    {
        // detect if another bird in neighbourhood - hardcoding radius to match neighbourhoodRadius is not optimal
        return Physics.OverlapSphere(GetParent().Bird.transform.position, 10f, Utils.LayerToLayerMask(Layers.Bird)).Length > 1;  // length == 1 means that the bird detected itself
    }

    public override void Reset()
    {
        GetParent().Bird.MoveSpeed = 3f;
        GetParent().Bird.IsStanding = true;
    }
}
