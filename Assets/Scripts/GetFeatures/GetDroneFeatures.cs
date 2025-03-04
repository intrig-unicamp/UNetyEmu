using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to add a read component about the drone's features, according to the data in the setupObjects.json file:
public class GetDroneFeatures : MonoBehaviour
{
    // Drone Features:
    [Header("Drone Features")]
    [ReadOnly] public float unladenWeight;
    [ReadOnly] public float approxMaxFlightTime;
    [ReadOnly] public float maxBatteryCapacity;
    [ReadOnly] public float batteryVoltage;
    [ReadOnly] public float batteryStartPercentage;
    [ReadOnly] public float maxAltitude;
    [ReadOnly] public float maxThrust;
    [ReadOnly] public float maxSpeedManufacturer;
    [ReadOnly] public float maximumTiltAngle;
    [ReadOnly] public float propellerMaxRPM;
}
