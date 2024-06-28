using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.SocialPlatforms;

public class AgentControllerFußball : Agent
{
    [Header("Player")]
    private Rigidbody rb;
    [SerializeField] private float moveSpeed = 4f;

    [Header("Ball")]
    public GameObject ball;

    [Header("Enviorment")]
    [SerializeField] private Transform enviorment;

    [Header("Goals")]
    public GameObject leftGoal;
    public GameObject rightGoal;



    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-4f,4f), 0.25f, Random.Range(-4f,4f));
        ball.transform.localPosition = new Vector3(Random.Range(-4f,4f), 0.25f, Random.Range(-4f,4f));
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(ball.transform.localPosition);
        sensor.AddObservation(leftGoal.transform.localPosition);
        sensor.AddObservation(rightGoal.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f,moveRotate * moveSpeed, 0f, Space.Self);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continiousActions = actionsOut.ContinuousActions;
        continiousActions[0] = Input.GetAxisRaw("Horizontal");
        continiousActions[1] = Input.GetAxisRaw("Vertical");
    }

    public void BallAction(Collider other)
    {
        Debug.Log(other.tag);

        if (other.gameObject.CompareTag(leftGoal.tag)) {
            AddReward(-300);
            EndEpisode();
        }

        if (other.gameObject.CompareTag(rightGoal.tag)) {
            AddReward(1000);
            EndEpisode();
        }

        if (other.gameObject.CompareTag("BorderArena")) {
            AddReward(-2);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("BorderArena")) {
            AddReward(-2);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag(ball.tag)) {
            other.rigidbody.AddForce((other.transform.position - transform.localPosition) * 5, ForceMode.Impulse);
            //StartCoroutine(GetKickReward(Vector3.Distance(ball.transform.position, rightGoal.transform.position), 0.5f));

            float distanceFromPlayerToGoal = Vector3.Distance(transform.position,rightGoal.transform.position);
            float distanceFromBallToGoal = Vector3.Distance(ball.transform.position,rightGoal.transform.position);

            if (distanceFromPlayerToGoal > distanceFromBallToGoal) {
                AddReward(3f);
                Debug.Log("+");
            } else {
                AddReward(-1f);
                Debug.Log("-");
            }
        }
    }

    IEnumerator GetKickReward(float distance, float delayTime) {
        yield return new WaitForSeconds(delayTime);

        float newDistance = Vector3.Distance(ball.transform.position, rightGoal.transform.position);

        if (newDistance < distance) {
            AddReward(5f);
            Debug.Log("+");
        } else {
            AddReward(-1f);
            Debug.Log("-");
        }
    }


}
