using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillOnCollision : MonoBehaviour
{
    GameMgr mgr;
    

    private void Awake()
    {
        mgr = GameObject.Find("GameMgr").GetComponent<GameMgr>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;

        if ( collision.collider.gameObject.layer == (int)Layers.SnowBuddy)
        {
            mgr.ReturnToSpawn();
        }
    }

}
