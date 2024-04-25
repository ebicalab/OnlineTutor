using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mono.Data.Sqlite;


public class TestingModule : MonoBehaviour {

    [SerializeField] private UIController uiController;

    [SerializeField] private RectTransform content;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Text emptyContentRowPrefab;
    [SerializeField] private EmotionsController emotionsController;

    private Dictionary<int, GameObject> questionIdToQuestionGameObj = new Dictionary<int, GameObject>();
    private Dictionary<int, List<GameObject>> questionIdToAnswerGameObjs = new Dictionary<int, List<GameObject>>();
    

    private int absoluteScore = 0;
    public int RelativeScoreInPercent { get; private set; } = 0;


    private void Start() {
        uiController = this.gameObject.GetComponent<UIController>();
    }


    public void UpdateQuestions() {
        clearScrollContent();

        List<Question> questions;
        if (GameController.CurrentLessonNumber > 3) {
            questions = getLessonQuestions(1);
        } else {
            questions = getLessonQuestions(GameController.CurrentLessonNumber);
        }
        
        //if (questions != null) {
            renderTestQuestionsAndAnswers(questions);
        //}
    }

    void renderTestQuestionsAndAnswers(List<Question> questions) {

        int questionNumber = 1;

        if (questions == null || questions.Count == 0) {
            var closeButtonInstanceWithNoQuestionsLecture = GameObject.Instantiate(buttonPrefab.gameObject) as GameObject;
            closeButtonInstanceWithNoQuestionsLecture.transform.SetParent(content, false);
            closeButtonInstanceWithNoQuestionsLecture.transform.GetChild(0).GetComponent<Text>().text = "Закрыть";
            closeButtonInstanceWithNoQuestionsLecture.GetComponent<Button>().onClick.AddListener(delegate {
                uiController.HideTestingScrollViewAdapter();
                RelativeScoreInPercent = 100;
                Messenger<int>.Broadcast(GameEvent.STUDENT_FINISHED_TESTING_MODULE, RelativeScoreInPercent);
            });

            return;
        }

        foreach (var question in questions) {

            List<QuestionAnswer> answers = getQuestionAnswers(question.Id);

            if (answers == null) break;

            questionIdToAnswerGameObjs.Add(question.Id, new List<GameObject>());

            var questionInstance = GameObject.Instantiate(buttonPrefab.gameObject) as GameObject;
            questionInstance.transform.SetParent(content, false);
            InitializeQuestionView(questionInstance, question, questionNumber);

            questionIdToQuestionGameObj.Add(question.Id, questionInstance);

            int answerNumber = 1;

            foreach (var answer in answers) {
                var answerInstance = GameObject.Instantiate(buttonPrefab.gameObject) as GameObject;
                answerInstance.transform.SetParent(content, false);
                InitializeQuestionAnswerView(answerInstance, answer, questionNumber, answerNumber);

                questionIdToAnswerGameObjs[answer.QuestionId].Add(answerInstance);
                answerNumber++;
            }

            var emptyContentRow = GameObject.Instantiate(emptyContentRowPrefab.gameObject) as GameObject;
            emptyContentRow.transform.SetParent(content, false);
            questionNumber++;

        }

        var closeButtonInstance = GameObject.Instantiate(buttonPrefab.gameObject) as GameObject;
        closeButtonInstance.transform.SetParent(content, false);
        closeButtonInstance.transform.GetChild(0).GetComponent<Text>().text = "Закрыть";
        closeButtonInstance.GetComponent<Button>().onClick.AddListener(delegate {
            uiController.HideTestingScrollViewAdapter();
            RelativeScoreInPercent = (int)(((float)absoluteScore / questionIdToQuestionGameObj.Keys.Count) * 100);
            Messenger<int>.Broadcast(GameEvent.STUDENT_FINISHED_TESTING_MODULE, RelativeScoreInPercent);
        });

        return;

    }

    void InitializeQuestionAnswerView(GameObject viewGameObject, QuestionAnswer answerModel, int questionNumber, int answerNumber) {

        var answerButtonText = viewGameObject.transform.GetChild(0).GetComponent<Text>();
        //answerButtonText.text = questionNumber + "." + answerNumber + " " + answerModel.Text;
        answerButtonText.text = "•  " + answerModel.Text;

        var answerClickButton = viewGameObject.GetComponent<Button>();
        //answerClickButton.onClick.AddListener(delegate {
        //    YandexSpeechKit.TextToSpeech(answerModel.Text, YSKVoice.ERMIL, YSKEmotion.NEUTRAL);
        //});

        answerClickButton.onClick.AddListener(delegate {

            if (answerModel.Correct == "x") {
                YandexSpeechKit.TextToSpeech("Правильный ответ. Отличная работа!", YSKVoice.ERMIL, YSKEmotion.GOOD);
                emotionsController.setEmotionForSomeSeconds("Happiness", 3, 65);
                absoluteScore++;
            }

            foreach (var answerGameObj in questionIdToAnswerGameObjs[answerModel.QuestionId]) {
                var buttonInstance = answerGameObj.GetComponent<Button>();
                buttonInstance.interactable = false;
            }

            questionIdToQuestionGameObj[answerModel.QuestionId].GetComponent<Button>().interactable = false;
        });
    }



