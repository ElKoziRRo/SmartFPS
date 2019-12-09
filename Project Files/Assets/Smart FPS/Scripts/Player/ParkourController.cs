using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    public enum pType { Vault, JumpOver, ClimbUp };
    public pType parkourType;

    [Range(0, 5)] public float parkourCheckDis = 1f;
    [Range(0, 5)] public float vaultBoundMin = 1f;
    [Range(0, 5)] public float vaultBoundMax = 2f;
    [Range(0, 5)] public float jumpOverBoundZ = 2f;
    [Range(0, 5)] public float climbUpBoundY = 2f;

    public float headHitY;
    public float legHitY;
    public float halfLocalScale;
    public float heightFromHit;
    public float actualHeightFromHit;

    public bool legHitInstantiated = false;
    public bool headHitInstantiated = false;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit waistHit; // For Checking Vaulting and Jump-Over
        RaycastHit headHit; // For Checking Climbing Up

        Vector3 headRay = transform.position;
        headRay.y += GetComponent<CharacterController>().height / 2;

        if (Physics.Raycast(headRay, transform.forward, out headHit, 1))
        {
            Debug.DrawRay(headRay, transform.forward * 1, Color.red); // Debug Raw from Head

            if (!headHitInstantiated)
            {
                headHitInstantiated = true; //Stopping Spam Instantiating

                GameObject headHitGO = new GameObject("HeadHitGO"); //Instantiate a New Gameobject with Name
                headHitGO.transform.position = headHit.point; // Set Global Position
                headHitGO.transform.rotation = Quaternion.FromToRotation(Vector3.up, headHit.point); //Rotation according to HitPoint(For Further Update,not neccesarry now)
                headHitGO.transform.parent = headHit.transform; // Changing Parent to Hit Object
                headHitY = headHitGO.transform.localPosition.y; // Where Hit Ray hits 

                halfLocalScale = headHit.transform.GetComponent<MeshRenderer>().bounds.size.y / 0.5f; // Half of Local Scale
                heightFromHit = 0.5f - headHitGO.transform.localPosition.y; // Height from Edge of Object
                actualHeightFromHit = heightFromHit * ((climbUpBoundY + 0.5f) * 2); // Actual Height from edge

                StartCoroutine(DestroyHeadHitGO(0.3f, headHitGO)); //Destroying it after 0.3f for next check

                if (actualHeightFromHit <= climbUpBoundY + 0.5f) //Check if Player eligible for Climp Up
                {
                    InitParkour(pType.ClimbUp);
                }
            }
        }

        else if (Physics.Raycast(transform.position, transform.forward, out waistHit, 1))
        {
            Debug.DrawRay(transform.position, transform.forward * 1, Color.red);

            if (waistHit.transform.GetComponent<MeshRenderer>().bounds.size.z > jumpOverBoundZ - 0.05f)
            {
                InitParkour(pType.JumpOver);
            }
            else
            {
                InitParkour(pType.Vault);
            }

        }
    }

    void InitParkour(pType initPType)
    {
        if (initPType == pType.Vault) StartCoroutine(Vault());
        else if (initPType == pType.ClimbUp) StartCoroutine(ClimbUp());
        else if (initPType == pType.JumpOver) StartCoroutine(JumpOver());
    }

    IEnumerator Vault()
    {
        Debug.Log("Vaulting");
        yield return null;
    }

    IEnumerator ClimbUp()
    {
        Debug.Log("Climbing Up");
        yield return null;
    }

    IEnumerator JumpOver()
    {
        Debug.Log("Jumping Over");
        yield return null;
    }

    IEnumerator DestroyHeadHitGO(float time,GameObject headHitGO)
    {
        yield return new WaitForSeconds(time);
        headHitInstantiated = false;
        Destroy(headHitGO);
    }

}
