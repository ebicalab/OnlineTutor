using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLock : MonoBehaviour
{

    [SerializeField] MouseLook mouseLook;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            mouseLook.enabled = true;
        }
        else if(Input.GetMouseButtonUp(1))
        {
            Cursor.lockState = CursorLockMode.Confined;
            mouseLook.enabled = false;
            Cursor.visible = true;
        }
    }
}
