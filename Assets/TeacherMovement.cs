using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacherMovement : MonoBehaviour {
    [SerializeField] private float _movSpeed = 5f; // Movement speed
    private float _xMax = 19.7f;
    private float _xMin = 10.7f;
    private float _yMin = 0.25f;
    private float _yMax = 6.7f;

    void Update() {
        // Get input for horizontal and vertical movement
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.L))
            horizontal = -1f; // Move right
        if (Input.GetKey(KeyCode.J))
            horizontal = 1f;  // Move left
        if (Input.GetKey(KeyCode.I))
            vertical = 1f;    // Move up
        if (Input.GetKey(KeyCode.K))
            vertical = -1f;   // Move down

        Vector3 movement = new Vector3(horizontal, vertical, 0f) * _movSpeed * Time.deltaTime;

        transform.Translate(movement);

        float clampedX = Mathf.Clamp(transform.position.x, _xMin, _xMax);
        float clampedY = Mathf.Clamp(transform.position.y, _yMin, _yMax);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
