using UnityEngine;

public class EmotionController : MonoBehaviour
{
    [SerializeField] private VHPEmotions m_VHPEmotions;
    //[SerializeField] private SkinnedMeshRenderer m_SkinnedMeshRenderer;


    public void SetBlendShapes(float[] blendShapes){
        m_VHPEmotions.SetBlendShapeValues(blendShapes);
    }


}
