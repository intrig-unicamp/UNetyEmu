using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using System.Collections.Generic; // Library to use lists
using System.Linq; // Library to use OrderBy
using System; // Library to use in DateTime class

// Class to manage the internal logistics of the drone
public class DroneInternalLogistics2 : MonoBehaviour
{

    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Internal class to store the mission information to show in the Inspector
    [Serializable] public class missionsDictionary
    {
        public string missionInfo;
        public int priority;
        public string status;
    }
    
    // Internal dictionary to store the mission information
    public Dictionary<string, string> internalMissionsDatabase;

    // Flag to assign a new mission
    public bool canAssignNewMission = true; 

    // Flag to identify if the mission is completed
    public bool missionFlagCompleted = false;

    // Flag to communicate the mission completed
    public bool missionCompletedFlagCommunication = false;

    // Flag to communicate the mission "DeliverPackage"
    public bool missionDeliverPackageFlagCommunication = false;
    
    // Variable to store the current mission
    public string currentMission;

    // Variable to store the current mission status
    public string currentMissionStatus;

    // List of states to complete during a mission
    public List<string> statesToComplete = new List<string>();
    
    // Get the DroneCurrentState component
    public DroneCurrentState droneCurrentState;

    // List of missions to be displayed in the Inspector
    public List<missionsDictionary> missionList;

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Get the DroneCommunication component
    private DroneSimpleCommunication droneCommunication;

    // Flag to identify if the drone is in the "StandBy" state
    private bool flagStateStandBy = true;

    // Flag to identify if the drone is in the "DeliverPackage" state
    private bool flagStateDeliverPackage = true;

    // List of pending missions
    private List<string> pendingMissions;

    // Get the DroneJoinedToPackage component
    private DroneJoinedToPackage droneJoinedToPackage;

    // Counter of FixedUpdate times
    private int countFixedUpdate;

    // Variable to store the priority of the mission
    private int getPriority;

    // Variable to store the status of the mission
    private string getStatus;

    // Variable to store the decoded mission for the Inspector
    private MissionInfo.Mission decodeMissionForInspector;

    // Variable to store the decoded mission received
    private MissionInfo.Mission decodeMissionReceived;

    // Variable to store the decoded mission related to the status
    private MissionInfo.Mission decodeMissionStatus;

    // Variable to store the decoded mission related to the completed mission
    private MissionInfo.Mission decodeMissionCompleted;

    // Variable to store the decoded mission related to the status of the mission
    private MissionInfo.Mission decodeMissionGetStatus;

    // Variable to store the decoded mission related to the priority of the mission
    private MissionInfo.Mission decodeMissionGetPriority;

    // Variable to store the decoded mission related to the set mission
    private MissionInfo.Mission decodeSetMission;

    // Variable to store the message before updating the mission status
    private string updateMissionStatus;

    // Variable to store the message before updating the mission completed
    private string updateMissionCompleted;

    // Variable to store the message before updating the set mission
    private string updateSetMission;

    // Variable to store the mission received
    private string missionReceived;

    // Variable to get the object package to be anchored to the drone
    private GameObject package;

    // Variable to store the initial position of the drone for take-off and landing
    private Vector3 initialDronePosition;

    // Variable to store the initial orientation of the drone for take-off and landing
    private float initialDroneOrientation;

    // Variable to store the delivery location
    private MissionInfo.Location newMissionDeliveryLocation;

    // Get the DroneStep variable from the DroneCurrentState component
    private DroneCurrentState.DroneStep subTask;

    // Variable to assign the half of the maximum velocity for take-off and landing
    private float halfMaxVelocity;

    // Variable to assign the slow velocity for pick-up and delivery
    private float slowVelocity = 0.3f;
    
    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the DroneCommunication component
        droneCommunication = GetComponent<DroneSimpleCommunication>();

        // Initialize the internal database of missions dictionary
        internalMissionsDatabase = new Dictionary<string, string>();

        // Initialize the current mission
        currentMission = "";

        // Initialize the current mission status
        currentMissionStatus = "";

        // Get the DroneCurrentState component
        droneCurrentState = GetComponent<DroneCurrentState>();
        
        // Initialize the list of pending missions
        pendingMissions = new List<string>();

        // Initialize the list of missions to be displayed in the Inspector
        missionList = new List<missionsDictionary>();

        // Initialize the counter of FixedUpdate
        countFixedUpdate = 0;

        // Get the DroneJoinedToPackage component
        droneJoinedToPackage = GetComponent<DroneJoinedToPackage>();

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // Check if there are new missions
        CheckNewMission();