    private List<QuestionAnswer> getQuestionAnswers(int questionId) {
        using (var connection = new SqliteConnection(DBInfo.DataBaseName)) {

            connection.Open();

            using (var command = connection.CreateCommand()) {
                try {
                    var query = $@"
                        SELECT
	                        answ.id,
	                        answ.answer,
                            answ.question_id,
                            answ.correct
                        FROM
	                        test_answers as answ
                        INNER JOIN
	                        test_questions as q
		                        ON q.id = answ.question_id
                        WHERE
	                        q.id = {questionId}
                    ";

                    command.CommandText = query;
                    using (var reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            List<QuestionAnswer> answers = new List<QuestionAnswer>();
                            while (reader.Read()) {
                                QuestionAnswer answer = new QuestionAnswer();
                                answer.Id = Convert.ToInt32(reader["id"]);
                                answer.Text = (string) reader["answer"];
                                answer.QuestionId = Convert.ToInt32(reader["question_id"]);
                                answer.Correct = (string) reader["correct"];
                                answers.Add(answer);
                            }
                            return answers;
                        }
                        else {
                            return null;
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogError(ex);
                    return null;
                }
                finally {
                    connection.Close();
                }
            }
        }
    }

    

    void InitializeQuestionView(GameObject viewGameObject, Question questionModel, int questionNumber) {

        var questionButtonText = viewGameObject.transform.GetChild(0).GetComponent<Text>();
        questionButtonText.text = "Вопрос " + questionNumber + ". " + questionModel.Text;

        viewGameObject.transform.GetComponent<Image>().color = Color.green;

        var questionClickButton = viewGameObject.GetComponent<Button>();
        questionClickButton.onClick.AddListener(delegate {
            YandexSpeechKit.TextToSpeech(questionModel.Description, YSKVoice.ERMIL, YSKEmotion.NEUTRAL);
            emotionsController.setEmotionForSomeSeconds("Happiness", 4, 40);
        });
    }

    private void clearScrollContent() {
        foreach (Transform child in content) {
            Destroy(child.gameObject);
        }
    }

    private List<Question> getLessonQuestions(int curentLessonNumber) {

        using (var connection = new SqliteConnection(DBInfo.DataBaseName)) {

            connection.Open();

            using (var command = connection.CreateCommand()) {
                try {
                    var query = $@"
                        SELECT
	                        q.id,
	                        q.question,
                            q.lesson_id,
                            q.description
                        FROM
	                        test_questions as q
                        INNER JOIN
	                        lessons as ls
		                        ON ls.id = q.lesson_id
                        WHERE
	                        ls.number = {curentLessonNumber}
                    ";

                    command.CommandText = query;
                    using (var reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            List<Question> questions = new List<Question>();
                            while (reader.Read()) {
                                Question question = new Question();
                                question.Id = Convert.ToInt32(reader["id"]);
                                question.Text = (string) reader["question"];
                                question.LessonId = Convert.ToInt32(reader["lesson_id"]);
                                question.Description = (string) reader["description"];
                                questions.Add(question);
                            }
                            return questions;
                        }
                        else {
                            Debug.LogError("DB is empty for test questions");
                            return null;
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogError(ex);
                    return null;
                }
                finally {
                    connection.Close();
                }
            }
        }
    }

    private class Question {

        public int Id { get; set; }
        public int LessonId { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }

    }

    private class QuestionAnswer {

        public int Id { get; set; }
        public string Text { get; set; }
        public int QuestionId { get; set; }
        public string Correct { get; set; }

    }

    private class ButtonModel {

        public string ButtonText { get; set; }
        public int ButtonId { get; set; }

    }

    private class ButtonView {

        public Text ButtonText { get; set; }
        public Button ClickButton { get; set; }

        public ButtonView(Transform rootView) {
            ButtonText = rootView.GetChild(0).GetComponent<Text>();
        }
    }
}
