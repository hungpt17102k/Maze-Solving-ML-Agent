using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class AgentMovmentV2 : Agent
{
    [SerializeField] Mode mode;

    [SerializeField] float distanceBwtStep = 5f;
    Rigidbody rb;

    [Header("Detect Layer Collision")]
    public LayerMask mask;
    bool canMove = false;

    // Input Checking
    private bool isMoveInputDown;
    private bool isRotateInputDown;
    private bool[] isKeyDownList;
    float angle;

    [Header("Show Info")]
    public TextMesh rewardText;
    float reward = 0f;
    public TextMesh stepText;
    int step = 0;
    bool isEnd = false;


    public MazeController maze;

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

        isKeyDownList = new bool[4];
    }

    // Update is called once per frame
    void Update()
    {
        InputRotation();

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
        angle = 0f;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isEnd)
        {
            // Rotation
            switch (actions.DiscreteActions[1])
            {
                case 0:
                    angle += 0f;
                    break;
                case 1:
                    angle = -90f;
                    break;
                case 2:
                    angle = 90f;
                    break;
                case 3:
                    angle = 0f;
                    break;
                case 4:
                    angle = -180f;
                    break;
            }
            FaceRotation();

            RayDetect();

            // Movement
            if (actions.DiscreteActions[0] == 1)
            {
                MoveForward();
            }

            step = StepCount; // StepCount dc goi theo fixedupdate = 0.02s 
            //print("step: " + step);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreateActions = actionsOut.DiscreteActions;

        // Rotation
        if (isRotateInputDown)
        {
            if (isKeyDownList[0])
            {
                discreateActions[1] = 1;
                //angle = -90f;
            }
            else if (isKeyDownList[1])
            {
                discreateActions[1] = 2;
                //angle = 90f;
            }
            else if (isKeyDownList[2])
            {
                discreateActions[1] = 3;
                //angle = 0f;
            }
            else if (isKeyDownList[3])
            {
                discreateActions[1] = 4;
                //angle = -180f;
            }
        }
        else
        {
            discreateActions[1] = 0;
        }

        isRotateInputDown = false;
        // To reset input list
        for (int i = 0; i < 4; i++)
        {
            isKeyDownList[i] = false;
        }

        // Movement
        discreateActions[0] = isMoveInputDown ? 1 : 0;

        isMoveInputDown = false;
    }

    void InputMovement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isMoveInputDown = true;
        }
    }

    void MoveForward()
    {
        if (canMove)
        {
            rb.position += transform.forward * distanceBwtStep;
        }
        else
        {
            // Add punishment
            reward += -1f;
            AddReward(-1f);
            //print("Can't move");
        }
    }

    void InputRotation()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            isRotateInputDown = true;
            isKeyDownList[0] = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            isRotateInputDown = true;
            isKeyDownList[1] = true;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            isRotateInputDown = true;
            isKeyDownList[2] = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            isRotateInputDown = true;
            isKeyDownList[3] = true;
        }
    }

    void FaceRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    void RayDetect()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 5, mask))
        {
            Debug.DrawLine(ray.origin, hitInfo.point, Color.red);
            canMove = false;

        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * 5, Color.white);
            canMove = true;
            //print("11111");
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
