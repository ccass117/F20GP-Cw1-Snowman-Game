using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CheckpointText : MonoBehaviour
{
    private TextMeshProUGUI UIText;
    // Start is called before the first frame update
    void Start()
    {
        //set the Checkpoint text to be invisible
        UIText = GetComponent<TextMeshProUGUI>();
        UIText.color = new Color(255f, 255f, 255f, 0f);
    }

    public void ShowText()
    {
        StartCoroutine(FadeText());
    }

    private IEnumerator FadeText()
    {
        float duration = 1f;
        float elapsedTime = 0f;

        //Fade in (alpha 0 -> 1)
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            UIText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        //Reset timer
        elapsedTime = 0f;

        //Fade out (alpha 1 -> 0)
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            UIText.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
    }
}
