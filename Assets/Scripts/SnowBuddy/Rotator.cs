using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField]
    GameObject bodyRef;
    private float xRotation;
    private float yRotation;
    float sensitivity = 1f;


    void FixedUpdate()
    {
        transform.position = bodyRef.transform.position;
    }


    void Update()
    {
        float xRot = Input.GetAxisRaw("Mouse Y");
        float yRot = Input.GetAxisRaw("Mouse X");

        xRotation -= xRot;
        yRotation += yRot;

        transform.localRotation = Quaternion.Euler(xRotation * sensitivity, yRotation * sensitivity, 0);
    }
}
