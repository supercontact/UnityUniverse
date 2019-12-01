using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FocusManager : MonoBehaviour {

    public static readonly string Default = "Default";

    public static string currentFocus = Default;

    public Camera currentCamera;
    public GraphicRaycaster graphicRaycaster;
    public bool raycast3DGraphics = true;
    public bool resetFocusWithEmptyClick = true;

    private void Update() {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            FocusableObject focusable;

            // Raycast UI
            if (graphicRaycaster != null) {
                var clickEvent = new PointerEventData(null);
                clickEvent.position = Input.mousePosition;
                var results = new List<RaycastResult>();
                graphicRaycaster.Raycast(clickEvent, results);
                if (results.Count > 0) {
                    focusable = results[0].gameObject?.GetComponentInParent<FocusableObject>();
                    if (focusable != null) {
                        focusable.Focus();
                        return;
                    }
                }
            }

            // Raycast 3D Graphics
            if (raycast3DGraphics) {
                Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
                Physics.Raycast(ray, out RaycastHit hitInfo);
                focusable = hitInfo.collider?.GetComponentInParent<FocusableObject>();
                if (focusable != null) {
                    focusable.Focus();
                    return;
                }
            }

            if (resetFocusWithEmptyClick) {
                currentFocus = Default;
            }
        }
    }
}
