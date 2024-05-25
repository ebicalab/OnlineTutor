using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public static string DefaultMicroName { get; set; }
    public static AudioClip MicAudioClip { get; set; }
    private static bool canRecordMicro = true;
    [Range(0.0f, 0.1f)] [SerializeField] private float minLoudThreeshold = 0.01f;
    [SerializeField] private static int sampleWindow = 10000;


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
        if (Microphone.devices.Length <= 0) {
            Debug.LogError("Не подключен ни один микрофон!");
        } else {
            DefaultMicroName = Microphone.devices[0];
        }
    }

    public void playShortSound(AudioClip clip) {
        _audioSource.PlayOneShot(clip);
    }

    public void playShortSound(string path) {
        AudioClip _clip = Resources.Load(path) as AudioClip;
        _audioSource.PlayOneShot(_clip);
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
}
