using System.Collections.Generic; // Library to use lists
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using System.Collections; // Library to use in IEnumerator class

// Class to set a single route planning for the drone
public class DroneSimpleRoutePlanning : MonoBehaviour
{

    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Variable to store the current point index
    public int currentPointIndex2 = 0;

    // List of route points
    [SerializeField] public List<Vector3> routePoints2 = new List<Vector3>{
        new Vector3(374.1f, 40f, 245.6f),
        new Vector3(336f, 40f, 277.5f), 
        new Vector3(292f, 10f, 258.4f), 
        new Vector3(247.7f, 30f, 220.1f), 
        new Vector3(169.6f, 50f, 243.1f),
        new Vector3(258f, 50f, 359.2f),
        new Vector3(320.9f, 30f, 306.4f),
        new Vector3(280.6f, 20f, 258.3f),
        new Vector3(336.8f, 10f, 216.4f)
    };

    // Variables to control the drone speed, rotation speed, and minimum error to target
    public float speed2 = 5.0f;
    public float rotationSpeed2 = 5f;
    public float minErrorToTarget2 = 0.5f;

    // Variable to store the time interval to take a photo
    public int timeIntervalToTakePhoto = 30;

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

    // DepthCamera sensor holder transform and depthCamera sensor component
    private Transform depthCameraHolderTransform;
    private DepthCamera depthCameraSensor;

    // Flag to control the coroutine time
    private bool coroutineFlagTime = false;

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
    
        // Get the object features and the object ID
        getObjectFeatures = GetComponent<GetObjectFeatures>();
        objectID = getObjectFeatures.objectID;

        // Get the sphere target object related to the drone
        targetObj = GameObject.Find("ID" + objectID + "Target");

        // Get the depthCamera sensor holder transform
        depthCameraHolderTransform = transform.Find("DepthCameraHolder");

        // Get the depthCamera sensor component
        if (depthCameraHolderTransform != null) depthCameraSensor = depthCameraHolderTransform.GetComponent<DepthCamera>();

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // Call the method to take a photo
        TakePhoto();

        // Call the method to follow the route
        FollowRoute();

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to take a photo using the depthCamera sensor:

    void TakePhoto()
    {

        // Check if the depthCamera sensor is not null
        if (depthCameraSensor != null)
        {

            // Check if the coroutine flag time is false
            if(!coroutineFlagTime)
            {
                coroutineFlagTime = true;
                StartCoroutine(WaitAndAct()); // Start the coroutine and take a photo
            }

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Coroutine to wait for seconds and take a photo:

    IEnumerator WaitAndAct()
    {
        
        // Wait for seconds
        yield return new WaitForSeconds(timeIntervalToTakePhoto); 

        // Take a photo
        depthCameraSensor.captureImage = true; 

        // Reset the coroutine flag time
        coroutineFlagTime = false; 

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to follow the route:

    void FollowRoute()
    {

        // Set the target position to the current point index
        targetPosition = routePoints2[currentPointIndex2];

        // Move the target object towards the target position
        targetObj.transform.position = Vector3.MoveTowards(targetObj.transform.position, targetPosition, speed2 * Time.deltaTime);

        // Calculate the direction to the target
        direction = (targetPosition - targetObj.transform.position).normalized;

        // Rotate the target object towards the target position
        if (direction != Vector3.zero)
        {
            
            // Set the target rotation
            targetRotation = Quaternion.LookRotation(direction);

            // Rotate the target object towards the target position
            targetObj.transform.rotation = Quaternion.Slerp(targetObj.transform.rotation, targetRotation, rotationSpeed2 * Time.deltaTime);

        }

        // Calculate the distance to the target
        distanceToTarget = Vector3.Distance(targetObj.transform.position, targetPosition);
        
        // Check if the distance to the target is less than the minimum error to target
        if (distanceToTarget < minErrorToTarget2)
        {
            
            // Update the current point index
            currentPointIndex2 = (currentPointIndex2 + 1) % routePoints2.Count;

        }

    }

}
