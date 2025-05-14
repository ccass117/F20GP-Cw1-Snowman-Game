using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

// Projectiles are a permanent fixture.
// Instead of Instantiating and Destroying projectiles when needed, we just enable/disable certain components
// This improves efficiency - especially if we add more projectiles in the future
public class SnowBallProjectile : FreezableEntity
{
    float speed = 40f;
    float spawnDistance = 3f;  // how far away the IcePick spawns from SnowBuddy's body
    Vector3 fixedScale = new Vector3(10, 5, 10);

    float lastShotTime = 0;
    int bounces = 0;
    int maxBounces = 3;

    // self refs
    Rigidbody rigidBody;
    MeshRenderer meshRenderer;
    SphereCollider sphereCollider;
    [SerializeField] Transform rotatorTransform;
    [SerializeField] Rigidbody bodyRB;

    // this is used to keep SnowBallProjectile close to SnowBuddy while inactive
    // this not incredibly necessary, but could reduce potential bugs
    // could definitely be removed for efficiency purposes
    ParentConstraint parentConstraint;

    private bool isActive = false;
    public bool IsActive
    {
        get
        {
            return isActive;
        }
        private set
        {
            isActive = value;

            meshRenderer.enabled = value;
            sphereCollider.enabled = value;
            parentConstraint.constraintActive = !value;
        }
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        sphereCollider = GetComponent<SphereCollider>();
        parentConstraint = GetComponent<ParentConstraint>();
    }

    void Start()
    {
        ResetProjectile();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();

        if (Time.time - lastShotTime > 4 && !Frozen)
            ResetProjectile();
    }

    public void Shoot(float dynamicScale)
    {
        // if is active, do nothing
        if (IsActive) ResetProjectile();

        lastShotTime = Time.time;
        bounces = 0;

        // get input direction
        Vector3 inputDir = bodyRB.GetComponent<SnowBuddy>().Move.normalized;
        if (Input.GetKey(KeyCode.Space)) inputDir += Vector3.up;
        inputDir = inputDir.normalized;

        if (inputDir == Vector3.zero)
            inputDir = rotatorTransform.forward;

        // reset body velocity
        bodyRB.velocity = Vector3.zero;
        // shoot body in new direction
        bodyRB.AddForce(Vector3.Scale(inputDir, fixedScale) * dynamicScale, ForceMode.Impulse);

        // launch snowball in opposite direction to body
        transform.position = rotatorTransform.position;
        rigidBody.velocity = inputDir * -1 * speed;
        transform.localScale = Vector3.one * dynamicScale;  // set to appropriate scale
        IsActive = true;
    }

    public void ResetProjectile()
    {
        if (IsActive)
        {
            rigidBody.velocity = Vector3.zero;  // reset speed
            IsActive = false;
            transform.position = rotatorTransform.position + Vector3.forward * spawnDistance;  // reset position
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (++bounces >= maxBounces || collision.gameObject.layer == (int)Layers.Enemy)
            Detonate();
    }

    void Detonate()
    {
        bounces = 0;
        rigidBody.velocity = Vector3.zero;  // reset speed
        sphereCollider.enabled = false;
        IsActive = false;
    }
}
