using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentMovement : Agent
{
    public float moveSpeed = 5f;
    public float smootMoveTime = 0.1f;
    public float turnSpeed = 10f;
    Vector3 moveDir;

    float angle;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    Vector3 velocity;

    Rigidbody rb;

    float timer = 0f;
    int secondToReduce = 20;

    [SerializeField] Transform goal;
    [SerializeField] GameObject bonusController;

    List<GameObject> floorsHasBeenStepOn;

    float disBefore = 2f;

    int hitWallCount = 0;

    private void Start() {
        rb = GetComponent<Rigidbody>();

        floorsHasBeenStepOn = new List<GameObject>();
    }

    private void FixedUpdate() {
        rb.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        rb.MovePosition(rb.position + velocity * Time.deltaTime);
    }

    public override void OnEpisodeBegin()
    {
        // Reset maze
        ResetMaze();

        // Reset Agent position
        transform.localPosition = new Vector3(0f, 1.25f, 0f);
        transform.localEulerAngles = Vector3.up * 90;
    }

    // public override void CollectObservations(VectorSensor sensor) {
    //     sensor.AddObservation(transform.localPosition);
    //     sensor.AddObservation(goal.transform.localPosition);
    //     sensor.AddObservation(Vector3.Distance(goal.transform.localPosition, this.transform.localPosition));
    // }
    
    public override void OnActionReceived(ActionBuffers actions) {
        //SetPanltyForAgentDuringEverySecond();
        AddReward(-0.001f);

        //AddRewardBasedOnDistance();

        float moveX = GetRandomIntValue(actions, 0);
        float moveZ = GetRandomIntValue(actions, 1);

        // float moveX = actions.DiscreteActions[0];
        // float moveZ = actions.DiscreteActions[1];
        
        //print("MoveX=" + moveX + " MoveZ=" + moveZ);

        // Movement combine with rotation
        moveDir = new Vector3(moveX, 0, moveZ).normalized;
        float inputMagnitude = moveDir.magnitude;
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smootMoveTime);

        float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

        transform.localEulerAngles = Vector3.up * angle;

        velocity = transform.forward * moveSpeed * smoothInputMagnitude;
    }

    float GetRandomIntValue(ActionBuffers actions, int index) {
        if(actions.DiscreteActions[index] == 0) {
            return -1f;
        } 
        else if(actions.DiscreteActions[index] == 1){
            return 0f;
        }
        else {
            return 1f;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreateActions = actionsOut.DiscreteActions;
        // continuousActions[0] = Input.GetAxisRaw("Horizontal");
        // continuousActions[1] = Input.GetAxisRaw("Vertical");
    
        if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            discreateActions[0] = 1;
        } 
        else if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            discreateActions[0] = -1;
        } 
        else {
            discreateActions[0] = 0;
        }

        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
            discreateActions[1] = 1;
        } 
        else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
            discreateActions[1] = -1;
        }
        else {
            discreateActions[1] = 0;
        }
    }

    void SetPanltyForAgentDuringEverySecond() {
        // Set penalty to make agent keep moving
        if(timer >= secondToReduce) {
            AddReward(-0.1f);
            //print(timer);
            timer = 0f;
            EndEpisode();
        } else {
            timer += Time.deltaTime;
        }
    }

    void AddRewardBasedOnDistance() {
        float distanceToGoal = Vector3.Distance(transform.localPosition, goal.transform.localPosition);

        if(distanceToGoal < disBefore) {
            disBefore = distanceToGoal;
            
            AddReward(0.1f);     
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Finish") {
            AddReward(10f);
            EndEpisode();
        }
        else if(other.tag == "Bonus") {
            AddReward(1f);
            other.gameObject.SetActive(false);
        }
    }

    private void OnCollisionStay(Collision other) {
        if(other.collider.tag.Equals("Wall")) {       
            AddReward(-0.1f);

            SetPanltyForAgentDuringEverySecond();
        }
        // else if(other.collider.tag == "Floor") {
        //     if(other.gameObject.GetComponent<FloorBehaver>().isStepOn) {
        //         AddReward(-0.1f);
        //     }
        // }
    }

    private void OnCollisionEnter(Collision other) {
        if(other.collider.tag == "Floor") {
            FloorBehaver floorBehaver = other.gameObject.GetComponent<FloorBehaver>();
            if(!floorBehaver.isStepOn) {
                floorBehaver.isStepOn = true;

                floorsHasBeenStepOn.Add(floorBehaver.gameObject);
                // Add reward
                AddReward(2f);
            }
        }
        else if(other.collider.tag == "Wall") {
            hitWallCount++;
            //print("hit");
            AddReward(-0.5f);

            if(hitWallCount >= 8) {
                hitWallCount = 0;
                EndEpisode();
            }
        }
    }

    void ResetMaze() {
        // Reset the floors
        foreach (var floor in floorsHasBeenStepOn)
        {
            floor.GetComponent<FloorBehaver>().isStepOn = false;
            floor.GetComponent<MeshRenderer>().material = floor.GetComponent<FloorBehaver>().matDefault;
        }

        floorsHasBeenStepOn.Clear();

        // Reset the bonus points
        bonusController.GetComponent<RewardHandle>().ResetBonus();
    }

}
