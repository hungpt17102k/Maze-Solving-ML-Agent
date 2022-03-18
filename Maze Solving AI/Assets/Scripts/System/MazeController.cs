using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeController : MonoBehaviour
{
    [SerializeField] List<GameObject> floorList;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FindAllFloors());
    }

    IEnumerator FindAllFloors() {
        yield return new WaitForSeconds(0.5f);
        floorList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Floor")).FindAll(f => f.transform.parent.IsChildOf(this.transform));
    }

    public void ResetMaze() {
        foreach(var floor in floorList) {
            floor.GetComponent<FloorBehaviorV2>().ResetFloor();
        }
    }

}
