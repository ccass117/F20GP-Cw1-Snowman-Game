using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timingscript : MonoBehaviour
{
    public GameObject FinishZone;
    private float playerTime;
    private float fastestTime;
    private BoxCollider finishCollider;
    private bool hasFinished = false;
    public GameObject HighscoreSign;
    public GameObject CurrentTimeSign;
    public TextMeshProUGUI TimeUI;

    private void Start()
    {
        //Get the initial fastest time, if none exist, set it to 999.99f
        fastestTime = PlayerPrefs.GetFloat("FastestTime", 999.99f);
        //Get collider for finish Zone
        finishCollider = FinishZone.GetComponent<BoxCollider>();
        Debug.Log(fastestTime);

        //If no current fast time, remove the sign else display text on sign for fastest time
        if (fastestTime == 999.99f)
        {
            Destroy(HighscoreSign);
        } else
        {
            TMPro.TextMeshPro textMesh = HighscoreSign.GetComponentInChildren<TMPro.TextMeshPro>();
            if (textMesh != null)
            {
                textMesh.text = $"Fastest Time: {fastestTime}";
            }
        }
    }

    void Update()
    {

        
        if (finishCollider != null)
        {
            Collider[] colliders = Physics.OverlapBox(finishCollider.bounds.center, finishCollider.bounds.extents);
            foreach (Collider col in colliders)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("SnowBuddy"))
                {
                    //Snowbuddy has finished!
                    OnSnowBuddyEnter();
                    break;
                }
            }
        }

        //Reset Fastest time & max penguin count when pressing R and Delete in sync (will refresh on scene reload, hold R)
        if (Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.Delete))
        {
            PlayerPrefs.SetFloat("FastestTime", 999.99f);
            PlayerPrefs.SetInt("MaxPenguin", 0);
        }

        //Increment timers while the player has not finished
        if (!hasFinished)
        {
            playerTime = Time.timeSinceLevelLoad;
            playerTime = Mathf.Round(playerTime * 100.0f) * 0.01f;

            string timeText = playerTime.ToString("F2");
            TMPro.TextMeshPro SignText = CurrentTimeSign.GetComponentInChildren<TMPro.TextMeshPro>();
            if (SignText != null)
            {     
                SignText.text = $"Current Time: {timeText}";
            }

            TimeUI.text = $"Time: {timeText}";

            //Once player is longer than their current fastest time, make the time text gray as indication
            if (playerTime > PlayerPrefs.GetFloat("FastestTime", 999.99f))
            {
                TimeUI.color = Color.gray;
            }

        }

    }

    private void OnSnowBuddyEnter()
    {
        //Finish game once player enters, and update fastest times
        if (!hasFinished) {
            playerTime = Time.timeSinceLevelLoad;
            Debug.Log("SnowBuddy reached FinishZone at: " + playerTime + " seconds");
            hasFinished = true;

            if (playerTime < fastestTime)
            {
                PlayerPrefs.SetFloat("FastestTime", Mathf.Round(playerTime * 100.0f)*0.01f);
                Debug.Log("New fastest time: " + playerTime + " seconds");
                TimeUI.color = Color.green;
            }

            //Reload the scene after 5 seconds
            StartCoroutine(ResetScene(5));
        }
        
    }
    IEnumerator ResetScene(int time)
    {
        yield return new WaitForSeconds(time);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}