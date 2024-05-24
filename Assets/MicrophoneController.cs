using UnityEngine;
using System.Collections;
using System.IO;

public class MicrophoneController : MonoBehaviour
{
    private string microphoneDevice;
    private AudioClip recordedClip;
    private bool isRecording = false;

    void Start()
    {
        // Check if there's a microphone connected
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone device found.");
            return;
        }

        // Get the default microphone device
        microphoneDevice = Microphone.devices[0];
    }

    // Start recording from the microphone
    public void StartRecording()
    {
        if (!isRecording)
        {
            isRecording = true;
            recordedClip = Microphone.Start(microphoneDevice, false, 60, 44100);
            Debug.Log("Recording started...");
        }
        else
        {
            Debug.LogWarning("Already recording...");
        }
    }

    // Stop recording and save the audio clip as a WAV file
    public void StopRecordingAndSave(string filePath)
    {
        if (isRecording)
        {
            isRecording = false;
            Microphone.End(microphoneDevice);

            // Save the recorded clip as a WAV file
            SavWav.Save(filePath, recordedClip);
            Debug.Log("Recording stopped and saved at: " + filePath);
        }
        else
        {
            Debug.LogWarning("Not currently recording...");
        }
    }
}
