using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBehaviorV2 : MonoBehaviour
{
    [Header("Material of the Floor")]
    public Material[] materials;
    public MeshRenderer meshRenderer;
    Material mat;

    [Header("Values of when steped on")]
    public int timeIsStepedOn = 0;
    public float[] rewardList;
    public float reward = 0f;
    

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        switch(timeIsStepedOn) {
            case 0:
                mat = materials[0];
                reward = rewardList[0];
                break;
            case 1:
                mat = materials[1];
                reward = rewardList[1];
                break;
            case 2:
                mat = materials[2];
                reward = rewardList[2];
                break;
            case 3:
                mat = materials[3];
                reward = rewardList[3];
                break;
        }

        meshRenderer.material = mat;
    }

    public void ResetFloor() {
        meshRenderer.material = materials[0];
        timeIsStepedOn = 0;
        reward = 0f;
    }
}
