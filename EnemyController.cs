using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Transform PTransform;

    public int sightDistance = 50;
    public int firingDistance = 150;
    public int runAwayDistance = 75;
    //public bool dodging = false;
    private Vector3 myPos;
    Enemy _enemy;
    bool allowShoot = true;

    public float dodgeSpeed = 15f;

    public bool isActive = true;




    private void Start()
    {
        PTransform = Player.Instance.transform;

    }

    public bool CombatChamber = false;

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0 || !isActive) return; // this skips the update when the game is paused
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            _enemy = enemy;
            if (!enemy.isDead && enemy.gameObject.activeInHierarchy)
            {
                
                if (enemy.isDodging)
                {
                    myPos = enemy.transform.position;
                    enemy.transform.position = Vector3.Lerp(myPos, enemy.GetComponent<BruiserGolem>().dodgePos, dodgeSpeed/100);
                    
                    if (Vector3.Distance(enemy.transform.position, enemy.GetComponent<BruiserGolem>().dodgePos) < 0.5f)
                    {
                        enemy.isDodging = false;
                    }
                }

                if (enemy.isDodging)
                {
                    enemy.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    enemy.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                    enemy.myAgent.enabled = false;
                    return;
                }

                if (!enemy.GetComponent<BalloonEnemy>())
                {
                    enemy.myAgent.enabled = true;
                    enemy.AnimationContorller.enabled = true;
                }
                
                if (!enemy.isStriking && !enemy.isStunnedAtk && !enemy.isDodging && !enemy.GetComponent<BalloonEnemy>())
                {
                    if (!enemy.isStunnedMove)
                    {
                        enemy.myAgent.SetDestination(PTransform.position);
                        if(enemy.myAgent.isOnOffMeshLink && !enemy.once)
                        {
                            enemy.StartCoroutine("Jump");

                            enemy.once = true;
                        }
                        if (!CombatChamber)
                        {
                            if (enemy.myAgent.remainingDistance > sightDistance)
                            {
                                enemy.myAgent.isStopped = true;
                            }
                            else
                            {
                                enemy.myAgent.isStopped = false;
                            }
                        }
                    }

                    if (Vector3.Distance(PTransform.position, enemy.transform.position) < enemy.strikeDistance)
                    {
                        if (enemy.GetComponent<BlasterEnemy>() && enemy.GetComponent<BlasterEnemy>().canStrike)
                        {
                            enemy.StartCoroutine("Strike");
                        }

                        

                        if (enemy.GetComponent<BruiserGolem>())
                        {
                            enemy.StartCoroutine("Strike");
                        }
}
                }

                if (enemy.GetComponent<BalloonEnemy>() && enemy.GetComponent<BalloonEnemy>().spin == false)
                {
                    enemy.StartCoroutine("Strike");
                }
            }
            else if(enemy && enemy.myAgent && enemy.AnimationContorller)
            {
                enemy.myAgent.enabled = false;
                enemy.AnimationContorller.enabled = false;
            }

        }
    }

    
    
}


    

