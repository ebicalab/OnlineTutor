using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;




public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public static string DefaultMicroName { get; set; }
    public static AudioClip MicAudioClip { get; set; }
    private static bool canRecordMicro = true;
    [Range(0.0f, 0.1f)] [SerializeField] private float minLoudThreeshold = 0.01f;
    [SerializeField] private static int sampleWindow = 10000;

    private string directoryPath;



    



    public void StartMicrophoneRecord() {
        StartCoroutine(StartMicrophoneRecordCoroutine());
    }   

    private IEnumerator StartMicrophoneRecordCoroutine() {
        yield return new WaitForSeconds(2.0f);
        MicAudioClip = Microphone.Start(DefaultMicroName, true, 100, 44100);
        StartCoroutine(checkMicroLoudness(DefaultMicroName, 2.0f, 1.0f, minLoudThreeshold));
    }

    private IEnumerator checkMicroLoudness(string microName, float startTimeOffset, float checkPeriod, float minLoudThreeshold) {
        yield return new WaitForSeconds(startTimeOffset);
        while (Microphone.IsRecording(DefaultMicroName)) {
            float averageLoudness = GetAverageLoudnessFromClipInLastSampleWindow(Microphone.GetPosition(microName), MicAudioClip, sampleWindow);
            Debug.LogError("average loudness = " + averageLoudness);
            if (averageLoudness < minLoudThreeshold) {
                Debug.LogError("Average loudness - " + averageLoudness + ", threeshold = " + this.minLoudThreeshold);
                Microphone.End(null);
                Messenger<AudioClip>.Broadcast(GameEvent.STUDENT_AUDIO_RECORD_FINISHED, MicAudioClip);
                break;
            }
            yield return new WaitForSeconds(checkPeriod);
        }
    }

    public float GetAverageLoudnessFromClipInLastSampleWindow(int clipPosition, AudioClip clip, int sampleWindow) {
        var startPosition = clipPosition - sampleWindow + 1;
        if (startPosition < 0) {
            return 0;
        }

        var waveData = new float[sampleWindow * clip.channels];
        clip.GetData(waveData, startPosition);

        return waveData.Select(x => System.Math.Abs(x))
                        .ToArray()
                        .Average();
    }

    private void Start() {
        directoryPath = $"{Application.dataPath}/Resources/Uploads";
        if (Microphone.devices.Length <= 0) {
            Debug.LogError("�� ��������� �� ���� ��������!");
        } else {
            DefaultMicroName = Microphone.devices[0];
        }
    }

    public void playShortSound(AudioClip clip) {
        _audioSource.PlayOneShot(clip);
    }

    public void playShortSound(string path){
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.Refresh();
        AudioClip _clip = Resources.Load<AudioClip>("Uploads/"+fileNameWithoutExtension);
        if (_clip != null)
        {
            Debug.Log("AudioClip loaded successfully.");
            _audioSource.PlayOneShot(_clip);
        }
        else
        {
           Debug.LogError("Failed to load AudioClip. Check the path and ensure the file is located in a Resources folder.");
        }
    }
     

    public void stopSound() {
        _audioSource.Stop();
    }

    public static float getClipLength(string path) {
        AudioClip clip = Resources.Load(path) as AudioClip;
        return clip.length;
    }

    public void setClipByPath(string pathToClip) {
        AudioClip clip = Resources.Load(pathToClip) as AudioClip;
        _audioSource.clip = clip;
        _audioSource.loop = false;
    }

    public void setClip(AudioClip clip) {
        _audioSource.clip = clip;
        _audioSource.loop = false;
    }

    public void PlayCurrentClip() {
        _audioSource.Play();
    }

    public float getCurrentClipLength() {
        if (_audioSource.clip != null) {
            return _audioSource.clip.length;
        }
        return 0.0f;
    }

    public void StopCurrentClip()
    {
        _audioSource.Stop();
    }

    public void setAudioSourceStartTime(float startTime) {
        _audioSource.time = startTime;
    }

    public void resetAudioSourceStartTime() {
        _audioSource.time = 0.0f;
    }

    public float getClipTime() {
        return _audioSource.time;
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
                    Debug.Log("All received audio deleted.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to delete received audio: " + ex.Message);
                }
            }

    }

     void OnDestroy()
     {
        Delete();
     }
}
