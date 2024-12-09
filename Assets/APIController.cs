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

[Serializable]
public class Response {
    public string text_question;

    public Response(string data_init) {
        text_question = data_init;
    }
}



[Serializable]
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

[Serializable]
public class ParametersResponse {
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

    private bool isSpeechRequestInProgress = false;

    void Start() {
        StartCoroutine(SendParametersEveryFourSeconds());
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.F3)) {
            getReset();
        }

    }

    public void StartReset() {
        if (!isSpeechRequestInProgress) {
            getReset(); // Trigger reset logic
        }
        else {
            Debug.Log("Speech request in progress. Please wait.");
        }
    }

    public async Task<string> postRequestAsync(string data, string type) {
        if (isSpeechRequestInProgress) {
            Debug.Log("Speech request is already in progress.");
            return null;
        }

        isSpeechRequestInProgress = true;

        try {
            var tcs = new TaskCompletionSource<string>();

            StartCoroutine(SendRequest(data, type, tcs));

            return await tcs.Task;
        }
        finally {
            isSpeechRequestInProgress = false;
        }
    }


    private IEnumerator SendRequest(string data, string type, TaskCompletionSource<string> tcs) {

        Response requestData = new Response(data);
        string jsonData = JsonUtility.ToJson(requestData);
        using (var uwr = new UnityWebRequest(URL + "/" + type, "POST")) {
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
    public Task<string> getReset() {
        var tcs = new TaskCompletionSource<string>();
        StartCoroutine(Reset(tcs));
        return tcs.Task;
    }

    private IEnumerator Reset(TaskCompletionSource<string> tcs) {
        using (var uwr = new UnityWebRequest(URL + "/reset", "GET")) {
            yield return uwr.SendWebRequest();

            // Check for errors
            if (uwr.result != UnityWebRequest.Result.Success) {
                Debug.Log("Error While Resetting: " + uwr.error);
                tcs.SetResult(null);
            }
            else {
                Debug.Log("Memory reset successfully:");
            }
        }
    }




    // Sending parameters every 4 seconds
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
            Debug.Log("We want to send: " + jsonData);

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
                }
            }

            yield return new WaitForSeconds(4f); // Wait for 4 seconds before sending again
        }
    }

    // Convert PNG image to base64 string
    public string ConvertPngToBase64(string filePath) {
        byte[] imageBytes = File.ReadAllBytes(filePath);
        string base64String = Convert.ToBase64String(imageBytes);
        return base64String;
    }
}
