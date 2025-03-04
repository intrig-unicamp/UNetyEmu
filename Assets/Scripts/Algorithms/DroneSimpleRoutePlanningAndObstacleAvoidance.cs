using System.Collections.Generic; // Library to use lists
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using System; // Library to use Serializable attribute

// Class to set a single route planning for the drone
public class DroneSimpleRoutePlanningAndObstacleAvoidance : MonoBehaviour
{

    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Variable to store the current point index
    public int currentPointIndex = 0;

    // List of route points
    [SerializeField] public List<Vector3> routePoints = new List<Vector3>{
        new Vector3(292.8f, 70f, 167.6f),
        new Vector3(204.6f, 40f, 244.2f), 
        new Vector3(176.2f, 20f, 315.6f), 
        new Vector3(200.29f, 20f, 343.35f),
        new Vector3(287.8f, 30f, 291.4f),
        new Vector3(344f, 60f, 242.5f)
    };

    // Variables to control the drone speed, rotation speed, and minimum error to target
    public float speed = 5.0f;
    public float rotationSpeed = 5f;
    public float minErrorToTarget = 0.5f;

    // Variables to control the obstacle detection range and evasion obstacle distance
    public float obstacleDetectionRange = 10.0f;
    public float evasionObstacleDistance = 10.0f;

    // Class to show the detection information
    [Serializable] public class ShowDetectionInfo
    {
        public float distanceDetected;
        public Vector3 directionDetected;
    }

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Get the object features and the object ID
    private GetObjectFeatures getObjectFeatures;
    private string objectID;

    // Sphere target object related to the drone
    private GameObject targetObj;

    // Variables to store the target position, rotation, direction, and distance to target
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 direction;
    private float distanceToTarget;

    // Lidar sensor holder transform and lidar sensor component
    private Transform lidarHolderTransform;
    private LidarSensor lidarSensor;

    // List to store the detected objects by the lidar sensor
    private List<LidarSensor.ShowDetectionInfo> detectedObjectsList;

    // Variables to control the evasion direction
    private Vector3 evadeDirection;

    // Variable to store the local direction
    private Vector3 localDirection;

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
    
        // Get the object features and the object ID
        getObjectFeatures = GetComponent<GetObjectFeatures>();
        objectID = getObjectFeatures.objectID;

        // Get the sphere target object related to the drone
        targetObj = GameObject.Find("ID" + objectID + "Target");

        // Get the lidar sensor holder transform
        lidarHolderTransform = transform.Find("LidarHolder");

        // Get the lidar sensor component
        if (lidarHolderTransform != null) lidarSensor = lidarHolderTransform.GetComponent<LidarSensor>();

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // Get the detected objects list from the lidar sensor
        if (lidarSensor != null) detectedObjectsList = lidarSensor.detectedObjectsList;

        // Check if there are obstacles in the way
        if (CheckForObstacles()) ObstacleAvoidance(); // If there are obstacles, avoid them
        else FollowRoute(); // If there are no obstacles, follow the route
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to check if there are obstacles in the way:

    bool CheckForObstacles()
    {
        
        // Get the detected objects list from the lidar sensor
        try
        {

            // Iterate through the detected objects list
            foreach (var detection in detectedObjectsList)
            {
                
                // Check if the distance detected is less than the obstacle detection range
                if (detection.distanceDetected <= obstacleDetectionRange)
                {

                    // Get the local direction based on the detected object
                    localDirection = targetObj.transform.InverseTransformDirection(detection.directionDetected);

                    // Only avoid obstacles if the detected object is in front of the drone
                    if (localDirection.z > 0) return true;

                } 

            }

        }
        catch (Exception){}

        // Return false if there are no obstacles less than the obstacle detection range
        return false;
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to avoid obstacles:

    void ObstacleAvoidance()
    {
        
        // Printa that the obstacle was detected
        Debug.Log( gameObject.name + ": Obstacle detected! Avoiding the obstacle...");

        // Set the target direction to avoid the obstacle
        evadeDirection = targetObj.transform.right;

        // Move the target object in the evade direction
        targetObj.transform.position += evadeDirection * evasionObstacleDistance * Time.deltaTime;

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to follow the route:

    void FollowRoute()
    {

        // Set the target position to the current point index
        targetPosition = routePoints[currentPointIndex];

        // Move the target object towards the target position
        targetObj.transform.position = Vector3.MoveTowards(targetObj.transform.position, targetPosition, speed * Time.deltaTime);

        // Calculate the direction to the target
        direction = (targetPosition - targetObj.transform.position).normalized;

        // Rotate the target object towards the target position
        if (direction != Vector3.zero)
        {
            
            // Set the target rotation
            targetRotation = Quaternion.LookRotation(direction);

            // Rotate the target object towards the target position
            targetObj.transform.rotation = Quaternion.Slerp(targetObj.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        }

        // Calculate the distance to the target
        distanceToTarget = Vector3.Distance(targetObj.transform.position, targetPosition);
        
        // Check if the distance to the target is less than the minimum error to target
        if (distanceToTarget < minErrorToTarget)
        {
            
            // Update the current point index
            currentPointIndex = (currentPointIndex + 1) % routePoints.Count;

        }

    }

}
