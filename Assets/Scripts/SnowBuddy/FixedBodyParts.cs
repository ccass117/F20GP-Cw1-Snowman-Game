using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FixedBodyParts : MonoBehaviour
{
    [SerializeField]
    GameObject bodyRef;

    void FixedUpdate()
    {
        if (bodyRef != null)
        {
            Vector3 move = bodyRef.GetComponent<SnowBuddy>().Move.normalized;
            if (move != Vector3.zero) transform.forward = move;
        }
    }
}
