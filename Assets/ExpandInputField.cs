using UnityEngine;
using TMPro;
using System.Collections;

public class ExpandInputField : MonoBehaviour {
    public TMP_InputField inputField;
    private RectTransform rectTransform;

    [Header("Input Field Height & Width Settings")]
    public float defaultHeight = 30f;
    public float defaultWidth = 200f;  // Default width
    public float expandedHeight = 200f;
    public float expandedWidth = 500f;  // Expanded width

    [Header("Animation Settings")]
    public float lerpTime = 0.5f;
    private float timeElapsed = 0f;

    

    void Start() {
        rectTransform = inputField.GetComponent<RectTransform>();

        SetInputFieldSize(defaultWidth, defaultHeight);

        // Ensure the RectTransform is anchored at the lower-left corner
        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(0f, 0f);
        rectTransform.pivot = new Vector2(0f, 0f);  // Pivot is at the lower-left corner

        // Set initial position to make sure it's visible
        rectTransform.anchoredPosition = new Vector2(0, 0);

        Debug.Log("Initial Position: " + rectTransform.anchoredPosition);
        Debug.Log("Initial Size: " + rectTransform.sizeDelta);
    }

    public void ExpandInputFieldSize() {
        timeElapsed = 0f;
        StartCoroutine(AnimateSizeChange(expandedWidth, expandedHeight));
    }

    public void ShrinkInputFieldSize() {
        timeElapsed = 0f;
        StartCoroutine(AnimateSizeChange(defaultWidth, defaultHeight));
    }

    private IEnumerator AnimateSizeChange(float targetWidth, float targetHeight) {
        float initialWidth = rectTransform.sizeDelta.x;
        float initialHeight = rectTransform.sizeDelta.y;

        while (timeElapsed < lerpTime) {
            timeElapsed += Time.deltaTime;
            // Interpolating width and height independently
            float currentWidth = Mathf.Lerp(initialWidth, targetWidth, timeElapsed / lerpTime);
            float currentHeight = Mathf.Lerp(initialHeight, targetHeight, timeElapsed / lerpTime);
            SetInputFieldSize(currentWidth, currentHeight);
            yield return null;
        }

        // Ensure we set the final target size
        SetInputFieldSize(targetWidth, targetHeight);
    }

    private void SetInputFieldSize(float newWidth, float newHeight) {
        rectTransform.sizeDelta = new Vector2(newWidth, newHeight);

        // Debug log to check the size and position
        Debug.Log("Updated Size: " + rectTransform.sizeDelta);
        Debug.Log("Updated Position: " + rectTransform.anchoredPosition);
    }

    private void Clear(){
        ShrinkInputFieldSize();
    }
}
