using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraStabilizer : MonoBehaviour {

    public float resetTime = 2.0f;

    void LateUpdate()
    {
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * resetTime);
    }
}
