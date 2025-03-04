using UnityEngine; // Library to use MonoBehaviour classes
using System.Collections; // Library to use IEnumerator
using System.Collections.Generic; // Library to use Dictionary
using System; // Library to use Serializable attribute

// Class to control the Lidar sensor
public class LidarSensor : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    public bool showAllPointCloud = false; // Flag to show all points of the point cloud
    public bool showDetectedPointCloud = false; // Flag to show only the detected points of the point cloud
    public bool showShortestDistanceDetected = false; // Flag to show only the shortest detected points of the point cloud

    // Dictionary to store the detected objects
    public Dictionary<string, DetectionInfo> detectedObjects = new Dictionary<string, DetectionInfo>();

    // Class to show the detection information
    [Serializable] public class ShowDetectionInfo
    {
        public float distanceDetected;
        public Vector3 directionDetected;
    }

    // Class to store the detection information
    [Serializable] public class DetectionInfo
    {
        
        // Variables of this class
        public float distanceDetected;
        public Vector3 directionDetected;

        // Constructor of this class
        public DetectionInfo(float dist, Vector3 dir)
        {
            distanceDetected = dist;
            directionDetected = dir;
        }
        
    }

    // List to store the detected objects that will be displayed in the Inspector
    [SerializeField] public List<ShowDetectionInfo> detectedObjectsList;
    
    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    private GetLidarFeatures getLidarFeatures; // GetLidarFeatures component of the drone
    private float lidarRange; // Lidar range
    private int numRaysHorizontal; // Number of horizontal rays
    private int numRaysVertical; // Number of vertical rays
    private float verticalFOV; // Vertical field of view
    private float pointsPerSecond; // Points per second of the Lidar sensor
    private int totalRaysPerScan; // Total rays per scan
    private float scanTimer = 0f; // Scan timer
    private float scanFrequency; // Scan frequency
    private bool isScanning = false; // Flag to control a new scan start
    
    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the GetLidarFeatures component of the drone
        getLidarFeatures = GetComponentInParent<GetLidarFeatures>();

        // If the getLidarFeatures is not null, get the Lidar features
        if (getLidarFeatures != null)
        {
            lidarRange = getLidarFeatures.lidarRange;
            numRaysHorizontal = getLidarFeatures.numRaysHorizontal;
            numRaysVertical = getLidarFeatures.numRaysVertical;
            verticalFOV = getLidarFeatures.verticalFOV;
            pointsPerSecond = getLidarFeatures.pointsPerSecond;
        }

        // Initialize the detected objects list
        detectedObjectsList = new List<ShowDetectionInfo>();

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // Calculate the total rays per scan
        totalRaysPerScan = numRaysHorizontal * numRaysVertical;

        // Calculate the scan frequency
        scanFrequency = pointsPerSecond / totalRaysPerScan;

        // Calculate the scan timer
        scanTimer += Time.deltaTime;

        // If the scan timer is greater than the scan frequency, perform the Lidar scan
        if (scanTimer >= 1.0f / scanFrequency)
        {
            
            // If the Lidar sensor is not scanning, start the Lidar scan
            if (!isScanning)
            {
                StartCoroutine(PerformLidarScanRoutine()); // Perform the Lidar scan routine
                isScanning = true; // Set the flag to true
            }

            // Reset the scan timer
            scanTimer = 0f;

        }

        // Clear the detected objects list that will be displayed in the Inspector
        detectedObjectsList.Clear();

        // Display the list in the Inspector
        foreach (var detection in detectedObjects)
        {
            
            // Add the detected object to the list
            detectedObjectsList.Add(new ShowDetectionInfo { 
                distanceDetected = detection.Value.distanceDetected, 
                directionDetected = detection.Value.directionDetected
            });

        }

    }

    // -----------------------------------------------------------------------------------------------------
    //  Subroutine to execute full lidar scanning:

    private IEnumerator PerformLidarScanRoutine()
    {
        
        // Clear the detected objects
        detectedObjects.Clear();

        // If the number of vertical rays is 1, perform a horizontal Lidar scan
        if (numRaysVertical == 1)
        {
            yield return StartCoroutine(PerformHorizontalLidarScanRoutine());
        }
        else
        {
            yield return StartCoroutine(Perform3DLidarScanRoutine());
        }

        // Set the scanning flag to false
        isScanning = false;

    }

    // -----------------------------------------------------------------------------------------------------
    // Subroutine for horizontal scanning:

    private IEnumerator PerformHorizontalLidarScanRoutine()
    {
        
        // Calculate the horizontal angle step
        float horizontalAngleStep = 360.0f / numRaysHorizontal;

        // For each horizontal ray
        for (int h = 0; h < numRaysHorizontal; h++)
        {
            
            // Calculate the horizontal angle
            float horizontalAngle = h * horizontalAngleStep;

            // Calculate the direction of the ray
            Vector3 direction = transform.rotation * Quaternion.Euler(0, horizontalAngle, 0) * Vector3.forward;

            // Initialize the raycast hit
            RaycastHit hit;

            // If the ray hits an object
            if (Physics.Raycast(transform.position, direction, out hit, lidarRange))
            {
                
                // If the flag to show all points or the flag to show only the detected points are true
                if (showAllPointCloud || showDetectedPointCloud)
                {
                    
                    // Draw the ray in the scene view
                    Debug.DrawRay(transform.position, direction * hit.distance, Color.red);

                }
 
                // Get the name of the detected object
                string objectNameDetected = hit.collider.name;

                // Get the distance detected
                float distanceDetected = hit.distance;

                // Get the direction detected
                Vector3 directionDetected = direction;

                // Create a detection information object
                DetectionInfo detectionInfo = new DetectionInfo(distanceDetected, directionDetected);

                // If the detected objects dictionary contains the object name
                if (detectedObjects.ContainsKey(objectNameDetected))
                {
                    
                    // If the distance detected is less than the previous distance detected
                    if (distanceDetected < detectedObjects[objectNameDetected].distanceDetected)
                    {
                        
                        // Update the detected object
                        detectedObjects[objectNameDetected] = detectionInfo;

                    }

                }
                else
                {
                    
                    // Add the detected object to the dictionary
                    detectedObjects.Add(objectNameDetected, detectionInfo);

                }

            }
            else
            {
                
                // If the flag to show all points is true
                if (showAllPointCloud)
                {
                    
                    // Draw the ray in the scene view
                    Debug.DrawRay(transform.position, direction * lidarRange, Color.green);

                }

            }
            
            // If the horizontal ray is a multiple of 10, yield null to wait for the next frame
            if (h % 10 == 0) yield return null;

        }

        // If the flag to show the shortest detected points is true and the flags to show the detected and all points are false
        if (showShortestDistanceDetected && !showDetectedPointCloud && !showAllPointCloud)
        {
            
            // For each detected object
            foreach (var entry in detectedObjects)
            {
                
                // Get the detection information
                DetectionInfo infoDetected = entry.Value;

                // Draw the ray in the scene view
                Debug.DrawRay(transform.position, infoDetected.directionDetected * infoDetected.distanceDetected, Color.red);

            }

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Subroutine for 3D scanning with multiple vertical angles:

    private IEnumerator Perform3DLidarScanRoutine()
    {
        
        // Calculate the vertical angle step
        float verticalAngleStep = verticalFOV / (numRaysVertical - 1);

        // Calculate the horizontal angle step
        float horizontalAngleStep = 360.0f / numRaysHorizontal;

        // For each vertical ray
        for (int v = 0; v < numRaysVertical; v++)
        {
            
            // Calculate the vertical angle
            float verticalAngle = -verticalFOV / 2 + v * verticalAngleStep;

            // For each horizontal ray
            for (int h = 0; h < numRaysHorizontal; h++)
            {
                
                // Calculate the horizontal angle
                float horizontalAngle = h * horizontalAngleStep;

                // Calculate the direction of the ray
                Vector3 direction = transform.rotation * Quaternion.Euler(verticalAngle, horizontalAngle, 0) * Vector3.forward;

                // Initialize the raycast hit
                RaycastHit hit;

                // If the ray hits an object
                if (Physics.Raycast(transform.position, direction, out hit, lidarRange))
                {
                    
                    // If the flag to show all points or the flag to show only the detected points are true
                    if (showAllPointCloud || showDetectedPointCloud)
                    {
                        
                        // Draw the ray in the scene view
                        Debug.DrawRay(transform.position, direction * hit.distance, Color.red);

                    }

                    // Get the name of the detected object
                    string objectNameDetected = hit.collider.name;

                    // Get the distance detected
                    float distanceDetected = hit.distance;

                    // Get the direction detected
                    Vector3 directionDetected = direction;

                    // Create a detection information object
                    DetectionInfo detectionInfo = new DetectionInfo(distanceDetected, directionDetected);

                    // If the detected objects dictionary contains the object name
                    if (detectedObjects.ContainsKey(objectNameDetected))
                    {
                        
                        // If the distance detected is less than the previous distance detected
                        if (distanceDetected < detectedObjects[objectNameDetected].distanceDetected)
                        {
                            
                            // Update the detected object
                            detectedObjects[objectNameDetected] = detectionInfo;

                        }

                    }
                    else
                    {
                        
                        // Add the detected object to the dictionary
                        detectedObjects.Add(objectNameDetected, detectionInfo);

                    }
                    
                }
                else
                {
                    
                    // If the flag to show all points is true
                    if (showAllPointCloud)
                    {
                        
                        // Draw the ray in the scene view
                        Debug.DrawRay(transform.position, direction * lidarRange, Color.green);

                    }

                }

                // If the horizontal ray is a multiple of 10, yield null to wait for the next frame
                if (h % 10 == 0) yield return null;

            }

        }

        // If the flag to show the shortest detected points is true and the flags to show the detected and all points are false
        if (showShortestDistanceDetected && !showDetectedPointCloud && !showAllPointCloud)
        {
            
            // For each detected object
            foreach (var entry in detectedObjects)
            {
                
                // Get the detection information
                DetectionInfo infoDetected = entry.Value;

                // Draw the ray in the scene view
                Debug.DrawRay(transform.position, infoDetected.directionDetected * infoDetected.distanceDetected, Color.red);

            }

        }

    }

}
