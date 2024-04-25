using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerGreetings : MonoBehaviour
{
    [SerializeField] private AudioController audioController;

    private int triggerCount = 0;
    

    private int countOfGreetings;

    private int countOfRequestsToSit;
    public int countToRequest = 15;

    string pathToGreetings = "Music/GeneralSounds/Greetings";
    string pathToRequestsToSit = "Music/GeneralSounds/RequestToStartTheLesson";


    // Start is called before the first frame update
    void Start()
    {
        countOfGreetings = DirInfo.getCountOfFilesInFolder("/Resources/" + pathToGreetings);
        countOfRequestsToSit = DirInfo.getCountOfFilesInFolder("/Resources/" + pathToRequestsToSit);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>())
        {
            if(triggerCount == 0)
            {
                int number = Random.Range(1, countOfGreetings + 1);
                string path = pathToGreetings + $"/greetings_{number}";
                audioController.playShortSound(path);
            }
            if(triggerCount % countToRequest == 0 && triggerCount != 0)
            {
                int number = Random.Range(1, countOfRequestsToSit + 1);
                string path = pathToRequestsToSit + $"/please_sit_{number}";
                audioController.playShortSound(path);
            }
            triggerCount++;
        }
    }
}
