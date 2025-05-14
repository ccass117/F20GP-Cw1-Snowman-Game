using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    float rotationSpeed = 180 / 10f;  // rotation speed in radians per second

    void Update()
    {
        //rotate camera slowly panning around the area
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);
    }
}
