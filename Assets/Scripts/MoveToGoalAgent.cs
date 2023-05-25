using System.Collections;
using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    private List<GameObject> holes = new List<GameObject>();
    private float[] holesOneHot = new float[16];
    [SerializeField] private Transform targetTransform;
    [SerializeField] private GameObject blocks;
    Rigidbody rBody;

    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin() {

        // Determine the agent's and target's positions
        // For ints Random.Range is maximum Exclusive
        int agentX = Random.Range(0, 4);
        int agentZ = Random.Range(0, 4);
        int targetX = Random.Range(0, 4);
        int targetZ = Random.Range(0, 4);

        // Make sure their positions don't overlap
        while (agentX == targetX && agentZ == targetZ) {
            targetX = Random.Range(0, 4);
            targetZ = Random.Range(0, 4);
        }

        // If the Agent fell, zero its momentum
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;

        // set the agent and goal positions
        transform.localPosition = new Vector3(agentX * 2, 0.41f, agentZ * 2);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        targetTransform.localPosition = new Vector3(targetX * 2, 0.5f, targetZ * 2);

        // if holes not empty, clear it and coordinates
        if (holes?.Any() == true) {
            foreach (GameObject hole in holes) {
                hole.SetActive(true);
            }
            holes.Clear();
        }

        // exclude agent and goal position from possible locations for holes
        var indices = Enumerable.Range(1, 16).ToList();
        indices.Remove(agentX + 1 + 4 * agentZ);
        indices.Remove(targetX + 1 + 4 * targetZ);

        // Reset the array to zeroes
        holesOneHot = new float[16];

        // choose locations for holes and update the one-hot vector of holes
        while (holes.Count < 4) {
            int random_idx = Random.Range(0, indices.Count);
            GameObject block = blocks.transform.Find("block " + indices[random_idx]).gameObject;
            holes.Add(block);
            block.SetActive(false);
            holesOneHot[indices[random_idx] - 1] = 1.0f;
            indices.RemoveAt(random_idx);
        }

    }

    public override void CollectObservations(VectorSensor sensor) {
        sensor.AddObservation(transform.localPosition[0]);
        sensor.AddObservation(transform.localPosition[2]);
        sensor.AddObservation(targetTransform.localPosition[0]);
        sensor.AddObservation(targetTransform.localPosition[2]);
        sensor.AddObservation(holesOneHot);

    }

    public override void OnActionReceived(ActionBuffers actions) {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float moveSpeed = 2.5f;
        rBody.MovePosition(transform.position + new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed);

        // Rewards
        float distanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);

        // Reached target
        if (distanceToTarget < 0.8f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        else if (transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }

        AddReward(-0.0005f);
        // Debug.Log(GetCumulativeReward());

    }

    public override void Heuristic(in ActionBuffers actionsOut) {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }
}
