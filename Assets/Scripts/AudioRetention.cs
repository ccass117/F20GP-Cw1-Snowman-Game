using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRetention : MonoBehaviour
{

    public static AudioRetention instance;
    private bool muteThatGodForsakenMusic = false;
    
    void Awake()
    {
        //Save instance of the audio object and ensure it is not destroyed
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); //Destroy the duplicate instances on game reload
        }

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            muteThatGodForsakenMusic ^= true;
        GetComponent<AudioSource>().volume = muteThatGodForsakenMusic ? 0f : 0.25f; //Good riddance

    }
}
