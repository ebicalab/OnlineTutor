using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestController : MonoBehaviour
{
    
    
    [SerializeField] private SlideController slideController;
    [SerializeField] private EmotionController emotionController;
    [SerializeField] private AudioController audioController;
    [SerializeField] private VHPManager manager;
    [SerializeField] private WebcamController webcamController;
    [SerializeField] private MicrophoneController microphoneController;





   [SerializeField] private bool print_blendshapes = false;
    [SerializeField] private bool random_audio = false;
    [SerializeField] private bool random_emotion = false;
    [SerializeField] private bool random_slide = false;
    [SerializeField] private bool turn_off_slide = false;
    [SerializeField] private bool take_photo = false;
    [SerializeField] private bool start_record = false;





    private string pathToGreetings = "Music/GeneralSounds/Greetings";
    private int number = 0;
    private float[] m_BlendShapes;
    private int count_slide = 0;
    string path_images; 
    string[] imageFiles;



    bool is_recording = false;



    void Update()
    {
        if (random_audio)
            RandomAudio();
        if (random_emotion)
            RandomBlendshapes();
        if (random_slide)
            RandomSlide();
        if (turn_off_slide)
            TurnOffSlide();
        if (print_blendshapes)
            PrintBlendshapes();
        if (take_photo)
            Photo();

        if (start_record && !is_recording)
        {
            is_recording = true;
            StartRecording();
        }

        if (!start_record && is_recording)
        {
            StopRecordingAndSave();
            is_recording = false;
        }


    }
    private void Start()
    {
        // Set the path to the images folder using Application.dataPath
        path_images = Path.Combine(Application.dataPath, "Images");

        // Get the image files only when the path is set correctly
        if (Directory.Exists(path_images))
        {
            imageFiles = GetImageFiles(path_images);
        }
        else
        {
            Debug.LogError("Images folder not found at path: " + path_images);
        }
    }


    void RandomAudio()
    {
        // Stop the previous audio clip
        audioController.StopCurrentClip();

        // Play the new audio clip
        string path = pathToGreetings + $"/greetings_{number % 3 + 1}";
        audioController.playShortSound(path);
        number++;
        random_audio = false;
    }


    void RandomBlendshapes()
    {
        m_BlendShapes = new float[300];
        for (int i = 0; i < m_BlendShapes.Length; i++)
        {
            m_BlendShapes[i] = Random.Range(0.0f, 100.0f);
        }
        emotionController.SetBlendShapes(m_BlendShapes);
        random_emotion = false;
    }

    void RandomSlide()
    {
        random_slide = false;
        
        
        slideController.SetImage(imageFiles[count_slide%imageFiles.Length]);
        count_slide++;
        
    }

    string[] GetImageFiles(string folderPath)
    {
        // Supported image extensions
        string[] extensions = { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.tga" };
        // List to hold the image file paths
        var imageFiles = new System.Collections.Generic.List<string>();

        foreach (string extension in extensions)
        {
            // Get files with the current extension and add them to the list
            imageFiles.AddRange(Directory.GetFiles(folderPath, extension));
        }

        return imageFiles.ToArray();
    }

    void TurnOffSlide()
    {
        slideController.DisableProjector();
        turn_off_slide = false;
    }

    void PrintBlendshapes()
    {
        manager.PrintBlendShapeNames();
        print_blendshapes = false;
    }

    public void Photo()
    {
        string folderPath = Path.Combine(Application.dataPath, "CapturedPhotos");
        webcamController.StartPhotoCapture(folderPath);
        take_photo = false;

    }

    public void StartRecording()
    {
        if (microphoneController != null)
        {
            microphoneController.StartRecording();
        }
        else
        {
            Debug.LogWarning("MicrophoneController not found.");
        }
    }

    // Method to stop recording and save the audio file
    public void StopRecordingAndSave()
    {
        string folderPath = Path.Combine(Application.dataPath, "RecordedAudio");
        string filePath = Path.Combine(folderPath, "AudioRecord.wav");


        if (microphoneController != null)
        {
            microphoneController.StopRecordingAndSave(filePath);
        }
        else
        {
            Debug.LogWarning("MicrophoneController not found.");
        }
    }





}
