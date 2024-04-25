using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;
using System;

public class EmotionsController : MonoBehaviour
{

    [SerializeField] VHPEmotions emotionsManager;
    [SerializeField] MoralSchema moralSchema;

    static Dictionary<string, Emotion> emotions = new Dictionary<string, Emotion>();

    private string JSON_PATH_ALL_EMOTIONS = Application.streamingAssetsPath + "\\Emotions.json";

    private float emotionTimer = 0.0f;

    public class Emotion
    {
        public double[] VIL;

        Emotion()
        {
            VIL = new double[3];
        }

        public void setVIL(double[] VIL)
        {
            VIL.CopyTo(this.VIL, 0);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            var emotion = getEmotion();
            Debug.Log($"emotion - {emotion}");
            setEmotion(emotion);
        }

        emotionTimer += Time.deltaTime;
        if (emotionTimer > 40) {
            string emotion = getEmotion();
            Debug.LogError("Teacher show " + emotion + " emotion in EmotionController");
            setEmotionForSomeSeconds(emotion, 3);
            moralSchema.makeIndependentActionByTeacher(emotion);
            emotionTimer = 0.0f;
        }
    }

    void Start()
    {
        setupEmotions();
    }

    private void setupEmotions()
    {
        emotions = JsonConvert.DeserializeObject<Dictionary<string, Emotion>>(File.ReadAllText(JSON_PATH_ALL_EMOTIONS));
    }

    public string getEmotion()
    {
        var feelings = moralSchema.getStudentFeelings();
        string answer = "";
        double min = 1000;
 
        foreach (var emotion in emotions)
        {
            double difference = 0;
            //Debug.LogError("Length: " + feelings.Length + "\t" + emotion.Value.VAD.Length);
            for (int i = 0; i < feelings.Length; i++) {
                difference += Math.Pow(feelings[i] - emotion.Value.VIL[i], 2);
            }
            //Debug.LogError(emotion.Key + " " + difference);
            if (difference < min) {
                min = difference;
                answer = emotion.Key;
            }
        }
        return answer;
    }

    public void setEmotion(string emotion, float emotionExtent = 45f)
    {
        Debug.Log($"set emotion - {emotion}");
        switch (emotion)
        {
            case "Anger":
                emotionsManager.anger = emotionExtent;
                break;
            case "Disgust":
                emotionsManager.disgust = emotionExtent;
                break;
            case "Fear":
                emotionsManager.fear = emotionExtent;
                break;
            case "Happiness":
                emotionsManager.happiness = emotionExtent;
                break;
            case "Sadness":
                emotionsManager.sadness = emotionExtent;
                break;
            case "Surprise":
                emotionsManager.surprise = emotionExtent;
                break;
            case "Neutral":
                resetEmotions();
                break;
            default:
                break;
        }
    }

    public void setEmotionForSomeSeconds(string emotion, int seconds, float emotionExtent = 40f) {
        StartCoroutine(setEmotionForSomeSecondsCoroutine(emotion, seconds, emotionExtent));
    }

    private IEnumerator setEmotionForSomeSecondsCoroutine(string emotion, int seconds, float emotionExtent = 40f) {
        Debug.LogError("Set emotion");
        switch (emotion) {
            case "Anger":
                emotionsManager.anger = emotionExtent;
                break;
            case "Disgust":
                emotionsManager.disgust = emotionExtent;
                break;
            case "Fear":
                emotionsManager.fear = emotionExtent;
                break;
            case "Happiness":
                emotionsManager.happiness = emotionExtent;
                break;
            case "Sadness":
                emotionsManager.sadness = emotionExtent;
                break;
            case "Surprise":
                emotionsManager.surprise = emotionExtent;
                break;
            case "Neutral":
                resetEmotions();
                break;
            default:
                break;
        }
        yield return new WaitForSeconds(seconds);
        Debug.LogError("reset emotions");
        resetEmotions();
    }

    public void resetEmotions()
    {
        emotionsManager.anger = 0;
        emotionsManager.disgust = 0;
        emotionsManager.fear = 0;
        emotionsManager.happiness = 0;
        emotionsManager.sadness = 0;
        emotionsManager.surprise = 0;
    }
}
