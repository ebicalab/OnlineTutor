using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    [SerializeField] private GameObject QuestionsScrollView;
    [SerializeField] Button questionButton;
    [SerializeField] MouseLook mouseLook;

    [SerializeField] MoralSchema moralSchema;

    [SerializeField] List<Button> interactionButtons;


    [SerializeField] private GameObject testingScrollView;
    [SerializeField] public Button startTestingButton;


    [SerializeField] private Button chatWithChatGptButton;


    private TestingModule testingModule;

 

    private void Awake()
    {
        Messenger.AddListener(GameEvent.LECTURE_PART_FINISHED, OnLecturePartFinished);
        Messenger.AddListener(GameEvent.ASKS_FINISHED, OnAsksFinished);
    }

    private void OnDestroy() {
        Messenger.RemoveListener(GameEvent.LECTURE_PART_FINISHED, OnLecturePartFinished);
        Messenger.RemoveListener(GameEvent.ASKS_FINISHED, OnAsksFinished);
    }

    private void Start() {
        questionButton.interactable = false;
        testingModule = this.gameObject.GetComponent<TestingModule>();
        testingModule.UpdateQuestions();
        HideChatWithChatGPTButton();
    }


    private void OnLecturePartFinished()
    {
        ShowQuestionButton();
    }

    private void OnAsksFinished()
    {
        HideQuestionButton();
    }

    public void HideQuestionButton()
    {
        questionButton.interactable = false;
    }

    public void ShowQuestionButton()
    {
        questionButton.interactable = true;
    }

    public void ShowAndHideQuestions()
    {
        QuestionsScrollView.SetActive(!QuestionsScrollView.activeSelf);
        //if(QuestionsScrollView.activeSelf)
        //    mouseLook.enabled = false;
        //else
        //    mouseLook.enabled = true;
    }
    public void ShowQuestions()
    {
        QuestionsScrollView.SetActive(true);
        //mouseLook.enabled = false;
    }
    public void HideQuestions()
    {
        QuestionsScrollView.SetActive(false);
        //mouseLook.enabled = true;
    }

    public void HideStartTestingModuleButton() {
        startTestingButton.interactable = false;
    }

    public void HideChatWithChatGPTButton() {
        chatWithChatGptButton.interactable = false;
    }

    public void ShowChatWithChatGPTButton() {
        chatWithChatGptButton.interactable = true;
    }

    public void TestingModuleButtonPressed() {
        HideStartTestingModuleButton();
        HideChatWithChatGPTButton();
        Messenger.Broadcast(GameEvent.STUDENT_PRESSED_TESTING_BUTTON);
    }

    public void ShowTestingScrollViewAdapter() {
        testingScrollView.SetActive(true);
    }

    public void HideTestingScrollViewAdapter() {
        testingScrollView.SetActive(false);
    }

    public void HideUserEstimatesButtons() {
        foreach (var button in interactionButtons) {
            button.gameObject.SetActive(false);
        }
    }


    public void GreatButtonAction() {
        moralSchema.makeIndependentAction("GreatButtonPressed");
        StartCoroutine("interactionButtonCLickedCoroutine");
    }
    public void LikeButtonAction() {
        moralSchema.makeIndependentAction("LikeButtonPressed");
        StartCoroutine("interactionButtonCLickedCoroutine");
    }
    public void AverageButtonAction() {
        moralSchema.makeIndependentAction("AverageButtonPressed");
        StartCoroutine("interactionButtonCLickedCoroutine");
    }
    public void DoesntLikeButtonAction() {
        moralSchema.makeIndependentAction("DoesntLikeButtonPressed");
        StartCoroutine("interactionButtonCLickedCoroutine");
    }
    public void HateButtonAction() {
        moralSchema.makeIndependentAction("HateButtonPressed");
        StartCoroutine("interactionButtonCLickedCoroutine");
    }

    private void DisableInteractionButtons() {
        foreach(var button in interactionButtons) {
            button.interactable = false;
        }
    }

    private void EnableInteractionButtons() {
        foreach (var button in interactionButtons) {
            button.interactable = true;
        }
    }


    private IEnumerator interactionButtonCLickedCoroutine() {
        DisableInteractionButtons();
        yield return new WaitForSeconds(5);
        EnableInteractionButtons();
    }
}
