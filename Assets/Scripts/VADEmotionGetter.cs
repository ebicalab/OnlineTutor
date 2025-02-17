using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VADEmotionGetter : MonoBehaviour
{
    [Serializable]
    public class Emotion {
        public string name;
        public Vector3 vector;
    }

    public List<Emotion> basicEmotions = new List<Emotion>()
    {
        new Emotion { name = "happiness", vector = new Vector3(0.76f, 0.48f, 0.35f)},
        new Emotion { name = "sadness", vector = new Vector3(-0.63f, 0.27f, -0.33f) },
        new Emotion { name = "anger", vector = new Vector3(-0.43f, 0.67f, 0.34f) },
        new Emotion { name = "fear", vector = new Vector3(-0.63f, 0.27f, -0.33f) },
        new Emotion { name = "disgust", vector = new Vector3(-0.6f, 0.35f, 0.11f) },
        new Emotion { name = "surprise", vector = new Vector3(0.4f, 0.67f, -0.13f) },
        new Emotion { name = "neutral", vector = new Vector3(0f, 0f, 0f) }
    };

    private float maxMagnitude = Mathf.Sqrt(3);

    public (string emotionName, float intensity) GetEmotionAndIntensity(Vector3 vad) {
        string closestEmotion = "";
        float minDistance = float.MaxValue;

        foreach (var emotion in basicEmotions) {
            float distance = Vector3.Distance(vad, emotion.vector);
            if (distance < minDistance) {
                minDistance = distance;
                closestEmotion = emotion.name;
            }
        }
        float intensity = Mathf.Clamp01(vad.magnitude / maxMagnitude) * 100f;

        return (closestEmotion, intensity);
    }

    
}
