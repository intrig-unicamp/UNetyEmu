using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to add a read component about the car's features, according to the data in the setupObjects.json file:
public class GetCarFeatures : MonoBehaviour
{
    // Car Features:
    [Header("Car Features")]
    [ReadOnly] public float unladenWeight;
    [ReadOnly] public float maxSpeedManufacturer;
}
