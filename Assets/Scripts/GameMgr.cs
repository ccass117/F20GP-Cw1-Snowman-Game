using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    SnowBuddy bodyRef;
    [SerializeField] GameObject SnowBuddyPrefab;
    [SerializeField] GameObject GoldenPenguinPrefab;
    [SerializeField] GameObject spawnPoint;

    private int deathCount = 0;
    public TextMeshProUGUI deathText;

    private void Awake()
    {
        // instantiates SnowBuddy at spawnpoint and stores a reference to SnowBuddy's body (the body contains the logic)
        bodyRef = Utils.InstantiateWithOptions(
            SnowBuddyPrefab,
            spawnPoint.transform.position,
            Quaternion.identity).GetComponentInChildren<SnowBuddy>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ReturnToSpawn()
    {
        bodyRef.transform.position = spawnPoint.transform.position;
        bodyRef.transform.localScale = new Vector3(1,1,1);
        bodyRef.GetComponent<Rigidbody>().mass = 1;
        bodyRef.GetComponent<Rigidbody>().velocity = Vector3.zero;
        deathCount++;
        deathText.text = $"Deaths: {deathCount}";
    }

    public void SpawnGoldenPenguin()
    {
        Instantiate(GoldenPenguinPrefab, new Vector3(90, 1, 22), Quaternion.identity);
    }
}
