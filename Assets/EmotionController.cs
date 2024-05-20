using UnityEngine;

public class EmotionController : MonoBehaviour
{
    [SerializeField] private VHPEmotions m_VHPEmotions;
    //[SerializeField] private SkinnedMeshRenderer m_SkinnedMeshRenderer;
    private float[] m_BlendShapes;
    [SerializeField] private bool start;

    void Start()
    {
        start = false;
    }

    void Update()
    {
        if(start){
            RandomBlendShapes();
            start = false;
        }
    }

    public void RandomBlendShapes(){
        //generate random blendshape value list
        m_BlendShapes = new float[300];
        for (int i = 0; i < m_BlendShapes.Length; i++)
        {
            m_BlendShapes[i] = Random.Range(0.0f, 100.0f);
        }
        SetBlendShapes(m_BlendShapes);

    }


    void SetBlendShapes(float[] blendShapes){
        m_VHPEmotions.SetBlendShapeValues(blendShapes);
    }


}
