using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MouseLook
{
    public float XaxisSensitivity = 2f;
    public float YaxisSensitivity = 2f;
    public bool clampVerticalRotation = true;
    public float MinimumX = -90F;
    public float MaximumX = 90F;
    public bool smooth;
    public float smoothTime = 5f;

    private Quaternion playerTargetRot;
    private Quaternion cameraTargetRot;

    public void Init(Transform player, Transform camera)
    {
        playerTargetRot = player.localRotation;
        cameraTargetRot = camera.localRotation;
    }

    public void LookRotation(Transform character, Transform camera)
    {
        float yRot = Input.GetAxis("Mouse X") * XaxisSensitivity;
        float xRot = Input.GetAxis("Mouse Y") * YaxisSensitivity;

        playerTargetRot *= Quaternion.Euler(0f, yRot, 0f);
        cameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);

        if (clampVerticalRotation)
            cameraTargetRot = ClampRotationAroundXAxis(cameraTargetRot);

        if (smooth)
        {
            character.localRotation = Quaternion.Slerp(character.localRotation, playerTargetRot,
                smoothTime * Time.deltaTime);
            camera.localRotation = Quaternion.Slerp(camera.localRotation, cameraTargetRot,
                smoothTime * Time.deltaTime);
        }
        else
        {
            character.localRotation = playerTargetRot;
            camera.localRotation = cameraTargetRot;
        }
    }

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
}