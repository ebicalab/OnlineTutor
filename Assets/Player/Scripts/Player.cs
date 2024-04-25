using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

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
        this.gameObject.SetActive(false);
    }
    private void OnPlayerWalking()
    {
        this.gameObject.SetActive(true);
    }

    void Start()
    {
    }

    void Update()
    {

    }
}
