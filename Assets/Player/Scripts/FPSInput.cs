using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSInput : MonoBehaviour
{
    public float speed = 3.0f;
    public const float baseSpeed = 6.0f;
    public float gravity = 9.8f;
    private CharacterController _charController;


    //private void Awake()
    //{
    //    Messenger<float>.AddListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);
    //}

    //private void OnDestroy()
    //{
    //    Messenger<float>.RemoveListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);
    //}

    //private void OnSpeedChanged(float value)
    //{
    //    speed = baseSpeed * value;
    //}

    // Start is called before the first frame update
    void Start()
    {
        _charController = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaX = Input.GetAxis("Horizontal") * speed;
        float deltaZ = Input.GetAxis("Vertical") * speed;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = Vector3.ClampMagnitude(movement, speed);
        movement *= Time.deltaTime;
        movement.y = -gravity;
        movement = transform.TransformDirection(movement);
        _charController.Move(movement);
    }
}
