using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RestServer;
using System.IO;
using System;
using UnityEngine.Networking;



public class Endpoint : MonoBehaviour
{
    [SerializeField] private RestServer.RestServer _server;
    [SerializeField] private GameObject _student;
    [SerializeField] private GazeController _gazeController;
    [SerializeField] private GameObject _teacher;
    [SerializeField] private AudioController _audioController;
    [SerializeField] private SlideController _slideController;
    [SerializeField] private EmotionController _emotionController;
    [SerializeField] private WebcamController _webcamController;
    [SerializeField] private MicrophoneController _microphoneController;

    private List<string> _emotions;



    private string audioFolderPath;
    private string slideFolderPath;


    void Start()
    {

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/capture_audio", request =>
        {
            try{
                ThreadingHelper.Instance.ThreadingMillisecondsTimeout = 5000;
                var audioFilePath = ThreadingHelper.Instance.ExecuteSync(() =>
                {
                    return _microphoneController.GetMostRecentAudioFile();
                });
                if (audioFilePath != null) {
                    byte[] audioBytes = File.ReadAllBytes(audioFilePath);

                    request.CreateResponse().Status(200).Body(audioBytes).SendAsync();

                    _microphoneController.Delete();
                }
                else {
                    request.CreateResponse().Status(220).Body("0").SendAsync();
                    _microphoneController.Delete();
                }
            }
            catch (Exception ex) {
                request.CreateResponse().Status(500).Body("Error capturing audio").SendAsync();
            }
           
        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/location", request =>
        {
            try {
                ThreadingHelper.Instance.ThreadingMillisecondsTimeout = 5000;
                var position = ThreadingHelper.Instance.ExecuteSync(() => {
                    return _student.transform.position;
                });
                request.CreateResponse().Status(200).BodyJson(position).SendAsync(); 
                }


            catch (Exception ex) {
                request.CreateResponse().Status(500).Body("Error getting location").SendAsync();
            }

        });


        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/capture_photo", request =>
        {
            try {
                ThreadingHelper.Instance.ThreadingMillisecondsTimeout = 5000;
                var photoPath = ThreadingHelper.Instance.ExecuteSync(() => {
                    return _webcamController.CapturePhoto();
                });

                if (photoPath == null) {
                    throw new Exception("Failed to find photo");
                }

                byte[] photoBytes = File.ReadAllBytes(photoPath);
                request.CreateResponse().Body(photoBytes).SendAsync();
                _webcamController.Delete();
            }
            catch (Exception ex) {
                Debug.Log("Error getting image: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
        });



        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/is_looking/teacher", request =>
        {
        try{
            ThreadingHelper.Instance.ThreadingMillisecondsTimeout = 5000;
            bool isLooking = ThreadingHelper.Instance.ExecuteSync(() =>
            {
                return _gazeController.IsStudentLookingAtTeacher();
            });
            string isLookingString = isLooking ? "true" : "false";
            request.CreateResponse().Status(200).Body(isLookingString).SendAsync();
            }
            catch (Exception ex) {
                Debug.Log("Error getting is_looking/teacher: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
            
        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/is_looking/board", request =>
        {
            try{
                ThreadingHelper.Instance.ThreadingMillisecondsTimeout = 5000;
                bool isLooking = ThreadingHelper.Instance.ExecuteSync(() =>
                {
                    return _gazeController.IsStudentLookingAtBoard();
                });
                string isLookingString = isLooking ? "true" : "false";
                request.CreateResponse().Status(200).Body(isLookingString).SendAsync();
            }
            catch(System.Exception ex){
                Debug.Log("Error getting is_looking/board: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            } 
        });


        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/gaze_direction", request =>
        {
            try
            {
                if (string.IsNullOrEmpty(request.Body)) 
                    throw new System.Exception("No data provided. Please send a valid json");
        
                var value = request.JsonBody<Look>();

                if(value.look_direction>4||value.look_direction<1)
                    request.CreateResponse().Status(500).Body("Invalid input data");

                Debug.Log("Received gaze direction: " + value);

                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    Debug.Log("Setting gaze direction");
                    _gazeController.SetGazeDirectionDetermined(value.look_direction);
                });

                Debug.Log("Gaze direction set successfully");
                request.CreateResponse().Status(200).Body("Gaze direction set successfully").SendAsync();
            }
            catch (Exception ex)
            {
                Debug.Log("Error processing gaze direction request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
        });


        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/slide", request =>
        {
            try
            {
                var slide = request.BodyBytes;

                if (slide == null || slide.Length == 0)
                {
                    throw new Exception("No slide data received.");
                }


                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    slideFolderPath = $"{Application.dataPath}/Slidestemp";
                    if (!Directory.Exists(slideFolderPath))
                    {
                        Directory.CreateDirectory(slideFolderPath);
                    }


                    string filePath = Path.Combine(slideFolderPath, "slide.png");
                    File.WriteAllBytes(filePath, slide);

                    Debug.Log("slide received and saved: " + filePath);
                    _slideController.SetImage(filePath);


                });

                request.CreateResponse().Status(200).Body("slide received and saved.").SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing slide request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/upload_audio", request =>
        {
            try
            {
                var file = request.BodyBytes;

                if (file == null || file.Length == 0)
                {
                    throw new Exception("No audio data received.");
                }

                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _audioController.Delete();

                    var uploadFolderPath = $"{Application.dataPath}/Resources/Uploads";

                    if (!Directory.Exists(uploadFolderPath))
                    {
                        Directory.CreateDirectory(uploadFolderPath);
                    }

                    string name = $"recording_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.wav";
                    string filePath = Path.Combine(uploadFolderPath, name);

                    File.WriteAllBytes(filePath, file);


                    Debug.Log("Audio received and saved: " + filePath);




                    _audioController.StopCurrentClip();
                    _audioController.playShortSound(name);


                });

                request.CreateResponse().Status(200).Body("Audio received and saved.").SendAsync();
            }
            catch (Exception ex)
            {
                Debug.Log("Error processing audio upload request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
        });


      

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion", request => {
            try {
                var body = request.Body;
                if (string.IsNullOrEmpty(body)) 
                    throw new System.Exception("No data provided. Please send a valid emotion and intensity.");
        

                var json = JsonUtility.FromJson<EmotionRequest>(body);

                var allowedEmotions = new HashSet<string> { "anger", "disgust", "fear", "happiness", "sadness", "surprise" };

                if (!allowedEmotions.Contains(json.emotion)) {
                    throw new System.Exception("Invalid emotion. Allowed emotions are: anger, disgust, fear, happiness, sadness, surprise.");
                }

                if (json.intensity < 0 || json.intensity > 100) {
                    throw new System.Exception("Intensity must be between 0 and 100.");
                }

                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion(json.emotion, json.intensity);
                });

                request.CreateResponse().Status(200).Body("emotion set successfully").SendAsync();
            }
            catch (Exception ex) {
                Debug.Log("Error processing emotion request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
        });

        

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_text", request => {
            try {
                var body = request.Body;
                if (string.IsNullOrEmpty(body)) {
                    throw new Exception("No data provided. Please send a valid JSON object.");
                }

                SlideText json;
                try {
                    json = JsonUtility.FromJson<SlideText>(body);
                }
                catch {
                    throw new Exception("Invalid JSON format. Please ensure the JSON matches the expected structure.");
                }

                if (string.IsNullOrEmpty(json?.text)) {
                    throw new Exception("The 'text' field is required and cannot be empty.");
                }

                Debug.Log("Received text: " + json.text);
                ThreadingHelper.Instance.ExecuteAsync(() => {
                    _slideController.TextShow(json.text);
                });

                request.CreateResponse().Status(200).Body("Text sent successfully").SendAsync();
            }
            catch (Exception ex) {
                Debug.LogError("Error processing text request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();  
            }
        });

    }
 

    [System.Serializable]
    public class Look {
        public int look_direction;
    }


    [System.Serializable]
    public class SlideText {
        public string text;
    }

    [System.Serializable]
    public class EmotionRequest {
        public string emotion;
        public float intensity;
    }

}

