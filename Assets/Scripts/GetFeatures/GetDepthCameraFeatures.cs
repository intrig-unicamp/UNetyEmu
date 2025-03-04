using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to add a read component about the depth camera's features, according to the data in the setupObjects.json file:
public class GetDepthCameraFeatures : MonoBehaviour
{
    // Depth Camera Features:
    [Header("Depth Camera Features")]
    [ReadOnly] public string scriptName;
    [ReadOnly] public float nearClipPlane;
    [ReadOnly] public float farClipPlane;
    [ReadOnly] public float fieldOfView;

    // Render Texture Features:
    [Header("Render Texture Features")]
    [ReadOnly] public int pixelWidth;
    [ReadOnly] public int pixelHeight;
}
