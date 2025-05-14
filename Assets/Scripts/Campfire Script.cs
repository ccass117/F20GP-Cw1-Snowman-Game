using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireScript : MonoBehaviour
{

    CheckpointText UIText;
    public GameObject spawnPoint;

    private void Awake()
    {
        //Get UI text
        UIText = GameObject.Find("Checkpoint Text").GetComponent<CheckpointText>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Check if the object is in the "SnowBuddy" layer
        if (other.gameObject.layer == LayerMask.NameToLayer("SnowBuddy"))
        {
            MoveSpawn();
        }
    }

    private void MoveSpawn()
    {
        //Move the Checkpoint to directly above campfire
        Debug.Log("Checkpoint Acquired!");
        UIText.ShowText();
        spawnPoint.transform.position = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
    }
}
