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
    

    private string audioFolderPath;
    private string slideFolderPath;
    

    void Start()
    {
        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/location", request =>
        {
            var position = ThreadingHelper.Instance.ExecuteSync(() =>
            {
                return _student.transform.position;
            });
            request.CreateResponse().BodyJson(position).SendAsync();
        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/is_looking/teacher", request =>
        {
            bool isLooking = ThreadingHelper.Instance.ExecuteSync(() =>
            {
                return _gazeController.IsStudentLookingAtTeacher();
            });
            string isLookingString = isLooking ? "true" : "false";
            request.CreateResponse().Body(isLookingString).SendAsync();
        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/is_looking/board", request =>
        {
            bool isLooking = ThreadingHelper.Instance.ExecuteSync(() =>
            {
                return _gazeController.IsStudentLookingAtBoard();
            });
            string isLookingString = isLooking ? "true" : "false";
            request.CreateResponse().Body(isLookingString).SendAsync();
        });


        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/gaze_direction", request =>
        {
            try
            {
                var gazeDirection = request.JsonBody<Vector3>();
                Debug.Log("Received gaze direction: " + gazeDirection);
                
                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    Debug.Log("Setting gaze direction");
                    _gazeController.SetGazeDirection(gazeDirection);
                });
                
                Debug.Log("Gaze direction set successfully");
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
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
                    throw new System.Exception("No slide data received.");
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

                request.CreateResponse().Body("slide received and saved.").SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing slide request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }
        });


        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/blendshapes_names", request =>
        {
            var position = ThreadingHelper.Instance.ExecuteSync(() =>
            {
                return _emotionController.Names();
            });
            request.CreateResponse().Body(position).SendAsync();
        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.GET, "/blendshapes_values", request =>
        {
            var position = ThreadingHelper.Instance.ExecuteSync(() =>
            {
                return _emotionController.Values();
            });
            request.CreateResponse().Body(position).SendAsync();
        });

        
        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion/anger", request =>
        {
            try
            {
                var value = (float) Convert.ToDouble(request.Body);


                if (value < 0 || value > 100)
                {
                    throw new System.Exception("Value must be between 0 and 100.");
                }
                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion("anger", value);
                });
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing anger request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion/disgust", request =>
        {
            try
            {
                var value = (float) Convert.ToDouble(request.Body);


                if (value < 0 || value > 100)
                {
                    throw new System.Exception("Value must be between 0 and 100.");
                }
                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion("disgust", value);
                });
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing disgust request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion/fear", request =>
        {
            try
            {

                var value = (float) Convert.ToDouble(request.Body);

                


                if (value < 0 || value > 100)
                {
                    throw new System.Exception("Value must be between 0 and 100.");
                }
                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion("fear", value);
                });
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing fear request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion/happiness", request =>
        {
            try
            {
                var value = (float) Convert.ToDouble(request.Body);


                if (value < 0 || value > 100)
                {
                    throw new System.Exception("Value must be between 0 and 100.");
                }
                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion("happiness", value);
                });
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing happiness request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion/sadness", request =>
        {
            try
            {
                var value = (float) Convert.ToDouble(request.Body);


                if (value < 0 || value > 100)
                {
                    throw new System.Exception("Value must be between 0 and 100.");
                }
                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion("sadness", value);
                });
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing sadness request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion/surprise", request =>
        {
            try
            {
                var value = (float) Convert.ToDouble(request.Body);


                if (value < 0 || value > 100)
                {
                    throw new System.Exception("Value must be between 0 and 100.");
                }
                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion("surprise", value);
                });
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing surprise request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion/surprise", request =>
        {
            try
            {
                var value = (float) Convert.ToDouble(request.Body);


                if (value < 0 || value > 100)
                {
                    throw new System.Exception("Value must be between 0 and 100.");
                }
                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion("surprise", value);
                });
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing surprise request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_emotion/neutral", request =>
        {
            try
            {
                var value = 0;  

                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    _emotionController.SetEmotion("neutral", value);
                });
                request.CreateResponse().SendAsync();
            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing surprise request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

        _server.EndpointCollection.RegisterEndpoint(HttpMethod.POST, "/set_blendshapes", request =>
        {
            try
            {
                string value = request.Body; 


                //if (!checkCorrect(value))
                 //  throw new System.Exception("The sent data is incorrect");

                ThreadingHelper.Instance.ExecuteAsync(() =>
                {
                    //(float) Convert.ToDouble(request.Body)
                    string[] s = value.Split(' ');
                    float[]values = new float[s.Length];
                    for(int i=0;i<s.Length;i++)
                        values[i] = (float) Convert.ToDouble(s[i]);

                    
                    _emotionController.SetBlendShapes(values);
                });
                request.CreateResponse().SendAsync();

            }
            catch (System.Exception ex)
            {
                Debug.Log("Error processing surprise request: " + ex.Message);
                request.CreateResponse().Status(500).Body(ex.Message).SendAsync();
            }

        });

    }
    
       
}