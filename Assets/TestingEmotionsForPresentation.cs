using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingEmotionsForPresentation : MonoBehaviour
{
    [SerializeField] VHPEmotions _vhpEmotions;

    private Emotion emotion = Emotion.NO_EMOTION;

    [Range(0.0f, 5.0f)]
    [SerializeField]
    private float emotionShift = 0.05f;

    private enum Emotion
    {
        ANGER,
        DISGUST,
        FEAR,
        HAPPINESS,
        SADNESS,
        SURPRISE,
        NO_EMOTION
    }


    // Update is called once per frame
    void Update()
    {
        if (emotion == Emotion.ANGER)
        {
            if (_vhpEmotions.anger > 100)
            {
                emotion = Emotion.NO_EMOTION;
            }
            _vhpEmotions.anger = _vhpEmotions.anger + emotionShift;
        }
        else if (emotion == Emotion.DISGUST)
        {
            if (_vhpEmotions.disgust > 100)
            {
                emotion = Emotion.NO_EMOTION;
            }
            _vhpEmotions.disgust = _vhpEmotions.disgust + emotionShift;
        }
        else if (emotion == Emotion.FEAR)
        {
            if (_vhpEmotions.fear > 100)
            {
                emotion = Emotion.NO_EMOTION;
            }
            _vhpEmotions.fear = _vhpEmotions.fear + emotionShift;
        }
        else if (emotion == Emotion.HAPPINESS)
        {
            if (_vhpEmotions.happiness > 100)
            {
                emotion = Emotion.NO_EMOTION;
            }
            _vhpEmotions.happiness = _vhpEmotions.happiness + emotionShift;
        }
        else if (emotion == Emotion.SADNESS)
        {
            if (_vhpEmotions.sadness > 100)
            {
                emotion = Emotion.NO_EMOTION;
            }
            _vhpEmotions.sadness = _vhpEmotions.sadness + emotionShift;
        }
        else if (emotion == Emotion.SURPRISE)
        {
            if (_vhpEmotions.surprise > 100)
            {
                emotion = Emotion.NO_EMOTION;
            }
            _vhpEmotions.surprise = _vhpEmotions.surprise + emotionShift;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            //_vhpEmotions.anger = 100;
            emotion = Emotion.ANGER;
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            //_vhpEmotions.disgust = 100;
            emotion = Emotion.DISGUST;
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            //_vhpEmotions.fear = 100;
            emotion = Emotion.FEAR;
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            //_vhpEmotions.happiness = 100;
            emotion = Emotion.HAPPINESS;
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            //_vhpEmotions.sadness = 100;
            emotion = Emotion.SADNESS;
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            //_vhpEmotions.surprise = 100;
            emotion = Emotion.SURPRISE;
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            removeEmotions();
        }
    }

    private void removeEmotions()
    {
        _vhpEmotions.anger = 0;
        _vhpEmotions.disgust = 0;
        _vhpEmotions.fear = 0;
        _vhpEmotions.happiness = 0;
        _vhpEmotions.sadness = 0;
        _vhpEmotions.surprise = 0;
    }
}
