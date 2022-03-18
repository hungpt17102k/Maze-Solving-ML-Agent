using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorBehaver : MonoBehaviour
{
    public bool isStepOn = false;

    public MeshRenderer meshRenderer;

    public Material matDefault;
    public Material matIsStepOn;

    private void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update() {
        if(!isStepOn) {
            meshRenderer.material = matDefault;
        }
        else {
            meshRenderer.material = matIsStepOn;
        }
    }
}
