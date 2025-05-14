using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    [SerializeField]
    Rigidbody snowbuddyRigidBody;
    int maxRotation = -90;
    int maxVelocity = 30;

    float eulerZ;
    public bool invert;

    private void Awake()
    {
        eulerZ = transform.localRotation.eulerAngles.z;
        if (invert) maxRotation *= -1;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        float t = Utils.RemoveY(snowbuddyRigidBody.velocity).magnitude / maxVelocity;
        float lerpRotation = Mathf.Lerp(0, maxRotation, t);

        transform.localRotation = Quaternion.Euler(0, Mathf.RoundToInt(lerpRotation), eulerZ);

        // flail arms proportionally (TODO, low prio)
        //if (t > 1) print("flail" + t);
    }
}
