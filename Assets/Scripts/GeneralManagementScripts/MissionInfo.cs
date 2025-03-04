using System; // Library to use serializable classes
using System.Collections.Generic; // Library to use List class
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using Newtonsoft.Json.Linq; // Library to use JSON objects

// Class to generate a random mission JSON using the DronePadCustomer objects in the scene
public class MissionInfo : MonoBehaviour
{
    // -----------------------------------------------------------------------------------------------------
    // Class to store mission details:

    [Serializable] public class Mission
    {
        
        // Variables of this class
        public string missionId; // Unique identifier of the mission
        public string arrivalDateTime; // Arrival date and time of the mission
        
        public string action; // NewMission, PickAndDelivery, ReturnToHub
        public string missionStatus; // Pending, InProgress, PackageDelivered, Completed, Canceled

        public Location deliveryLocation; // Delivery location
        public float packageWeight; // Package weight
        public int priority; // Priority of the mission
        
        public Location pickupLocation; // Pickup location
        public FlightPreferences flightPreferences; // Flight preferences of the mission (Route planning, avoid zones, etc.)

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to represent location details:

    [Serializable] public class Location
    {
        
        // Variables of this class
        public float latitude; // Unity z-axis
        public float longitude; // Unity x-axis
        public float altitude; // Unity y-axis
        public float azimuth; // Orientation angle

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to store the drone's flight preferences:

    [Serializable] public class FlightPreferences
    {
        public float maxVelocity;
        public float maxAltitude;
        public List<Location> initialPath;
        public List<AvoidZone> avoidZones;
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to store the avoid zones:
    [Serializable]
    public class AvoidZone
    {
        public float latitude;
        public float longitude;
        public float radius;
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to store the drone's information:

    [Serializable]
    public class DroneInfo
    {
        public string playerName;
        public string currentTime;
        public Location positionOrientation;
        public float batteryLevel;
        public string currentState;
        public string missionId;
        public string missionStatus;
    }

    // -----------------------------------------------------------------------------------------------------
    // Function to encode the mission details into a JSON string:

    public static string EncodeMissionInfo(Mission mission)
    {
        return JObject.FromObject(mission).ToString();
    }

    // -----------------------------------------------------------------------------------------------------
    // Function to decode the mission details from a JSON string:

    public static Mission DecodeMissionInfo(string jsonMessage)
    {
        
        // Check if the JSON message is empty or null
        if (string.IsNullOrEmpty(jsonMessage))
        {
            Debug.LogError("Mission is empty or null.");
            return null;
        }

        // Try to parse the JSON message
        try
        {
            return JObject.Parse(jsonMessage).ToObject<Mission>();
        }
        catch (Exception e)
        {
            Debug.LogError("Error decoding JSON message: " + e.Message); // Log the error message if an exception occurs
            return null;
        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Function to encode the mission details into a JSON string:

    public static string EncodeDroneInfo(DroneInfo droneInfo)
    {
        return JObject.FromObject(droneInfo).ToString();
    }

    // -----------------------------------------------------------------------------------------------------
    // Function to decode the mission details from a JSON string:

    public static DroneInfo DecodeDroneInfo(string jsonMessage)
    {
        
        // Check if the JSON message is empty or null
        if (string.IsNullOrEmpty(jsonMessage))
        {
            Debug.LogError("DroneInfo is empty or null.");
            return null;
        }

        // Try to parse the JSON message
        try
        {
            return JObject.Parse(jsonMessage).ToObject<DroneInfo>();
        }
        catch (Exception e)
        {
            Debug.LogError("Error decoding JSON message: " + e.Message); // Log the error message if an exception occurs
            return null;
        }
        
    }
    
}
