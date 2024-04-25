using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class YandexSpeechKit {

    private static readonly string IAmToken = "t1.9euelZqPy5OJkJPLjcuQzI2Oys7Imu3rnpWalpGJzJCUmIqUmImSkZiUlY_l9PdYAhFb-e82GA3P3fT3GDEOW_nvNhgNz83n9euelZqQzszOkZCZkpyVnMmYy86Ymu_8xeuelZqQzszOkZCZkpyVnMmYy86Ymg.W-lMpeerJnTq7b5Ctdmf1NNjZLuiz6oP3ni-fzNZ_trd4ZrTrgoFPFuqqyQ0zFah1CQNExQqZxv3Pvp4NLLTDw";
    private static readonly string FolderId = "b1gs7puvlr7hqmmsjk4d";

    public static Action<byte []> onSpeechSynthesized;

    public static void TextToAudioFile(string text, string pathFromProjectFolder, string fileName, YSKVoice voice = YSKVoice.ERMIL, YSKLang lang = YSKLang.RU,
                                YSKEmotion emotion = YSKEmotion.NEUTRAL, YSKSpeed speed = YSKSpeed.x1, YSKAudioFormat audioFormat = YSKAudioFormat.MP3) {

        var newPath = Application.dataPath.Replace("/Assets", "") + pathFromProjectFolder;
        if (!Directory.Exists(newPath)) {
            Debug.LogError($"[YandexSpeechKit]: path [{newPath}] doesnt exist!");
            return;
        }

        SpeechToFile(text, newPath, fileName, voice, lang, emotion, speed, audioFormat);
        return;
    }

    public static void TextToSpeech(string text, YSKVoice voice = YSKVoice.ERMIL, YSKEmotion emotion = YSKEmotion.NEUTRAL,
                                        YSKSpeed speed = YSKSpeed.x1, YSKLang lang = YSKLang.RU) {
        Speech(text, voice, lang, emotion, speed);
        return;
    }

    public async static Task<string> SpeechToText(byte[] data, YSKLang lang = YSKLang.RU, YSKAudioFormat format = YSKAudioFormat.OggOpus) {
        HttpClient httpClient = new HttpClient();
        try {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + IAmToken);

            var parameters = new Dictionary<string, object>
            {
                    { "data", data },
                    { "lang", YSKLang.RU.getStringValue() },
                    { "format", format.getStringValue() },
                    { "folderId", FolderId }
            };
            string query = QueryString(parameters);

            ByteArrayContent byteContent = new ByteArrayContent(data);
            var response = await httpClient.PostAsync("https://stt.api.cloud.yandex.net/speech/v1/stt:recognize?" + query, byteContent);
            var recognizedString = await response.Content.ReadAsStringAsync();

            var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(recognizedString);

            if (!values.ContainsKey("result")) {
                return null;
            }

            var resultText = values["result"];
            Debug.LogError(resultText);
            return resultText;
        }
        catch (Exception ex) {
            Debug.LogError($"[YandexSpeechKit] SpeechToText error: {ex}");
            return "";
        }
        finally {
            httpClient.Dispose();
        }
    }

    private static string QueryString(IDictionary<string, object> dict) {
        var list = new List<string>();
        foreach (var item in dict) {
            list.Add(item.Key + "=" + item.Value);
        }
        return string.Join("&", list);
    }


    #region private speech methods

    private static async void SpeechToFile(string text, string path, string fileName, YSKVoice voice, YSKLang lang, YSKEmotion emotion,
                                                                YSKSpeed speed, YSKAudioFormat audioFormat) {

        HttpClient httpClient = new HttpClient();

        try {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + IAmToken);

            var values = new Dictionary<string, string>
            {
                    { "text", text },
                    { "lang", lang.getStringValue() },
                    { "speed", speed.getStringValue() },
                    { "emotion", emotion.getStringValue() },
                    { "voice", voice.getStringValue() },
                    { "format", audioFormat.getStringValue() },
                    { "folderId", FolderId },
            };

            if (audioFormat == YSKAudioFormat.PCM) values.Add("sampleRateHertz", "48000");

            var content = new FormUrlEncodedContent(values);

            var response = await httpClient.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);
            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            var loadedFileName = Path.Combine(path, fileName);
            File.WriteAllBytes(loadedFileName + ".mp3", responseBytes);
            Debug.LogError($"[YandexSpeechKit]: File {loadedFileName} was successfully recorded!");
        }
        catch (Exception ex) {
            Debug.LogError($"[YandexSpeechKit]: error: {ex}");
        }
        finally {
            httpClient.Dispose();
        }
    }

    private static async void Speech(string text, YSKVoice voice, YSKLang lang, YSKEmotion emotion,
                                                                    YSKSpeed speed) {

        HttpClient httpClient = new HttpClient();

        try {
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + IAmToken);

            var values = new Dictionary<string, string>
            {
                    { "text", text },
                    { "lang", lang.getStringValue() },
                    { "speed", speed.getStringValue() },
                    { "emotion", emotion.getStringValue() },
                    { "voice", voice.getStringValue() },
                    { "format", YSKAudioFormat.PCM.getStringValue() },
                    { "folderId", FolderId },
                    { "sampleRateHertz", "48000" },
            };

            var content = new FormUrlEncodedContent(values);

            var response = await httpClient.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize", content);
            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            Debug.LogError(Encoding.Default.GetString(responseBytes));
            //File.WriteAllBytes("testaudio2.mp3", responseBytes);
            onSpeechSynthesized?.Invoke(responseBytes);
        }
        catch (Exception ex) {
            Debug.LogError($"[YandexSpeechKit]: error: {ex}");
        }
        finally {
            httpClient.Dispose();
        }
    }

    #endregion
}

#region enums

public enum YSKLang {
    [Description("ru-RU")]
    RU,
    [Description("en-US")]
    EN
}

public enum YSKVoice {
    [Description("jane")]
    JANE,
    [Description("alena")]
    ALENA,
    [Description("filipp")]
    FILIPP,
    [Description("ermil")]
    ERMIL,
    [Description("madirus")]
    MADIRUS,
    [Description("omazh")]
    OMAZH,
    [Description("zahar")]
    ZAHAR
}

public enum YSKEmotion {
    [Description("neutral")]
    NEUTRAL,
    [Description("good")]
    GOOD,
    [Description("evil")]
    EVIL
}

public enum YSKSpeed {
    [Description("1.0")]
    x1,
    [Description("1.5")]
    x1_5,
    [Description("2.0")]
    x2_0,
    [Description("0.5")]
    x0_5
}

public enum YSKAudioFormat {
    [Description("oggopus")]
    OggOpus,
    [Description("lpcm")]
    PCM,
    [Description("mp3")]
    MP3
}

public enum YSKStorageMode {
    FILE,
    MEMORY
}


public static class MyEnumExtensions {
    public static string getStringValue(this YSKAudioFormat val) {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
    public static string getStringValue(this YSKSpeed val) {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
    public static string getStringValue(this YSKEmotion val) {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
    public static string getStringValue(this YSKLang val) {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
    public static string getStringValue(this YSKVoice val) {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
}

#endregion