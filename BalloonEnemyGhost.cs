using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonEnemyGhost : MonoBehaviour
{
    Transform playerT;
    public GameObject eye;
    public GameObject eyeBase;

    // Start is called before the first frame update
    void Start()
    {
        playerT = Player.Instance.transform;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(playerT);
        //Debug.Log("playerT = " + playerT);
    }
}
