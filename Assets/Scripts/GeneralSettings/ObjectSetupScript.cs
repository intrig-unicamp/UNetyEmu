using System; // Library to use Serializable classes
using System.Collections.Generic; // Library to use List<T>
using UnityEngine; // Library to use MonoBehaviour classes

// Class to create all objects/components/features according to the data in the JSON file:
public class ObjectSetupScript : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Classes added to extract the data from JSON file:

    // Internal variable according to the data structure in the JSON file
    [System.Serializable] public class ObjectData
    {
        public string prefabName;
        public string group;
        public string type;
        public PlayerFeatures playerFeatures;
        public bool addTargetGameObject;
        public List<string> dynamicScripts;
        public List<string> algorithmScripts;
        public List<string> otherInternalScripts;
        public LidarFeatures lidarFeatures;
        public DepthCameraFeatures depthCameraFeatures;
        public CommunicationFeatures communicationFeatures;

    }

    // Internal variable of the structure called "playerFeatures"
    [System.Serializable] public class PlayerFeatures
    {
        public float unladenWeight;
        public float approxMaxFlightTime;
        public float maxBatteryCapacity;
        public float batteryVoltage;
        public float batteryStartPercentage;
        public float maxAltitude;
        public float maxThrust;
        public float maxSpeedManufacturer;
        public float maximumTiltAngle;
        public float propellerMaxRPM;
    }

    // Internal variable of the structure called "lidarFeatures"
    [System.Serializable] public class LidarFeatures
    {
        public string scriptName;
        public float lidarRange;
        public int numRaysHorizontal;
        public int numRaysVertical;
        public float verticalFOV;
        public float pointsPerSecond;
    }

    // Internal variable of the structure called "depthCameraFeatures"
    [System.Serializable] public class DepthCameraFeatures
    {
        public string scriptName;
        public float nearClipPlane;
        public float farClipPlane;
        public float fieldOfView;
        public int pixelWidth;
        public int pixelHeight;
    }

    // Internal variable of the structure called "communicationFeatures"
    [System.Serializable] public class CommunicationFeatures
    {
        public string scriptName;
    }

    // Internal variable to store all the data structure coming from the JSON file
    [System.Serializable] public class GetSetupPlayers
    {
        public ObjectData[] players;
    }
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:
    
    // Variable to store the name of the JSON file
    [Header("JSON Setup File Name")]
    public string jsonFile;

    // List of objects to add colliders
    [Header("Objects to add Box or Mesh Collider")]
    public List<string> parentObjectNameToAddCollider;
    public List<bool> TrueForBoxColliderFalseForMeshCollider;

    // List of objects to instantiate players
    [Header("Object General Features")]
    public List<string> playerGroupNames;
    public List<int> numberOfPlayersPerGroup;
    public List<string> parentObjectNameToInstantiatePlayersPerGroup;
    
    // -----------------------------------------------------------------------------------------------------
    // Private variables of the ObjectSetupScript class:

    private TextAsset jsonSetupObjectsTextFile; // Variable to store the JSON file
    private GetSetupPlayers getSetupPlayers;  // Variable to store all the data

    private int currentID = 1; // Variable to store the ID of the player

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Load JSON file from Resources
        jsonSetupObjectsTextFile = Resources.Load<TextAsset>(jsonFile);

        if (jsonSetupObjectsTextFile != null)
        {
            
            // Deserialize the JSON data
            getSetupPlayers = JsonUtility.FromJson<GetSetupPlayers>(jsonSetupObjectsTextFile.text);

            if(parentObjectNameToAddCollider.Count != TrueForBoxColliderFalseForMeshCollider.Count)
            {
                Debug.LogWarning("The number of elements in the lists ´parentObjectNameToAddCollider´, and ´TrueForBoxColliderFalseForMeshCollider´ must be the same to add the colliders");
            }
            else
            {
                AddColliders(); // Add colliders to the objects
            }

            // Only instantiate players if the number of elements in the lists are the same
            if(playerGroupNames.Count != numberOfPlayersPerGroup.Count || playerGroupNames.Count != parentObjectNameToInstantiatePlayersPerGroup.Count)
            {
                Debug.LogWarning("The number of elements in the lists ´playerGroupNames´, ´numberOfPlayersPerGroup´, and ´parentObjectNameToInstantiatePlayersPerGroup´ must be the same to instantiate players");
            }
            else
            {
                InstantiateObjects(); // Instantiate the players found in the JSON file
            }

        }
        else
        {
            Debug.LogWarning("JSON file ´" + jsonFile + "´ not found. No objects can be instantiated without a JSON file.");
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to add colliders to the objects:

    void AddColliders()
    {

        // For each object in the list
        for (int i = 0; i < parentObjectNameToAddCollider.Count; i++)
        {
            
            // Find the object in the scene
            GameObject objectToAddCollider = GameObject.Find(parentObjectNameToAddCollider[i]);

            if(objectToAddCollider != null)
            {
                
                // For each child object of the object
                for (int j = 0; j < objectToAddCollider.transform.childCount; j++)
                {
                    
                    // Find the child object
                    GameObject objectChildToAddCollider = objectToAddCollider.transform.GetChild(j).gameObject;

                    // Add the collider according to the boolean value
                    if (TrueForBoxColliderFalseForMeshCollider[i] == true)
                    {
                        if (objectChildToAddCollider.GetComponent<BoxCollider>() == null)
                        {
                            objectChildToAddCollider.AddComponent<BoxCollider>();
                        }
                    }
                    else if (TrueForBoxColliderFalseForMeshCollider[i] == false)
                    {
                        if (objectChildToAddCollider.GetComponent<MeshCollider>() == null)
                        {
                            objectChildToAddCollider.AddComponent<MeshCollider>();
                        }
                    }

                }

            }
            
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method for instantiating players in the scene:

    void InstantiateObjects()
    {
        
        // For each player in the getSetupPlayers
        foreach (ObjectData obj in getSetupPlayers.players)
        {
            
            // Load the prefab from the Resources folder
            GameObject prefab = Resources.Load<GameObject>("Prefabs/"+obj.prefabName);

            if (prefab != null)
            {
                
                // For each group in the list
                for (int i = 0; i < playerGroupNames.Count; i++)
                {
                    
                    // If the group of the player is the same as the group in the list
                    if(obj.group == playerGroupNames[i])
                    {

                        // Find the parent object in the scene to instantiate the players
                        GameObject playerStart = GameObject.Find(parentObjectNameToInstantiatePlayersPerGroup[i]);

                        if(playerStart != null)
                        {

                            // If the number of players requested is greater than the number of child objects of the parent object
                            if(numberOfPlayersPerGroup[i] > playerStart.transform.childCount)
                            {
                                Debug.Log("The ´" + numberOfPlayersPerGroup[i] + "´ requested players belonging to ´" + playerGroupNames[i] + "´, exceed the number of child objects of the parent object ´" + parentObjectNameToInstantiatePlayersPerGroup[i] + "´. Excess players will not be instantiated.");
                            }

                            // For each child object of the parent object
                            for (int j = 0; j < playerStart.transform.childCount; j++)
                            {

                                // Find the child object
                                GameObject playerStartChildObject = playerStart.transform.GetChild(j).gameObject;
                                
                                    // While the number of players requested is greater than the number of child objects of the parent object
                                    if ( (j + 1) <= numberOfPlayersPerGroup[i] )
                                    {
                                        
                                        // Create a vector for the initial position
                                        Vector3 position = new Vector3(
                                            playerStartChildObject.transform.position.x, 
                                            playerStartChildObject.transform.position.y, 
                                            playerStartChildObject.transform.position.z
                                        );

                                        // Create a vector for initial orientation
                                        Quaternion rotation = Quaternion.Euler(
                                            playerStartChildObject.transform.eulerAngles.x, 
                                            playerStartChildObject.transform.eulerAngles.y, 
                                            playerStartChildObject.transform.eulerAngles.z
                                        );

                                        // Instantiate the player in the specified position and orientation
                                        GameObject instantiatedObject = Instantiate(prefab, position, rotation);
                                        
                                        // Set all the features of the player
                                        SetObjectFeatures(instantiatedObject, obj);

                                        // If the player is a drone
                                        if(obj.type == "Drone"){
                                            playerStartChildObject.tag = "DronePadStart";
                                            instantiatedObject.name = "DRO"+currentID.ToString("D3")+playerGroupNames[i];
                                            SetDroneFeatures(instantiatedObject, obj);
                                        } 

                                        // If the player is a car
                                        if(obj.type == "Car"){
                                            playerStartChildObject.tag = "CarPadStart";
                                            instantiatedObject.name = "CAR"+currentID.ToString("D3")+playerGroupNames[i];
                                            SetCarFeatures(instantiatedObject, obj);
                                        } 

                                        // If the player has a target game object
                                        if(obj.addTargetGameObject == true) SetTargetGameObject(instantiatedObject);

                                        // If the player has internal scripts
                                        SetInternalScripts(instantiatedObject, obj);

                                        // If the player has a lidar sensor
                                        SetLidarFeatures(instantiatedObject, obj);
                                        
                                        // If the player has a depth camera sensor
                                        SetDepthCameraFeatures(instantiatedObject, obj);

                                        // If the player has communication features
                                        SetCommunicationFeatures(instantiatedObject, obj);

                                        // Add new ID number for the next player
                                        currentID++;

                                    }

                            }

                        }
                        else
                        {
                            
                            // If the parent object is not found
                            if(numberOfPlayersPerGroup[i] > 0)
                            {
                                Debug.LogWarning("Parent object ´" + parentObjectNameToInstantiatePlayersPerGroup[i] + "´ not found. No players can be instantiated without a parent object that contains child objects to extract its initial position and rotation.");
                            }

                        }

                    }
                    
                }

            }
            else
            {
                // If the prefab is not found
                Debug.LogWarning("PrefabName: ´" + obj.prefabName + "´ not found. No player can be instantiated without a prefab object.");
            }

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set the features of the player:

    void SetObjectFeatures(GameObject instantiatedObject, ObjectData obj)
    {

        // Set the tag of the instantiated player
        instantiatedObject.tag = obj.type;

        // Add a reading component of the player features. NOTE: See the GetObjectFeatures script to unify variables
        GetObjectFeatures getObjectFeatures = instantiatedObject.AddComponent<GetObjectFeatures>();
        getObjectFeatures.group = obj.group;
        getObjectFeatures.objectID = currentID.ToString("D3");
        getObjectFeatures.prefabName = obj.prefabName;
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set the features of the drone:

    void SetDroneFeatures(GameObject instantiatedObject, ObjectData obj)
    {
        
        // Set the mass in the Rigidbody
        Rigidbody rb = instantiatedObject.GetComponent<Rigidbody>();
        if(obj.playerFeatures.unladenWeight == 0) rb.mass = 15.0f;
        else rb.mass = obj.playerFeatures.unladenWeight; 
        
        // Add a reading component of the drone features. NOTE: See the GetDroneFeatures script to unify variables
        GetDroneFeatures getDroneFeatures = instantiatedObject.AddComponent<GetDroneFeatures>();
        getDroneFeatures.unladenWeight = rb.mass;

        if(obj.playerFeatures.approxMaxFlightTime == 0) getDroneFeatures.approxMaxFlightTime = 20.0f;
        else getDroneFeatures.approxMaxFlightTime = obj.playerFeatures.approxMaxFlightTime;

        if(obj.playerFeatures.maxBatteryCapacity == 0) getDroneFeatures.maxBatteryCapacity = 3000.0f;
        else getDroneFeatures.maxBatteryCapacity = obj.playerFeatures.maxBatteryCapacity;

        if(obj.playerFeatures.batteryVoltage == 0) getDroneFeatures.batteryVoltage = 11.1f;
        else getDroneFeatures.batteryVoltage = obj.playerFeatures.batteryVoltage;
        
        if(obj.playerFeatures.batteryStartPercentage == 0) getDroneFeatures.batteryStartPercentage = 100.0f;
        else getDroneFeatures.batteryStartPercentage = obj.playerFeatures.batteryStartPercentage;
        
        getDroneFeatures.maxAltitude = obj.playerFeatures.maxAltitude;

        // If the maxThrust is not set, calculate it based on the mass of the drone
        if(obj.playerFeatures.maxThrust == 0) getDroneFeatures.maxThrust = rb.mass * 2.0f * Mathf.Abs(Physics.gravity.y);
        else getDroneFeatures.maxThrust = obj.playerFeatures.maxThrust;

        getDroneFeatures.maxSpeedManufacturer = obj.playerFeatures.maxSpeedManufacturer;

        // If the maximumTiltAngle is not set, set it to 30 degrees
        if(obj.playerFeatures.maximumTiltAngle == 0) getDroneFeatures.maximumTiltAngle = 30.0f;
        else getDroneFeatures.maximumTiltAngle = obj.playerFeatures.maximumTiltAngle;
        
        getDroneFeatures.propellerMaxRPM = obj.playerFeatures.propellerMaxRPM;

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set the features of the car:

    void SetCarFeatures(GameObject instantiatedObject, ObjectData obj)
    {
        
        // Add a reading component of the car features. NOTE: See the GetCarFeatures script to unify variables
        GetCarFeatures getCarFeatures = instantiatedObject.AddComponent<GetCarFeatures>();
        getCarFeatures.unladenWeight = obj.playerFeatures.unladenWeight;
        getCarFeatures.maxSpeedManufacturer = obj.playerFeatures.maxSpeedManufacturer;

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set the target game object:

    void SetTargetGameObject(GameObject instantiatedObject){

        
        // Create a new object (sphere) to be the target of the player
        GameObject sphereTargetHolder = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Set the name and scale of the target object
        sphereTargetHolder.name = "ID"+currentID.ToString("D3")+"Target";
        sphereTargetHolder.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        // Set the position and orientation of the target object
        sphereTargetHolder.transform.position = instantiatedObject.transform.position;
        sphereTargetHolder.transform.eulerAngles = instantiatedObject.transform.eulerAngles;

        // Eliminate the collider of the target object
        Destroy(sphereTargetHolder.GetComponent<Collider>());
       
    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set the internal scripts:

    void SetInternalScripts(GameObject instantiatedObject, ObjectData obj)
    {
        
        // If the player has internal scripts
        if(obj.dynamicScripts != null)
        {

            // For each script name in the list
            foreach (string dynamic in obj.dynamicScripts)
            {
                
                // Add the component related to the script operation based on the script name
                System.Type scriptDynamic = System.Type.GetType(dynamic);

                // If the script is found, add it to the instantiated object
                if (scriptDynamic != null) instantiatedObject.AddComponent(scriptDynamic);
                else Debug.LogWarning("Player name: "+instantiatedObject.name+" | dynamicScript: ´" + dynamic + "´ not found.");

            }

        }

        // If the player has algorithm scripts
        if(obj.algorithmScripts != null)
        {

            // For each script name in the list
            foreach (string algorithm in obj.algorithmScripts)
            {
                
                // Add the component related to the algorithm operation based on the script name
                System.Type scriptAlgorithm = System.Type.GetType(algorithm);

                // If the script is found, add it to the instantiated object
                if (scriptAlgorithm != null) instantiatedObject.AddComponent(scriptAlgorithm);
                else Debug.LogWarning("Player name: "+instantiatedObject.name+" | algorithmScripts: ´" + algorithm + "´ not found.");

            }

        }
        
        // If the player has other internal scripts (e.g. animations)
        if(obj.otherInternalScripts != null)
        {

            // For each script name in the list
            foreach (string otherInternal in obj.otherInternalScripts)
            {
                
                // Add the component related to the other internal operation based on the script name
                System.Type scriptOtherInternal = System.Type.GetType(otherInternal);

                // If the script is found, add it to the instantiated object
                if (scriptOtherInternal != null) instantiatedObject.AddComponent(scriptOtherInternal);
                else Debug.LogWarning("Player name: "+instantiatedObject.name+" | otherInternalScripts: ´" + otherInternal + "´ not found.");

            }

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set the lidar features:

    void SetLidarFeatures(GameObject instantiatedObject, ObjectData obj)
    {
        
        // Find the LidarHolder object in the instantiated object
        Transform lidarHolderTransform = instantiatedObject.transform.Find("LidarHolder");

        // If the LidarHolder object is found
        if (lidarHolderTransform != null)
        {
            
            // Create a new object to be the holder of the lidar sensor
            GameObject lidarHolder = lidarHolderTransform.gameObject;

            // Add a reading component of the lidar features. NOTE: See the GetLidarFeatures script to unify variables
            GetLidarFeatures getLidarFeatures = instantiatedObject.AddComponent<GetLidarFeatures>();
            getLidarFeatures.scriptName = obj.lidarFeatures.scriptName;
            
            // If the lidar range is not set, set it to 50 meters
            if(obj.lidarFeatures.lidarRange == 0) getLidarFeatures.lidarRange = 50.0f;
            else getLidarFeatures.lidarRange = obj.lidarFeatures.lidarRange;

            // If the number of horizontal rays is not set, set it to 360
            if(obj.lidarFeatures.numRaysHorizontal == 0) getLidarFeatures.numRaysHorizontal = 360;
            else getLidarFeatures.numRaysHorizontal = obj.lidarFeatures.numRaysHorizontal;

            // If the number of vertical rays is not set, set it to 1
            if(obj.lidarFeatures.numRaysVertical == 0) getLidarFeatures.numRaysVertical = 1;
            else getLidarFeatures.numRaysVertical = obj.lidarFeatures.numRaysVertical;

            getLidarFeatures.verticalFOV = obj.lidarFeatures.verticalFOV;

            // If the number of points per second is not set, set it to 1
            if(obj.lidarFeatures.pointsPerSecond == 0) getLidarFeatures.pointsPerSecond = 1;
            else getLidarFeatures.pointsPerSecond = obj.lidarFeatures.pointsPerSecond;
            
            // If the script name is not null
            if(obj.lidarFeatures.scriptName != null){

                // Add the component related to the lidar operation based on the script name
                System.Type scriptLidar = System.Type.GetType(obj.lidarFeatures.scriptName);

                // If the script is found, add it to the instantiated object
                if (scriptLidar != null) lidarHolder.AddComponent(scriptLidar);
                else Debug.LogWarning("Player name: "+instantiatedObject.name+" | lidarFeatures / scriptName: ´" + obj.lidarFeatures.scriptName + "´ not found. No lidar sensor can be created without a reference script for its operation");

            }

        }
 
    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set the depth camera features:

    void SetDepthCameraFeatures(GameObject instantiatedObject, ObjectData obj)
    {

        // Find the DepthCameraHolder object in the instantiated object
        Transform depthCameraHolderTransform = instantiatedObject.transform.Find("DepthCameraHolder");

        // If the DepthCameraHolder object is found
        if (depthCameraHolderTransform != null)
        {
            
            // Add a reading component of the depth camera features. NOTE: See the GetDepthCameraFeatures script to unify variables
            GetDepthCameraFeatures getDepthCameraFeatures = instantiatedObject.AddComponent<GetDepthCameraFeatures>();
            getDepthCameraFeatures.scriptName = obj.depthCameraFeatures.scriptName;
            
            // If the near clip plane is not set, set it to 0.3 meters
            if(obj.depthCameraFeatures.nearClipPlane == 0) getDepthCameraFeatures.nearClipPlane = 0.3f;
            else getDepthCameraFeatures.nearClipPlane = obj.depthCameraFeatures.nearClipPlane;

            // If the far clip plane is not set, set it to 200 meters
            if(obj.depthCameraFeatures.farClipPlane == 0) getDepthCameraFeatures.farClipPlane = 200.0f;
            else getDepthCameraFeatures.farClipPlane = obj.depthCameraFeatures.farClipPlane;

            // If the field of view is not set, set it to 60 degrees
            if(obj.depthCameraFeatures.fieldOfView == 0) getDepthCameraFeatures.fieldOfView = 60.0f;
            else getDepthCameraFeatures.fieldOfView = obj.depthCameraFeatures.fieldOfView;

            // If the pixel width is not set, set it to 256 pixels
            if(obj.depthCameraFeatures.pixelWidth == 0) getDepthCameraFeatures.pixelWidth = 256;
            else getDepthCameraFeatures.pixelWidth = obj.depthCameraFeatures.pixelWidth;

            // If the pixel height is not set, set it to 256 pixels
            if(obj.depthCameraFeatures.pixelHeight == 0) getDepthCameraFeatures.pixelHeight = 256;
            else getDepthCameraFeatures.pixelHeight = obj.depthCameraFeatures.pixelHeight;
            
            // Create a new object to be the holder of the depth camera sensor
            GameObject depthCameraHolder = depthCameraHolderTransform.gameObject;

            // Add the depth camera to the child player
            Camera depthCamera = depthCameraHolder.AddComponent<Camera>();

            // Add depth camera features
            depthCamera.nearClipPlane = getDepthCameraFeatures.nearClipPlane;
            depthCamera.farClipPlane = getDepthCameraFeatures.farClipPlane;
            depthCamera.fieldOfView = getDepthCameraFeatures.fieldOfView;

            // Create a RenderTexture and assign it to the depth camera
            RenderTexture renderTexture = new RenderTexture(
                getDepthCameraFeatures.pixelWidth, 
                getDepthCameraFeatures.pixelHeight,
                16 // 16 bits for the depth + 8 bits for the stencil
            );
            depthCamera.targetTexture = renderTexture;
    
            // If the script name is not null
            if(getDepthCameraFeatures.scriptName != null){

                // Add the component related to the camera operation based on the script name
                System.Type scriptCamera = System.Type.GetType(obj.depthCameraFeatures.scriptName);

                // If the script is found, add it to the instantiated object
                if (scriptCamera != null) depthCameraHolder.AddComponent(scriptCamera);
                else Debug.LogWarning("Player name: "+instantiatedObject.name+" | depthCameraFeatures / scriptName: ´" + obj.depthCameraFeatures.scriptName + "´ not found. No Depth Camera sensor can be created without a reference script for its operation");

            }
            
            // Disable the camera until an image needs to be captured
            depthCamera.enabled = false;

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to set the communication features:

    void SetCommunicationFeatures(GameObject instantiatedObject, ObjectData obj)
    {
        
        // Add a reading component of the communication features. NOTE: See the GetCommunicationFeatures script to unify variables
        GetCommunicationFeatures getCommunicationFeatures = instantiatedObject.AddComponent<GetCommunicationFeatures>();
        getCommunicationFeatures.scriptName = obj.communicationFeatures.scriptName;
        
        // If the script name is not null
        if(obj.communicationFeatures.scriptName != null){

            // Add the component related to the camera operation based on the script name
            System.Type scriptCommunication = System.Type.GetType(obj.communicationFeatures.scriptName);

            // If the script is found, add it to the instantiated object
            if (scriptCommunication != null) instantiatedObject.AddComponent(scriptCommunication);
            else Debug.LogWarning("Player name: "+instantiatedObject.name+" | communicationFeatures / scriptName: ´" + obj.communicationFeatures.scriptName + "´ not found. No Communication features can be created without a reference script for its operation");

        }

    }

}
