using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaveDesk : MonoBehaviour
{
    private Camera _camera;

    // Start is called before the first frame update
    void Start()
    {
        _camera = this.gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0));
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            //Debug.Log(hit.transform.gameObject.name);
            if(hit.transform.gameObject.tag == "LeaveDesk")
            {
                if(PlayerState.getPlayerState() == PlayerStateEnum.SIT && Input.GetKeyDown(KeyCode.E))
                {
                    Messenger.Broadcast(PlayerEvent.WALK);
                    PlayerState.setPlayerState(PlayerStateEnum.WALK);
                }
            }
        }
    }
}
