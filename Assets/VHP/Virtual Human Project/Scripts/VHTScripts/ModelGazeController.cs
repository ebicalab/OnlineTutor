using UnityEngine;

public class ModelGazeController : MonoBehaviour
{
    public Transform target; // The target the character will look at
    public VHPGaze vhpGaze; // Reference to the VHPGaze component

    private void Start()
    {
        // Get the VHPGaze component attached to the model
        vhpGaze = GetComponent<VHPGaze>();
    }

    private void Update()
    {
        if (target != null && vhpGaze != null)
        {
            // Calculate the direction to the target
            Vector3 gazeDirection = target.position - transform.position;

            // Set the gaze direction using the VHPGaze component's method
            Debug.Log("HI");
            vhpGaze.SetGazeDirection(gazeDirection);
        }
    }
}
