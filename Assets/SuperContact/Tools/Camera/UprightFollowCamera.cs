using UnityEngine;

public class UprightFollowCamera : MonoBehaviour {

    private const float None = -999f;

	public float mouseRotateSpeed = 0.15f;
    public float mouseRotateSmoothFactor = 3f;
    public float mouseZoomSpeed = 1f;
	public float mouseZoomSmoothFactor = 3f;
    public float translateSmoothFactor = 1f;
    public float rotateSmoothFactor = 1f;
    public float overHeadDistance = 1f;
    public GameObject followTarget;
	public float targetDistance = 6;
    public float targetAngleH = 0f;
    public float targetAngleV = 0f;
    public float maxAngle = 80;
	public float minAngle = -5;

	public GameObject cameraLight;
	
	private float currentDistance;
    private float currentAngleH;
    private float currentAngleV;
    private Vector3 currentFollowPoint;
    private Vector3 relativePosition;
    private float mouseXold = None;
	private float mouseYold = None;

	void Awake() {
        // Set target angle from initial position, but not target distance.
		relativePosition = transform.position - followTarget.transform.position;
		Vector3 eularAngles = Quaternion.LookRotation(-relativePosition, Vector3.up).eulerAngles;
		targetAngleH = eularAngles.y;
		targetAngleV = eularAngles.x;
        currentAngleH = targetAngleH;
        currentAngleV = targetAngleV;
		currentDistance = relativePosition.magnitude;
		currentFollowPoint = followTarget.transform.position + overHeadDistance * Vector3.up;
    }

    void LateUpdate() {
        UpdateRelativePosition();
        UpdateFollowPoint();
        UpdateRotation();
        if (cameraLight != null) {
            cameraLight.transform.position = transform.position;
            cameraLight.transform.rotation = transform.rotation;
        }
    }

    public void ChangeTarget(GameObject newTarget, Vector3 newPosition, bool immediate = false) {
		followTarget = newTarget;
		Vector3 newRelativePos = newPosition - followTarget.transform.position;
        Quaternion newRotation = Quaternion.LookRotation(-newRelativePos, Vector3.up);
        Vector3 eular = newRotation.eulerAngles;
		targetAngleH = eular.y;
		targetAngleV = eular.x;
		if (targetAngleV > 180f) {
			targetAngleV-=360f;
		}
		targetDistance = newRelativePos.magnitude;

        if (immediate) {
            transform.position = newPosition;
            transform.rotation = newRotation;
            currentDistance = targetDistance;
            currentAngleH = targetAngleH;
            currentAngleV = targetAngleV;
            currentFollowPoint = followTarget.transform.position + overHeadDistance * Vector3.up;
        }
	}

	void UpdateRelativePosition() {
		if (Input.GetButton("MouseClick")) {
			if (mouseXold != None) {
				targetAngleH += (Input.mousePosition.x - mouseXold) * mouseRotateSpeed;
				targetAngleV -= (Input.mousePosition.y - mouseYold) * mouseRotateSpeed;
                targetAngleV = Mathf.Clamp(targetAngleV, minAngle, maxAngle);
			}
			mouseXold = Input.mousePosition.x;
			mouseYold = Input.mousePosition.y;
		} else {
			mouseXold = None;
			mouseYold = None;
		}
        Quaternion oldAngle = Quaternion.Euler(currentAngleV, currentAngleH, 0);
        currentAngleH = Mathf.Lerp(currentAngleH, targetAngleH, mouseRotateSmoothFactor * Time.deltaTime);
        currentAngleV = Mathf.Lerp(currentAngleV, targetAngleV, mouseRotateSmoothFactor * Time.deltaTime);
        Quaternion newAngle = Quaternion.Euler(currentAngleV, currentAngleH, 0);

        float deltaMouseWheel = Input.GetAxis("Mouse ScrollWheel");
		if (deltaMouseWheel != 0) {
			targetDistance *= Mathf.Exp(-deltaMouseWheel * mouseZoomSpeed);
		}
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, mouseZoomSmoothFactor * Time.deltaTime);

        Quaternion deltaRotation = newAngle * Quaternion.Inverse(oldAngle);
		transform.rotation = deltaRotation * transform.rotation;
		relativePosition = newAngle * Vector3.back * currentDistance;
	}

    void UpdateFollowPoint() {
        Vector3 targetPoint = followTarget.transform.position + overHeadDistance * Vector3.up;
        currentFollowPoint = Vector3.Lerp(currentFollowPoint, targetPoint, Time.deltaTime * translateSmoothFactor);
        transform.position = currentFollowPoint + relativePosition;
    }

    void UpdateRotation() {
        Vector3 targetPoint = followTarget.transform.position + overHeadDistance * Vector3.up;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPoint - transform.position, Vector3.up), Time.deltaTime * rotateSmoothFactor);
    }
}
