using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Spawns a prefab on every detected image and hides it when the image
/// is not actively tracked. AR Foundation 6 API (trackablesChanged).
/// </summary>
public class TrackedImages : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] GameObject prefabToSpawn;

    void Awake()
    {
        // Fallback if the reference was not assigned in the Inspector
        if (m_TrackedImageManager == null)
            m_TrackedImageManager = FindAnyObjectByType<ARTrackedImageManager>();

        if (m_TrackedImageManager == null)
        {
            Debug.LogError("TrackedImages: no ARTrackedImageManager found in the scene.");
            enabled = false;
        }
    }

    void OnEnable()
    {
        if (m_TrackedImageManager != null)
            m_TrackedImageManager.trackablesChanged.AddListener(OnChanged);
    }

    void OnDisable()
    {
        if (m_TrackedImageManager != null)
            m_TrackedImageManager.trackablesChanged.RemoveListener(OnChanged);
    }

    void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            Debug.Log($"Image added: {GetImageName(newImage)}");

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
            Debug.Log($"Image removed: {GetImageName(pair.Value)}");
            // Children are destroyed along with the ARTrackedImage GameObject
        }
    }

    // referenceImage.name is empty when the detected image does not match any
    // entry in the Reference Image Library (e.g. a Simulated Tracked Image with no texture)
    static string GetImageName(ARTrackedImage image)
    {
        return string.IsNullOrEmpty(image.referenceImage.name)
            ? $"<unnamed, id {image.trackableId}>"
            : image.referenceImage.name;
    }
}
