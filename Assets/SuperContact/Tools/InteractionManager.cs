using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour {

    public Camera currentCamera;
    public GraphicRaycaster graphicRaycaster;
    public bool raycast3DGraphics = true;

    private InteractableObject hoveringObject;
    private Dictionary<int, InteractableObject> interactingObjects = new Dictionary<int, InteractableObject>();

    private void Update() {
        InteractableObject interactable = null;
        Vector3 worldPos = Vector3.zero;

        // Raycast UI
        if (graphicRaycaster != null) {
            var clickEvent = new PointerEventData(null);
            clickEvent.position = Input.mousePosition;
            var results = new List<RaycastResult>();
            graphicRaycaster.Raycast(clickEvent, results);
            if (results.Count > 0) {
                interactable = results[0].gameObject?.GetComponentInParent<InteractableObject>();
                worldPos = results[0].worldPosition;
            }
        }

        // Raycast 3D Graphics
        if (interactable == null && raycast3DGraphics) {
            Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hitInfo);
            interactable = hitInfo.collider?.GetComponentInParent<InteractableObject>();
            worldPos = hitInfo.point;
        }

        if (hoveringObject != null && interactable != hoveringObject) {
            hoveringObject.onPointerExit?.Invoke(new PointerData(Vector3.zero, Input.mousePosition, -1 /* button */));
        }
        if (interactable != null && interactable != hoveringObject) {
            interactable.onPointerEnter?.Invoke(new PointerData(worldPos, Input.mousePosition, -1 /* button */));
        }
        hoveringObject = interactable;

        for (int i = 0; i <= 2; i++) {
            if (Input.GetMouseButtonDown(i)) {
                if (interactable != null) {
                    interactable.onPointerDown?.Invoke(new PointerData(worldPos, Input.mousePosition, i));
                    interactingObjects[i] = interactable;
                }
            }
            if (Input.GetMouseButtonUp(i)) {
                if (interactable != null) {
                    interactable.onPointerUp?.Invoke(new PointerData(worldPos, Input.mousePosition, i));
                    if (interactingObjects.ContainsKey(i) && interactingObjects[i] == interactable) {
                        interactable.onPointerClick?.Invoke(new PointerData(worldPos, Input.mousePosition, i));
                    }
                }
                interactingObjects.Remove(i);
            }
        }
    }
}

[Serializable]
public class PointerEvent : UnityEvent<PointerData> { }

[Serializable]
public class PointerData {
    public Vector3 worldPos;
    public Vector3 screenPos;
    public int button;

    public PointerData(Vector3 worldPos, Vector3 screenPos, int button) {
        this.worldPos = worldPos;
        this.screenPos = screenPos;
        this.button = button;
    }
}