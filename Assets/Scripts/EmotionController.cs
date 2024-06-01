using UnityEngine;

public class EmotionController : MonoBehaviour
{
    [SerializeField] private VHPEmotions m_VHPEmotions;
    [SerializeField] private VHPManager m_VHPManager;
    //[SerializeField] private SkinnedMeshRenderer m_SkinnedMeshRenderer;


    public void SetBlendShapes(float[] blendShapes){
        m_VHPEmotions.SetBlendShapeValues(blendShapes);
    }

    public string Names(){
        return m_VHPManager.PrintBlendShapeNames();
    }

    public string Values(){
        return m_VHPManager.PrintBlendShapeValues();
    }

    public void SetEmotion(string name, float value){
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


}
