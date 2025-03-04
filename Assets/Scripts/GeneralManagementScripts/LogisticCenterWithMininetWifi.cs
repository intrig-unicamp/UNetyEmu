using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using System; // Library to use in DateTime class
using System.Collections; // Library to use in IEnumerator class
using System.Collections.Generic; // Library to use in List and Dictionary classes

// Class to manage the logistic center and generate missions for the drones
public class LogisticCenterWithMininetWifi : MonoBehaviour
{

    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Variable to store the decoded unassigned mission
    public MissionGenerator.NewMission decodedUnassignedMission;

    // Variable to store the decoded mission info    
    public MissionInfo.Mission decodeMissionInfo;

    // Variable to store the pick-up location of the mission
    public MissionInfo.Location pickUpLocation;

    // Variable to set the drone's speed, considered as a maximum velocity
    public float setDroneSpeed;
    
    // Dictionary to store the message from the LogisticCenter to the BaseStation
    public Dictionary<string, string> messageLogisticCenterToBaseStation = new Dictionary<string, string>();

    // Dictionary to store the message from the BaseStation to the LogisticCenter
    public Dictionary<string, string> messageBaseStationToLogisticCenter = new Dictionary<string, string>();

    // Get the BaseStationMininetWifi script to get the flag from MininetWifi
    public BaseStationMininetWifi baseStationMininetWifi;

    // Flag to indicate if MininetWifi is ready
    public bool flagFromMininetWifi;

    // Variable to store the base station game object name
    public string baseStationGameObjectName = "BaseStation";

    // List to store the drones' names
    public List<string> dronesName;

    // List to store the drones' names in the database
    public List<string> dronesNameInDataBase;

    // List to store the drones' names on a mission
    [SerializeField] public List<String> droneOnMission;

    // Flag to indicate if the mission interval is dynamic    
    public bool dynamicMissionInterval;

    // Variable to store the mission interval time
    public float missionIntervalTime;
    
    // List to store the generated missions
    [SerializeField] public List<String> generatedMissionsList;

    // Flag to indicate the first message
    public bool firstMessageFlag = true;

    // Variable to store the total number of drones in the scene
    public int totalNumberOfDrones;

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Variable to store the encoded mission info to send to the BaseStation
    private string encodeMissionInfo;

    // Dictionary to store the players' info during the simulation
    private Dictionary<string, List<string>> playersInfoDuringSimulationDataBase;

    // Dictionary to store the created missions' info
    private Dictionary<string, string> createdMissionsInfoDataBase;

    // List to store the unassigned missions
    private List<String> unassignedMissions;

    // Variable to store the new mission variable string
    private string newMissionString;
    
    // Flag to indicate mission generation
    private bool isGeneratingMission = false; 

    // Game object to store the random drone pad package
    private GameObject randomDronePadPackage;

    // Game object to store the package prefab
    private GameObject prefabPackage;

    // Game object to store the packages
    private GameObject packages;

    // List to store the drone pads with packages
    private List<String> DronePadPackageList;

    // List to store the drone pads with customers
    private List<String> DronePadCustomersList;

    // Variable to store the random index in the selection of the drone pad package
    private int randomIndex;

    // Variable for the wait time in the mission generation
    private float waitTime;

    // Variable to get the current hour of the day
    private int currentHour;

    // Variable to save the player's assigned mission
    private string assignMission;

    // Variable to store the player's name in the new mission
    private string playerNameInNewMission;

    // List to store the player's last message received
    private List<string> playerLastMessageReceived;

    // Variable to store the last message info in the new mission
    private string lastMessageInfoNewMission;

    // Variable to store the decoded drone info in the new mission
    private MissionInfo.DroneInfo decodeDroneInfoCheckStatus;

    // Game object to store the assigned package
    private GameObject assignedPackage;

    // Variable to store the decoded mission info in the created mission stage
    private MissionInfo.DroneInfo decodeDroneInfoInCreateMission;

    // Game object to store the player object
    private GameObject playerObject;

    // Variable to store the get object features
    private GetObjectFeatures getObjectFeatures;

