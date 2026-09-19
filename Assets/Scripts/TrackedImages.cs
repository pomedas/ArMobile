using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackedImages : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] GameObject prefabToSpawn;

    void Awake()
    {
        // Fallback if the reference was not assigned in the Inspector
        if (m_TrackedImageManager == null)
            m_TrackedImageManager = FindAnyObjectByType<ARTrackedImageManager>();
    }

    void OnEnable() => m_TrackedImageManager.trackablesChanged.AddListener(OnChanged);
    void OnDisable() => m_TrackedImageManager.trackablesChanged.RemoveListener(OnChanged);

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            Debug.Log($"Image added: {newImage.referenceImage.name}");

            if (prefabToSpawn == null) continue;
            var content = Instantiate(prefabToSpawn, newImage.transform); // child: follows the image
            content.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            // Hide content when the image is not actively tracked (Limited / None)
            bool visible = updatedImage.trackingState == TrackingState.Tracking;
            foreach (Transform child in updatedImage.transform)
                child.gameObject.SetActive(visible);
        }

        foreach (var pair in eventArgs.removed)
        {
            Debug.Log($"Image removed: {pair.Value.referenceImage.name}");
            // Children are destroyed along with the ARTrackedImage GameObject
        }
    }
}