using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.AI;
using System;
using TMPro;

public class Bird : FreezableEntity
{
    [SerializeField] Rigidbody rigidBody;
    public float MoveSpeed = 3f;
    // oa == Obstacle Avoidance
    Quaternion rotationOverTime = Quaternion.identity;
    float oaCooldown = 0f;
    float maxOACooldown = 0.4f;

    float ledgeCooldown = 0f;
    float maxLedgeCooldown = 0.4f;
    public bool Rotating { get; private set; } = false;
    public bool AvoidingObstacle { get { return oaCooldown > 0; } }
    [SerializeField] float oaRotationSpeed;  // rotation speed for in radians per second
    [SerializeField] float oaCastLength;  // length of spherecast used to detect obstacles#

    Transform _penguin;
    Transform penguin
    {
        get
        {
            if (_penguin == null)
                _penguin = transform.GetChild(0);

            return _penguin;
        }
    }

    // determines if bird is standing on it's legs or lying on it's belly
    [NonSerialized] public bool IsStanding = true;
    float desiredPenguinXRot { get { return IsStanding ? 0 : 90; } }

    private void Update()
    {
        if (oaCooldown > 0)
            oaCooldown -= Time.deltaTime;

        if (ledgeCooldown > 0)
            ledgeCooldown -= Time.deltaTime;

        AnimateStanding();
    }


    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        RaycastHit hit;

        // detect ledges
        if (ledgeCooldown <= 0)
        {
            bool isLedge = !Physics.Raycast(
                transform.position + transform.forward * 2f,
                transform.forward + (Vector3.down * 0.3f),
                out hit,
                oaCastLength * 2f);

            if (isLedge)
            {
                RotateOverTime(Quaternion.Euler(0, transform.rotation.eulerAngles.y + 180, 0));
                oaCooldown = maxOACooldown;
                ledgeCooldown = maxLedgeCooldown;
            }
        }

        // if off cooldown: detect obstacle and begin avoiding
        if (oaCooldown <= 0f)
        {
            // ray for detecting obstacles (starting position is from slightly behind centre of self)
            bool isObstacle = Physics.SphereCast(
                transform.position - transform.forward * 0.5f,
                0.5f,
                transform.forward,
                out hit,
                oaCastLength);

            if (isObstacle)
            {
                // this ensures that the bird only rotates on y axis - this may be inefficient and inaccurate
                float yRot = Quaternion.LookRotation(transform.forward + hit.normal, Vector3.up).eulerAngles.y;
                RotateOverTime(Quaternion.Euler(0, yRot, 0));
                oaCooldown = maxOACooldown;
            }

        }

        // carry out avoiding obstacle with rotation over time
        if (Rotating)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotationOverTime,
                180 / oaRotationSpeed * Time.deltaTime);

            // determine if avoid rotation is complete
            if (transform.rotation == rotationOverTime)
                Rotating = false;
        }

        // set velocity to match forward direction
        rigidBody.velocity = MoveSpeed * transform.forward;
    }


    public void RotateOverTime(Quaternion finalRotation)
    {
        rotationOverTime = finalRotation;
        Rotating = true;
    }

    void AnimateStanding()
    {
        if (penguin.localRotation.eulerAngles.x == desiredPenguinXRot)
            return;

        // rotate the penguin between 0 and 90 degrees for a smooth belly slide
        penguin.localRotation = Quaternion.RotateTowards(
            penguin.localRotation,
            Quaternion.Euler(Vector3.right * desiredPenguinXRot),
            180 / oaRotationSpeed * Time.deltaTime);

        // smoothly lower and raise penguin so that they don't clip into the ground while belly sliding or standing up
        float coef = penguin.localRotation.eulerAngles.x / 90;  // effectively gets the result of RotateTowards as a lerp between 0 and 1;

        penguin.position =
            Utils.RemoveY(transform.position)
            + Vector3.up * 0.1f
            + Vector3.up * 0.75f * coef;
    }

    public void SetIsStanding(bool value)
    {
        IsStanding = value;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // if (is snowBuddy || is snowBallProjectile)
        SnowBuddy snowBuddy;
        if (collision.gameObject.TryGetComponent(out SnowBallProjectile snowBall))
        {
            snowBall.transform.parent.GetChild(0).TryGetComponent(out snowBuddy);
        }
        else
        {
            collision.gameObject.TryGetComponent(out snowBuddy);
        }

        if (snowBuddy == null) return;

        // capture penguin
        snowBuddy.IncrementPenguinCounter();
        gameObject.SetActive(false);
    }
}
