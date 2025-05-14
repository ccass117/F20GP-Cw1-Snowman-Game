using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.XR;

public enum GroupSteerMode
{
    Cohesion = 0,
    Alignment,
    Avoidant,
    Randomised
}

public class BirdGroupSteering : State
{
    List<Bird> neighbourhood = new List<Bird>();
    float neighbourhoodRefreshRate = 1f;
    float neighbourhoodRefreshCooldown = 0f;
    float neighbourhoodRadius = 10f;
    Vector3 neighbourhoodRot = Vector3.zero;

    GroupSteerMode currentSteerMode = GroupSteerMode.Cohesion;
    float steerModeCooldown = 0f;
    Dictionary<GroupSteerMode, float> steerModeCooldowns = new Dictionary<GroupSteerMode, float>()
    {
        { GroupSteerMode.Cohesion, 0.2f },
        { GroupSteerMode.Alignment, 0.2f },
        { GroupSteerMode.Avoidant, 0.2f },
        { GroupSteerMode.Randomised, 0.2f },
    };

    public BirdGroupSteering(StateMachine parentMachine) : base(parentMachine) { }
    private BirdStateMachine GetParent()
    {
        return (BirdStateMachine)parent;
    }

    System.Random rand = new System.Random();

    public override void Act()
    {
        // check exit condition(s)
        if (FleeExitCondition())
        {
            parent.UpdateCurrentState(typeof(BirdFlee));
            return;
        }


        // check neighbourhood cooldown
        neighbourhoodRefreshCooldown -= Time.deltaTime;
        if (neighbourhoodRefreshCooldown < 0f)
            RefreshNeighbourhood();

        // check wander exit condition (must be done AFTER calculating the neighbourhood)
        if (WanderExitCondition())
        {
            parent.UpdateCurrentState(typeof(BirdWander));
            return;
        }

        // do steering stuff
        if (!GetParent().Bird.AvoidingObstacle)
        {
            // check mode cooldown
            steerModeCooldown -= Time.deltaTime;
            if (steerModeCooldown < 0f)
            {
                currentSteerMode = (GroupSteerMode)(((int)currentSteerMode + 1) % 3); // cycle to next mode
                steerModeCooldown = steerModeCooldowns[currentSteerMode];  // refresh cooldown accordingly
            }

            switch (currentSteerMode)
            {
                case GroupSteerMode.Cohesion:
                    Cohesion();
                    break;
                case GroupSteerMode.Alignment:
                    Alignment();
                    break;
                case GroupSteerMode.Avoidant:
                    Avoidant();
                    break;
                case GroupSteerMode.Randomised:
                    Randomised();
                    break;
            }
        }
    }

    void Cohesion()
    {
        if (neighbourhood.Count <= 0)
            return;

        Vector3 aggregatePosition = Vector3.zero;
        foreach (Bird nb in neighbourhood)
        {
            aggregatePosition += nb.transform.position;
        }

        Vector3 avgPosition = (aggregatePosition.magnitude / (float)neighbourhood.Count) * aggregatePosition.normalized;
        
        GetParent().Bird.RotateOverTime(Quaternion.LookRotation(avgPosition - GetParent().transform.position, Vector3.up));
    }

    void Alignment()
    {
        if (neighbourhood.Count <= 0)
            return;

        float newYRot = 0;
        foreach (Bird nb in neighbourhood)
        {
            newYRot += nb.transform.rotation.eulerAngles.y;
        }

        neighbourhoodRot = new Vector3(0, newYRot / neighbourhood.Count, 0);
        GetParent().Bird.RotateOverTime(Quaternion.Euler(neighbourhoodRot));
    }


    void Avoidant()
    {
        if (neighbourhood.Count <= 0)
            return;

        Vector3 aggregatePosition = Vector3.zero;
        foreach (Bird nb in neighbourhood)
        {
            aggregatePosition += nb.transform.position;
        }

        Vector3 avgPosition = (aggregatePosition.magnitude / (float)neighbourhood.Count) * aggregatePosition.normalized;

        GetParent().Bird.RotateOverTime(Quaternion.LookRotation(GetParent().transform.position - avgPosition, Vector3.up));
    }

    void Randomised()
    {
        if (neighbourhood.Count <= 0)
            return;

        // if first pass of randomise in time slice
        if (steerModeCooldown <= Time.deltaTime)
            GetParent().Bird.RotateOverTime(Quaternion.Euler(0, rand.Next(360), 0));
    }


    void RefreshNeighbourhood()
    {
        Collider[] colliders = Physics.OverlapSphere(GetParent().Bird.transform.position, neighbourhoodRadius, Utils.LayerToLayerMask(Layers.Bird));
        neighbourhoodRefreshCooldown = neighbourhoodRefreshRate;
        neighbourhood.Clear();

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == GetParent().gameObject)
                continue;

            collider.gameObject.TryGetComponent<Bird>(out Bird bird);
            neighbourhood.Add(bird);
        }
    }

    private bool FleeExitCondition()
    {
        bool withinBounds = (GetParent().SnowBuddy.position - GetParent().transform.position).magnitude < GetParent().ThreatDetectionRadius;
        return withinBounds;
    }

    private bool WanderExitCondition()
    {
        return neighbourhood.Count <= 0;
    }
}
