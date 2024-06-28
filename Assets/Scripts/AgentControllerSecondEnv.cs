using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentControllerSecondEnv : Agent
{
    [SerializeField] private Transform target;
    
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody  rb;

    public int coinCount;
    public GameObject coin;
    [SerializeField] private List<GameObject> spawnedCointList = new List<GameObject>();

    [SerializeField] private Transform enviormentLocation;

    Material envMaterial;
    public GameObject env;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-4f,4f), 0.1f, Random.Range(-4f,4f));
        //target.localPosition = new Vector3(Random.Range(-4f,4f),0.1f, Random.Range(-4f,4f));

        CreateCoins();
    }

    private void CreateCoins() 
    {
        distanceList.Clear();
        badDistanceList.Clear();

        if (spawnedCointList.Count != 0)
        {
            RemoveCoins(spawnedCointList);
        }

        for (int i = 0; i< coinCount; i++)
        {
            int counter = 0;
            bool distanceGood;
            bool alreadyDecremented = false;

            GameObject newCoin = Instantiate(coin);
            newCoin.transform.parent = enviormentLocation;
            Vector3 coinLocation = new Vector3(Random.Range(-4f,4f),0.1f, Random.Range(-4f,4f));

            if (spawnedCointList.Count != 0)
            {
                for (int j = 0; j < spawnedCointList.Count; j++) {
                    if (counter < 10)
                    {
                        distanceGood = CheckOverlap(coinLocation, spawnedCointList[j].transform.localPosition, 5f);
                        if (distanceGood == false) {
                            coinLocation = new Vector3(Random.Range(-4f,4f),0.1f, Random.Range(-4f,4f));
                            j--;
                            alreadyDecremented = true;
                        }

                         distanceGood = CheckOverlap(coinLocation, transform.localPosition, 5f);
                        if (distanceGood == false) {
                            coinLocation = new Vector3(Random.Range(-4f,4f),0.1f, Random.Range(-4f,4f));
                            if(alreadyDecremented == false) {
                                j--;
                            }
                        }
                        counter++;
                    } else {
                        j = spawnedCointList.Count;
                    }
                }
            }

            newCoin.transform.localPosition = coinLocation;
            spawnedCointList.Add(newCoin);
        }
    }

    public List<float> distanceList = new List<float>();
    public List<float> badDistanceList = new List<float>();

    private bool CheckOverlap(Vector3 objWeWantToAvoidOverlapping, Vector3 alredyExistingObj, float minDistanceWanted)
    {
        float DistanceBetweenObjects = Vector3.Distance(objWeWantToAvoidOverlapping,alredyExistingObj);

        if (minDistanceWanted <= DistanceBetweenObjects){
            distanceList.Add(DistanceBetweenObjects);
            return true;
        }
        badDistanceList.Add(DistanceBetweenObjects);
        return false;
    }

    private void RemoveCoins(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach(GameObject coin in toBeDeletedGameObjectList)
        {
            Destroy(coin.gameObject);
        }
        toBeDeletedGameObjectList.Clear();
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
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

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Coin")) {
            spawnedCointList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(10f);
            
            if (spawnedCointList.Count == 0)    
            {
                envMaterial.color = Color.green;
                AddReward(5f);
                RemoveCoins(spawnedCointList);
                EndEpisode();
            }
        }

        if (other.gameObject.CompareTag("Wall")) {
            envMaterial.color = Color.red;
            AddReward(-15f);
            RemoveCoins(spawnedCointList);
            EndEpisode();
        }
    }
}