    // Variable to store the get drone features
    private GetDroneFeatures getDroneFeatures;

    // Variable to store the set pick-up location
    private MissionInfo.Location setPickUpLocation;

    // Variable to store the set flight preferences
    private MissionInfo.FlightPreferences setFlightPreferences;

    // Vector to store the pick-up position
    private Vector3 pickUpPosition;
    
    // Vector to store the delivery position
    private Vector3 deliveryPosition;

    // Vector to store the drone's current position
    private Vector3 droneCurrentPosition;

    // Vector to store the middle point for the drone's pick-up, used for the drone's path
    private Vector3 MiddlePointDronePickUp;

    // Vector to store the middle point for the pick-up delivery, used for the drone's path
    private Vector3 MiddlePointPickUpDelivery;

    // Vector to store the middle point for the delivery return to the hub, used for the drone's path
    private Vector3 MiddlePointDeliveryReturnToHub;

    // Vectors to store the check points for the drone's path
    private Vector3 checkPoint1;
    private Vector3 checkPoint2;
    private Vector3 checkPoint3;
    private Vector3 checkPoint4;
    private Vector3 checkPoint5;
    private Vector3 checkPoint6;
    private Vector3 checkPoint7;
    private Vector3 checkPoint8;
    private Vector3 checkPoint9;

    // Variables to store the check orientations for the drone's path
    private float checkOrientation1;
    private float checkOrientation2;
    private float checkOrientation3;
    private float checkOrientation4;
    private float checkOrientation5;
    private float checkOrientation6;
    private float checkOrientation7;
    private float checkOrientation8;
    private float checkOrientation9;

    // Variable to store the drone's flight preferences
    private MissionInfo.FlightPreferences droneFlightPreferences;

    // Variable to store the distance and azimuth between two points
    private Vector3 distanceAzimuth;

    // Variable to store the angle in radians
    private float angleInRadians;

    // Game object list to store the instantiated package
    private GameObject[] dronePadsPackage;

    // List to store the random index list
    private List<int> randomIndexList;

    // Vector to store the position of the package
    private Vector3 positionPackage;

    // Quaternion to store the rotation of the package
    private Quaternion rotationPackage;

    // Game object to store the instantiated package
    private GameObject instantiatedPackage;

    // Variable to get the package's rigidbody component
    private Rigidbody rbPackage;

    // Variable to store the decoded drone info message in the CSV file
    private MissionInfo.DroneInfo decodeDroneInfo;

    // Variable to store the message info in the CSV file
    private string messageInfo;

    // Variable to store the decoded created mission info
    private MissionInfo.Mission decodeCreatedMissionInfo;

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:
    
    void Start()
    {
        
        // Initialize the total number of drones in the scene
        totalNumberOfDrones = 0;

        // Start the coroutine to wait for the drones to be created
        StartCoroutine(WaitForDrones());
        
        // Start the coroutine to continuously generate missions:
        StartCoroutine(GenerateMissions());
        
        // When the simulation starts, create a new CSV file
        MissionLogger.StartNewSimulationLog();

        // Get the BaseStationMininetWifi script
        baseStationMininetWifi = GameObject.Find(baseStationGameObjectName).GetComponent<BaseStationMininetWifi>();

        // Initialize the new mission string
        newMissionString = "";

        // Initialize the list of unassigned missions
        unassignedMissions = new List<String>();

        // Initialize the list of drones on a mission
        droneOnMission = new List<String>();

        // Initialize the encoded mission to send to the BaseStation
        encodeMissionInfo = "";

        // Initialize the set drone speed
        setDroneSpeed = 19f; // Default drone speed

        // Initialize the flag to indicate if a mission is being generated
        isGeneratingMission = false;

        // Initialize the list of the drones' names
        dronesName = new List<string>();

        // Initialize the list of the drones' names in the database
        dronesNameInDataBase = new List<string>();

        // Initialize the dictionary to store the players' info during the simulation
        playersInfoDuringSimulationDataBase = new Dictionary<string, List<string>>();

        // Initialize the dictionary to store the created missions' info
        createdMissionsInfoDataBase = new Dictionary<string, string>();

        // Initialize the list of generated missions
        generatedMissionsList = new List<String>();

        // Initialize the game object to store the packages
        packages = GameObject.Find("Packages");

        // Initialize the list of the drone pads with packages
        DronePadPackageList = new List<String>();

        // Initialize the list of the drone pads with customers
        DronePadCustomersList = new List<String>();

        // Initialize the flag to indicate the first message
        firstMessageFlag = true;

    }

