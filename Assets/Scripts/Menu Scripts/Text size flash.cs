using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class TextSizeFlash : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    private bool textGrow = true;
    private float targetSize;

    private const float MinSize = 48f;
    private const float MaxSize = 56f;
    private const float Speed = 10f;
    //private const float Speed = 0.3f;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        targetSize = textMesh.fontSize;
    }

    void Update()
    {
        //Set fontSize to be closer to target size
        textMesh.fontSize = Mathf.MoveTowards(textMesh.fontSize, targetSize, Speed * Time.deltaTime);
        //textMesh.fontSize = Mathf.MoveTowards(textMesh.fontSize, targetSize, Speed);

        //Swap target size if reached current target
        if (Mathf.Approximately(textMesh.fontSize, targetSize))
        {
            targetSize = textGrow ? MaxSize : MinSize;
            textGrow = !textGrow;
        }
    }
}