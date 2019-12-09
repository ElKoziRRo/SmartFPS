using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRigController : MonoBehaviour {

    public enum PlayerState { Idle, Walking, Running, OnJump };
    public PlayerState _PlayerState;

    public enum WalkBobStyle { OnlyPosition, OnlyRotation, Both };
    public WalkBobStyle _WalkBobStyle;

    public enum RunBobStyle { OnlyPosition, OnlyRotation, Both };
    public RunBobStyle _RunBobStyle;

    [Header("Walk Settings")]
    [Space(5)]
    public Vector3 PositionAmounts_w;
    public Vector3 PositionSpeeds_w;

    [Range(0, 100)] public float PositionSmooth_w;

    public Vector3 RotationAmounts_w;
    public Vector3 RotationSpeed_w;

    [Range(0, 100)] public float RotationSmooth_w;
    [Space(10)]

    [Header("Run Settings")]
    [Space(5)]

    public Vector3 PositionAmounts_r;
    public Vector3 PositionSpeeds_r;

    [Range(0, 100)] public float PositionSmooth_r;

    public Vector3 RotationAmounts_r;
    public Vector3 RotationSpeed_r;

    [Range(0, 100)] public float RotationSmooth_r;
    [Space(10)]

    [Header("Jump Settings")]
    [Space(5)]

    public Vector3 JumpStart_MaximumIncrease;
    [Range(0, 100)] public float JumpStart_InterpolationSpeed;

    public Vector3 JumpEnd_MaximumDecrease;
    [Range(0, 100)] public float JumpEnd_InterpolationSpeed;
    [Range(0, 100)] public float JumpEnd_ResetSpeed;
    [Space(10)]

    [Range(0, 50)] public float ResetSpeed;

    public Transform Target;

    private bool b_JumpCheck = false;
    private bool b_StateChanged = false;
    private bool b_PositionAffected;
    private bool b_RotationAffected;

    private Coroutine co_JumpStart;
    private Coroutine co_JumpEnd;
    private Coroutine co_Reset;

    private Vector3 v3_InitialTargetEUL;
    private Vector3 v3_InitialTargetPOS;

    void Start()
    {
        v3_InitialTargetEUL = Target.localEulerAngles;
        v3_InitialTargetPOS = Target.localPosition;

        _PlayerState = PlayerState.Idle;

        if (_WalkBobStyle == WalkBobStyle.OnlyPosition || _WalkBobStyle == WalkBobStyle.Both || _RunBobStyle == RunBobStyle.OnlyPosition || _RunBobStyle == RunBobStyle.Both)
            b_PositionAffected = true;

        if (_WalkBobStyle == WalkBobStyle.OnlyRotation || _WalkBobStyle == WalkBobStyle.Both || _RunBobStyle == RunBobStyle.OnlyRotation || _RunBobStyle == RunBobStyle.Both)
            b_RotationAffected = true;
    }

    void Update()
    {
        if (_PlayerState == PlayerState.Walking && !b_JumpCheck)
        {
            if (b_StateChanged)
            {
                b_StateChanged = false;

                if (co_Reset != null)
                    StopCoroutine(co_Reset);
            }

            WalkBob();
        }

        else if (_PlayerState == PlayerState.Running && !b_JumpCheck)
        {
            if (b_StateChanged)
            {
                b_StateChanged = false;

                if (co_Reset != null)
                    StopCoroutine(co_Reset);
            }

            RunBob();
        }
        else if (_PlayerState == PlayerState.Idle && !b_StateChanged)
        {
            b_StateChanged = true;

            if (co_Reset != null)
                StopCoroutine(co_Reset);

            co_Reset = StartCoroutine(IEReset());
        }
    }

    void WalkBob()
    {
        if (_WalkBobStyle == WalkBobStyle.OnlyPosition)
        {
            float posTargetX = Mathf.Sin(Time.time * PositionSpeeds_w.x) * PositionAmounts_w.x;
            float posTargetY = Mathf.Sin(Time.time * PositionSpeeds_w.y) * PositionAmounts_w.y;
            float posTargetZ = Mathf.Sin(Time.time * PositionSpeeds_w.z) * PositionAmounts_w.z;

            Vector3 posTarget = new Vector3(posTargetX, posTargetY, posTargetZ);

            Target.localPosition = Vector3.Lerp(Target.localPosition, posTarget, Time.deltaTime * PositionSmooth_w);
        }
        else if (_WalkBobStyle == WalkBobStyle.OnlyRotation)
        {
            float eulTargetX = Mathf.Sin(Time.time * RotationSpeed_w.x) * RotationAmounts_w.x;
            float eulTargetY = Mathf.Sin(Time.time * RotationSpeed_w.y) * RotationAmounts_w.y;
            float eulTargetZ = Mathf.Sin(Time.time * RotationSpeed_w.z) * RotationAmounts_w.z;
            Vector3 eulTarget = new Vector3(eulTargetX, eulTargetY, eulTargetZ);

            Target.localRotation = Quaternion.Lerp(Target.localRotation, Quaternion.Euler(eulTarget), Time.deltaTime * RotationSmooth_w);

        }
        else
        {
            float posTargetX = Mathf.Sin(Time.time * PositionSpeeds_w.x) * PositionAmounts_w.x;
            float posTargetY = Mathf.Sin(Time.time * PositionSpeeds_w.y) * PositionAmounts_w.y;
            float posTargetZ = Mathf.Sin(Time.time * PositionSpeeds_w.z) * PositionAmounts_w.z;
            Vector3 posTarget = new Vector3(posTargetX, posTargetY, posTargetZ);

            float eulTargetX = Mathf.Sin(Time.time * RotationSpeed_w.x) * RotationAmounts_w.x;
            float eulTargetY = Mathf.Sin(Time.time * RotationSpeed_w.y) * RotationAmounts_w.y;
            float eulTargetZ = Mathf.Sin(Time.time * RotationSpeed_w.z) * RotationAmounts_w.z;
            Vector3 eulTarget = new Vector3(eulTargetX, eulTargetY, eulTargetZ);

            Target.localPosition = Vector3.Lerp(Target.localPosition, posTarget, Time.deltaTime * PositionSmooth_w);
            Target.localRotation = Quaternion.Lerp(Target.localRotation, Quaternion.Euler(eulTarget), Time.deltaTime * RotationSmooth_w);
        }
    }

    void RunBob()
    {
        if (_RunBobStyle == RunBobStyle.OnlyPosition)
        {
            float posTargetX = Mathf.Sin(Time.time * PositionSpeeds_r.x) * PositionAmounts_r.x;
            float posTargetY = Mathf.Sin(Time.time * PositionSpeeds_r.y) * PositionAmounts_r.y;
            float posTargetZ = Mathf.Sin(Time.time * PositionSpeeds_r.z) * PositionAmounts_r.z;

            Vector3 posTarget = new Vector3(posTargetX, posTargetY, posTargetZ);

            Target.localPosition = Vector3.Lerp(Target.localPosition, posTarget, Time.deltaTime * PositionSmooth_r);
        }
        else if (_RunBobStyle == RunBobStyle.OnlyRotation)
        {
            float eulTargetX = Mathf.Sin(Time.time * RotationSpeed_r.x) * RotationAmounts_r.x;
            float eulTargetY = Mathf.Sin(Time.time * RotationAmounts_r.y) * RotationAmounts_r.y;
            float eulTargetZ = Mathf.Sin(Time.time * RotationAmounts_r.z) * RotationAmounts_r.z;
            Vector3 eulTarget = new Vector3(eulTargetX, eulTargetY, eulTargetZ);

            Target.localRotation = Quaternion.Lerp(Target.localRotation, Quaternion.Euler(eulTarget), Time.deltaTime * RotationSmooth_r);
        }
        else
        {
            float posTargetX = Mathf.Sin(Time.time * PositionSpeeds_r.x) * PositionAmounts_r.x;
            float posTargetY = Mathf.Sin(Time.time * PositionSpeeds_r.y) * PositionAmounts_r.y;
            float posTargetZ = Mathf.Sin(Time.time * PositionSpeeds_r.z) * PositionAmounts_r.z;
            Vector3 posTarget = new Vector3(posTargetX, posTargetY, posTargetZ);

            float eulTargetX = Mathf.Sin(Time.time * RotationSpeed_r.x) * RotationAmounts_r.x;
            float eulTargetY = Mathf.Sin(Time.time * PositionAmounts_r.y) * RotationAmounts_r.y;
            float eulTargetZ = Mathf.Sin(Time.time * PositionAmounts_r.z) * RotationAmounts_r.z;
            Vector3 eulTarget = new Vector3(eulTargetX, eulTargetY, eulTargetZ);

            Target.localPosition = Vector3.Lerp(Target.localPosition, posTarget, Time.deltaTime * PositionSmooth_r);
            Target.localRotation = Quaternion.Lerp(Target.localRotation, Quaternion.Euler(eulTarget), Time.deltaTime * RotationSmooth_r);
        }
    }


    public void JumpStarted()
    {
        if (co_JumpEnd != null)
            StopCoroutine(co_JumpEnd);

        co_JumpStart = StartCoroutine(IEJumpStart());
    }

    public void JumpEnded()
    {
        if (co_JumpStart != null)
            StopCoroutine(co_JumpStart);

        co_JumpEnd = StartCoroutine(IEJumpEnd());
    }

    IEnumerator IEJumpStart()
    {
        b_JumpCheck = true;

        float i = 0.0f;
        float rate = JumpStart_InterpolationSpeed;

        Vector3 targetEUL = new Vector3(Target.localEulerAngles.x - JumpStart_MaximumIncrease.x, Target.localEulerAngles.y - JumpStart_MaximumIncrease.y, Target.localEulerAngles.z - JumpStart_MaximumIncrease.z);

        Quaternion currentQ = Target.localRotation;

        Quaternion targetQ = Quaternion.Euler(targetEUL);

        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            Target.localRotation = Quaternion.Lerp(currentQ, targetQ, i);
            yield return null;
        }
    }

    IEnumerator IEJumpEnd()
    {
        float i = 0.0f;
        float rate = JumpEnd_InterpolationSpeed;

        Vector3 targetEUL = new Vector3(v3_InitialTargetEUL.x + JumpEnd_MaximumDecrease.x, v3_InitialTargetEUL.y + JumpEnd_MaximumDecrease.y, v3_InitialTargetEUL.z + JumpEnd_MaximumDecrease.z);

        Quaternion currentQ = Target.localRotation;
        Quaternion targetQ = Quaternion.Euler(targetEUL);

        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            Target.rotation = Quaternion.Lerp(currentQ, targetQ, i);
            yield return null;
        }

        i = 0;
        rate = JumpEnd_ResetSpeed;
        currentQ = Target.localRotation;
        targetQ = Quaternion.Euler(v3_InitialTargetEUL);

        while (i < 1.0f)
        {
            i += Time.deltaTime * rate;
            Target.localRotation = Quaternion.Lerp(currentQ, targetQ, i);
            yield return null;
        }

        b_JumpCheck = false;
    }

    IEnumerator IEReset()
    {
        float i = 0.0f;
        float rate = ResetSpeed;

        Quaternion currentQ = Target.localRotation;
        Vector3 currentPos = Target.localPosition;

        while (i < 1.0f)    // Run the coroutine for a desired time which is altered by the speed given.
        {
            i += Time.deltaTime * rate;
            if (b_RotationAffected)
                Target.localRotation = Quaternion.Lerp(currentQ, Quaternion.Euler(v3_InitialTargetEUL), i);
            if (b_PositionAffected)
                Target.localPosition = Vector3.Lerp(currentPos, v3_InitialTargetPOS, i);

            yield return null;
        }

        b_JumpCheck = false;
    }
}
