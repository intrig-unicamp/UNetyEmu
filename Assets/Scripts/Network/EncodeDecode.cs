using System; // Library to use the Environment class
using UnityEngine; // Library to use the MonoBehaviour class
using Newtonsoft.Json.Linq; // Library to use the JObject and JArray classes

// Class to encode and decode JSON messages between Unity and Mininet-WiFi
public class EncodeDecode : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:
    
    // Class to get the position of the objects
    [Serializable] public class Position
    {
        public float x;
        public float y;
        public float z;

        public Position(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    // Class to store the base station and drone positions
    [Serializable] public class BaseStationData
    {
        public Position baseStationPosition;
        public Position[] drones;

        public BaseStationData(Position baseStationPosition, Position[] drones)
        {
            this.baseStationPosition = baseStationPosition;
            this.drones = drones;
        }
    }

    // Class to store the coverage radius
    [Serializable] public class CoverageData
    {
        public float coverageRadius;
    }

    // -----------------------------------------------------------------------------------------------------
    // Static class to encode the position and coverage radius as a JSON message

    public static string EncodePositionAndCoverageRadiusMessageOnce(Vector3 baseStationPosition, string label)
    {
        
        // Create the JSON object
        JObject jsonData = new JObject();
        jsonData["label"] = label;  // Add the label to the message
        JObject baseStationJson = new JObject(); // Add base station
        JObject baseStationPositionJson = new JObject(); // Add base station position

        // Add the base station position
        baseStationPositionJson["x"] = baseStationPosition.x;
        baseStationPositionJson["y"] = baseStationPosition.y;
        baseStationPositionJson["z"] = baseStationPosition.z;

        // Add the position object and ID to the base station JSON
        baseStationJson["position"] = baseStationPositionJson;
        baseStationJson["id"] = "ap1";  // Example ID for the base station

        // Add base station data to the root JSON object
        jsonData["baseStation"] = baseStationJson;

        // Create an array for drones
        JArray droneArray = new JArray();

        // Get all the drones in the scene
        GameObject[] droneObjects = GameObject.FindGameObjectsWithTag("Drone");

        // For each drone, add its position and ID to the JSON object
        foreach (GameObject drone in droneObjects)
        {
            
            // Get the drone's position
            Vector3 dronePosition = drone.transform.position;

            // Create a JSON object for the drone
            JObject droneJson = new JObject();
            JObject positionJson = new JObject();

            // Add the drone's position and ID
            positionJson["x"] = dronePosition.x;
            positionJson["y"] = dronePosition.y;
            positionJson["z"] = dronePosition.z;

            // Add the position object and ID to the drone JSON
            droneJson["position"] = positionJson;
            droneJson["id"] = drone.name;  // Use the drone's name or other unique identifier as the ID

            // Add the drone JSON object to the array
            droneArray.Add(droneJson);

        }

        // Add the drone array to the root JSON object
        jsonData["drones"] = droneArray;

        // Convert the JSON object to a string
        string jsonMessage = jsonData.ToString();
        
        // Return the JSON message
        return jsonMessage;

    }

    // -----------------------------------------------------------------------------------------------------
    // Static class to encode the base station and drone positions as a JSON message

    public static string EncodeBaseStationMessage(Vector3 baseStationPosition, string label)
    {
        
        // Create the JSON object
        JObject jsonData = new JObject(); // Create the root JSON object
        jsonData["label"] = label;  // Add the label to the message

        // Create a JSON object for the base station
        JObject baseStationJson = new JObject();
        JObject baseStationPositionJson = new JObject();

        // Add the base station position
        baseStationPositionJson["x"] = baseStationPosition.x;
        baseStationPositionJson["y"] = baseStationPosition.y;
        baseStationPositionJson["z"] = baseStationPosition.z;

        // Add the position object and ID to the base station JSON
        baseStationJson["position"] = baseStationPositionJson;
        baseStationJson["id"] = "ap1";  // Example ID for the base station

        // Add base station data to the root JSON object
        jsonData["baseStation"] = baseStationJson;

        // Convert the JSON object to a string
        string jsonMessage = jsonData.ToString();

        // Return the JSON message
        return jsonMessage;

    }

    // -----------------------------------------------------------------------------------------------------
    // Static class to encode the drone positions as a JSON message:

    public static string EncodeSingleDronePositionWithLabelAndID(GameObject drone, string label)
    {
        
        // Create the JSON object
        JObject jsonData = new JObject();
        jsonData["label"] = label;  // Add the label to the message

        // Add drone data
        JObject droneJson = new JObject();
        JObject positionJson = new JObject();

        // Get the drone's position
        Vector3 dronePosition = drone.transform.position;
        positionJson["x"] = dronePosition.x;
        positionJson["y"] = dronePosition.y;
        positionJson["z"] = dronePosition.z;

        // Add the position object and ID to the drone JSON
        droneJson["position"] = positionJson;
        droneJson["id"] = drone.name;  // Use the drone's name as the IDct
        jsonData["drone"] = droneJson;

        // Convert the JSON object to a string
        string jsonMessage = jsonData.ToString();

        // Return the JSON message
        return jsonMessage;

    }

}
