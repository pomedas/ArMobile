# ArMobile – AR Foundation practice project

Practice project for the **Virtual & Augmented Reality** course at **CITM (UPC)**.
It is based on Unity's **AR Mobile template** and contains the **solution** to the exercises done in class:

| In class you build… | Solution scene |
|---|---|
| 1. Plane detection<br>2. Placing objects on planes (raycasting)<br>3. Anchors (and point cloud) | `Assets/Scenes/SpaceTracking.unity` |
| 4. Image tracking | `Assets/Scenes/ImageTracking.unity` |

Build each scene yourself in class following the steps below. If you get stuck, open the solution scene and compare it with yours.

---

## Requirements

| | Version |
|---|---|
| Unity | **6000.2.9f1** (Unity 6.2) with the **Android Build Support** module |
| AR Foundation | 6.2 |
| Google ARCore XR Plug-in | 6.2 |
| Input System | 1.14 (the scripts use the **new** Input System) |
| Device | Android phone with [ARCore support](https://developers.google.com/ar/devices) |

You don't need a phone to start: every exercise can be tested in the Editor with **XR Simulation**.

## Project structure

```
Assets/
├── Scenes/
│   ├── SpaceTracking.unity      ← Solution exercises 1–3: planes, raycast, anchors, point cloud
│   └── ImageTracking.unity      ← Solution exercise 4: image tracking
├── Scripts/
│   ├── PlaceOnPlane.cs          ← Exercise 2
│   ├── ARAnchorPlacer.cs        ← Exercise 3 (anchor attached to a plane)
│   ├── FreeSpaceAnchorPlacer.cs ← Exercise 3 (standalone anchor)
│   └── TrackedImages.cs         ← Exercise 4
├── Image Library/               ← ReferenceImageLibrary + images to track
├── Prefabs/                     ← Cube, Sphere, TriAxes, Point Cloud visualizer
├── Simulation Environments/     ← XR Simulation environment used for testing
└── MobileARTemplateAssets/, XR/, XRI/ ← from the AR Mobile template
```

---

## 0. Setup

1. Clone the repository and open it with **Unity Hub** (Unity 6000.2.9f1).
   - To start from scratch instead: *Unity Hub → New project → **AR Mobile** template*.
2. **File → Build Profiles** → select **Android** → **Switch Platform**.
3. **Edit → Project Settings → XR Plug-in Management**:
   - **Android** tab → check **Google ARCore**.
   - **Windows/Mac/Linux** tab → check **XR Simulation** (to test in Play mode).
4. **Window → XR → AR Foundation → XR Environment** opens the simulated environment.
   Use the dropdown in the Scene view overlay to choose the room you want to test in.

### How to test with XR Simulation

Press **Play**. You move through the simulated room like this:

| Action | Control |
|---|---|
| Look around | Hold **right mouse button** + move the mouse |
| Move | **W A S D**, **Q / E** for down / up (while holding right mouse) |
| Tap on screen | **Left click** |

Planes and images are only detected once the camera has **looked at them**, just like on a real phone.

---

## 1. Plane detection

**Solution:** `SpaceTracking`

AR Foundation detects real-world flat surfaces (floors, walls, tables) and creates an **`ARPlane`** GameObject for each one.
The planes are continuously updated as the device learns more about the environment.

**What you need**

1. Select **XR Origin (Mobile AR)** and add an **AR Plane Manager** component.
2. Assign a plane prefab to **Plane Prefab**, for example **AR Feathered Plane** (from the template), so you can *see* the detected planes.
3. **Detection Mode**: Horizontal, Vertical or both.

**Try it:** press Play and look at the floor and the wall. Planes appear and grow as you move.

> ❓ *Why does a plane grow and change shape over time? What happens if two planes end up on the same surface?*

---

## 2. Place objects on planes (Raycast)

**Solution:** `SpaceTracking` · **Script:** `PlaceOnPlane.cs`

A **raycast** shoots a ray from a screen point (the finger) into the real world and returns what it hits: planes, feature points, etc.

**What you need**

1. Add an **AR Raycast Manager** to the **XR Origin**.
2. Add the **`PlaceOnPlane`** component to the same object and assign **Placed Prefab** (for example `Prefabs/Cube`).

**How it works**

```csharp
if (m_RaycastManager.Raycast(screenPosition, s_Hits, TrackableType.PlaneWithinPolygon))
{
    Pose hitPose = s_Hits[0].pose;   // closest hit
    // first touch → Instantiate, next touches → move the same object
}
```

- `TrackableType.PlaneWithinPolygon` only hits **inside** the detected plane's boundaries.
- Keep pressing and drag to move the object.
- Touches on UI buttons are ignored (`EventSystem.current.IsPointerOverGameObject()`).

⚠️ The object is **not anchored**: it is placed at a world position, and if tracking readjusts, it can **drift**.

---

## 3. Anchors

**Solution:** `SpaceTracking` · **Scripts:** `ARAnchorPlacer.cs`, `FreeSpaceAnchorPlacer.cs`

An **anchor** (`ARAnchor`) is a point the AR system keeps **fixed in the real world**.
When the device corrects its understanding of the world (camera moves, lighting changes…), anchored content is corrected with it instead of drifting.

**What you need**

1. Add an **AR Anchor Manager** to the **XR Origin**.
2. Enable **only one** of the placement scripts (`PlaceOnPlane`, `ARAnchorPlacer`, `FreeSpaceAnchorPlacer`). They all react to the same tap.

### 3a. Anchor attached to a plane: `ARAnchorPlacer`

```csharp
ARAnchor anchor = anchorManager.AttachAnchor(hitPlane, hitPose);
Instantiate(prefabToPlace, anchor.transform);   // content is a child of the anchor
```

The anchor moves together with the plane when the plane is refined. This is the **most stable option** for content that sits on surfaces.

### 3b. Standalone anchor: `FreeSpaceAnchorPlacer`

```csharp
var anchorObject = new GameObject("FreeAnchor");
anchorObject.transform.SetPositionAndRotation(position, rotation);
ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();   // adding the component creates the anchor
```

- The raycast also accepts **feature points** (`TrackableType.FeaturePoint`), so you can place objects where there is no plane yet.
- The anchor is **not** linked to any plane.
- `ClearAnchors()` removes all of them. Connect it to a UI "Reset" button.

### 3c. Point cloud

The **AR Point Cloud Manager** on the XR Origin shows the **feature points** the device is tracking (prefab `AR Point Cloud Debug Visualizer`).
They are the points `FreeSpaceAnchorPlacer` can hit.

> ❓ *Try pointing at a white wall and at a textured poster. Where do you get more feature points? Why?*

### Best practices

| Situation | Use |
|---|---|
| Content must stay **fixed** in the room (game board, furniture, markers) | **Anchor** (`ARAnchorPlacer`) |
| Content is **moved constantly** by the user (drag, preview before placing) | **No anchor** (`PlaceOnPlane`), then anchor it when the user confirms |

---

## 4. Image tracking

**Solution:** `ImageTracking` · **Script:** `TrackedImages.cs`

AR Foundation can detect **2D images** (posters, cards, markers) and track their position and rotation.
Each detected image becomes an **`ARTrackedImage`**, and you can attach 3D content to it.

### 4.1 Reference Image Library

The library is the list of images the app can recognise.

1. *Assets → right click → Create → XR → **Reference Image Library*** (already created: `Image Library/ReferenceImageLibrary`).
2. **Add Image**, then drag a texture (PNG, JPG…) and give it a **Name**. The name is what you get in code (`referenceImage.name`).
3. Check **Specify Size** and enter the **printed width in meters** (10 cm → `0.1`, not `10`).
   The size is **required on iOS** and helps ARCore detect the image faster and at the correct scale.

### 4.2 AR Tracked Image Manager

1. Select the **XR Origin** and add an **AR Tracked Image Manager**.
2. **Serialized Library** → the `ReferenceImageLibrary`.
3. **Max Number Of Moving Images** → how many images can be tracked **while moving** at the same time.
4. **Tracked Image Prefab** (optional) → a prefab spawned automatically on **every** detected image. This is the no-code option.

### 4.3 Simulated image (XR Simulation)

To test in the Editor, the simulated environment needs an image to find:

1. **Window → XR → AR Foundation → XR Environment**. If the default environment can't be edited, **duplicate** it (overlay **+** menu → *Duplicate environment*).
2. Open the environment prefab (`Simulation Environments/DefaultSimulationEnvironment 1`) and select **Simulated Tracked Image Astronaut**.
3. In the **Simulated Tracked Image** component:
   - **Image** → **the same texture** you added to the Reference Image Library.
   - **Image Physical Size Meters** → the same size as in the library.
4. Keep the Transform **Scale** at `(1, 1, 1)`. The size comes from *Image Physical Size*.

How the object works:

```
DefaultSimulationEnvironment 1         ← root (Simulation Environment component)
└── Simulated Tracked Image Astronaut  ← Simulated Tracked Image + MeshFilter + MeshRenderer
                                          (the quad is generated from the texture; image faces local +Y)
```

⚠️ **Common mistake:** if the texture in **Simulated Tracked Image → Image** is empty or different from the one in the library, the image may still be "detected", but `referenceImage.name` is **empty**, and code that uses the name as a key fails (`ArgumentNullException`).

### 4.4 Respond to detected images: `TrackedImages.cs`

AR Foundation 6 uses the **`trackablesChanged`** event:

```csharp
void OnEnable()  => m_TrackedImageManager.trackablesChanged.AddListener(OnChanged);
void OnDisable() => m_TrackedImageManager.trackablesChanged.RemoveListener(OnChanged);

void OnChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
{
    foreach (var image in eventArgs.added)   { /* new image → spawn content as a child */ }
    foreach (var image in eventArgs.updated) { /* show/hide content depending on trackingState */ }
    foreach (var pair  in eventArgs.removed) { /* pair.Key = TrackableId, pair.Value = ARTrackedImage */ }
}
```

- The content is instantiated **as a child** of the `ARTrackedImage`, so it follows the image automatically. No need to copy the position every frame.
- On Android, an image that goes out of view is rarely `removed`; it changes to `TrackingState.Limited`. That's why the script **hides** the content when the state is not `Tracking`.

> **Coming from AR Foundation 5?** `trackedImagesChanged` / `ARTrackedImagesChangedEventArgs` are **obsolete**.
> Use `trackablesChanged.AddListener(...)` and note that `removed` is now a list of `KeyValuePair<TrackableId, ARTrackedImage>`.

---

## 5. Build for Android

1. Test in **Play mode** with XR Simulation first.
2. **File → Build Profiles** → **Scene List**: add the scene you want to build and **uncheck the others** (currently only `SpaceTracking` is in the list).
3. **Build** → choose a destination folder → you get the **`.apk`**.
4. Install it on your phone (copy the APK or use **Build And Run** with the phone connected via USB and **USB debugging** enabled).
5. For image tracking, **print** the image (or show it on another screen) at the size you set in the library.

---

## Troubleshooting

| Problem | Likely cause |
|---|---|
| No planes appear | Missing **AR Plane Manager** or plane prefab / the camera hasn't looked at the surfaces yet |
| Tapping does nothing | Missing **AR Raycast Manager** / no plane detected yet / the tap is on top of a UI element |
| Objects appear twice | More than one placement script is **enabled** |
| Image never detected in the Editor | **Simulated Tracked Image → Image** empty, or the texture is not in the Reference Image Library |
| `ArgumentNullException ... key` | `referenceImage.name` is empty (see 4.3) |
| Content stays floating when the image is lost | Handle `updated` and check `trackingState` |
| Black screen on the phone | ARCore not enabled in **XR Plug-in Management → Android**, or the phone is not ARCore compatible |

---

## References

- [AR Foundation documentation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.2/manual/index.html)
- [AR Foundation Samples (GitHub)](https://github.com/Unity-Technologies/arfoundation-samples)
- [XR Simulation](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@6.2/manual/xr-simulation/simulation.html)
