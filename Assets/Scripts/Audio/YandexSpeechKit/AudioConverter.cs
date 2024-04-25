using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class AudioConverter
{

    #region Debug properties

    public static bool showDebugMessages = false;

    #endregion

    #region public methods

    public static AudioClip Convert(byte[] pcmBinary) {
        if (showDebugMessages) Debug.Log("[AudioConverter] starting: " + pcmBinary.Length);
        Debug.Log($"[AudioConverter]: get bytes. Length - {pcmBinary.Length}");

        return CreateAudioClip(pcmBinary);
    }

    public static AudioClip Convert(string fullFileName) {
        if (showDebugMessages) Debug.Log("[AudioConverter] starting: " + fullFileName);

        return CreateAudioClip(File.ReadAllBytes(fullFileName));
    }

    public static byte[] ConvertClipToOGG(AudioClip clip) {
        return OggVorbis.VorbisPlugin.GetOggVorbis(clip);
    }

    #endregion


    #region private methods 

    private static AudioClip CreateAudioClip(byte[] pcmBinary) {
        float[] audioBinary = PCM2Floats(pcmBinary);

        var audioClip = AudioClip.Create("MyPlayback", audioBinary.Length, 1, 48000, false);
        audioClip.SetData(audioBinary, 0);

        return audioClip;
    }

    private static float[] PCM2Floats(byte[] bytes) {
        float max = -(float)System.Int16.MinValue;
        float[] samples = new float[bytes.Length / 2];

        for (int i = 0; i < samples.Length; i++) {
            short int16sample = System.BitConverter.ToInt16(bytes, i * 2);
            samples[i] = (float)int16sample / max;
        }

        return samples;
    }

    #endregion
}
