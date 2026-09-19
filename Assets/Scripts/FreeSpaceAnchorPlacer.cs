using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Tap to create a standalone anchor (NOT attached to a plane).
/// A raycast hit (feature point or plane) is still needed to know the depth
/// of the tapped point; the difference with ARAnchorPlacer is that the anchor
/// is independent of any plane.
/// </summary>
public class FreeSpaceAnchorPlacer : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager; // Must exist on the XR Origin for ARAnchor components to be tracked
    public GameObject prefabToPlace;

    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    // Keep a reference to the anchors so they can be cleared later
    private readonly List<ARAnchor> placedAnchors = new List<ARAnchor>();

    void Update()
    {
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
            return;

        // Ignore taps on UI buttons
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 screenPosition = Pointer.current.position.ReadValue();

        if (!raycastManager.Raycast(screenPosition, hits, TrackableType.FeaturePoint | TrackableType.PlaneWithinPolygon))
            return;

        Vector3 position = hits[0].pose.position;

        // Feature point hits have an arbitrary orientation, so we build our own:
        // upright, facing the camera (yaw only)
        Quaternion rotation = Quaternion.identity;
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 toCamera = cam.transform.position - position;
            toCamera.y = 0f;
            if (toCamera.sqrMagnitude > 0.0001f)
                rotation = Quaternion.LookRotation(toCamera, Vector3.up);
        }

        // Adding an ARAnchor component to a GameObject creates the anchor
        var anchorObject = new GameObject("FreeAnchor");
        anchorObject.transform.SetPositionAndRotation(position, rotation);
        ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();

        placedAnchors.Add(anchor);
        Instantiate(prefabToPlace, anchor.transform);

        Debug.Log($"Free-space anchor created at {position} (hit type: {hits[0].hitType}).");
    }

    // Remove all anchors placed by this script (e.g. call it from a UI "Reset" button)
    public void ClearAnchors()
    {
        foreach (var anchor in placedAnchors)
        {
            if (anchor != null)
                Destroy(anchor.gameObject);
        }
        placedAnchors.Clear();
    }
}
