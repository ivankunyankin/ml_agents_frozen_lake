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
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject blocks;
    private float gotTargetReward = 1.0f;
    private float moveSpeed = 2.5f;
    private int holeLayer;
    private int blockLayer;
    public Material holeMaterial;
    public Material blockMaterial;
    Rigidbody rBody;

    void Start () {

        rBody = GetComponent<Rigidbody>();

        // Get the layer IDs based on the layer names
        holeLayer = LayerMask.NameToLayer("HoleLayer");
        blockLayer = LayerMask.NameToLayer("BlockLayer");

    }

    public override void OnEpisodeBegin() {

        // Reset base reward in case it was altered
        ResetReward();

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

        // if holes not empty, transoform them back to ice blocks and clear the list
        if (holes?.Any() == true) {
            foreach (GameObject hole in holes) {

                // change the layer back for it to be invisible for the observation camera
                hole.layer = blockLayer;

                // switch back to the ice (block) material
                Renderer rendererComponent = hole.GetComponent<Renderer>();
                rendererComponent.material = blockMaterial;
            }
            holes.Clear();
        }

        // Exclude agent and goal positions from possible locations for holes
        var indices = Enumerable.Range(1, 16).ToList();
        indices.Remove(agentX + 1 + 4 * agentZ);
        indices.Remove(targetX + 1 + 4 * targetZ);

        // Choose locations for holes and transorm ice blocks to holes
        while (holes.Count < 4) {  // set the desired number of holes

            // Randomly choose an index to access the remaining indices (after excluding agent/target positions)
            int random_idx = Random.Range(0, indices.Count);

            // Retrieve the corresponding block
            GameObject block = blocks.transform.Find("block " + indices[random_idx]).gameObject;

            holes.Add(block); // add it to the list to change it back at the beginning of the next episode

            // Change the layer so its visible for the observation camera
            block.layer = holeLayer;

            // Change the material so its more clear for on the observation camera view
            Renderer rendererComponent = block.GetComponent<Renderer>();
            rendererComponent.material = holeMaterial;

            // Remove the taken index from the available positions
            indices.RemoveAt(random_idx);
        }

        AddRewardForDifficulty();

    }

    public override void CollectObservations(VectorSensor sensor) {

        sensor.AddObservation(transform.localPosition[0]);
        sensor.AddObservation(transform.localPosition[2]);
        sensor.AddObservation(target.transform.localPosition[0]);
        sensor.AddObservation(target.transform.localPosition[2]);

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
            AddReward(-1.0f);
            EndEpisode();
        }

        // Punisgh for idleness
        AddReward(-0.001f);
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

    private void OnCollisionEnter(Collision collision) {

    if (collision.gameObject == target) {

        AddReward(gotTargetReward);
        EndEpisode();

        }
    }

    private void AddRewardForDifficulty() {

        Vector3 directionToGoal = (target.transform.localPosition - transform.localPosition).normalized;
        float distanceToGoal = Vector3.Distance(transform.localPosition, target.transform.localPosition);

        // Check if there's a hole between the agent and the goal
        RaycastHit hit;
        LayerMask layerMask = LayerMask.GetMask("HoleLayer");

        // If there is a hole, add increase the reward
        if (Physics.SphereCast(transform.localPosition, 0.6f, directionToGoal, out hit, distanceToGoal, layerMask))
        {
            gotTargetReward = 1.5f;
        }

    }

    private void ResetReward() {

        gotTargetReward = 1.0f;
    
    }

}