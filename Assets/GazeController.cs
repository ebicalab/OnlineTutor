using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float x;
    [SerializeField] private float y;
    [SerializeField] private float z;

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
