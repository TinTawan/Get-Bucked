using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SlotWheel : MonoBehaviour
{
    [SerializeField] HandCursor hand;

    [SerializeField] float torqueVal = 50f, maxDrag = 1f, finalAngularVelocityThreshold = 0.5f, baseSnapSpeed = 1f, snapDelta = 0.5f;
    [SerializeField] Rigidbody wheel1rb, wheel2rb, wheel3rb;

    bool isSpinning, wheel1Snapping, wheel2Snapping, wheel3Snapping;

    [SerializeField] GameObject lever;
    [SerializeField] Transform leverEnd;
    GameObject grabSphere;
    Vector3 grabSpherePos, initLeverRotation;
    bool handlePulled;
    [SerializeField] Vector3 leverRotateTo = new(-70f, 0, 0);

    private void Start()
    {
        StartCoroutine(StartTorque());

        initLeverRotation = lever.transform.localEulerAngles;
        
    }

    private void Update()
    {
        PullLever();
    }

    private void FixedUpdate()
    {
        SnapWheelFaces();
        GrabLever();
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
        yield return new WaitForSeconds(Random.Range(1, 3));
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

    }

    void SnapWheelFaces()
    {
        if (isSpinning)
        {
            if (!wheel1Snapping && wheel1rb.angularVelocity.magnitude < finalAngularVelocityThreshold)
            {
                wheel1Snapping = true;
                StartCoroutine(SnapToFace(wheel1rb));
            }
            if (!wheel2Snapping && wheel2rb.angularVelocity.magnitude < finalAngularVelocityThreshold)
            {
                wheel2Snapping = true;
                StartCoroutine(SnapToFace(wheel2rb));
            }
            if (!wheel3Snapping && wheel3rb.angularVelocity.magnitude < finalAngularVelocityThreshold)
            {
                wheel3Snapping = true;
                StartCoroutine(SnapToFace(wheel3rb));
            }
        }
    }


    void GrabLever()
    {
        if (hand.GetIsClicking())
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(hand.GetMousePos());
            if (Physics.Raycast(ray, out hit))
            {
                if(grabSphere == null) grabSphere = hit.collider.gameObject;

                Vector3 grabSpherePos = new(grabSphere.transform.position.x, hand.transform.position.y, grabSphere.transform.position.z);
                //grabSphere.transform.position = grabSpherePos;
                Rigidbody rb = grabSphere.GetComponent<Rigidbody>();
                rb.MovePosition(grabSpherePos);

                if(grabSpherePos.y < 1.7f)
                {
                    handlePulled = true;

                }
            }
        }
        else
        {
            if(grabSphere != null)
            {
                grabSphere.transform.position = leverEnd.position;

                handlePulled = false;

            }

        }
        
    }

    void PullLever()
    {
        if(handlePulled)
        {
            Debug.Log(GetWheelAngle(lever.transform));

            //lever.transform.rotation = Quaternion.FromToRotation(initLeverRotation, leverRotateTo);
            //Mathf.Clamp(lever.transform.rotation.x, -6f, -70f);
        }
    }


}

