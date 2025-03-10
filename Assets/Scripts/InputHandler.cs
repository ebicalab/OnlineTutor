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
    [SerializeField] private SlideController _slideController;
    [SerializeField] private ExpandInputField _expandInputField;


    public bool isTyping = false;

    private void Update() {

        if(Input.GetKeyDown(KeyCode.F2)){
            if(!isTyping){
                FocusInputField();
                _expandInputField.ExpandInputFieldSize();
            }else{
                _inputField.DeactivateInputField();
                _expandInputField.ShrinkInputFieldSize();
                isTyping = false;
            }
        }
        //new line
        if (isTyping && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Return)) {
            AddNewLine();
        }
        //submit
        if (isTyping && Input.GetKeyDown(KeyCode.Return) && !Input.GetKey(KeyCode.LeftShift)) {
            SubmitInput();
        }
        //clear
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X)) {
            Clear();
            _inputField.DeactivateInputField();
            _expandInputField.ShrinkInputFieldSize();

        }
        if(isTyping)
            BlockMovementKeys();
        else
            UnlockMovementKeys();
        

    }

    private void FocusInputField() {
        _inputField.ActivateInputField();
        _inputField.Select();
        _inputField.caretPosition = _inputField.text.Length;
        isTyping = true;
    }

    private async void SubmitInput() {
        //isTyping = false;
        if (_inputField.text == "")
            return;

        _inputField.DeactivateInputField();
        string question = _inputField.text;
        //_inputField.text = "";

        string jsonResponse = await _apiController.postRequestAsync(question, "text");
        Debug.Log("Received: " + jsonResponse);
        if (jsonResponse != null) {
            SpeechResponse speechResponse = JsonUtility.FromJson<SpeechResponse>(jsonResponse);
            _slideController.TextShow(speechResponse.board);
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

    private void AddNewLine() {
        // Add a newline character to the input field text
        _inputField.text += "\n";
    }

    private void BlockMovementKeys() {
        // Block movement
        _firstPersoncontroller.playerCanMove = false;
        _firstPersoncontroller.enableJump = false;
        _firstPersoncontroller.enableSprint = false;
    }

    private void UnlockMovementKeys() {
        // Unlock movement
        _firstPersoncontroller.playerCanMove = true;
        _firstPersoncontroller.enableJump = true;
        _firstPersoncontroller.enableSprint = true;
    }

    private void Clear(){
        _inputField.text = "";
    }

    [Serializable]
    public class SpeechResponse {
        public string audio_base64;
        public string board;
    }
}
