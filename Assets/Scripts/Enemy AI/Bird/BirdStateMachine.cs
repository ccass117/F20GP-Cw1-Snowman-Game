using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdStateMachine : StateMachine
{
    // this setup is requied to serialize a public property that has a private setter
    [SerializeField] private Bird bird;
    public Bird Bird { get { return bird; } private set { bird = value; } }

    // this setup is required to account for the fact that SnowBuddy gets instantiated after start
    private Transform snowBuddy;
    public Transform SnowBuddy {
        get
        {
            if (snowBuddy == null)
                snowBuddy = GameObject.Find("SnowBuddy(Clone)").transform.GetChild(0); // get snowbuddy body

            return snowBuddy;
        }
        private set { snowBuddy = value; }
    }

    public float ThreatDetectionRadius { get; private set; } = 20f;

    private void Awake()
    {
        states = new List<State>() { new BirdWander(this), new BirdFlee(this), new BirdGroupSteering(this) };
        UpdateCurrentState(typeof(BirdWander));
        //currentState = states[0];
        currentState.Reset();

        Bird = GetComponent<Bird>();
    }
}
