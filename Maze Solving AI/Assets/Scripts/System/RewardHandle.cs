using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardHandle : MonoBehaviour
{
    List<GameObject> bonusList;

    // Start is called before the first frame update
    void Start()
    {
        bonusList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Bonus")).FindAll(b => b.transform.parent.IsChildOf(this.transform));
    }

    public void ResetBonus() {
        foreach(var b in bonusList) {
            if(b.activeSelf == false) {
                b.SetActive(true);
            }
        }
    }
}