        // Check the status of the current mission
        CheckStatusMission();

        // Check if the mission is completed
        CheckMissionCompleted();

        // Check if there are pending missions
        CheckPendingMissions();
        
        // Clear the mission list that will be displayed in the Inspector
        missionList.Clear();

        // Display the task list in the Inspector
        foreach (var missionFound in internalMissionsDatabase)
        {
            
            // Decode the mission information
            decodeMissionForInspector = MissionInfo.DecodeMissionInfo(missionFound.Value);

            // Add the mission to the list
            missionList.Add(new missionsDictionary { 
                missionInfo = decodeMissionForInspector.missionId, 
                priority = decodeMissionForInspector.priority, 
                status = decodeMissionForInspector.missionStatus 
            });

        }

        // Get the current mission status
        currentMissionStatus = GetStatus(currentMission);
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to check if there are new missions:

    void CheckNewMission()
    {
        
        // If droneCommunication is not null
        if(droneCommunication != null)
        {

            // If there are missions in the list
            if (droneCommunication.missionReceivedGeneralList.Count > 0)
            {
                
                // For each mission in the list
                for (int i = droneCommunication.missionReceivedGeneralList.Count - 1; i >= 0; i--)
                {
                    
                    // Get the mission
                    missionReceived = droneCommunication.missionReceivedGeneralList[i];

                    // Decode the mission received
                    decodeMissionReceived = MissionInfo.DecodeMissionInfo(missionReceived);

                    // Add the mission to the internal database
                    internalMissionsDatabase.Add(decodeMissionReceived.missionId, missionReceived);

                    // Remove the mission from the droneCommunication list
                    droneCommunication.missionReceivedGeneralList.RemoveAt(i);

                }

            }
            
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to check the status of the current mission:

    void CheckStatusMission()
    {
        
        // If there are states to complete
        if(statesToComplete.Count > 0)
        {
            
            // Identify only once if the current status is "DeliverPackage"
            if( (droneCurrentState.currentStateString == "DeliverPackage") && flagStateDeliverPackage )
            {
                flagStateDeliverPackage = false; // Reset the flag to continue
                return;
            }

            // Identify when the current status is different from "DeliverPackage"
            if( (droneCurrentState.currentStateString != "DeliverPackage") && (!flagStateDeliverPackage) )
            {

                // Decode the current mission information
                decodeMissionStatus = MissionInfo.DecodeMissionInfo(internalMissionsDatabase[currentMission]);

                // Update the mission status to "PackageDelivered"
                decodeMissionStatus.missionStatus = "PackageDelivered";

                // Encode the updated mission status
                updateMissionStatus = MissionInfo.EncodeMissionInfo(decodeMissionStatus);

                // Update the mission status in the internal database
                internalMissionsDatabase[currentMission] = updateMissionStatus;
                
                // Reset the flag for the DronCommunication component
                missionDeliverPackageFlagCommunication = true;
                
                // Reset the flag for DeliverPackage
                flagStateDeliverPackage = true;

            }
            
            // Identify only once if the current status is "StandBy"
            if( (droneCurrentState.currentStateString != "StandBy") || flagStateStandBy )
            {
               flagStateStandBy = false; // Reset the flag to continue
               return;
            }

            // Identify when the current status is different from "StandBy"
            if( (droneCurrentState.currentStateString == "StandBy") && (!flagStateStandBy) )
            {
  
                // Print the mission completed in the console
                Debug.Log($"Drone: {gameObject.name}. Mission Completed: {currentMission}");
                
                // Activate the flag to move on to the next mission
                missionFlagCompleted = true;

                // Clear the internal route planning
                droneCurrentState.internalRoutePlanning.Clear();

                // Reset the current step internal index
                droneCurrentState.currentStepInternalIndex = 0;

                // Clear the states to complete
                statesToComplete.Clear();
                
                // Reset the flag for StandBy
                flagStateStandBy = true;

                // Reset the flag for the DronCommunication component
                missionCompletedFlagCommunication = true;

            }

        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to check if the mission is completed:

    void CheckMissionCompleted()
    {

        // Wait for missionFlagCompleted, before moving forward
        if (!canAssignNewMission && missionFlagCompleted)
        {
            
            // If the mission is in the internal database
            if (internalMissionsDatabase.ContainsKey(currentMission))
            {           
                
                // Decode the mission information
                decodeMissionCompleted = MissionInfo.DecodeMissionInfo(internalMissionsDatabase[currentMission]);

                // Update the mission status to "Completed"
                decodeMissionCompleted.missionStatus = "Completed";

                // Encode the updated mission status
                updateMissionCompleted = MissionInfo.EncodeMissionInfo(decodeMissionCompleted);

                // Update the mission status in the internal database
                internalMissionsDatabase[currentMission] = updateMissionCompleted;

            }

            // Reset the flag to process the next mission
            missionFlagCompleted = false; 

            // Reset the flag to assign a new mission
            canAssignNewMission = true;

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to check if there are pending missions:

    void CheckPendingMissions()
    {

        // Check if there are pending missions and if we can process the current one
        if(internalMissionsDatabase.Count > 0)
        {

            // Filter the pending missions
            pendingMissions = internalMissionsDatabase
                .Where(e => GetStatus(e.Key) == "Pending") // Filter by status "Pending"
                .OrderBy(e => GetPriority(e.Key)) // Sort by priority
                .Select(e => e.Key) // Select the mission ID
                .ToList();

            // If there are pending missions            
            if (pendingMissions.Count > 0)
            {
                
                // Check if we can assign a new mission
                if(canAssignNewMission)
                {

                    // Wait for 5 FixedUpdate times before advancing. Just to ensure that all variables are updated
                    countFixedUpdate++;
                    
                    // After the 5 FixedUpdate times have passed
                    if(countFixedUpdate > 5)
                    {
                        
                        // Set the new mission
                        currentMission = pendingMissions[0];

                        // Activate the flag to move on to the next mission
                        flagStateStandBy = true;

                        // Set the new mission                        
                        setMission();

                        // Reset the flag to not assign new missions
                        canAssignNewMission = false;
                        
                        // Reset the counter of FixedUpdate times
                        countFixedUpdate = 0;

                    }

                }

            }

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to get the priority of the mission:

    int GetPriority(string keyMessage)
    {
        
        // Initialize the priority
        getPriority = 0;

        // If the mission is in the internal database
        if (internalMissionsDatabase.ContainsKey(keyMessage))
        {
            
            // Decode the mission information
            decodeMissionGetPriority = MissionInfo.DecodeMissionInfo(internalMissionsDatabase[keyMessage]);

            // Get the priority
            getPriority = decodeMissionGetPriority.priority;

        }

        // Return the priority
        return getPriority;

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to get the status of the mission:
    
    string GetStatus(string keyMessage)
    {
        
        // Initialize the status
        getStatus = "";

        // If the mission is in the internal database
        if (internalMissionsDatabase.ContainsKey(keyMessage))
        {
            
            // Decode the mission information
            decodeMissionGetStatus = MissionInfo.DecodeMissionInfo(internalMissionsDatabase[keyMessage]);

            // Get the status
            getStatus = decodeMissionGetStatus.missionStatus;

        }

        // Return the status
        return getStatus;

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set a new mission:

    void setMission()
    {
        
        // Decode the mission information
        decodeSetMission = MissionInfo.DecodeMissionInfo(internalMissionsDatabase[currentMission]);

        // If the action is "PickAndDelivery"
        if(decodeSetMission.action == "PickAndDelivery"){

            // Assign the subtasks to complete the action "PickAndDelivery"
            statesToComplete = ifActionPickAndDelivery(decodeSetMission);

        }

        // Here you can add more actions to assign subtasks to complete the missions
        // ...

        // If the mission is in the internal database
        if (internalMissionsDatabase.ContainsKey(currentMission))
        {           
            
            // Update the mission status to "InProgress"
            decodeSetMission.missionStatus = "InProgress";

            // Encode the updated mission status
            updateSetMission = MissionInfo.EncodeMissionInfo(decodeSetMission);

            // Update the mission status in the internal database
            internalMissionsDatabase[currentMission] = updateSetMission;

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to assign the states to complete for the PickAndDelivery action:
    
    List<string> ifActionPickAndDelivery(MissionInfo.Mission decodeMission)
    {
        
        // Identify the package to be anchored to the drone
        package = GameObject.Find(decodeMission.missionId);

        // If the package is found. Anchor the package to the drone
        if(package != null) droneJoinedToPackage.objectPackage = package.transform;
        
        // Identify the initial and final position and orientation of the drone
        initialDronePosition = new Vector3(
            transform.position.x,
            transform.position.y,
            transform.position.z
        );
        initialDroneOrientation = transform.rotation.eulerAngles.y; 

        // Identify the delivery location
        newMissionDeliveryLocation = new MissionInfo.Location
        {
            latitude = decodeMission.deliveryLocation.latitude,
            longitude = decodeMission.deliveryLocation.longitude,
            altitude = decodeMission.deliveryLocation.altitude,
            azimuth = decodeMission.deliveryLocation.azimuth
        };

        // Set the half of the maximum velocity for take-off and landing only
        halfMaxVelocity = decodeMission.flightPreferences.maxVelocity / 2f;
        
        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.TakeOff, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[0].longitude, 
                decodeMission.flightPreferences.initialPath[0].altitude,
                decodeMission.flightPreferences.initialPath[0].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[0].azimuth, // Target orientation
            halfMaxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("TakeOff"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.MoveToCheckPoint, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[1].longitude, 
                decodeMission.flightPreferences.initialPath[1].altitude,
                decodeMission.flightPreferences.initialPath[1].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[1].azimuth, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("MoveToCheckPoint"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.MoveToPickupPackage, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[2].longitude, 
                decodeMission.flightPreferences.initialPath[2].altitude,
                decodeMission.flightPreferences.initialPath[2].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[2].azimuth, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("MoveToPickupPackage"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.Land, // State
            new Vector3(
                decodeMission.pickupLocation.longitude, 
                decodeMission.pickupLocation.altitude + 1f,
                decodeMission.pickupLocation.latitude
            ), // Target position
            decodeMission.pickupLocation.azimuth, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("Land"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.PickUpPackage, // State
            new Vector3(
                decodeMission.pickupLocation.longitude, 
                decodeMission.pickupLocation.altitude,
                decodeMission.pickupLocation.latitude
            ), // Target position
            decodeMission.pickupLocation.azimuth, // Target orientation
            slowVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("PickUpPackage"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.TakeOff, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[3].longitude, 
                decodeMission.flightPreferences.initialPath[3].altitude,
                decodeMission.flightPreferences.initialPath[3].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[3].azimuth, // Target orientation
            halfMaxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("TakeOff"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.MoveToCheckPoint, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[4].longitude, 
                decodeMission.flightPreferences.initialPath[4].altitude,
                decodeMission.flightPreferences.initialPath[4].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[4].azimuth, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("MoveToCheckPoint"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.MoveToDelivery, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[5].longitude, 
                decodeMission.flightPreferences.initialPath[5].altitude,
                decodeMission.flightPreferences.initialPath[5].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[5].azimuth, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("MoveToDelivery"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.Land, // State
            new Vector3(
                newMissionDeliveryLocation.longitude, 
                newMissionDeliveryLocation.altitude + 1f,
                newMissionDeliveryLocation.latitude
            ), // Target position
            newMissionDeliveryLocation.azimuth, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("Land"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.DeliverPackage, // State
            new Vector3(
                newMissionDeliveryLocation.longitude, 
                newMissionDeliveryLocation.altitude,
                newMissionDeliveryLocation.latitude
            ), // Target position
            newMissionDeliveryLocation.azimuth, // Target orientation
            slowVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("DeliverPackage"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.TakeOff, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[6].longitude, 
                decodeMission.flightPreferences.initialPath[6].altitude,
                decodeMission.flightPreferences.initialPath[6].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[6].azimuth, // Target orientation
            halfMaxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("TakeOff"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.MoveToCheckPoint, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[7].longitude, 
                decodeMission.flightPreferences.initialPath[7].altitude,
                decodeMission.flightPreferences.initialPath[7].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[7].azimuth, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("MoveToCheckPoint"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.ReturnToHub, // State
            new Vector3(
                decodeMission.flightPreferences.initialPath[8].longitude, 
                decodeMission.flightPreferences.initialPath[8].altitude,
                decodeMission.flightPreferences.initialPath[8].latitude
            ), // Target position
            decodeMission.flightPreferences.initialPath[8].azimuth, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("ReturnToHub"); // Add the subtask to the states to complete

        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.Land, // State
            new Vector3(
                initialDronePosition.x,
                initialDronePosition.y,
                initialDronePosition.z
            ), // Target position
            initialDroneOrientation, // Target orientation
            decodeMission.flightPreferences.maxVelocity // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("Land"); // Add the subtask to the states to complete
        
        // Assign a new subtask to complete the action
        subTask = new DroneCurrentState.DroneStep(
            DroneCurrentState.DroneState.StandBy, // State
            new Vector3(
                initialDronePosition.x,
                initialDronePosition.y,
                initialDronePosition.z
            ), // Target position
            initialDroneOrientation, // Target orientation
            0f // Maximum velocity
        );
        droneCurrentState.internalRoutePlanning.Add(subTask); // Add the subtask to the internal route planning
        statesToComplete.Add("StandBy"); // Add the subtask to the states to complete

        // Return the states to complete
        return statesToComplete;
        
    }

}
