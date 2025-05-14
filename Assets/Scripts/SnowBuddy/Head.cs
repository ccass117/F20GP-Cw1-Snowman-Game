using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Head : MonoBehaviour
{
    [SerializeField]
    GameObject bodyRef;
    Rigidbody rigidBody;
    float desiredDistanceFromHead = 3f;
    float rotationSpeed = 20f;

    private float speedAmp = 5f;

    Vector3 distance = Vector2.zero;

    // Start is called before the first frame update
    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // calculate desired distance between body and head (based on the scale of the body)
        desiredDistanceFromHead = bodyRef.transform.localScale.magnitude / 1.4f + 2f;

        // if moving with any significant speed
        if ( bodyRef.GetComponent<Rigidbody>().velocity.magnitude > 0.1f)
        {
            // accelerate towards desired position (with velocity scaling with distance to desired position)
            Vector3 targetPos = bodyRef.transform.position + new Vector3(0, desiredDistanceFromHead, 0);
            distance = targetPos - transform.position;

            rigidBody.velocity = distance * speedAmp;

            // rotation has issues at super slow speeds
            if (bodyRef.GetComponent<Rigidbody>().velocity.magnitude > 0.5f)
            {
                // rotate with smoothing
                Quaternion toBody = Quaternion.LookRotation(bodyRef.transform.position - transform.position, Vector3.up);
                Quaternion toBodyAdjusted = Quaternion.Euler(270 + toBody.eulerAngles.x, toBody.eulerAngles.y, toBody.eulerAngles.z);  // fbx was exported at wrong angle - this has been fixed in future imports
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toBodyAdjusted, 180 / rotationSpeed);
            }
        }
        // when extremely close to desired position:
        else if ((bodyRef.transform.position - transform.position).magnitude - desiredDistanceFromHead > 0.05f)
        {
            // the above velocity approach encounters problems when super close to desired position - alter position directly with smoothing
            transform.position = Vector3.MoveTowards(transform.position, bodyRef.transform.position, speedAmp * Time.deltaTime);
        }
    }
}
