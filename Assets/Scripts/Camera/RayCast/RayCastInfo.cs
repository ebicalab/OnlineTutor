using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RayCastInfo : MonoBehaviour
{

    private bool isSitting = false;
    private float timer = 0.0f;
    private float timerBound = 1.0f;

    [SerializeField] Camera mainCamera;
    [SerializeField] Camera sittingCamera;
    [SerializeField] UnityEngine.UI.Text textLabel;
    [SerializeField] MoralSchema moralSchema;

    private Camera currentCamera;

    private void Awake()
    {
        Messenger.AddListener(PlayerEvent.SIT, OnPlayerSatDown);
        Messenger.AddListener(PlayerEvent.WALK, OnPlayerWalk);

        currentCamera = mainCamera;

    }
    private void OnDestroy()
    {
        Messenger.RemoveListener(PlayerEvent.SIT, OnPlayerSatDown);
        Messenger.RemoveListener(PlayerEvent.WALK, OnPlayerWalk);
    }

    private void OnPlayerSatDown()
    {
        currentCamera = sittingCamera;
    }

    private void OnPlayerWalk()
    {
        currentCamera = mainCamera;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > timerBound)
        {
            timer = 0.0f;
            if (Input.GetMouseButtonDown(1))
            {
                RaycastHit hit;
                if (Physics.Raycast(currentCamera.transform.position, currentCamera.transform.forward, out hit))
                {
                    if (hit.transform.tag == "Teacher" || hit.transform.tag == "Board")
                    {
                        textLabel.text = $"Student look at {hit.transform.tag}";
                        moralSchema.makeIndependentAction("lookAtTeacher");
                    }
                    else
                    {
                        textLabel.text = $"Student doesnt look at teacher or board";
                        moralSchema.makeIndependentAction("doesntLookAtTeacher");
                    }
                }
                else
                {
                    textLabel.text = $"Student doesnt look at teacher or board";
                }
                Debug.DrawRay(currentCamera.transform.position, currentCamera.transform.forward * 100f, Color.red, duration: 2f, depthTest: false);
            }
            else
            {
                RaycastHit hit;
                Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    if(hit.transform.tag == "Teacher" || hit.transform.tag == "Board")
                    {
                        textLabel.text = $"Student look at {hit.transform.tag}";
                        moralSchema.makeIndependentAction("lookAtTeacher");
                    }
                    else
                    {
                        textLabel.text = $"Student doesnt look at teacher or board";
                        moralSchema.makeIndependentAction("doesntLookAtTeacher");
                    }
                }
                else
                {
                    textLabel.text = $"Student doesnt look at teacher or board";
                }
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, duration: 2f, depthTest: false);
            }
        }
    }
}
