using System.Collections.Generic;
using System.Linq; 
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToGoalAgent : Agent
{
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject blocks;
    private List<GameObject> holes = new List<GameObject>();
    private float[] holesOneHot = new float[16];
    private bool reachedTarget = false;
    private float moveSpeed = 2.5f;
    private Rigidbody rBody;


    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin() {

        // Use a flag to react to the episode end within the OnActionReceived method
        reachedTarget = false;

        // Determine the agent's and target's positions
        int agentX = Random.Range(0, 4);
        int agentZ = Random.Range(0, 4);
        int targetX = Random.Range(0, 4);
        int targetZ = Random.Range(0, 4);

        // Make sure their positions don't overlap
        while (agentX == targetX && agentZ == targetZ) {
            targetX = Random.Range(0, 4);
            targetZ = Random.Range(0, 4);
        }

        // Zero agent's momentum
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;

        // Set the agent's and goal's positions on the board
        transform.localPosition = new Vector3(agentX * 2, 0.41f, agentZ * 2);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        target.transform.localPosition = new Vector3(targetX * 2, 0.5f, targetZ * 2);

        // if holes not empty, activate the blocks back and clear the list
        if (holes?.Any() == true) {
            foreach (GameObject hole in holes) {
                hole.SetActive(true);
            }
            holes.Clear();
        }

        // Exclude agent and goal positions from possible locations for holes
        var indices = Enumerable.Range(1, 16).ToList();
        indices.Remove(agentX + 1 + 4 * agentZ);
        indices.Remove(targetX + 1 + 4 * targetZ);

        holesOneHot = new float[16];
        int numHoles = Mathf.FloorToInt(Academy.Instance.EnvironmentParameters.GetWithDefault("number_of_holes", 4f));
        // Choose locations for holes and transorm ice blocks to holes
        while (holes.Count < numHoles) {  // set the desired number of holes

            // Randomly choose an index to access the remaining indices (after excluding agent/target positions)
            int random_idx = Random.Range(0, indices.Count);

            // Retrieve the corresponding block
            GameObject block = blocks.transform.Find("block " + indices[random_idx]).gameObject;

            // Add it to the list to change it back at the beginning of the next episode
            holes.Add(block);
            
            // Deactivate the block to create a hole
            block.SetActive(false);

            // Set the corresponding index value to 1
            holesOneHot[indices[random_idx] - 1] = 1.0f;

            // Remove the taken index from the available positions
            indices.RemoveAt(random_idx);
        }
    }

    public override void CollectObservations(VectorSensor sensor) {

        sensor.AddObservation(transform.localPosition[0] / 6f);
        sensor.AddObservation(transform.localPosition[2] / 6f);
        sensor.AddObservation(target.transform.localPosition[0] / 6f);
        sensor.AddObservation(target.transform.localPosition[2] / 6f);
        sensor.AddObservation(holesOneHot);

    }

    public override void OnActionReceived(ActionBuffers actions) {

        float forwardAction = Mathf.FloorToInt(actions.DiscreteActions[0]);
        float sidewaysAction = Mathf.FloorToInt(actions.DiscreteActions[1]);

        Vector3 movement = Vector3.zero;

        if (forwardAction == 1)
        {
            movement += new Vector3(0, 0, 1); // forward
        }
        else if (forwardAction == 2)
        {
            movement += new Vector3(0, 0, -1); // backward
        }
        
        if (sidewaysAction == 1)
        {
            movement += new Vector3(-1, 0, 0); // left
        }
        else if (sidewaysAction == 2)
        {
            movement += new Vector3(1, 0, 0); // right
        }
        
        rBody.MovePosition(transform.position + movement * Time.deltaTime * moveSpeed);

        // If fell off platform
        if (transform.localPosition.y < 0)
        {
            AddReward(-1f);
            EndEpisode();
        }

        if (reachedTarget)
        {
            AddReward(1f);
            EndEpisode();
        }

        // Punisgh for idleness
        AddReward(-0.0005f);
    }

    public override void Heuristic(in ActionBuffers actionsOut) {

        var discreteActions = actionsOut.DiscreteActions;

        // Default to no movement
        discreteActions[0] = 0;
        discreteActions[1] = 0;

        // Forward / Backward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActions[0] = 2;
        }

        // Left / Right
        if (Input.GetKey(KeyCode.A))
        {
            discreteActions[1] = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActions[1] = 2;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == target)
        {
            reachedTarget = true;
        }
    }
}