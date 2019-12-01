/*
 * Created by Ruoqi He 
 */

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A user-controllable camera.
/// Rotate the camera by dragging with left mouse key;
/// Zoom the camera with mouse scroll wheel;
/// Pan the camera (parallel to the view) by dragging with right mouse key;
/// Pan the camera (forward / backward) by using mouse scroll wheel when the right mouse key is pressed;
/// Reset the camera center position by pressing the mouse middle key.
/// </summary>
public class ObserveCamera : MonoBehaviour {

	public GameObject target;
	public CameraCenterScript center;
    public string focusName = "Default";
    public bool allowRotating = true;
    public bool allowPanning = true;
    public bool allowZooming = true;
	public float mouseRotationControlDistance = 500;
	public float mouseRotationFactor = 1;
	public float mousePanningFactor = 1;
	public float mouseScrollZoomingFactor = 0.1f;
	public float mouseScrollMovingFactor = 0.1f;
    public float mouseScrollMaxTickPerFrame = 2;
	public float smoothT = 0.1f;
    public bool adaptiveCrosshairSize = true;
    public float adaptiveCrosshairSizeMultiplier = 0.25f;

    public bool autoSetTargetDistanceAndRotation = true;
    public float targetDistance;
    public Quaternion targetRotation;
    public Vector3 targetOffset = Vector3.zero;

    private Camera cam;
    private FocusableInput input;

	private float distance;
    private Vector3 offset = Vector3.zero;
    private Vector3 prevMousePos;
	private int mouseMode = -1;  // 0 = rotation mode, 1 = panning mode.

    private void Awake() {
        cam = GetComponent<Camera>();
        input = new FocusableInput(focusName);
    }

    private void Start () {
        distance = (target.transform.position - transform.position).magnitude;
        transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);

        if (autoSetTargetDistanceAndRotation) {
            targetDistance = distance;
            targetRotation = transform.rotation;
        }
	}

    private void Update () {
		if (Time.unscaledDeltaTime != 0f) {
            float mouseScrollAmount = Mathf.Clamp(input.mouseScrollDelta.y, -mouseScrollMaxTickPerFrame, mouseScrollMaxTickPerFrame);

            if (allowRotating && input.GetMouseButtonDown(0)) {
				mouseMode = 0;
				center.Hide();
			} else if (allowPanning && input.GetMouseButtonDown(1)) {
				mouseMode = 1;
				center.Show();
			} else if (allowRotating && input.GetMouseButton(0) && mouseMode == 0) {
				// Rotate the camera.
				Vector3 v1 = new Vector3(prevMousePos.x - Screen.width / 2, prevMousePos.y - Screen.height / 2, -mouseRotationControlDistance);
				Vector3 v2 = new Vector3(input.mousePosition.x - Screen.width / 2, input.mousePosition.y - Screen.height / 2, -mouseRotationControlDistance);
				Quaternion rotation = Quaternion.Inverse(Quaternion.FromToRotation(v1, v2));
				rotation.ToAngleAxis(out float angle, out Vector3 axis);
				rotation = Quaternion.AngleAxis(angle * mouseRotationFactor, axis);
				targetRotation = targetRotation * rotation;
			} else if (allowPanning && input.GetMouseButton(1) && mouseMode == 1) {
				// Pan the camera.
				float distancePixelRatio = (cam.ScreenToWorldPoint(new Vector3(1, 0, targetDistance)) - cam.ScreenToWorldPoint(new Vector3(0, 0, targetDistance))).magnitude * mousePanningFactor;
				Vector3 move = new Vector3((prevMousePos.x - input.mousePosition.x) * distancePixelRatio,
				                           (prevMousePos.y - input.mousePosition.y) * distancePixelRatio,
                                           mouseScrollAmount * targetDistance * mouseScrollMovingFactor);
				targetOffset += targetRotation * move;
			} else {
				mouseMode = -1;
                center.Hide();
            }

			if (allowZooming && mouseMode != 1) {
                // Zoom the camera.
				targetDistance *= Mathf.Exp(-mouseScrollAmount * mouseScrollZoomingFactor);
			}
			if (allowPanning && input.GetMouseButtonDown(2)) {
				// Reset the camera center position.
				targetOffset = Vector3.zero;
			}

			// Smooth transition
			transform.rotation = Quaternion.Slerp(targetRotation, transform.rotation, Mathf.Exp(- Time.unscaledDeltaTime / smoothT));
			distance = Mathf.Lerp(targetDistance, distance, Mathf.Exp(- Time.unscaledDeltaTime / smoothT));
			offset = Vector3.Lerp(targetOffset, offset, Mathf.Exp(- Time.unscaledDeltaTime / smoothT));
			transform.position = target.transform.position + distance * (transform.rotation * Vector3.back) + offset;
			center.transform.localPosition = new Vector3(0, 0, distance);
            if (adaptiveCrosshairSize) {
                center.transform.localScale = Vector3.one * distance * adaptiveCrosshairSizeMultiplier;
            }

            prevMousePos = input.mousePosition;
        }
	}

    //public void OnClick(BaseEventData data) {
    //    if (directMouseInput) {
    //        throw new System.Exception("Direct mouse input mode is in use!");
    //    }
    //    PointerEventData pdata = (PointerEventData)data;
    //    clicking = pdata.button == PointerEventData.InputButton.Left;
    //}
}
