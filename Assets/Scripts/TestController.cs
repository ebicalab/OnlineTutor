using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TestController : MonoBehaviour
{
    [SerializeField] private AudioController audioController;
    [SerializeField] private bool random_audio = false;
    [SerializeField] private EmotionController emotionController;
    private string pathToGreetings = "Music/GeneralSounds/Greetings";
    private int number = 0;
    [SerializeField] private bool random_emotion = false;
    private float[] m_BlendShapes;
    [SerializeField] private bool random_slide = false;
    [SerializeField] private SlideController slideController;
    private int count_slide = 0;
    [SerializeField] private bool turn_off_slide = false;


    string path_images; // Define the path variable here

    string[] imageFiles;

    void Update()
    {
        if (random_audio)
        {
            RandomAudio();
        }

        if (random_emotion)
        {
            RandomBlendshapes();
        }
        if (random_slide)
        {
            RandomSlide();
        }
        if (turn_off_slide)
        {
            TurnOffSlide();
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



}
