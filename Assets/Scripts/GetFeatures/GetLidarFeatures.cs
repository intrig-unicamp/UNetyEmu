using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to add a read component about the lidar's features, according to the data in the setupObjects.json file:
public class GetLidarFeatures : MonoBehaviour
{
    // Lidar Features:
    [Header("Lidar Features")]
    [ReadOnly] public string scriptName;
    [ReadOnly] public float lidarRange;
    [ReadOnly] public int numRaysHorizontal;
    [ReadOnly] public int numRaysVertical;
    [ReadOnly] public float verticalFOV;
    [ReadOnly] public float pointsPerSecond;
}
