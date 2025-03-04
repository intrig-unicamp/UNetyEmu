using UnityEngine; // Library to use in MonoBehaviour classes

// Class to control the main camera that follows the players
public class FollowCamera : MonoBehaviour
{
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:
    
    public string typeObjectName; // Name of the game object type (TAG) that the camera will follow
    public Vector3 offSetPos; // Desired position of the camera with an offset relative to the drone's rotation on all axes
    public float offSetRot; // Desired rotation of the camera with an offset relative to the drone's rotation on all axes
    public float smoothSpeed; // Smoothness of camera movement
    public float rotationSmoothSpeed; // Smoothness of camera rotation
    public Transform cityViewObject; // Object to obtain a reference point of the scene view
    public float mouseSensitivity; // Sensitivity of the mouse for camera rotation
    public float moveSpeed; // Base speed of camera movement
    public float sprintMultiplier; // Multiplier for camera movement speed when sprinting
    public string playerName; // Name of the player

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    private Transform target; // Reference to the target object to follow
    private int numDrone = 0; // Index of the drone to follow
    private bool inCityViewMode = false; // Control if the camera is in city view mode
    private Vector3 cityViewPositionOffset = Vector3.zero; // Offset for the city view camera position
    private float rotationX = 0f; // Accumulated rotation on the X axis for city view
    private float rotationY = 0f; // Accumulated rotation on the Y axis for city view
    private bool enableMouseRotation = false; // Control if the mouse rotation is enabled

    private GameObject[] targetObjects; // Array of game objects with the specified tag
    private GameObject targetObject; // Reference to the target object to follow

    private Vector3 desiredPosition; // Desired position of the camera
    private Vector3 smoothedPosition; // Smoothed position of the camera
    private Quaternion fixedRotationX; // Fixed rotation of the camera in the X axis
    private Quaternion finalRotation; // Final rotation of the camera
    private Quaternion smoothedRotation; // Smoothed rotation of the camera

    private float currentMoveSpeed; // Current speed of camera movement
    private float adjustedMoveSpeed; // Adjusted speed of camera movement
    private float mouseX; // Mouse movement on the X axis
    private float mouseY; // Mouse movement on the Y axis

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Make sure the cursor is unlocked and visible at startup
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Check if the city view object is assigned
        if (cityViewObject == null) Debug.LogWarning("CityViewObject is not assigned. Please assign a reference object for the city view.");

        // Set the initial value of the player name
        playerName = "";

    }

    // -----------------------------------------------------------------------------------------------------
    // Update is called once per frame:

    void Update()
    {
        
        // Change to player view mode
        if (Input.GetKeyDown(KeyCode.M))
        {
            
            // Flag to indicate that the camera is not in city view mode
            inCityViewMode = false;
            
            // Keep the cursor unlocked and visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }

        // Next or previous player to follow
        if (Input.GetKeyDown(KeyCode.C)) numDrone += 1;
        if (Input.GetKeyDown(KeyCode.V)) numDrone -= 1;
        
        // Change to city view mode
        if (Input.GetKeyDown(KeyCode.N))
        {
            
            // Flag to indicate that the camera is in city view mode
            inCityViewMode = true;

            // Set the configuration of the city view
            SetCityView();
            
            // Keep the cursor unlocked and visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }

        // Toggle mouse rotation
        if (Input.GetKeyDown(KeyCode.R)) enableMouseRotation = !enableMouseRotation;

        // Handle city view controls
        if (inCityViewMode) HandleCityViewControls();
        
    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at a fixed interval:

    void FixedUpdate()
    {
        
        // If the camera is in city view mode, do not follow the player
        if (inCityViewMode)
        {
            playerName = "";
            return;
        }

        // Try to find the target object to follow
        try
        {
            
            // Find all game objects with the specified tag
            targetObjects = GameObject.FindGameObjectsWithTag(typeObjectName);

            // If there are objects with the specified tag
            if (targetObjects.Length > 0)
            {
                
                // Limit the index to the range of the array
                if (numDrone >= targetObjects.Length) numDrone = targetObjects.Length - 1;
                if (numDrone < 0) numDrone = 0;
                
                // Get the target object to follow
                targetObject = targetObjects[numDrone];
                
                // Get the name of the player
                playerName = targetObject.name;

                // Set the target object and move the camera to the desired position
                target = targetObject.transform;
                moveToPosition();

            }
        }
        catch (System.Exception)
        {
            numDrone = 0; // Reset the index
            playerName = ""; // Set the player name to empty
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to move the camera to the desired position:

    void moveToPosition()
    {
        
        // Desired camera position with a displacement relative to the rotation of the drone in all axes
        desiredPosition = target.position + target.rotation * offSetPos;
        smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Desired camera rotation with a displacement relative to the rotation of the drone in the X axis
        fixedRotationX = Quaternion.Euler(offSetRot, 0f, 0f);
        finalRotation = target.rotation * fixedRotationX;

        // Smooth camera rotation
        smoothedRotation = Quaternion.Slerp(transform.rotation, finalRotation, rotationSmoothSpeed * Time.deltaTime);
        transform.rotation = smoothedRotation;
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Method for setting the view of the city:

    void SetCityView()
    {
        
        // If the city view object is assigned
        if (cityViewObject != null)
        {
            
            // Set the position and rotation of the camera to the city view object
            transform.position = cityViewObject.position;
            transform.rotation = cityViewObject.rotation;

            // Initializes the accumulated rotations based on the current camera rotation
            rotationX = transform.eulerAngles.x;
            rotationY = transform.eulerAngles.y;

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to handle city view controls:

    void HandleCityViewControls()
    {
        
        // Determines the speed of movement: applies the multiplier if the Key Shift is pressed
        currentMoveSpeed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);
        adjustedMoveSpeed = currentMoveSpeed * Time.deltaTime;

        // Camera movements in city view using local axes
        if (Input.GetKey(KeyCode.W))
        {
            cityViewPositionOffset += transform.forward * adjustedMoveSpeed;  // Zoom In
        }
        if (Input.GetKey(KeyCode.S))
        {
            cityViewPositionOffset -= transform.forward * adjustedMoveSpeed;  // Zoom Out
        }
        if (Input.GetKey(KeyCode.A))
        {
            cityViewPositionOffset -= transform.right * adjustedMoveSpeed;  // Left
        }
        if (Input.GetKey(KeyCode.D))
        {
            cityViewPositionOffset += transform.right * adjustedMoveSpeed;  // Right
        }
        if (Input.GetKey(KeyCode.Q))
        {
            cityViewPositionOffset += transform.up * adjustedMoveSpeed;  // Up
        }
        if (Input.GetKey(KeyCode.E))
        {
            cityViewPositionOffset -= transform.up * adjustedMoveSpeed;  // Down
        }

        // Applies the offset to the camera position
        transform.position = cityViewObject.position + cityViewPositionOffset;

        // Camera rotation using mouse movement, only if enableMouseRotation is true
        if (enableMouseRotation)
        {
            
            // Get the mouse movement
            mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            // Accumulate the rotation on the X and Y axes
            rotationX -= mouseY;
            rotationY += mouseX;

            // Limit the rotation on the X axis to -90 and 90 degrees 
            rotationX = Mathf.Clamp(rotationX, -90f, 90f);

            // Apply the accumulated rotation to the camera
            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);

        }

    }
    
}
