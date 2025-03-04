using System; // Library to use DateTime class
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using System.Collections.Generic; // Library to use Dictionary class

// Class to control the communication of the drone
public class DroneSimpleCommunication : MonoBehaviour
{

    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Send stage: Dictionary to store the message handlers of the base stations
    public static Dictionary<string, Action<string>> baseStationMessageHandlers = new Dictionary<string, Action<string>>();

    // Receive stage: Variable to store the name of the base station Game Object
    public string baseStationName = "BaseStation";

    // Variable to store the message interval time
    public float messageIntervalTime = 5f;

    // Variable to store the drone's wifi signal
    public float droneWifiSignal; 

    // Variable to store the list of missions received by the drone
    public List<string> missionReceivedGeneralList = new List<string>();

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Variable to get the DroneInternalLogistics component
    private DroneInternalLogistics2 droneInternalLogistics;

    // Variable to get the DroneDynamics component
    private DroneDynamics droneDynamics;
    private float droneBatteryLevel;

    // Variable to get the DroneCurrentState component
    private DroneCurrentState droneCurrentState;
    private string droneState;

    // Variable to store the current position and orientation of the drone
    private MissionInfo.Location currentPositionOrientation;

    // Variable to store the decoded/encode drone information message
    private MissionInfo.DroneInfo sendDecodeDroneInfo;
    private MissionInfo.DroneInfo decodeDroneInfo;
    private string sendEncodeDroneInfo;

    // Variable to store the message received by the drone
    private string droneMessage;
    private string droneName;

    // Variable to store the flag to send a message to the base station
    private int flagSendMessage;
    
    // Variables to store the current time
    private float currentTime;
    private DateTime now;
    private string timeString;
    
    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Awake is called when the script instance is being loaded:

    void Awake()
    {
        
        //  Assign the GameObject name before Start()
        droneName = gameObject.name;

    }

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Set the initial wifi signal of the drone. In the future, this information should come from Mininet-Wifi.
        droneWifiSignal = 1f; 

        // Initialize the message received by the drone
        droneMessage = ""; 

        // Create a new list to store the received missions
        missionReceivedGeneralList = new List<string>(); 
        
        // Initialize the current time
        currentTime = messageIntervalTime;

        // Get the DroneInternalLogistics script
        droneInternalLogistics = GetComponent<DroneInternalLogistics2>();

        // Get the DroneDynamics script
        droneDynamics = GetComponent<DroneDynamics>();

        // Get the DroneCurrentState script
        droneCurrentState = GetComponent<DroneCurrentState>();

        // Initialize the flag to send a message to the base station
        flagSendMessage = 0;

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:
    
    void FixedUpdate()
    {
        
        // Receive stage: Check if the drone has received a message from the base station
        if (droneMessage != "")
        {
            
            // Print the message received by the drone
            Debug.Log($"UNITY-UNITY: {droneName} Received message: {droneMessage}");

            // Add the received message to the list of missions
            missionReceivedGeneralList.Add(droneMessage);

            // Reset the message received by the drone
            droneMessage = "";

            // Activate flag to send a message to the base station
            flagSendMessage = 1;

        }

        // Check if the mission has been completed or the package has been delivered
        if ( droneInternalLogistics.missionCompletedFlagCommunication || droneInternalLogistics.missionDeliverPackageFlagCommunication )	
        {

            // Reset the flags of the mission completed and package delivered
            droneInternalLogistics.missionCompletedFlagCommunication = false;
            droneInternalLogistics.missionDeliverPackageFlagCommunication = false;

            // Activate flag to send a message to the base station
            flagSendMessage = 1;

        }

        // Send stage: Increment the current time
        currentTime += Time.fixedDeltaTime;

        // Check if the current time is greater than the message interval time or the flag to send a message is activated    
        if ( (currentTime >= messageIntervalTime) || (flagSendMessage == 1) )
        {
            
            // Reset the current time
            currentTime = 0f;

            // Get the drone current information
            sendDecodeDroneInfo = GetDroneInfo();

            // Encode the drone information
            sendEncodeDroneInfo = MissionInfo.EncodeDroneInfo(sendDecodeDroneInfo);

            // Send a message to the base station with the drone information
            SendMessageToBaseStation(baseStationName, sendEncodeDroneInfo, sendDecodeDroneInfo.playerName);

            // Reset the flag to send a message
            flagSendMessage = 0;

        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Send stage: Class to send messages to the base stations:

    public static void SendMessageToBaseStation(string baseStationName, string message, string playerName)
    {
        
        // If the base station name is in the dictionary, send the message
        if (baseStationMessageHandlers.ContainsKey(baseStationName))
        {
            
            // Print the message sent to the base station
            Debug.Log($"UNITY-UNITY: Message sent to {baseStationName}: {message}");

            // Invoke the message handler of the base station
            baseStationMessageHandlers[baseStationName]?.Invoke(message);

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to get the drone information:

    MissionInfo.DroneInfo GetDroneInfo()
    {

        // Get the current position and orientation of the drone
        currentPositionOrientation = new MissionInfo.Location
        {
            latitude = gameObject.transform.position.z,
            longitude = gameObject.transform.position.x,
            altitude = gameObject.transform.position.y,
            azimuth = gameObject.transform.eulerAngles.y
        };

        // Get the battery level of the drone
        droneBatteryLevel = droneDynamics.batteryLevel;

        // Get the current state of the drone
        droneState = droneCurrentState.currentStateString;

        // Get the current time
        now = DateTime.Now;
        timeString = now.ToString("HH:mm:ss");

        // Create the drone information variable
        decodeDroneInfo = new MissionInfo.DroneInfo
        {
            playerName = droneName,
            currentTime = timeString,
            positionOrientation = currentPositionOrientation,
            batteryLevel = droneBatteryLevel,
            currentState = droneState,
            missionId = droneInternalLogistics.currentMission,
            missionStatus = droneInternalLogistics.currentMissionStatus
        };

        // Return the drone information variable
        return decodeDroneInfo;

    }

    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Class to enable the message handlers of the base station:

    void OnEnable()
    {
        
        // If the dictionary does not contain the drone name
        if (!SimpleBaseStation.droneMessageHandlers.ContainsKey(droneName))
        {
            
            // Add the new message received from the base station to the dictionary
            SimpleBaseStation.droneMessageHandlers[droneName] = UnityToUnityReceiveMessageFromBaseStationToDrone;

        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Class to disable the message handlers of the base station:

    void OnDisable()
    {
        
        // If the dictionary contains the drone name        
        if (SimpleBaseStation.droneMessageHandlers.ContainsKey(droneName))
        {
            
            // Remove the corresponding message handler from the dictionary
            SimpleBaseStation.droneMessageHandlers.Remove(droneName);

        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Class to receive messages from the base station:

    void UnityToUnityReceiveMessageFromBaseStationToDrone(string message)
    {
        
        // Save the message received from the base station 
        droneMessage = message;

    }

}
