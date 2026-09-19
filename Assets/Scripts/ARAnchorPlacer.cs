using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Tap on a detected plane to create an anchor attached to that plane,
/// and place a prefab as a child of the anchor.
/// </summary>
public class ARAnchorPlacer : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager;
    public GameObject prefabToPlace;

    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // New Input System: works with mouse (Editor / XR Simulation) and touch (device)
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
            return;

        // Ignore taps on UI buttons
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 screenPosition = Pointer.current.position.ReadValue();

        // Only planes are hit, so hits[0].trackable is always an ARPlane
        if (!raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            return;

        Pose hitPose = hits[0].pose;
        ARPlane hitPlane = hits[0].trackable as ARPlane;

        // Attaching the anchor to the plane keeps it stable when the plane is refined
        ARAnchor anchor = anchorManager.AttachAnchor(hitPlane, hitPose);

        if (anchor == null)
        {
            Debug.LogWarning("Failed to create anchor.");
            return;
        }

        Instantiate(prefabToPlace, anchor.transform);
        Debug.Log($"Anchor {anchor.trackableId} created on plane {hitPlane.trackableId}.");
    }
}
