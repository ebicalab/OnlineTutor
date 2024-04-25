using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriigerToSit : MonoBehaviour {

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private GameObject _player;

    private bool goSit = false;


    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        if (goSit) {
            if (Input.GetKeyDown(KeyCode.E)) {
                Messenger.Broadcast(PlayerEvent.SIT);
                PlayerState.setPlayerState(PlayerStateEnum.SIT);
                goSit = false;
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.GetComponent<Player>()) {
            //Debug.Log("OnTriggerStay!");
            //Ray ray = _mainCamera.ScreenPointToRay(new Vector3(_mainCamera.pixelWidth / 2, _mainCamera.pixelHeight / 2, 0));
            //RaycastHit hit;
            //Debug.Log(LayerMask.GetMask("RayCastObjects"));
            //if(Physics.Raycast(ray, out hit, LayerMask.GetMask("RayCastObjects")))
            //{
            if (PlayerState.getPlayerState() == PlayerStateEnum.WALK) {
                goSit = true;
            }
            else {
                goSit = false;
            }
            //}
        }
    }

    private void OnTriggerExit(Collider other) {
        goSit = false;
    }
}
