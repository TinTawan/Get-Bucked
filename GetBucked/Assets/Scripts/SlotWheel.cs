using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class SlotWheel : MonoBehaviour
{
    [SerializeField] float torqueVal = 50f, maxDrag = 1f, finalAngularVelocityThreshold = 0.5f, baseSnapSpeed = 1f, snapDelta = 0.5f;
    [SerializeField] Rigidbody wheel1rb, wheel2rb, wheel3rb;

    bool isSpinning, wheel1Snapping, wheel2Snapping, wheel3Snapping;

    private void Start()
    {
        StartCoroutine(StartTorque());

    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        /*if (spinFinished)
        {
            wheel1Rotation = wheel1rb.transform.rotation.x;
            float wheel1AngVel = Mathf.Abs(wheel1rb.angularVelocity.x);
            if (wheel1AngVel < finalAngularVelocityThreshold)
            {
                Quaternion targetRot = Quaternion.Euler(TargetWheelAngle(wheel1rb), 0f, 0f);
                wheel1rb.MoveRotation(targetRot);
            }
            else
            {
                Debug.Log(wheel1AngVel);
            }
        }
        if (isSpinning)
        {
            if (!wheelSnapping && wheel1rb.angularVelocity.magnitude < finalAngularVelocityThreshold)
            {
                Debug.Log("snap");
                wheelSnapping = true;
                StartCoroutine(SnapToFace(wheel1rb));
            }
        }*/

        //Debug.Log($"AngVel magniture: {wheel1rb.angularVelocity.magnitude}");

        SnapWheelFaces();
    }

    IEnumerator StartTorque()
    {
        yield return new WaitForSeconds(.5f);
        wheel1rb.AddTorque(transform.right * -Random.Range(torqueVal - 10, torqueVal + 10), ForceMode.Impulse);
        yield return new WaitForSeconds(.5f);
        wheel2rb.AddTorque(transform.right * -Random.Range(torqueVal - 10, torqueVal + 10), ForceMode.Impulse);
        yield return new WaitForSeconds(.5f);
        wheel3rb.AddTorque(transform.right * -Random.Range(torqueVal - 10, torqueVal + 10), ForceMode.Impulse);

        StartCoroutine(SetDrag());
    }

    IEnumerator SetDrag()
    {
        yield return new WaitForSeconds(Random.Range(1, 5));
        wheel1rb.angularDrag = maxDrag;
        yield return new WaitForSeconds(.5f);
        wheel2rb.angularDrag = maxDrag;
        yield return new WaitForSeconds(.5f);
        wheel3rb.angularDrag = maxDrag;

        isSpinning = true;
    }

    float GetWheelAngle(Transform t)
    {
        Vector3 up = t.up;
        float angle = Mathf.Atan2(up.z, up.y) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        return angle;
    }

    float TargetWheelAngle(Rigidbody rb)
    {
        float normalized = (GetWheelAngle(rb.transform) + 360) % 360;
        float index = Mathf.Round(normalized / 45f) % 8;
        return index * 45f;
    }
    int WheelFinalIndex(Rigidbody rb)
    {
        float normalized = (GetWheelAngle(rb.transform) + 360) % 360;
        return (int)Mathf.Round(normalized / 45f) % 8;
    }

    IEnumerator SnapToFace(Rigidbody rb)
    {
        Quaternion startRot = rb.rotation;
        Quaternion targetRot = Quaternion.Euler(TargetWheelAngle(rb), 0f, 0f);

        float snapSpeed = Random.Range(baseSnapSpeed - snapDelta, baseSnapSpeed + snapDelta);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.fixedDeltaTime * snapSpeed;
            Quaternion smoothRot = Quaternion.Slerp(startRot, targetRot, Mathf.SmoothStep(0f, 1f, t));
            rb.MoveRotation(smoothRot);
            yield return null;
        }

        // Ensure it lands perfectly
        rb.MoveRotation(targetRot);
        rb.angularVelocity = Vector3.zero;
        //isSpinning = false;

    }

    void SnapWheelFaces()
    {
        if (isSpinning)
        {
            if (!wheel1Snapping && wheel1rb.angularVelocity.magnitude < finalAngularVelocityThreshold)
            {
                //Debug.Log("snap");
                wheel1Snapping = true;
                StartCoroutine(SnapToFace(wheel1rb));
            }
            if (!wheel2Snapping && wheel2rb.angularVelocity.magnitude < finalAngularVelocityThreshold)
            {
                //Debug.Log("snap");
                wheel2Snapping = true;
                StartCoroutine(SnapToFace(wheel2rb));
            }
            if (!wheel3Snapping && wheel3rb.angularVelocity.magnitude < finalAngularVelocityThreshold)
            {
                //Debug.Log("snap");
                wheel3Snapping = true;
                StartCoroutine(SnapToFace(wheel3rb));
            }
        }
    }
}

