using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallControll : MonoBehaviour
{
    public GameObject player;
    
    private void OnTriggerEnter(Collider other) {
        player.GetComponent<AgentControllerFußball>().BallAction(other);
    }
}
