using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

// Ice Pick is a permanent fixture.
// Instead of Instantiating and Destroying IcePicks when needed, we just enable/disable certain components
// This improves efficiency - especially if we add more projectiles in the future
public class IcePick : MonoBehaviour
{
    float speed = 80f;
    float spawnDistance = 3f;  // how far away the IcePick spawns from SnowBuddy's body

    float maxDuration = 0.5f;
    float currentDuration = 0f;

    // sets how much control SnowBuddy has over their movement while hooked
    // e.g. 0.6 means Movement is only 60% effective while hooked
    public float movementWhileHooked { get; private set; } = 0.6f;

    public bool Hooked { get; private set; } = false;
    public bool HookAvailable = true;  // hook can only be used once during a jump

    [SerializeField] GameObject iceCubePrefab;
    GameObject iceCubeRef;

    // self refs
    Rigidbody rigidBody;
    MeshRenderer meshRenderer;
    CapsuleCollider capsuleCollider;
    LineRenderer lineRenderer;
    SpringJoint bodySpringJoint;  // used for controlling player's movement while hooked
    [SerializeField] Transform rotatorTransform;
    [SerializeField] Rigidbody bodyRB;
    [SerializeField] Transform tip;  // used for raycasts

    SnowBuddy _snowBuddy;
    SnowBuddy snowBuddy
    {
        get
        {
            if (_snowBuddy == null)
                _snowBuddy = GameObject.Find("SnowBuddy(Clone)").transform.GetChild(0).GetComponent<SnowBuddy>();
            return _snowBuddy;
        }
    }

    FreezableEntity currentFrozenEntity;

    // used to keep IcePick close to SnowBuddy while inactive
    // this not incredibly necessary, but could reduce potential bugs
    // could definitely be removed for efficiency purposes
    ParentConstraint parentConstraint;

    private bool isActive = false;
    public bool IsActive {
        get
        {
            return isActive;
        }
        private set
        {
            isActive = value;

            meshRenderer.enabled = value;
            capsuleCollider.enabled = value;
            parentConstraint.constraintActive = !value;
        }
    }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        parentConstraint = GetComponent<ParentConstraint>();
        lineRenderer = GetComponent<LineRenderer>();
        bodySpringJoint = bodyRB.GetComponent<SpringJoint>();
    }

    void Start()
    {
        ResetProjectile();
    }

    void FixedUpdate()
    {
        if (IsActive)
        {
            currentDuration += Time.deltaTime;

            if (!lineRenderer.enabled) lineRenderer.enabled = true;
            lineRenderer.SetPositions(new Vector3[] { transform.position, rotatorTransform.position });

            if (!Hooked)
            {
                meshRenderer.transform.Rotate(new Vector3(10, 0, 0));
                if (currentDuration >= maxDuration)
                    ResetProjectile();
            }
        }
    }

    public void Shoot()
    {
        // can only shoot if available and if projectile is not currently active
        if (IsActive || (!HookAvailable && !snowBuddy.FlyCheat)) return;

        Hooked = false;
        currentDuration = 0f;
        transform.position = rotatorTransform.position + Utils.RemoveY(rotatorTransform.forward).normalized * 3;
        IsActive = true;
        transform.forward = Utils.RemoveY(Camera.main.transform.forward);  // get horizontal components of camera angle
        rigidBody.velocity = transform.forward * speed;
    }

    public void ResetProjectile()
    {
        if (bodySpringJoint != null)
            Destroy(bodySpringJoint);
        if (iceCubeRef != null)
            Destroy(iceCubeRef);
        if (currentFrozenEntity != null)
            currentFrozenEntity.Unfreeze();

        if (IsActive)
        {
            if (Hooked && !snowBuddy.Grounded)
                HookAvailable = false;

            currentDuration = 0f;
            rigidBody.velocity = Vector3.zero;  // reset speed
            lineRenderer.enabled = false;
            IsActive = false;
            transform.position = rotatorTransform.position + Vector3.forward * spawnDistance;  // reset position
            Hooked = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Hooked) return;

        // IcePick moves pretty fast, so the collision is sometimes detected once the pick is fully inside a wall/object - which looks weird
        // the collider is also quite generous to make it feel good, but this means shooting an object from an acute angle looks weird as well

        // worst case: solution: snap IcePick to nearest point in collider.bounds - but this looks weird if it suddenly jumps a big distance
        // best case: perform raycast to see if IcePick Renderer has not hit wall, then continue movement to RayHit.point in a smooth fashion

        // raycast without x rotation
        Physics.SphereCast(tip.position, 0.5f, transform.forward, out RaycastHit hit);

        if (hit.collider != null && hit.collider == collision.collider)
            transform.position = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        else
        {
            // this is when the IcePick rendererer is missing the object, but the collider is hitting
            // snap to Collider's nearest point

            Vector3 closestPointRelativeToTip = collision.collider.gameObject.GetComponent<Renderer>().bounds.ClosestPoint(tip.position);
            Vector3 distanceFromTip = tip.position - closestPointRelativeToTip;
            transform.position -= distanceFromTip;
        }

        Hook(collision.gameObject);
    }

    void Hook(GameObject obj)
    {
        rigidBody.velocity = Vector3.zero;  // reset speed
        capsuleCollider.enabled = false;
        meshRenderer.transform.localRotation = Quaternion.Euler(-35, 0, 0);  // set to preferred rotation
        Hooked = true;

        // create springjoint for movement
        bodySpringJoint = bodyRB.AddComponent<SpringJoint>();
        bodySpringJoint.autoConfigureConnectedAnchor = false;
        bodySpringJoint.connectedAnchor = transform.position;
        bodySpringJoint.maxDistance = (transform.position - bodyRB.position).magnitude;

        bodySpringJoint.spring = 1f;  // force applied when at maxdistance
        bodySpringJoint.damper = 2f;  // damping issued to spring force
        bodySpringJoint.massScale = 3f;  // used to scale spring force according to mass

        // create ice effect on target object
        Vector3 size = obj.GetComponent<Renderer>().bounds.size;
        iceCubeRef = Instantiate(iceCubePrefab, obj.transform.position, Quaternion.identity, obj.transform);

        // this accounts for cases where localScale and lossyScale (global scale) are not the same
        Vector3 scale = new Vector3(
            iceCubeRef.transform.localScale.x / iceCubeRef.transform.lossyScale.x,
            iceCubeRef.transform.localScale.y / iceCubeRef.transform.lossyScale.y,
            iceCubeRef.transform.localScale.z / iceCubeRef.transform.lossyScale.z);

        iceCubeRef.transform.localScale = Vector3.Scale(size, scale) + scale;  // scale icecube to the object hit, but making it slightly 1m bigger in all axis
        iceCubeRef.transform.position = obj.GetComponent<Renderer>().bounds.center;

        // apply frozen status
        obj.TryGetComponent<FreezableEntity>(out FreezableEntity e);
        if (e != null)
        {
            currentFrozenEntity = e;
            e.Freeze();
        }
    }

    public void OnLanding()
    {
        // renew 
    }
}
