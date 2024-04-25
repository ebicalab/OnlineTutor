using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SittingPlayerCameraScript : MonoBehaviour
{
    private Camera _mainCamera;
    private Camera currentCamera;
    private AudioListener _audioListener;


    private void Awake()
    {
        Messenger.AddListener(PlayerEvent.SIT, OnPlayerSatDown);
        Messenger.AddListener(PlayerEvent.WALK, OnPlayerWalking);
    }

    private void OnDestroy()
    {
        Messenger.RemoveListener(PlayerEvent.SIT, OnPlayerSatDown);
        Messenger.RemoveListener(PlayerEvent.WALK, OnPlayerWalking);
    }

    private void OnPlayerSatDown()
    {
        //_mainCamera.enabled = !_mainCamera.enabled;
        currentCamera.enabled = true;
        //_audioListener.enabled = !_audioListener.enabled;
    }
    private void OnPlayerWalking()
    {
        currentCamera.enabled = false;
        //_mainCamera.enabled = !_mainCamera.enabled;
        //_audioListener.enabled = !_audioListener.enabled;
    }


    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        currentCamera = this.gameObject.GetComponent<Camera>();
        _audioListener = this.gameObject.GetComponent<AudioListener>();
    }
}
