using System; // Library to use serializable classes
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using System.Collections.Generic; // Library to use lists

// Class to control the communication of the base station with the drones (Unity to Unity)
public class SimpleBaseStation : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:
    
    // Send stage: Dictionary to store the message handlers of the drones
    public static Dictionary<string, Action<string>> droneMessageHandlers = new Dictionary<string, Action<string>>();

    // Get the Logistic Center script
    public LogisticCenter logisticCenterScript;

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Receive stage: Variable to store the name of the base station
    private string baseStationName;

    // Variables to store the messages that arrive when using the Invoke action
    private List<string> messageList = new List<string>(); // List of messages that arrive when using the Invoke action
    private List<string> messagesToProcess = new List<string>(); // List of messages to process
    private List<string> messageReceivedByDrones = new List<string>(); // List of messages received by the drones
    private object listLock = new object(); // Blocking for safe threads
    private int maxMessagesPerFrame = 1000; // Maximum number of messages per frame

    // List of drones available in the scene
    private List<string> dronesName;

    // Variable to count the number of messages per frame
    private int countMessagesPerFrame;

    // Variable to store the decoded drone information
    private MissionInfo.DroneInfo decodeDroneInfo;

    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Awake is called when the script instance is being loaded:

    void Awake()
    {
        
        //  Assign the GameObject name before Start()
        baseStationName = gameObject.name;

    }
    
    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the Logistic Center script
        logisticCenterScript = FindObjectOfType<LogisticCenter>();

        // Initialize the lists
        messageList = new List<string>(); // List of messages that arrive when using the Invoke action
        messagesToProcess = new List<string>(); // List of messages to process
        messageReceivedByDrones = new List<string>(); // List of messages received by the drones

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // If the flag is true, the base station waits for all drones to be ready
        if(logisticCenterScript.firstMessageFlag)
        {
            
            // Wait for all drones in the scene to send their initial messages
            WaitForDrones();
            
        }
        else // If firstMessageFlag is false, the base station sends new missions messages to the drones
        {
            
            // If there are messages to send to the drones, send them
            if(logisticCenterScript.messageLogisticCenterToBaseStation.Count > 0)
            {
                
                // Get the names of the drones
                dronesName = new List<string>(logisticCenterScript.messageLogisticCenterToBaseStation.Keys);

                // Send the messages to each drone
                foreach (string droneName in dronesName)
                {
                    
                    // Send the message to the drone
                    UnityToUnitySendMessageToDrone(droneName, logisticCenterScript.messageLogisticCenterToBaseStation[droneName]);
                    
                    // Clear the message from the shared dictionary
                    logisticCenterScript.messageLogisticCenterToBaseStation.Remove(droneName);

                }

            }

        }

        // Receive stage: Receive messages from the drones
        UnityToUnityReceiveMessageFromDrone();

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to wait for all drones to be ready:

    void WaitForDrones()
    {

        // Wait for all drones in the scene to send their initial messages
        if(logisticCenterScript.totalNumberOfDrones == messageReceivedByDrones.Count)
        {
            
            // Reset the flag of the first message and start the whole simulation
            logisticCenterScript.firstMessageFlag = false;

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Send stage: Class to send messages to the drones:

    public static void UnityToUnitySendMessageToDrone(string droneName, string message)
    {
        
        // If the drone name is in the dictionary, send the message
        if (droneMessageHandlers.ContainsKey(droneName))
        {
            // Print the message sent to the drone
            Debug.Log($"UNITY-UNITY: Message sent to {droneName}: {message}");

            // Invoke the message handler of the drone
            droneMessageHandlers[droneName]?.Invoke(message);

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Class to receive messages from the drones:

    void UnityToUnityReceiveMessageFromDrone()
    {

        // Block for copying messages and cleaning the list
        lock (listLock) 
        {
            messagesToProcess = new List<string>(messageList); // Copy the list of messages to process
            messageList.Clear(); // Clear the list of messages
        }

        // A maximum number of messages per frame is processed
        countMessagesPerFrame = Mathf.Min(messagesToProcess.Count, maxMessagesPerFrame);

        // Process the messages received
        for (int i = 0; i < countMessagesPerFrame; i++)
        {
            
            // Print the message received by the base station
            Debug.Log($"UNITY-UNITY: {baseStationName} Received message: {messagesToProcess[i]}");

            // Add the message to the list of messages received by the drones
            messageReceivedByDrones.Add(messagesToProcess[i]);

            // Decode the message received by the base station
            decodeDroneInfo = MissionInfo.DecodeDroneInfo(messagesToProcess[i]);

            // Send to Logistic Center the current status of the drones
            logisticCenterScript.messageBaseStationToLogisticCenter.Add(decodeDroneInfo.playerName,messagesToProcess[i]);

        }

        // If there are unprocessed messages, we add them back to the list
        if (messagesToProcess.Count > maxMessagesPerFrame)
        {
            
            // Block for adding messages back to the list
            lock (listLock)
            {
                
                // Add the unprocessed messages back to the list
                messageList.AddRange(messagesToProcess.GetRange(maxMessagesPerFrame, messagesToProcess.Count - maxMessagesPerFrame));

            }

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Class to enable the message handlers of the drones:

    void OnEnable()
    {
        
        // If the dictionary does not contain the base station name
        if (!DroneSimpleCommunication.baseStationMessageHandlers.ContainsKey(baseStationName))
        {
            
            // Add the new message received from the drone to the dictionary
            DroneSimpleCommunication.baseStationMessageHandlers[baseStationName] = UnityToUnityReceiveMessageFromDroneToBaseStation;

        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Class to disable the message handlers of the drones:

    void OnDisable()
    {
        
        // If the dictionary contains the base station name
        if (DroneSimpleCommunication.baseStationMessageHandlers.ContainsKey(baseStationName))
        {
            
            // Remove the corresponding message handler from the dictionary
            DroneSimpleCommunication.baseStationMessageHandlers.Remove(baseStationName);

        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Receive stage: Class to receive messages from the drones:

    void UnityToUnityReceiveMessageFromDroneToBaseStation(string message)
    {
        
        // Block for adding the message to the list
        lock (listLock)
        {
            
            // Add the new message to the list
            messageList.Add(message);

        }

    }

}
