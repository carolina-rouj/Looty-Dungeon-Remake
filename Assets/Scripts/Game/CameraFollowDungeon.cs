using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowDungeon : MonoBehaviour
{
    private Transform target;
    private Vector3 staticFocus;
    private bool useStaticFocus;
    private Vector3 offset = new Vector3(3.92f, 10.18f, -5.01f);
    private float shakeUntil;
    private float shakePower;

    public void SetTarget(Transform followTarget)
    {
        target = followTarget;
        useStaticFocus = false;
        Snap();
    }

    public void SetStaticFocus(Vector3 focus)
    {
        target = null;
        staticFocus = focus;
        useStaticFocus = true;
        Snap();
    }

    public void Snap()
    {
        if (TryGetFocus(out Vector3 focus))
        {
            transform.position = focus + offset;
        }
    }

    public void Shake(float duration, float power)
    {
        shakeUntil = Mathf.Max(shakeUntil, Time.time + duration);
        shakePower = Mathf.Max(shakePower, power);
    }

    private void LateUpdate()
    {
        if (!TryGetFocus(out Vector3 focus))
        {
            return;
        }

        Vector3 desired = focus + offset;
        if (Time.time < shakeUntil)
        {
            desired += Random.insideUnitSphere * shakePower;
        }
        else
        {
            shakePower = 0f;
        }

        transform.position = Vector3.Lerp(transform.position, desired, 8f * Time.deltaTime);
        transform.rotation = Quaternion.Euler(58f, -38f, 0f);
    }

    private bool TryGetFocus(out Vector3 focus)
    {
        if (useStaticFocus)
        {
            focus = staticFocus;
            return true;
        }

        if (target != null)
        {
            focus = target.position;
            return true;
        }

        focus = Vector3.zero;
        return false;
    }
}

