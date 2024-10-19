using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System;

public class MicrophoneController2 : MonoBehaviour
{
    [SerializeField] private APIController _apiController;
    [SerializeField] private AudioController _audioController;

    private bool isRecording = false;
    private AudioClip audioClip;
    private int sampleRate = 44100;
    private int maxRecordingDuration = 300;
    private int recordingStartPosition = 0;

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

    public async void StopRecording()  // Make this method async
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

                string clipBase64 = ACToBS64(trimmedClip);

                // Await the async task
                string response = _apiController.SendAudioToSpeechAPI(clipBase64);//SendAudioToSpeechAPI
                AudioClip resultClip = Base64ToAudioClip(response, "response");

                // Play the result
                _audioController.playShortSound(resultClip);
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

    public string ACToBS64(AudioClip audioClip)
    {
        float[] data = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(data, 0);

        byte[] bytes = new byte[data.Length * 2];  // 16-bit PCM
        int index = 0;
        foreach (float sample in data)
        {
            short convertedSample = (short)(sample * short.MaxValue);
            BitConverter.GetBytes(convertedSample).CopyTo(bytes, index);
            index += 2;
        }

        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + bytes.Length);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16);  // Subchunk1Size
                writer.Write((ushort)1);  // AudioFormat (PCM)
                writer.Write((ushort)audioClip.channels);
                writer.Write(audioClip.frequency);
                writer.Write(audioClip.frequency * audioClip.channels * 2);  // ByteRate
                writer.Write((ushort)(audioClip.channels * 2));  // BlockAlign
                writer.Write((ushort)16);  // BitsPerSample
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
            byte[] wavBytes = stream.ToArray();
            return Convert.ToBase64String(wavBytes);
        }
    }

    public static AudioClip Base64ToAudioClip(string base64, string clipName)
    {
        byte[] wavData = Convert.FromBase64String(base64);
        return ConvertWavToAudioClip(wavData, clipName);
    }

    private static AudioClip ConvertWavToAudioClip(byte[] wavData, string clipName)
    {
        int channelCount = 2;  // Assuming stereo
        int sampleRate = 44100;  // Change as necessary
        int sampleCount = wavData.Length / 4;  // 16-bit PCM (2 bytes per sample)
        float[] samples = new float[sampleCount];

        Buffer.BlockCopy(wavData, 44, samples, 0, wavData.Length - 44);  // Skip the header

        AudioClip audioClip = AudioClip.Create(clipName, sampleCount, channelCount, sampleRate, false);
        audioClip.SetData(samples, 0);
        return audioClip;
    }
}