using UnityEngine;
using System.IO;
using System;


public class MicrophoneController : MonoBehaviour
{
    private bool isRecording = false;
    private AudioClip audioClip;
    private string filePath;
    private int sampleRate = 44100;
    private int maxRecordingDuration = 300; // Maximum duration for recording
    private int recordingStartPosition = 0;
    private string directoryPath;
    void Awake()
    {
        directoryPath = $"{Application.dataPath}/Recordings";
    }
    
    void Start()
    {
        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);
    }    


    void Update()
    {
         if (Input.GetKeyDown(KeyCode.T))
        {
            StartRecording();
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            StopRecording();
        }

    }



    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            audioClip = Microphone.Start(null, false, maxRecordingDuration, sampleRate);
            recordingStartPosition = Microphone.GetPosition(null);
            Debug.Log("Recording started");
        }
        else
        {
            Debug.LogWarning("Already recording.");
        }
    }

    public void StopRecording()
    {
        if (isRecording)
        {
            isRecording = false;
            int recordingEndPosition = Microphone.GetPosition(null);
            Microphone.End(null);

            if (recordingEndPosition > recordingStartPosition)
            {
                int recordingLength = recordingEndPosition - recordingStartPosition;
                AudioClip trimmedClip = TrimAudioClip(audioClip, recordingStartPosition, recordingLength);

            
                string filePath = $"{directoryPath}/recording_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.wav";
                SavWav.SaveWav(filePath, trimmedClip);

                Debug.Log($"Saved recording to: {filePath}");
                
            }
            else
            {
                Debug.LogWarning("Failed to get recording position.");
            }
        }
        else
        {
            Debug.LogWarning("Not currently recording.");
        }
    }

    private AudioClip TrimAudioClip(AudioClip clip, int startSample, int lengthSamples)
    {
        float[] data = new float[lengthSamples];
        clip.GetData(data, startSample);

        AudioClip trimmedClip = AudioClip.Create("TrimmedClip", lengthSamples, clip.channels, clip.frequency, false);
        trimmedClip.SetData(data, 0);
        return trimmedClip;
    }

    public string GetMostRecentAudioFile()
    {
        Debug.Log(directoryPath);
        if (Directory.Exists(directoryPath))
        {
            DirectoryInfo di = new DirectoryInfo(directoryPath);
            FileInfo[] files = di.GetFiles("*.wav");

            if (files.Length > 0)
            {
                Array.Sort(files, (x, y) => y.CreationTime.CompareTo(x.CreationTime)); // Sort files by creation time, newest first
                return files[0].FullName;
            }
        }
        return null;
    }

    public void Delete()
    {
         if (Directory.Exists(directoryPath))
            {
                try
                {
                    DirectoryInfo di = new DirectoryInfo(directoryPath);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    Debug.Log("All audio deleted.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to delete audio: " + ex.Message);
                }
            }

    }

     void OnDestroy()
     {
        Delete();
     }
}