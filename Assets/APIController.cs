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
using System.IO;


[System.Serializable]
public class SpeechResponse {
    public string audio_base64;

    public SpeechResponse(string audioBase64) {
        audio_base64 = audioBase64;
    }
}

[System.Serializable]
public class ParametersRequest {
    public Vector3 position; 
    public bool is_looking_teacher; 
    public bool is_looking_board;
    public string photo_base64;

    public ParametersRequest(Vector3 position, string photoBase64, bool isLookingTeacher, bool isLookingBoard) {
        this.position = position;
        this.photo_base64 = photoBase64;
        this.is_looking_teacher = isLookingTeacher;
        this.is_looking_board = isLookingBoard;
    }
}

[System.Serializable]
public class ParametersResponse { //look_direction, emotion, intensity
    public int look_direction;
    public string emotion;
    public float intensity;
}


public class APIController : MonoBehaviour {
    [SerializeField] private string URL = "http://127.0.0.1:5000";
    [SerializeField] private AudioController _audioController;
    [SerializeField] private GameObject _student;
    [SerializeField] private GazeController _gazeController;
    [SerializeField] private WebcamController _webcamController;
    [SerializeField] private EmotionController _emotionController;



    void Start() {
        StartCoroutine(SendParametersEveryFourSeconds());
    }


    public Task<string> postRequestAsync(string audioBase64) {
        var tcs = new TaskCompletionSource<string>();

        StartCoroutine(SendRequest(audioBase64, tcs));

        return tcs.Task;
    }

    private IEnumerator SendRequest(string audioBase64, TaskCompletionSource<string> tcs) {
        SpeechResponse requestData = new SpeechResponse(audioBase64);
        string jsonData = JsonUtility.ToJson(requestData);

        using (var uwr = new UnityWebRequest(URL + "/speech", "POST")) {
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonData);
            uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
            uwr.downloadHandler = new DownloadHandlerBuffer();
            uwr.SetRequestHeader("Content-Type", "application/json");

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success) {
                Debug.Log("Error While Sending: " + uwr.error);
                tcs.SetResult(null);

            }
            else {
       
                tcs.SetResult(uwr.downloadHandler.text);
            }
        }
    }

    void Update() {
        
    }

    private IEnumerator SendParametersEveryFourSeconds() {
        while (true) {
            Vector3 studentPosition = _student.transform.position;
           
            string photoPath = _webcamController.CapturePhoto();

            if (string.IsNullOrEmpty(photoPath)) {
                Debug.Log("Failed to capture photo. Skipping this iteration.");
                yield return new WaitForSeconds(1f);
                continue; 
            }

            string photoBase64 = ConvertPngToBase64(photoPath);
            bool isLookingTeacher = _gazeController.IsStudentLookingAtTeacher();
            bool isLookingBoard = _gazeController.IsStudentLookingAtBoard();

            ParametersRequest parametersRequest = new ParametersRequest(studentPosition, photoBase64, isLookingTeacher, isLookingBoard);
            string jsonData = JsonUtility.ToJson(parametersRequest);
            Debug.Log("We want to send!:"+jsonData);

            using (var uwr = new UnityWebRequest(URL + "/parameters", "POST")) {
                byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonData);
                uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
                uwr.downloadHandler = new DownloadHandlerBuffer();
                uwr.SetRequestHeader("Content-Type", "application/json");

                yield return uwr.SendWebRequest();

                if (uwr.result != UnityWebRequest.Result.Success) {
                    Debug.Log("Error While Sending Parameters: " + uwr.error);
                }
                else {
                    string responseText = uwr.downloadHandler.text;
                    Debug.Log("Parameters sent successfully: " + responseText);

                    ParametersResponse response = JsonUtility.FromJson<ParametersResponse>(responseText);
                    
                    _emotionController.SetEmotion(response.emotion, response.intensity);
                    _gazeController.SetGazeDirectionDetermined(response.look_direction);
                    if (response != null) {
                        Debug.Log("No Response");
                    }
                    else {
                        Debug.LogError("Failed to deserialize response.");
                    }
                }
            }

            yield return new WaitForSeconds(1f); // Wait for 4 seconds before sending again
        }
    }


    public string ConvertPngToBase64(string filePath) {
        byte[] imageBytes = File.ReadAllBytes(filePath);

        string base64String = Convert.ToBase64String(imageBytes);

        return base64String;
    }


}