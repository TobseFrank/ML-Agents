using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentControllerFirstEnv : Agent
{
    [SerializeField] private Transform target;
    
    [SerializeField] private float moveSpeed = 4f;


    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-4f,4f), 0.1f, Random.Range(-4f,4f));
        target.localPosition = new Vector3(Random.Range(-4f,4f),0.1f, Random.Range(-4f,4f));
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 velocity = new Vector3(moveX, 0f, moveZ);
        velocity = velocity.normalized * Time.deltaTime  * moveSpeed;
        
        transform.localPosition += velocity;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continiousActions = actionsOut.ContinuousActions;
        continiousActions[0] = Input.GetAxisRaw("Horizontal");
        continiousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Coin")) {
            AddReward(2f);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("Wall")) {
            AddReward(-1f);
            EndEpisode();
        }
    }
}
