using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Range(10.7f, 19.7f)] [SerializeField] private float x;
    [Range(0.25f, 6.5f)] [SerializeField] private float y;
    [Range(3f, 15.7f)] [SerializeField] private float z;

    [SerializeField] private Transform _student;
    [SerializeField] private Transform _eyes;
    [SerializeField] private Transform _mouth;
    [SerializeField] private Transform _left;
    [SerializeField] private Transform _right;






    void Start()
    {
        // Initialize x, y, z with the target's initial position
        if (target != null)
        {
            x = target.position.x;
            y = target.position.y;
            z = target.position.z;
        }
        else
        {
            Debug.LogError("Target is not assigned.");
        }

    }

    void Update()
    {
        if (target != null)
        {
            target.position = new Vector3(x, y, z);
        }


        if (Input.GetKeyDown(KeyCode.Keypad1))
            SetGazeDirectionDetermined(1);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            SetGazeDirectionDetermined(2);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            SetGazeDirectionDetermined(3);
        if (Input.GetKeyDown(KeyCode.Keypad4))
            SetGazeDirectionDetermined(4);



    }



    public void SetGazeDirection(Vector3 gazeDirection)
    {
        x = gazeDirection.x;
        y = gazeDirection.y;
        z = gazeDirection.z;
    }


    public bool IsStudentLookingAtTeacher()
    {
        RaycastHit hit;
        Debug.DrawRay(_student.position, _student.forward * 100, Color.red);
        if(Physics.Raycast(_student.position, _student.forward, out hit)     &&
                    hit.collider.gameObject.CompareTag("Teacher"))
                    return true;
        return false;
    }

    public bool IsStudentLookingAtBoard()
    {
        RaycastHit hit;
        Debug.DrawRay(_student.position, _student.forward * 100, Color.red);
        if(Physics.Raycast(_student.position, _student.forward, out hit)     &&
                    hit.collider.gameObject.CompareTag("Board"))
                    return true;
        return false;
    }

    public void SetGazeDirectionDetermined(int direction)
    {
        switch(direction){

            case 1:
                //eyes     
                SetGazeDirection(_eyes.position);               
                break;
            case 2:
                //mouth
                SetGazeDirection(_mouth.position);
                break;
            case 3:
                //right
                SetGazeDirection(_right.position);
                break;
            case 4:
                //left
                SetGazeDirection(_left.position);
                break;
            
        };

    }
}
