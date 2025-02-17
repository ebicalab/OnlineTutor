using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RestServer;
using System.IO;
using System;
using UnityEngine.Networking;



public class EndpointZoom : MonoBehaviour {
    [SerializeField] private RestServer.RestServer _server;


    [SerializeField] private AudioController _audioController;

    [SerializeField] private EmotionController _emotionController;

    private VADEmotionGetter _vadEmotionGetter;

   private List<string> _emotions;



    private string audioFolderPath;
    private string slideFolderPath;

    void Awake() {
        _vadEmotionGetter = GetComponent<VADEmotionGetter>();
        if (_vadEmotionGetter == null) {
            Debug.LogError("VADEmotionGetter component not found on the same GameObject.");
        }
    }


    void Start() {

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

                ThreadingHelper.Instance.ExecuteAsync(() => {
                    _emotionController.SetEmotion(json.emotion, json.intensity);
                });

                request.CreateResponse().Status(200).Body("emotion set successfully").SendAsync();
            }
            catch (Exception ex) {
                Debug.Log("Error processing emotion request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/makeAddress", request => {
            try {
                var body = request.Body;
                if (string.IsNullOrEmpty(body))
                    throw new System.Exception("No data provided. Please send a valid emotion and intensity.");

                var json = JsonUtility.FromJson<AudioAdress>(body);

                ThreadingHelper.Instance.ExecuteAsync(async () => {
                    try {
                        _audioController.Delete();
                        if (!File.Exists(json.address))
                            throw new System.Exception("File not Found");


                        _audioController.StopCurrentClip();
                        await _audioController.PlayShortSoundAbsolute(json.address);

                        request.CreateResponse().Status(200).Body("Audio received").SendAsync();
                    }
                    catch (Exception ex) {
                        Debug.Log("Error processing audio: " + ex.Message);
                        request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
                    }
                });
            }
            catch (Exception ex) {
                Debug.Log("Error processing request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/VADInput", request =>
        {
            try {
                var body = request.Body;
                if (string.IsNullOrEmpty(body))
                    throw new System.Exception("No data provided. Please send valid VAD coordinates.");

                Vector3 vadCoordinates = JsonUtility.FromJson<Vector3>(body);

                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    try {
                        (string emotion, float intensity) = _vadEmotionGetter.GetEmotionAndIntensity(vadCoordinates);

                        _emotionController.SetEmotion(emotion, intensity);

                        request.CreateResponse()
                               .Status(200)
                               .Body("Emotion processed successfully.")
                               .SendAsync();
                    }
                    catch (Exception ex) {
                        Debug.Log("Error transforming VAD: " + ex.Message);
                        request.CreateResponse()
                               .Status(500)
                               .Body(ex.Message)
                               .SendAsync();
                    }
                });
            }
            catch (Exception ex) {
                Debug.Log("Error processing VAD: " + ex.Message);
                request.CreateResponse()
                       .Status(500)
                       .Body(ex.Message)
                       .SendAsync();
            }
        });

    }
    [System.Serializable]
    public class AudioAdress {
        public string address;
    }

    [System.Serializable]
    public class EmotionRequest {
        public string emotion;
        public float intensity;
    }

    //P.S. понимаю что говнокод ((((

}


