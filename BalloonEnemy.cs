using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Experimental.VFX;

public class BalloonEnemy : Enemy
{
    #region Variables

    public bulletType color;

    public Transform[] teleLoc;
    Transform playerT;
    public float teleportDist;
    public GameObject Bullet;
    public GameObject GunMuzzle;
    public float BulletDelay;
    public float TimeBetweenAttacks;

    public float timeSpentSpinning;
    public float timeForSpin;
    public float timeForBlink;
    public int timesToBlink;
    public float timeForDistortion;


    float _degreesPerSec;

    public float distFromCenter;

    public bool spin = false;
    [SerializeField]
    bool allowBlink = true;
    [SerializeField]
    bool allowTeleport = false;

    public GameObject ringInner;
    public GameObject ringMiddle;
    public GameObject ringOuter;
    public GameObject ringOuterParent;
    public GameObject eye;
    public GameObject eyeBase;
    public GameObject seal;

    bool allowShoot = true;

    public GameObject distortion;

    public float increaseRate;
    bool increaseSpin;
    bool resetSpin;
    public float minMultiplyer;
    public float maxMultiplyer;
    [SerializeField]
    float _multiplyer;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _multiplyer = minMultiplyer;

        distortion.gameObject.SetActive(false);
        SetColorAndTag();
    }

    // Update is called once per frame
    void Update()
    {
        distortion.transform.position = transform.position;

        playerT = Player.Instance.transform;
        _degreesPerSec = (360 / timeForSpin);
        

        if ((!CheckSight() || CheckDistance() >= teleportDist) && allowBlink)
        {
            StartCoroutine(Blink());
            allowBlink = false;

        }

        if (resetSpin)
        {
            _multiplyer = Mathf.Lerp(_multiplyer, minMultiplyer, increaseRate);
            if(_multiplyer == minMultiplyer)
            {
                resetSpin = false;
            }
        }

        if (increaseSpin)
        {
            _multiplyer = Mathf.Lerp(_multiplyer, maxMultiplyer, increaseRate);
            if(_multiplyer == maxMultiplyer)
            {
                increaseSpin = false;
            }
        }

        if (true)
        {
            InnerRingSpin(_multiplyer);
            MiddleRingSpin(_multiplyer);
            OuterRingSpin(_multiplyer);
        }
        ringOuterParent.transform.LookAt(playerT);
    }

    IEnumerator Blink()
    {
        allowShoot = false;
        for (int i = 0; i < timesToBlink; i++)
        {
            
            GetComponentInChildren<MeshRenderer>().enabled = false;
            ringInner.GetComponent<MeshRenderer>().enabled = false;
            ringMiddle.GetComponent<MeshRenderer>().enabled = false;
            ringOuter.GetComponent<MeshRenderer>().enabled = false;
            eye.GetComponent<MeshRenderer>().enabled = false;
            eyeBase.GetComponent<MeshRenderer>().enabled = false;
            seal.GetComponent<MeshRenderer>().enabled = false;

            yield return new WaitForSeconds(timeForBlink);

            
            GetComponentInChildren<MeshRenderer>().enabled = true;
            ringInner.GetComponent<MeshRenderer>().enabled = true;
            ringMiddle.GetComponent<MeshRenderer>().enabled = true;
            ringOuter.GetComponent<MeshRenderer>().enabled = true;
            eye.GetComponent<MeshRenderer>().enabled = true;
            eyeBase.GetComponent<MeshRenderer>().enabled = true;
            seal.GetComponent<MeshRenderer>().enabled = true;

            yield return new WaitForSeconds(timeForBlink);
        }
        distortion.SetActive(true);
        Debug.Log("After other wait");

        GetComponentInChildren<MeshRenderer>().enabled = false;
        ringInner.GetComponent<MeshRenderer>().enabled = false;
        ringMiddle.GetComponent<MeshRenderer>().enabled = false;
        ringOuter.GetComponent<MeshRenderer>().enabled = false;
        eye.GetComponent<MeshRenderer>().enabled = false;
        eyeBase.GetComponent<MeshRenderer>().enabled = false;
        seal.GetComponent<MeshRenderer>().enabled = false;

        StartCoroutine(Teleport());

    }

    override public IEnumerator Strike()
    {

        if (!isDead && !isStriking && allowShoot && !spin)
        {

            spin = true;
            increaseSpin = true;
            yield return new WaitForSeconds(timeSpentSpinning);
            spin = false;
            resetSpin = true;
            increaseSpin = false;
            

            isStriking = true;yield return new WaitForSeconds(BulletDelay);

            if (!isDead && !spin)
            {
                Vector3 GoalPos = GunMuzzle.transform.position;
                var newBullet = Instantiate(Bullet, GoalPos, Quaternion.identity);
                newBullet.GetComponent<EnemyBullet>().Damage = strikeDamage;
                yield return new WaitForSeconds(0.1f);

                yield return new WaitForSeconds(TimeBetweenAttacks);
                isStriking = false;
            }
        }
        
    }

    override public IEnumerator Die()
    {
        isDead = true;

        Player.Instance.GetComponentInChildren<Gun>().AddDeadeye(25);

        int i = 0;

        foreach (GameObject BodyPart in BodyParts)
        {
            BodyPart.gameObject.GetComponent<MeshCollider>().enabled = true;
            BodyPart.gameObject.GetComponent<Rigidbody>().useGravity = true;
            BodyPart.gameObject.GetComponent<Rigidbody>().isKinematic = false;

            i++;
        }

        foreach (GameObject Thing in ThingsToDestroy)
        {
            Destroy(Thing);

        }

        Destroy(this.gameObject.GetComponent<Animator>());
        yield return new WaitForSeconds(5);

        for (int LoopVar = 0; LoopVar < 50; LoopVar++)
        {
            foreach (GameObject BodyPart in BodyParts)
            {
                BodyPart.GetComponent<Rigidbody>().isKinematic = true;
                Vector3 TempPos = BodyPart.gameObject.transform.position;
                BodyPart.gameObject.transform.position = new Vector3(TempPos.x, TempPos.y - 0.025f, TempPos.z);

            }
            yield return new WaitForSeconds(0.00001f);
        }

        Destroy(this.gameObject);
    }

    IEnumerator WaitForDistortion()
    {
        Debug.Log("Waiting for Distortion");
        
        allowTeleport = false;
        yield return new WaitForSeconds(timeForDistortion);
        
        allowTeleport = true;
    }

    IEnumerator Teleport()
    {
        Debug.Log("Before wait");
        distortion.gameObject.SetActive(true);

        yield return new WaitForSeconds(timeForDistortion);
        distortion.gameObject.SetActive(false);

        Debug.Log("After wait");
            float closestDist = 1000;
            int locNum = -1;
            //find closest teleport location

        for (int i = 0; i < teleLoc.Length; i++)
        {
            if (Vector3.Distance(playerT.position, teleLoc[i].position) < closestDist)
            {
                Debug.Log("closestDist = " + closestDist);
                closestDist = Vector3.Distance(playerT.position, teleLoc[i].position);
                locNum = i;
            }
        }

        transform.position = teleLoc[locNum].transform.position;

        distortion.gameObject.SetActive(true);
        yield return new WaitForSeconds(timeForDistortion);
        distortion.gameObject.SetActive(false);
        GetComponentInChildren<MeshRenderer>().enabled = true;
        ringInner.GetComponent<MeshRenderer>().enabled = true;
        ringMiddle.GetComponent<MeshRenderer>().enabled = true;
        ringOuter.GetComponent<MeshRenderer>().enabled = true;
        eye.GetComponent<MeshRenderer>().enabled = true;
        eyeBase.GetComponent<MeshRenderer>().enabled = true;
        seal.GetComponent<MeshRenderer>().enabled = true;

        allowBlink = true;
        allowShoot = true;
        
    }

    float CheckDistance()
    {
        return Vector3.Distance(playerT.position, this.transform.position);
    }

    bool CheckSight()
    {
        if (Physics.Raycast(transform.position, playerT.position, out var hit))
        {
            return true;
        }
        else
            return false;
    }

    void InnerRingSpin(float multiplyer)
    {
        ringInner.transform.Rotate(new Vector3(0, _degreesPerSec * Time.deltaTime * multiplyer, _degreesPerSec * Time.deltaTime * .75f * multiplyer));
    }

    void MiddleRingSpin(float multiplyer)
    {
        ringMiddle.transform.Rotate(new Vector3(0, 0, _degreesPerSec * Time.deltaTime * multiplyer));
    }

    void OuterRingSpin(float multiplyer)
    {
        ringOuter.transform.Rotate(new Vector3(_degreesPerSec * Time.deltaTime * .5f * multiplyer, 0, 0));
    }

    public override void hit(int dam, Color damColor)
    {
        base.hit(dam, damColor);
        if(dam >= Gun.instance.baseDamage * 2)
        {
            ChangeColor();
        }
    }

    private void SetColorAndTag()
    {
        foreach (Material m in Resources.LoadAll<Material>("BalloonMats"))
        {
            m.SetColor("_EmissiveColor", Gun.instance.GetColor((int)color)/10f);
        }
        gameObject.tag = color.ToString();
    }

    private void ChangeColor()
    {
        color = (bulletType)Random.Range(0, 5);
        while (!Gun.instance.isColorUnlocked((int)color))
        {
            color = (bulletType)Random.Range(0, 5);
        }
        SetColorAndTag();
    }


}
