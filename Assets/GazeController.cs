using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [Range(10.7f, 19.7f)] [SerializeField] private float x;
    [Range(0.25f, 6.5f)] [SerializeField] private float y;
    [Range(3f, 15.7f)] [SerializeField] private float z;

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
            // Update target's position based on x, y, z
            target.position = new Vector3(x, y, z);
        }
    }
}
