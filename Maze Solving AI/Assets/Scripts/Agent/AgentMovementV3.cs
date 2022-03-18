using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentMovementV3 : Agent
{
    [SerializeField] Mode mode;

    [SerializeField] float distanceBwtStep = 5f;
    Rigidbody rb;

    [Header("Detect Layer Collision")]
    public LayerMask mask;
    bool[] dirCanMoveList;
    Ray[] rayList;
    RaycastHit[] hitInfo;

    [Header("Show Info")]
    public TextMesh rewardText;
    float reward = 0f;
    public TextMesh stepText;
    int step = 0;
    bool isEnd = false;

    public MazeController maze;

    // Input checking
    private bool[] isKeyDownList;
    private bool isMoveInputDown;

    // Start is called before the first frame update
    void Start()
    {

        if (mode == Mode.Testing)
        {
            Application.targetFrameRate = 30;
        }
        else {
            Application.targetFrameRate = 60;
        }

        rb = GetComponent<Rigidbody>();

        rayList = new Ray[4];
        hitInfo = new RaycastHit[4];

        dirCanMoveList = new bool[4];
        /*
        dirCanMoveList[0] => Up
        dirCanMoveList[1] => Down
        dirCanMoveList[2] => Right
        dirCanMoveList[3] => Left
        */

        isKeyDownList = new bool[4];
    }

    // Update is called once per frame
    void Update()
    {
        InputMovement();

        ShowInfo();
    }

    public override void OnEpisodeBegin()
    {
        // Reset maze
        maze.ResetMaze();

        // Reset info
        reward = 0f;
        step = 0;

        // Reset Agent position
        transform.localPosition = new Vector3(0f, 1.25f, 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isEnd)
        {
            // Movement
            int dir = actions.DiscreteActions[0];

            // Need to detect before move
            RayDetect();

            MoveAble(dir);

            step = StepCount; // StepCount dc goi theo fixedupdate = 0.02s 
            //print("step: " + step);
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreateActions = actionsOut.DiscreteActions;

        if (isMoveInputDown)
        {
            if (isKeyDownList[0])
            {
                discreateActions[0] = 0;
                // Up
            }
            else if (isKeyDownList[1])
            {
                discreateActions[0] = 1;
                // Down
            }
            else if (isKeyDownList[2])
            {
                discreateActions[0] = 2;
                // Right
            }
            else if (isKeyDownList[3])
            {
                discreateActions[0] = 3;
                // Left
            }
        }
        else {
            discreateActions[0] = 5;
        }

        isMoveInputDown = false;
        // To reset input list
        for (int i = 0; i < 4; i++)
        {
            isKeyDownList[i] = false;
        }
    }

    void InputMovement()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            isMoveInputDown = true;
            isKeyDownList[0] = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            isMoveInputDown = true;
            isKeyDownList[1] = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            isMoveInputDown = true;
            isKeyDownList[2] = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            isMoveInputDown = true;
            isKeyDownList[3] = true;
        }
    }

    void MoveAble(int dir)
    {
        // Up 
        if (dir == 0 && dirCanMoveList[0])
        {
            rb.position += Vector3.forward * distanceBwtStep;
        }
        else if (dir == 0 && !dirCanMoveList[0])
        {
            // Add punishment
            reward += -1f;
            AddReward(-1f);
        }

        // Down
        if (dir == 1 && dirCanMoveList[1])
        {
            rb.position += Vector3.back * distanceBwtStep;
        }
        else if (dir == 1 && !dirCanMoveList[1])
        {
            // Add punishment
            reward += -1f;
            AddReward(-1f);
        }

        // Right
        if (dir == 2 && dirCanMoveList[2])
        {
            rb.position += Vector3.right * distanceBwtStep;
        }
        else if (dir == 2 && !dirCanMoveList[2])
        {
            // Add punishment
            reward += -1f;
            AddReward(-1f);
        }

        // Left
        if (dir == 3 && dirCanMoveList[3])
        {
            rb.position += Vector3.left * distanceBwtStep;
        }
        else if (dir == 3 && !dirCanMoveList[3])
        {
            // Add punishment
            reward += -1f;
            AddReward(-1f);
        }

    }

    void RayDetect()
    {
        // Create 4 rays with 4 directions
        rayList[0] = new Ray(transform.position, transform.forward);
        rayList[1] = new Ray(transform.position, -transform.forward);
        rayList[2] = new Ray(transform.position, transform.right);
        rayList[3] = new Ray(transform.position, -transform.right);

        // Check each ray if it hit the wall
        for (int i = 0; i < rayList.Length; i++)
        {
            if (Physics.Raycast(rayList[i], out hitInfo[i], distanceBwtStep, mask))
            {
                Debug.DrawLine(rayList[i].origin, hitInfo[i].point, Color.red);
                dirCanMoveList[i] = false;
            }
            else
            {
                Debug.DrawLine(rayList[i].origin, rayList[i].origin + rayList[i].direction * distanceBwtStep, Color.white);
                dirCanMoveList[i] = true;
            }
        }
    }

    void ShowInfo()
    {
        rewardText.text = "Reward: " + reward.ToString();
        stepText.text = "Step: " + step.ToString();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag == "Floor")
        {
            FloorBehaviorV2 floorBehavior = other.gameObject.GetComponent<FloorBehaviorV2>();
            if (floorBehavior.timeIsStepedOn < 3)
            {
                floorBehavior.timeIsStepedOn++;
            }

            // Add reward
            reward += floorBehavior.reward;
            AddReward(floorBehavior.reward);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Finish")
        {
            // Add reward
            reward += 100f;
            AddReward(100f);

            if (mode == Mode.Training)
            {
                EndEpisode();
            }
            else
            {
                distanceBwtStep = 0f;
                isEnd = true;
            }
        }
    }

}

enum Mode
{
    Training, Testing
}
