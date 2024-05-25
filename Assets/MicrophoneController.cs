using UnityEngine;
using System.IO;

public class MicrophoneController : MonoBehaviour
{
    private bool isRecording = false;
    private AudioClip audioClip;
    private string filePath;

    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            audioClip = Microphone.Start(null, false, 60, 44100); // Adjust length and frequency as needed
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
            if (Microphone.IsRecording(null))
            {
                Microphone.End(null);

                // Create directory if it doesn't exist
                string directoryPath = $"{Application.dataPath}/Recordings";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Save the audio clip to the directory
                string filePath = $"{directoryPath}/recording.wav";
                SavWav.SaveWav(filePath, audioClip);

                Debug.Log($"Saved recording to: {filePath}");
            }
            else
            {
                Debug.LogWarning("Microphone is not recording.");
            }
        }
        else
        {
            Debug.LogWarning("Not currently recording.");
        }
    }
}

