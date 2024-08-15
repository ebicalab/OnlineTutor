using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Range(10.7f, 19.7f)] [SerializeField] private float x;
    [Range(0.25f, 6.5f)] [SerializeField] private float y;
    [Range(3f, 15.7f)] [SerializeField] private float z;

    [SerializeField] private Transform student;
    [SerializeField] private LayerMask teacherLayer;
    [SerializeField] private LayerMask boardLayer;
   
    

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
        Debug.DrawRay(student.position, student.forward * 100, Color.red);
        if(Physics.Raycast(student.position, student.forward, out hit)     &&
                    hit.collider.gameObject.CompareTag("Teacher"))
                    return true;
        return false;
    }

    public bool IsStudentLookingAtBoard()
    {
        RaycastHit hit;
        Debug.DrawRay(student.position, student.forward * 100, Color.red);
        if(Physics.Raycast(student.position, student.forward, out hit)     &&
                    hit.collider.gameObject.CompareTag("Board"))
                    return true;
        return false;
    }
}