    // -----------------------------------------------------------------------------------------------------
    // Coroutine to wait for the drones to be created:

    IEnumerator WaitForDrones()
    {
        
        // Wait until the drones are created in the scene
        while (GameObject.FindGameObjectsWithTag("Drone").Length == 0)
        {
            yield return new WaitForSeconds(1);  // Wait for 1 second before checking again
        }

        // Get the total number of drones in the scene
        totalNumberOfDrones = GameObject.FindGameObjectsWithTag("Drone").Length;
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Coroutine to generate missions at regular intervals:

    IEnumerator GenerateMissions()
    {
        
        // Infinite loop to keep generating missions
        while (true) 
        {
            
            // Default wait time
            waitTime = 5; 

            // Check if the dynamic interval is enabled
            if(dynamicMissionInterval)
            {
                
                // Use a dynamic interval based on the time of day
                waitTime = Mathf.Round(GetDynamicInterval() * 100f) / 100f; 

                // Update the mission interval time for debugging
                missionIntervalTime = waitTime; 

            }
            else
            {
                
                // Check if the mission interval time is greater than 0
                if (missionIntervalTime > 0)
                {
                    waitTime = missionIntervalTime; // Use a fixed interval
                }
                else
                {
                    waitTime = 0; // Generate missions as fast as possible if 0 or negative
                }

            }

            // Activate the flag before generating the mission
            isGeneratingMission = true; 

            // Wait for the calculated interval before generating the next mission
            yield return new WaitForSeconds(waitTime);

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to calculate the dynamic interval based on the time of day:

    float GetDynamicInterval()
    {
        
        // Get the current hour of the day
        currentHour = DateTime.Now.Hour; 

        // Simulating peak hours: more orders between 12:00-14:00 and 18:00-21:00, fewer during midnight hours
        if (currentHour >= 12 && currentHour < 14)
            return UnityEngine.Random.Range(2f, 5f); // Peak hours: very frequent orders
        else if (currentHour >= 18 && currentHour < 21)
            return UnityEngine.Random.Range(3f, 7f); // Another busy period in the evening
        else if (currentHour >= 0 && currentHour < 6)
            return UnityEngine.Random.Range(20f, 40f); // Midnight: fewer orders, longer intervals
        else
            return UnityEngine.Random.Range(8f, 15f); // Normal hours: moderate order frequency
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Fixed update is called at a fixed interval:

    void FixedUpdate()
    {
        
        // Check if the first message flag is true
        if(!firstMessageFlag)
        {

            // Check if the message from the BaseStation to the LogisticCenter is not empty
            if(messageBaseStationToLogisticCenter.Count > 0)
            {
                
                // Get the names of the drones mentioned in the message
                dronesName = new List<string>(messageBaseStationToLogisticCenter.Keys);

                // Loop through the drones' names
                foreach (string droneName in dronesName)
                {
                    
                    // Check if the drone name is already in the database
                    if (playersInfoDuringSimulationDataBase.ContainsKey(droneName))
                    {
                        
                        // Add the message to the database
                        playersInfoDuringSimulationDataBase[droneName].Add(messageBaseStationToLogisticCenter[droneName]);

                    }
                    else // If the drone name is not in the database
                    {
                        
                        // Create a new list for the drone name
                        playersInfoDuringSimulationDataBase[droneName] = new List<string> { messageBaseStationToLogisticCenter[droneName] };

                    }

                    // Save the drone's info details to a CSV file
                    SaveToCSV(droneName, messageBaseStationToLogisticCenter[droneName]);

                    // Check the status of the mission
                    CheckStatusMission(messageBaseStationToLogisticCenter[droneName]);

                }

                // Clear the message from the BaseStation to the LogisticCenter
                messageBaseStationToLogisticCenter.Clear();

            }

            // Check if the flag from MininetWifi is true
            if(baseStationMininetWifi.flagMininetWifiReady)
            {
                
                // Set the new mission to the BaseStation
                NewMissionToBaseStation();

            }
            
        }
        
        // Check if a mission is being generated and while the number of generated missions is less than the total number of drones
        if ( isGeneratingMission && (generatedMissionsList.Count < totalNumberOfDrones) )
        {
            
            // Generate a new mission 
            newMissionString = MissionGenerator.GenerateMission(DronePadCustomersList);

            // Add the new mission to the list of unassigned missions
            unassignedMissions.Add(newMissionString);

            // Add the new mission to the list of generated missions
            generatedMissionsList.Add(newMissionString);

            // Reset flag after generating the mission
            isGeneratingMission = false;

        }

        // Reset the new mission string
        newMissionString = "";

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set a new mission in the BaseStation and then send it to a Drone:

    void NewMissionToBaseStation()
    {

        // Check if there are unassigned missions
        if (unassignedMissions.Count > 0)
        {

            // Assign the first unassigned mission to a drone
            assignMission = unassignedMissions[0];
 
            // Decode the unassigned mission
            decodedUnassignedMission = MissionGenerator.DecodeMessageMissionGenerator(assignMission);
          
            // Loop through the players' info
            foreach (var entry in playersInfoDuringSimulationDataBase)
            {
                
                // Get the player name and the last message received
                playerNameInNewMission = entry.Key; 
                playerLastMessageReceived = entry.Value;

                // Check if the player has received a message
                if (playerLastMessageReceived.Count > 0)
                {

                    // Get the last message received by the player
                    lastMessageInfoNewMission = playerLastMessageReceived[playerLastMessageReceived.Count - 1];

                    // Check if the player is already on a mission
                    if (messageLogisticCenterToBaseStation.ContainsValue(encodeMissionInfo) || droneOnMission.Contains(playerNameInNewMission))
                    {
                        continue;
                    }

                    // Create the mission info
                    decodeMissionInfo = CreateMissionInfo(playerNameInNewMission, lastMessageInfoNewMission, decodedUnassignedMission);

                    // Encode the mission info
                    encodeMissionInfo = MissionInfo.EncodeMissionInfo(decodeMissionInfo);

                    // Add the player to the list of drones on a mission
                    droneOnMission.Add(playerNameInNewMission);

                    // Add the mission to the list of messages from the LogisticCenter to the BaseStation
                    messageLogisticCenterToBaseStation.Add(playerNameInNewMission, encodeMissionInfo);

                    // Add the mission to the database
                    createdMissionsInfoDataBase.Add(decodeMissionInfo.missionId, encodeMissionInfo);

                }

            }

            // Remove the assigned mission from the list
            unassignedMissions.Remove(assignMission);

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to check the status of the mission:

    void CheckStatusMission(string lastMessageInfoCheckStatus)
    {

        // Decode the last message received by the drone 
        decodeDroneInfoCheckStatus = MissionInfo.DecodeDroneInfo(lastMessageInfoCheckStatus);

        // Check if the mission status is "PackageDelivered"
        if (decodeDroneInfoCheckStatus.missionStatus == "PackageDelivered") DeleteDeliveredPackageGameObject(decodeDroneInfoCheckStatus.missionId);

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to delete the delivered package game object:

    void DeleteDeliveredPackageGameObject(string packageName)
    {
        
        // Find the delivered package game object
        assignedPackage = GameObject.Find(packageName);

        // Check if the package game object exists
        if(assignedPackage != null) Destroy(assignedPackage); // Destroy the package game object

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to create the mission info:

    MissionInfo.Mission CreateMissionInfo(string playerNameInCreateMission, string lastMessageInfoInCreateMission, MissionGenerator.NewMission decodedUnassignedMission)
    {

        // Decode the last message received by the drone
        decodeDroneInfoInCreateMission = MissionInfo.DecodeDroneInfo(lastMessageInfoInCreateMission);

        // Find the player object in the scene
        playerObject = GameObject.Find(playerNameInCreateMission);

        // Get the features of the player object
        getObjectFeatures = playerObject.GetComponent<GetObjectFeatures>();
        getDroneFeatures = playerObject.GetComponent<GetDroneFeatures>();
        
        // Set the random pick-up location
        setPickUpLocation = SetRandomPickupLocation(
            decodedUnassignedMission.missionId,
            getObjectFeatures.prefabName,
            decodedUnassignedMission.packageWeight
        );
        
        // Set the flight preferences
        setFlightPreferences = SetFlightPreferences(
            decodeDroneInfoInCreateMission.positionOrientation,
            decodedUnassignedMission.deliveryLocation,
            setPickUpLocation,
            getDroneFeatures.maxAltitude,
            setDroneSpeed
        );
        
        // Create the mission info details
        decodeMissionInfo = new MissionInfo.Mission
        {
            missionId = decodedUnassignedMission.missionId,
            arrivalDateTime = decodedUnassignedMission.arrivalDateTime,
            action = "PickAndDelivery", // Default action for a new mission
            missionStatus = "Pending", // Default status for a new mission
            deliveryLocation = new MissionInfo.Location
            {
                latitude = decodedUnassignedMission.deliveryLocation.latitude,
                longitude = decodedUnassignedMission.deliveryLocation.longitude,
                altitude = decodedUnassignedMission.deliveryLocation.altitude,
                azimuth = decodedUnassignedMission.deliveryLocation.azimuth
            },
            packageWeight = decodedUnassignedMission.packageWeight,
            priority = decodedUnassignedMission.priority,
            pickupLocation = setPickUpLocation,
            flightPreferences = setFlightPreferences
        };

        // Return the mission info details
        return decodeMissionInfo;

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set the flight preferences:

    MissionInfo.FlightPreferences SetFlightPreferences(MissionInfo.Location droneCurrentPositionOrientation, MissionGenerator.Location droneDeliveryLocation, 
    MissionInfo.Location dronePickUpLocation, float droneMaxAltitude, float droneMaxVelocity)
    {

        // Get the pick-up position 
        pickUpPosition = new Vector3(
            dronePickUpLocation.longitude,
            dronePickUpLocation.altitude,
            dronePickUpLocation.latitude
        );

        // Get the delivery position
        deliveryPosition = new Vector3(
            droneDeliveryLocation.longitude,
            droneDeliveryLocation.altitude,
            droneDeliveryLocation.latitude
        );

        // Get the drone's current position
        droneCurrentPosition = new Vector3(
            droneCurrentPositionOrientation.longitude,
            droneCurrentPositionOrientation.altitude,
            droneCurrentPositionOrientation.latitude
        );

        // Calculate the middle points for the drone's path
        MiddlePointDronePickUp = (droneCurrentPosition + pickUpPosition) / 2;
        MiddlePointPickUpDelivery = (pickUpPosition + deliveryPosition) / 2;
        MiddlePointDeliveryReturnToHub = (deliveryPosition + droneCurrentPosition) / 2;

        // Calculate the check points for the drone's path
        checkPoint1 = new Vector3(droneCurrentPosition.x, droneMaxAltitude, droneCurrentPosition.z);
        checkPoint2 = new Vector3(MiddlePointDronePickUp.x, droneMaxAltitude, MiddlePointDronePickUp.z);
        checkPoint3 = new Vector3(pickUpPosition.x, droneMaxAltitude, pickUpPosition.z);
        checkPoint4 = checkPoint3;
        checkPoint5 = new Vector3(MiddlePointPickUpDelivery.x, droneMaxAltitude, MiddlePointPickUpDelivery.z);
        checkPoint6 = new Vector3(deliveryPosition.x, droneMaxAltitude, deliveryPosition.z);
        checkPoint7 = checkPoint6;
        checkPoint8 = new Vector3(MiddlePointDeliveryReturnToHub.x, droneMaxAltitude, MiddlePointDeliveryReturnToHub.z);
        checkPoint9 = new Vector3(droneCurrentPosition.x, droneMaxAltitude, droneCurrentPosition.z);

        // Calculate the orientation for the drone's path
        checkOrientation1 = GetAzimuth(checkPoint3, checkPoint1);
        checkOrientation2 = checkOrientation1;
        checkOrientation3 = checkOrientation2;
        checkOrientation4 = GetAzimuth(checkPoint6, checkPoint4);
        checkOrientation5 = checkOrientation4;
        checkOrientation6 = checkOrientation5;
        checkOrientation7 = GetAzimuth(checkPoint9, checkPoint7);
        checkOrientation8 = checkOrientation7;
        checkOrientation9 = checkOrientation8;

        // Set the drone's flight preferences
        droneFlightPreferences = new MissionInfo.FlightPreferences
        {
            maxVelocity = droneMaxVelocity,
            maxAltitude = droneMaxAltitude,
            initialPath = new List<MissionInfo.Location>
            {
                new MissionInfo.Location { latitude = checkPoint1.z, longitude = checkPoint1.x, altitude = checkPoint1.y, azimuth = checkOrientation1 },
                new MissionInfo.Location { latitude = checkPoint2.z, longitude = checkPoint2.x, altitude = checkPoint2.y, azimuth = checkOrientation2 },
                new MissionInfo.Location { latitude = checkPoint3.z, longitude = checkPoint3.x, altitude = checkPoint3.y, azimuth = checkOrientation3 },
                new MissionInfo.Location { latitude = checkPoint4.z, longitude = checkPoint4.x, altitude = checkPoint4.y, azimuth = checkOrientation4 },
                new MissionInfo.Location { latitude = checkPoint5.z, longitude = checkPoint5.x, altitude = checkPoint5.y, azimuth = checkOrientation5 },
                new MissionInfo.Location { latitude = checkPoint6.z, longitude = checkPoint6.x, altitude = checkPoint6.y, azimuth = checkOrientation6 },
                new MissionInfo.Location { latitude = checkPoint7.z, longitude = checkPoint7.x, altitude = checkPoint7.y, azimuth = checkOrientation7 },
                new MissionInfo.Location { latitude = checkPoint8.z, longitude = checkPoint8.x, altitude = checkPoint8.y, azimuth = checkOrientation8 },
                new MissionInfo.Location { latitude = checkPoint9.z, longitude = checkPoint9.x, altitude = checkPoint9.y, azimuth = checkOrientation9 }
            },
            avoidZones = new List<MissionInfo.AvoidZone>
            { 
                new MissionInfo.AvoidZone { latitude = 0, longitude = 0, radius = 0 } // To be implemented
            }
        };

        // Return the drone's flight preferences
        return droneFlightPreferences;

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to calculate the azimuth angle between two points:

    float GetAzimuth(Vector3 from, Vector3 to)
    {
        
        //  Calculate the distance and azimuth between two points
        distanceAzimuth = from - to;

        // Calculate the angle in radians
        angleInRadians = Mathf.Atan2(distanceAzimuth.z, distanceAzimuth.x);

        // Return the angle in degrees
        return 90 - angleInRadians * Mathf.Rad2Deg;

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set a random pick-up location:

    MissionInfo.Location SetRandomPickupLocation(string missionID, string prefabName, float packageWeight)
    {
        
        // Get the list of drone pads with Tag = "DronePadPackage"
        dronePadsPackage = GameObject.FindGameObjectsWithTag("DronePadPackage");

        // Initialize the random index list
        randomIndexList = new List<int>();

        // Generate a random pick-up location
        do
        {
            
            // Get a DronePadPackage object at a random index
            randomIndex = UnityEngine.Random.Range(0, dronePadsPackage.Length);
            randomDronePadPackage = dronePadsPackage[randomIndex];

            // Check if the random index is already in the list
            if (randomIndexList.Contains(randomIndex)) continue;

            // Add the random index to the list
            randomIndexList.Add(randomIndex);

        } while (DronePadPackageList.Contains(randomDronePadPackage.name) && randomIndexList.Count < dronePadsPackage.Length);

        // Add the random DronePadPackage object to the list
        DronePadPackageList.Add(randomDronePadPackage.name);
        
        // Check if there are DronePadPackage objects in the scene
        if (dronePadsPackage.Length > 0)
        {

            // Set the pick-up location to the random DronePadPackage object
            pickUpLocation = new MissionInfo.Location
            {
                latitude = randomDronePadPackage.transform.position.z,
                longitude = randomDronePadPackage.transform.position.x,
                altitude = randomDronePadPackage.transform.position.y,
                azimuth = randomDronePadPackage.transform.eulerAngles.y
            };

            // Load the package prefab based on the drone pad name
            if (prefabName.Contains("1"))
            {
                prefabPackage = Resources.Load<GameObject>("Prefabs/"+"prefabPackage1");
            }
            else
            {
                prefabPackage = Resources.Load<GameObject>("Prefabs/"+"prefabPackage2");
            }

            // Set the package position and orientation
            positionPackage = new Vector3(
                randomDronePadPackage.transform.position.x, 
                randomDronePadPackage.transform.position.y, 
                randomDronePadPackage.transform.position.z
            );

            // Set the package rotation
            rotationPackage = Quaternion.Euler(
                randomDronePadPackage.transform.eulerAngles.x, 
                randomDronePadPackage.transform.eulerAngles.y, 
                randomDronePadPackage.transform.eulerAngles.z
            );

            // Instantiate the package game object
            instantiatedPackage = Instantiate(prefabPackage, positionPackage, rotationPackage);

            // Set the package game object properties
            instantiatedPackage.transform.SetParent(packages.transform);
            instantiatedPackage.transform.name = missionID;
            instantiatedPackage.transform.tag = "Package";

            // Get the package's rigidbody component
            rbPackage = instantiatedPackage.GetComponent<Rigidbody>();

            // Set the package's mass
            rbPackage.mass = packageWeight;

        }

        // Return the pick-up location
        return pickUpLocation;

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to save the mission's historical data:

    void SaveToCSV(string playerNameInCSV, string messageValue)
    {
        
        // Decode the message
        decodeDroneInfo = MissionInfo.DecodeDroneInfo(messageValue);

        // Check if the mission ID is in the database
        if (createdMissionsInfoDataBase.ContainsKey(decodeDroneInfo.missionId))
        {
            
            // Get the message info from the database
            messageInfo = createdMissionsInfoDataBase[decodeDroneInfo.missionId];

            // Decode the message info from the database
            decodeCreatedMissionInfo = MissionInfo.DecodeMissionInfo(messageInfo);

            // Save the drone's info details to a CSV file
            MissionLogger.LogMissionEvent(
                playerNameInCSV,
                decodeDroneInfo.currentTime,
                (float)Math.Round((double)decodeDroneInfo.positionOrientation.latitude, 2),
                (float)Math.Round((double)decodeDroneInfo.positionOrientation.longitude, 2),
                (float)Math.Round((double)decodeDroneInfo.positionOrientation.altitude, 2),
                (float)Math.Round((double)decodeDroneInfo.positionOrientation.azimuth, 2),
                (float)Math.Round((double)decodeDroneInfo.batteryLevel, 2),
                decodeDroneInfo.currentState,
                decodeDroneInfo.missionId,
                decodeDroneInfo.missionStatus,
                decodeCreatedMissionInfo.arrivalDateTime,
                decodeCreatedMissionInfo.action,
                decodeCreatedMissionInfo.priority,
                (float)Math.Round((double)decodeCreatedMissionInfo.packageWeight, 2)
            );

        }
        else // If the mission ID is not in the database
        {

            // Save the drone's info details to a CSV file with default values
            MissionLogger.LogMissionEvent(
                playerNameInCSV,
                decodeDroneInfo.currentTime,
                decodeDroneInfo.positionOrientation.latitude,
                decodeDroneInfo.positionOrientation.longitude,
                decodeDroneInfo.positionOrientation.altitude,
                decodeDroneInfo.positionOrientation.azimuth,
                decodeDroneInfo.batteryLevel,
                decodeDroneInfo.currentState,
                "NaN",
                "NaN",
                "NaN",
                "NaN",
                0,
                0
            );

        }

    }   

}
