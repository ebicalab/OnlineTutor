using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System;

[System.Serializable]
public class SpeechResponse
{
    public string audio_base64;
}

public class APIController : MonoBehaviour
{
    string URL = "http://localhost:8000";

    [SerializeField] private AudioController _audioController;

    public Task<string> SendAudioToSpeechAPI(string audioBase64)
{
    var taskCompletionSource = new TaskCompletionSource<string>();

    string url = URL + "/speech";
    var requestData = new
    {
        audio_base64 = audioBase64
    };

    string jsonData = JsonUtility.ToJson(requestData);

    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.SendWebRequest().completed += (asyncOp) =>
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                SpeechResponse response = JsonUtility.FromJson<SpeechResponse>(jsonResponse);
                taskCompletionSource.SetResult(response.audio_base64);
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
                taskCompletionSource.SetResult(null);  
            }
        };
    }

    return taskCompletionSource.Task;
}

}