using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using UnityEditor;
using Models;
using Proyecto26;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public class SpeechResponse {
    public string audio_base64;
}

public class APIController : MonoBehaviour {
    [SerializeField] private string URL = "http://127.0.0.1:5000";
    [SerializeField] private AudioController _audioController;

    public string SendAudioToSpeechAPI(string audioBase64) {
        string url = URL + "/speech";
        var requestData = new {
            audio_base64 = audioBase64
        };

        string jsonData = JsonUtility.ToJson(requestData);

        var requestHelper = new RequestHelper {
            Uri = url,
            Method = "POST",
            Timeout = 10, 
            BodyString = jsonData,  
            ContentType = "application/json",
            EnableDebug = true  
        };

        RestClient.Post<SpeechResponse>(requestHelper)
            .Then(response => {
                // Success: Handle the response
                if (response != null) {
                    string audio= response.audio_base64;
                    Debug.Log("Audio base64 received: " + audioBase64);
                }
                else {
                    Debug.LogError("Failed to receive a valid response.");
                }
            })
            .Catch(error => {
                Debug.LogError($"Error sending audio data: {error.Message}");
            });
            return "hi";
    }
}