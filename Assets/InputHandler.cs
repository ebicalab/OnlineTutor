using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.IO;

public class InputHandler : MonoBehaviour {
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private APIController _apiController;
    [SerializeField] private AudioController _audioController;
    [SerializeField] private FirstPersonController _firstPersoncontroller;


    private bool isTyping = false;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            FocusInputField();
        }

        if (isTyping) {
            BlockMovementKeys();
        }
        else
            UnlockMovementKeys();

        if (isTyping && Input.GetKeyDown(KeyCode.Return)) {
            SubmitInput();
        }
        
    }

    private void FocusInputField() {
        _inputField.ActivateInputField();
        _inputField.Select();
        isTyping = true;
    }

    private async void SubmitInput() {
        isTyping = false;
        if (_inputField.text == "")
            return;
        _inputField.DeactivateInputField();
        string question = _inputField.text;
        _inputField.text = "";

        string jsonResponse = await _apiController.postRequestAsync(question, "text");
        Debug.Log("Received: " + jsonResponse);
        if (jsonResponse != null) {
            SpeechResponse speechResponse = JsonUtility.FromJson<SpeechResponse>(jsonResponse);
            var uploadFolderPath = $"{Application.dataPath}/Resources/Uploads";
            if (!Directory.Exists(uploadFolderPath)) {
                Directory.CreateDirectory(uploadFolderPath);
            }
            string name = $"recording_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.wav";
            string filePath = Path.Combine(uploadFolderPath, name);

            SaveWavToFile(speechResponse.audio_base64, filePath);

            _audioController.playShortSound(name);
        }
        else {
            Debug.LogWarning("Received null response from API.");
        }

    }

    private void SaveWavToFile(string base64, string filePath) {
        byte[] wavData = Convert.FromBase64String(base64);
        File.WriteAllBytes(filePath, wavData);
        Debug.Log($"WAV file saved to: {filePath}");
    }


    private void BlockMovementKeys() {
        //block movement
        _firstPersoncontroller.playerCanMove = false;
        _firstPersoncontroller.enableJump = false;
        _firstPersoncontroller.enableSprint = false;
    }
    private void UnlockMovementKeys(){
        //unlock
        _firstPersoncontroller.playerCanMove = true;
        _firstPersoncontroller.enableJump = true;
        _firstPersoncontroller.enableSprint = true;
    }

    [Serializable]
    public class SpeechResponse {
        public string audio_base64; 
    }
}
