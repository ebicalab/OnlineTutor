using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using System;
using System.Linq;

public class SpeechRecognizerController : MonoBehaviour
{
    public bool canAsk = false;
    [SerializeField] private UIController uiController;
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, int> actions = new Dictionary<string, int>();

    private void Awake() {
        //DontDestroyOnLoad(this.transform.gameObject);

        //actions.Add("first", 1);
        //actions.Add("second", 2);
        //actions.Add("third", 3);
        //actions.Add("fourth", 4);
        //actions.Add("fifth", 5);
        //actions.Add("sixth", 6);
        //actions.Add("seventh", 7);
        //actions.Add("eighth", 8);
        //actions.Add("ninth", 9);
        //actions.Add("tenth", 10);

        //keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        //keywordRecognizer.OnPhraseRecognized += RecognizedSpeech;

        //keywordRecognizer.Start();

    }

    private void RecognizedSpeech(PhraseRecognizedEventArgs speech)
    {
        Debug.LogError(speech.text);
        if (canAsk)
        {
            Messenger<int>.Broadcast(GameEvent.STUDENT_ASK_QUESTION, actions[speech.text]);
            uiController.HideQuestions();
            canAsk = false;
        }
    }
}