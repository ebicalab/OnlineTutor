using UnityEngine;
using System.Collections;

public class EmotionController : MonoBehaviour
{
    [SerializeField] private VHPEmotions m_VHPEmotions;
    [SerializeField] private VHPManager m_VHPManager;

    public void SetBlendShapes(float[] blendShapes){
        m_VHPEmotions.SetBlendShapeValues(blendShapes);
    }

    public string Names(){
        return m_VHPManager.PrintBlendShapeNames();
    }

    public string Values(){
        return m_VHPManager.PrintBlendShapeValues();
    }

    public void SetEmotion(string name, float value, float transitionDuration = 1){
        StartCoroutine(TransitionEmotion(name, value, transitionDuration));
    }

    private IEnumerator TransitionEmotion(string name, float targetValue, float duration)
    {
        float currentValue = GetCurrentEmotionValue(name);
        float elapsedTime = 0f;

        string currentEmotion = GetActiveEmotion();
        float currentEmotionValue = GetCurrentEmotionValue(currentEmotion);

        if (name == currentEmotion)
        {
            while (elapsedTime < duration)
            {
                
                elapsedTime += Time.deltaTime;
                float newValue = Mathf.Lerp(currentValue, targetValue, elapsedTime / duration);
                ApplyEmotionValue(name, newValue);
                yield return null;
            }

            ApplyEmotionValue(name, targetValue);
        }
        else
        {
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newValue = Mathf.Lerp(currentEmotionValue, 0, elapsedTime / duration);
                ApplyEmotionValue(currentEmotion, newValue);
                yield return null;
            }

            elapsedTime = 0f;
            float targetEmotionValue = GetCurrentEmotionValue(name); // Get the initial value for the target emotion

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float newValue = Mathf.Lerp(currentValue, targetValue, elapsedTime / duration);
                ApplyEmotionValue(name, newValue);
                yield return null;
            }

            ApplyEmotionValue(name, targetValue);
        }
    }


    private float GetCurrentEmotionValue(string name){
        switch(name){
            case "anger":
                return m_VHPEmotions.anger;
            case "disgust":
                return m_VHPEmotions.disgust;
            case "fear":
                return m_VHPEmotions.fear;
            case "happiness":
                return m_VHPEmotions.happiness;
            case "sadness":
                return m_VHPEmotions.sadness;
            case "surprise":
                return m_VHPEmotions.surprise;
            case "neutral":
                return 0f; 
            default:
                Debug.LogError("Invalid emotion name");
                return 0f;
        }
    }

    private void ApplyEmotionValue(string name, float value){
        switch(name){
            case "anger":
                m_VHPEmotions.anger = value;
                break;
            case "disgust":
                m_VHPEmotions.disgust = value;
                break;
            case "fear":
                m_VHPEmotions.fear = value;
                break;
            case "happiness":
                m_VHPEmotions.happiness = value;
                break;
            case "sadness":
                m_VHPEmotions.sadness = value;
                break;
            case "surprise":
                m_VHPEmotions.surprise = value;
                break;
            case "neutral":
                m_VHPEmotions.anger = value;
                m_VHPEmotions.disgust = value;
                m_VHPEmotions.fear = value;
                m_VHPEmotions.happiness = value;
                m_VHPEmotions.sadness = value;
                m_VHPEmotions.surprise = value;
                break;
            default:
                Debug.LogError("Invalid emotion name");
                break;
        }
    }

    private string GetActiveEmotion()
{
    string[] names = { "anger", "disgust", "fear", "happiness", "sadness", "surprise" };
    
    for (int i = 0; i < names.Length; i++)
    {
        if (GetCurrentEmotionValue(names[i]) != 0)
        {
            return names[i];
        }
    }
    
    return "neutral"; 
}

}
