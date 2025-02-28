using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using UnityEngine.Networking;
using System.Threading.Tasks;




public class AudioController : MonoBehaviour {
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
        }
        else {
            DefaultMicroName = Microphone.devices[0];
        }
    }

    public void playShortSound(AudioClip clip) {
        _audioSource.PlayOneShot(clip);
    }

    public void playShortSound(string path) {
        StopCurrentClip();
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);

#if UNITY_EDITOR
        AssetDatabase.Refresh();
#endif

        AudioClip _clip = Resources.Load<AudioClip>("Uploads/" + fileNameWithoutExtension);

        if (_clip != null) {
            Debug.Log("AudioClip loaded successfully.");
            _audioSource.PlayOneShot(_clip);
        }
        else {
            Debug.LogError("Failed to load AudioClip. Check the path and ensure the file is located in a Resources folder.");
        }
    }

    public async Task PlayShortSoundAbsolute(string path) {
        StopCurrentClip();

        try {
            AudioClip _clip = await LoadClip(path);
            if (_clip == null)
                throw new Exception("Failed to load AudioClip.");

            _audioSource.PlayOneShot(_clip);

        }
        catch (Exception ex) {
            Debug.Log($"Error playing sound: {ex.Message}");
            throw;
        }
    }

    private async Task<AudioClip> LoadClip(string path) {
        string uriPath = GetPlatformSpecificPath(path);
        Debug.Log("Loading audio from URI: " + uriPath);

        string rawPath = new Uri(uriPath).LocalPath;
        Debug.Log($"Extracted raw path: {rawPath}");

        if (!File.Exists(rawPath)) {
            Debug.Log("File does not exist: " + rawPath);
            return null;
        }
        if (!IsValidAudioFile(rawPath)) {
            Debug.Log("File is not a valid audio file: " + rawPath);
            return null;
        }

        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uriPath, AudioType.WAV)) {
            uwr.SendWebRequest();

            try {
                while (!uwr.isDone) {
                    await Task.Delay(5);
                }

                if (uwr.result != UnityWebRequest.Result.Success) {
                    Debug.Log($"Error loading audio: {uwr.error}");
                    return null;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                if (clip != null) {
                    Debug.Log($"Loaded AudioClip: {clip.name} (Length: {clip.length}s)");
                }
                else {
                    Debug.Log("DownloadHandlerAudioClip returned null.");
                }
                return clip;
            }
            catch (Exception err) {
                Debug.Log($"Exception: {err.Message}");
                return null;
            }
        }
    }



    private string GetPlatformSpecificPath(string path) {
        string formattedPath = path.Replace("\\", "/");
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
            return "file:///" + formattedPath; // Windows: `file:///C:/...`
        }
        else {
            return "file://" + formattedPath; // Mac/Linux: `file:///Users/...`
        }
    }


    private bool IsValidAudioFile(string path) {
        try {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                if (fs.Length < 4) {
                    Debug.Log("File is too small to be a valid WAV file.");
                    return false;
                }

                byte[] header = new byte[4];
                fs.Read(header, 0, 4);

                string headerStr = System.Text.Encoding.ASCII.GetString(header);
                Debug.Log("File header: " + headerStr);

                if (headerStr == "RIFF") // Standard WAV file signature
                {
                    Debug.Log("Valid WAV file detected.");
                    return true;
                }
                else {
                    Debug.Log("Invalid WAV file header.");
                    return false;
                }
            }
        }
        catch (Exception ex) {
            Debug.Log($"Error validating audio file: {ex.Message}");
            return false;
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

    public void StopCurrentClip() {
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

    public void Delete() {
        if (Directory.Exists(directoryPath)) {
            try {
                DirectoryInfo di = new DirectoryInfo(directoryPath);
                foreach (FileInfo file in di.GetFiles()) {
                    file.Delete();
                }
                Debug.Log("All received audio deleted.");
            }
            catch (System.Exception ex) {
                Debug.LogError("Failed to delete received audio: " + ex.Message);
            }
        }

    }

    void OnDestroy() {
        Delete();
    }
}