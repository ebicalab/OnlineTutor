using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mono.Data.Sqlite;


public class ScrollViewAdapter : MonoBehaviour {
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private RectTransform content;
    [SerializeField] private UIController uiController;


    private void Update() {
        if (Input.GetKeyDown(KeyCode.T)) {
            UpdateQuestions();
        }
    }

    private void Start() {
        uiController = this.gameObject.GetComponent<UIController>();
    }


    private class ButtonModel {
        private string buttonText;
        private int buttonId;

        public string ButtonText { get => buttonText; set => buttonText = value; }
        public int ButtonId { get => buttonId; set => buttonId = value; }
    }

    private class ButtonView {
        private Text buttonText;
        private Button clickButton;

        public Text ButtonText { get => buttonText; set => buttonText = value; }
        public Button ClickButton { get => clickButton; set => clickButton = value; }

        public ButtonView(Transform rootView) {
            buttonText = rootView.GetChild(0).GetComponent<Text>();
        }
    }

    public void UpdateQuestions() {
        GetItems(GameController.CurrentLessonNumber, GameController.CurrentSlideNumber, results => OnReceivedModels(results));
    }

    private void GetItems(int curentLesson, int currentSlide, System.Action<ButtonModel[]> callback) {

        using (var connection = new SqliteConnection(DBInfo.DataBaseName)) {
            connection.Open();

            using (var command = connection.CreateCommand()) {
                try {
                    var query = $@"
                        SELECT
	                        q.unique_number_through_lecture,
	                        q.text
                        FROM
	                        questions as q
                        INNER JOIN
	                        slides as sl
		                        ON q.slide_id = sl.id
                        INNER JOIN
	                        lessons as ls
		                        ON ls.id = sl.lesson_id
                        WHERE
	                        ls.number = {curentLesson}
	                        AND sl.number <= {currentSlide}
                        ORDER BY q.unique_number_through_lecture
                    ";

                    command.CommandText = query;
                    using (var reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            Dictionary<int, string> questions = new Dictionary<int, string>();
                            while (reader.Read()) {
                                questions.Add(Convert.ToInt32(reader["unique_number_through_lecture"]), (string)reader["text"]);
                            }
                            var results = new ButtonModel[questions.Keys.Count];
                            int i = 0;
                            foreach (var question in questions) {
                                results[i] = new ButtonModel();
                                results[i].ButtonId = question.Key;
                                results[i].ButtonText = question.Value;
                                i++;
                            }
                            callback(results);
                        }
                        else {
                            var res = new ButtonModel[0];
                            callback(res);
                        }
                    }
                }
                catch (Exception ex) {
                    Debug.LogError(ex);
                }
                finally {
                    connection.Close();
                }
            }
        }
    }

    void OnReceivedModels(ButtonModel[] models) {
        foreach (Transform child in content) {
           // Destroy(child.gameObject);
        }

        foreach (var model in models) {
            var instance = GameObject.Instantiate(buttonPrefab.gameObject) as GameObject;
            instance.transform.SetParent(content, false);
            InitializeButtonView(instance, model);
        }
    }

    void InitializeButtonView(GameObject viewGameObject, ButtonModel model) {
        ButtonView view = new ButtonView(viewGameObject.transform);
        view.ButtonText.text = model.ButtonText;
        view.ClickButton = viewGameObject.GetComponent<Button>();
        view.ClickButton.onClick.AddListener(delegate {
            Messenger<int>.Broadcast(GameEvent.STUDENT_ASK_QUESTION, model.ButtonId);
        });
        //view.ClickButton.onClick.AddListener( delegate { Debug.LogError("Button clicked {model.buttonId}"); });
        view.ClickButton.onClick.AddListener(uiController.HideQuestions);
    }
}
