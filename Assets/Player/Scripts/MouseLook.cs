using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXandY = 0,
        MouseX = 1,
        MouseY = 2
    }

    public RotationAxes axes = RotationAxes.MouseXandY;

    public float sensitivityHor = 9.0f;
    public float sensitivityVert = 9.0f;

    public float minimumVert = -45.0f;
    public float maximumVert = 45.0f;

    public float minimumHor = -80.0f;
    public float maximumHor = 80.0f;

    private float _rotationX = 0;
    private float _rotationY = 0;

    // Start is called before the first frame update
    void Start()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
            body.freezeRotation = true;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (axes == RotationAxes.MouseXandY)
        {
            _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            _rotationY += Input.GetAxis("Mouse X") * sensitivityHor;

            _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);
            _rotationY = Mathf.Clamp(_rotationY, minimumHor, maximumHor);

            transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0);
        }
        else if (axes == RotationAxes.MouseX)
        {

            transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityHor, 0);
        }
        else
        {
            _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            _rotationX = Mathf.Clamp(_rotationX, minimumVert, maximumVert);
            float rotationY = transform.localEulerAngles.y;
            transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
        }

    }
}
