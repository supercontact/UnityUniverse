using UnityEngine;
using System.Collections;

public class FlightCamera : MonoBehaviour {

    public GameObject target;
    public bool fix = false;
    public float startingSmoothTransitionSpeed = 0.2f;

    public float force = 50f;
    public float damping = 25f;
    public float rForce = 50f;
    public float rDamping = 25f;

    public Vector3 relativePosition;
    public Vector3 relativeRotationEular;
    private Quaternion relativeRotation;

    private Vector3 velocity = Vector3.zero;
    private Vector3 prevTargetPoint;
    private Vector3 angularVelocity = Vector3.zero;
    private Quaternion prevTargetRotation;

    private float factor = 0;

    // Use this for initialization
    void Awake() {
        relativeRotation = Quaternion.Euler(relativeRotationEular);
    }

    void Start() {
        prevTargetPoint = target.transform.TransformPoint(relativePosition);
        prevTargetRotation = target.transform.rotation;
    }

    // Update is called once per frame
    void LateUpdate() {
        if (fix || Time.deltaTime == 0f || target == null) {
            return;
        }

        if (factor < 1f) {
            factor += startingSmoothTransitionSpeed * Time.deltaTime;
        } else {
            factor = 1f;
        }

        Vector3 p = transform.position;
        Vector3 targetPoint = target.transform.TransformPoint(relativePosition);
        Vector3 targetVelocity = (targetPoint - prevTargetPoint) / Time.deltaTime;
        velocity += (targetPoint - p) * force * Time.deltaTime * factor;
        velocity = targetVelocity + (velocity - targetVelocity) * Mathf.Exp(-damping * Time.deltaTime);
        p += velocity * Time.deltaTime;
        transform.position = p;
        prevTargetPoint = targetPoint;
        //transform.position = Vector3.Lerp (p, target.transform.TransformPoint (relativePosition), factor);

        Quaternion r = transform.rotation;
        Quaternion targetRotation = target.transform.rotation * relativeRotation;
        Vector3 axisTemp;
        float angleTemp;
        Quaternion diff;
        diff = targetRotation * Quaternion.Inverse(prevTargetRotation);
        diff.ToAngleAxisCorrected(out angleTemp, out axisTemp);
        Vector3 targetAngularVelocity = (angleTemp / Time.deltaTime) * axisTemp;

        diff = targetRotation * Quaternion.Inverse(r);
        diff.ToAngleAxisCorrected(out angleTemp, out axisTemp);
        angularVelocity += (angleTemp * axisTemp) * rForce * Time.deltaTime * factor;
        angularVelocity = targetAngularVelocity + (angularVelocity - targetAngularVelocity) * Mathf.Exp(-rDamping * Time.deltaTime);
        r = Quaternion.AngleAxis(angularVelocity.magnitude * Time.deltaTime, angularVelocity) * r;
        transform.rotation = r;
        prevTargetRotation = targetRotation;
        //transform.rotation = Quaternion.Slerp (r, target.transform.rotation * relativeRotation, factor);
    }

    public void ImmediateJump() {
        transform.position = target.transform.TransformPoint(relativePosition);
        transform.rotation = target.transform.rotation * relativeRotation;
        velocity = Vector3.zero;
        angularVelocity = Vector3.zero;
        prevTargetPoint = target.transform.TransformPoint(relativePosition);
        prevTargetRotation = target.transform.rotation;
    }
}
