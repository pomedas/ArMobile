using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Touch a plane to place a single object. Keep pressing and drag to move it.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
    public GameObject m_PlacedPrefab;
    public GameObject spawnedObject;

    ARRaycastManager m_RaycastManager;
    static readonly List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // isPressed (not wasPressedThisFrame): the object follows the finger while dragging
        if (Pointer.current == null || !Pointer.current.press.isPressed)
            return;

        // Ignore touches on UI buttons
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 screenPosition = Pointer.current.position.ReadValue();

        if (!m_RaycastManager.Raycast(screenPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            return;

        Pose hitPose = s_Hits[0].pose;

        if (spawnedObject == null)
            spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
        else
            spawnedObject.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
    }
}
