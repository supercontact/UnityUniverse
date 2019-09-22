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
    public bool directMouseInput = true;
    public bool allowPanning = true;
    public bool allowZooming = true;
	public Vector3 targetOffset = Vector3.zero;
	public float mouseRotationControlDistance = 500;
	public float mouseRotationFactor = 1;
	public float mousePanningFactor = 1;
	public float mouseScrollZoomingFactor = 0.1f;
	public float mouseScrollMovingFactor = 0.1f;
	public float smoothT = 0.1f;
    public bool adaptiveCrosshairSize = true;
    public float adaptiveCrosshairSizeMultiplier = 0.25f;

	private Camera cam;

	private Vector3 offset = Vector3.zero;
	private float targetDistance;
	private float distance;
	private Quaternion targetRotation;

	private Vector3 prevMousePos;
	private int mouseMode = -1;

	private bool clicking;

    private void Awake() {
        cam = GetComponent<Camera>();
    }

    private void Start () {
		targetDistance = (target.transform.position - transform.position).magnitude;
        distance = targetDistance;
		targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
		transform.rotation = targetRotation;
	}

    private void Update () {
		if (Time.unscaledDeltaTime != 0f) {
            if ((directMouseInput && Input.GetMouseButtonDown(0)) || clicking) {
                clicking = false;
				prevMousePos = Input.mousePosition;
				mouseMode = 0;
				center.Hide();
			} else if (allowPanning && Input.GetMouseButtonDown(1)) {
				prevMousePos = Input.mousePosition;
				mouseMode = 1;
				center.Show();
			} else if (Input.GetMouseButton(0) && mouseMode == 0) {
				// Rotate the camera.
				Vector3 v1 = new Vector3(prevMousePos.x - Screen.width / 2, prevMousePos.y - Screen.height / 2, -mouseRotationControlDistance);
				Vector3 v2 = new Vector3(Input.mousePosition.x - Screen.width / 2, Input.mousePosition.y - Screen.height / 2, -mouseRotationControlDistance);
				Quaternion rot = Quaternion.Inverse(Quaternion.FromToRotation(v1, v2));
				float angle;
				Vector3 axis;
				rot.ToAngleAxis(out angle, out axis);
				rot = Quaternion.AngleAxis(angle * mouseRotationFactor, axis);
				targetRotation = targetRotation * rot;
				prevMousePos = Input.mousePosition;
			} else if (allowPanning && Input.GetMouseButton(1) && mouseMode == 1) {
				// Pan the camera.
				float distancePixelRatio = (cam.ScreenToWorldPoint(new Vector3(1, 0, targetDistance)) - cam.ScreenToWorldPoint(new Vector3(0, 0, targetDistance))).magnitude * mousePanningFactor;
				Vector3 move = new Vector3((prevMousePos.x - Input.mousePosition.x) * distancePixelRatio,
				                           (prevMousePos.y - Input.mousePosition.y) * distancePixelRatio,
				                           Input.mouseScrollDelta.y * targetDistance * mouseScrollMovingFactor);
				targetOffset += targetRotation * move;
				prevMousePos = Input.mousePosition;
			} else {
				mouseMode = -1;
			}

			if (Input.GetMouseButtonUp(1)) {
				center.Hide();
			}

			if (allowZooming && !Input.GetMouseButton(1) || mouseMode != 1) {
				// Zoom the camera.
				targetDistance *= Mathf.Exp(- Input.mouseScrollDelta.y * mouseScrollZoomingFactor);
			}
			if (allowPanning && Input.GetMouseButtonDown(2)) {
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
		}
	}

	public void OnClick(BaseEventData data) {
        if (directMouseInput) {
            throw new System.Exception("Direct mouse input mode is in use!");
        }
		PointerEventData pdata = (PointerEventData) data;
		clicking = pdata.button == PointerEventData.InputButton.Left;
	}
}
