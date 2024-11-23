using UnityEngine;
using TMPro;
using System.Collections;

public class ExpandInputField : MonoBehaviour {
    public TMP_InputField inputField;  
    private RectTransform rectTransform; 

    [Header("Input Field Height Settings")]
    public float defaultHeight = 30f;  
    public float expandedHeight = 100f; 


    [Header("Animation Settings")]
    public float lerpTime = 0.5f; 
    private float timeElapsed = 0f;

    void Start() {
        rectTransform = inputField.GetComponent<RectTransform>();
   
        SetInputFieldHeight(defaultHeight);
    }

    void Update() {
        
        if (Input.GetKeyDown(KeyCode.M)) {
            ExpandInputFieldSize();
        }

        
        if (Input.GetKeyDown(KeyCode.Return)) {
            ShrinkInputFieldSize();
        }
    }

    private void ExpandInputFieldSize() {
       
        timeElapsed = 0f;
        StartCoroutine(AnimateHeightChange(expandedHeight));
    }

    private void ShrinkInputFieldSize() {
        
        timeElapsed = 0f;
        StartCoroutine(AnimateHeightChange(defaultHeight));
    }

    private IEnumerator AnimateHeightChange(float targetHeight) {
        float initialHeight = rectTransform.sizeDelta.y;

        while (timeElapsed < lerpTime) {
            timeElapsed += Time.deltaTime;
            float currentHeight = Mathf.Lerp(initialHeight, targetHeight, timeElapsed / lerpTime);
            SetInputFieldHeight(currentHeight);
            yield return null; 
        }

    
        SetInputFieldHeight(targetHeight);
    }

    private void SetInputFieldHeight(float newHeight) {
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
    }
}
