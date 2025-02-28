using UnityEngine;

public class MouseUnlock: MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Cursor.lockState = CursorLockMode.None; 
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0)) 
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
