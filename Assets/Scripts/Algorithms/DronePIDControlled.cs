using System; // Library to use MathF class
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Main Class to control the drone's movements using PID control:
public class DronePIDControlled : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Set the adjusted variable from velocity correction, may need to be optimized
    public float adjustedVariableFromVelocity = 20.0f;

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // DroneDynamics component of the drone
    private DroneDynamics droneDynamics;

    // Target orientation of the drone
    private float targetOrientation;

    // PID control variables
    private float KpThrottle, KiThrottle, KdThrottle;    
    private float KpPitch, KiPitch, KdPitch;
    private float KpRoll, KiRoll, KdRoll;
    private float KpYaw, KiYaw, KdYaw;

    // PID control objects
    private PID throttlePID;
    private PID pitchPID;
    private PID rollPID;
    private PID yawPID;

    // Rigidbody component of the drone
    private Rigidbody rb;

    // Target object
    private GameObject targetObj;

    // GetObjectFeatures component of the drone
    private GetObjectFeatures getObjectFeatures;
    private string objectID; // Name of the object

    // Target position
    private Vector3 targetPosition;

    // Variables to calculate the error in the PID control
    private float currentY;
    private float yError;
    private float currentZ;
    private float currentX;
    private float zError;
    private float xError;
    private float currentYaw;
    private float yawError;

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the Rigidbody component of the drone
        rb = GetComponent<Rigidbody>();

        // Set the center of mass of the drone
        rb.centerOfMass = new Vector3(0, 0, 0);

        // Get the DroneDynamics component of the drone
        droneDynamics = GetComponent<DroneDynamics>();

        // Initialize the PID control objects
        throttlePID = new PID();
        pitchPID = new PID();
        rollPID = new PID();
        yawPID = new PID();
        
        // Get the drone's features from the GetDroneFeatures script:
        getObjectFeatures = GetComponent<GetObjectFeatures>();
        objectID = getObjectFeatures.objectID; // Get the object ID

        // Find the target object
        targetObj = GameObject.Find("ID"+objectID+"Target");

        // Get the target position and orientation
        targetOrientation = targetObj.transform.eulerAngles.y;

    }
    
    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // If the droneDynamics and getObjectFeatures are null, break the loop
        if(droneDynamics == null) return;
        if(getObjectFeatures == null) return;

        // Get the target position and orientation
        targetPosition = targetObj.transform.position;
        targetOrientation = targetObj.transform.eulerAngles.y;
        
        // Apply the Yaw, Throttle, Pitch, and Roll PID controllers
        ApplyYawPIDControl();
        ApplyThrottlePIDControl();
        ApplyPitchRollPIDControl();

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to apply the throttle PID control:

    void ApplyThrottlePIDControl()
    {
 
        // Get the current position of the drone
        currentY = transform.position.y;

        // Calculate the error between the target position and the current position
        yError = targetPosition.y - currentY;

        // Set the PID control parameters, may need to be optimized
        KpThrottle = Mathf.Lerp(2.5f, 1.5f, Mathf.Clamp01( MathF.Abs(rb.velocity.y) / adjustedVariableFromVelocity ));
        KiThrottle = Mathf.Lerp(0.0f, 0.1f, Mathf.Clamp01( MathF.Abs(yError) / adjustedVariableFromVelocity ));
        KdThrottle = Mathf.Lerp(0.5f, 1.0f, Mathf.Clamp01( MathF.Abs(rb.velocity.y) / adjustedVariableFromVelocity ));

        // Apply the PID control to the throttle, only if the drone is not shutting down
        if(!droneDynamics.flagDroneShutdown) droneDynamics.throttle = throttlePID.Compute(yError, KpThrottle, KiThrottle, KdThrottle);

        // Reset the PID control to the throttle if the error is less than 0.5
        if(yError < 0.5f) throttlePID.Reset();

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to apply the pitch and roll PID control:

    void ApplyPitchRollPIDControl()
    {
        
        // Get the current position of the drone in the z and x axis
        currentZ = transform.position.z;
        currentX = transform.position.x;
        
        // Calculate the error between the target position and the current position
        zError = targetPosition.z - currentZ;
        xError = targetPosition.x - currentX;

        // Set the PID control parameters for pitch, may need to be optimized
        KpPitch = Mathf.Lerp(4f, 0.5f, Mathf.Clamp01( MathF.Abs(rb.velocity.z) / adjustedVariableFromVelocity ));
        KiPitch = Mathf.Lerp(0.0f, 0.1f, Mathf.Clamp01( MathF.Abs(zError) / adjustedVariableFromVelocity ));
        KdPitch = Mathf.Lerp(4.0f, 3.0f, Mathf.Clamp01( MathF.Abs(rb.velocity.z) / adjustedVariableFromVelocity ));

        // Set the PID control parameters for roll, may need to be optimized
        KpRoll = Mathf.Lerp(4f, 0.5f, Mathf.Clamp01( MathF.Abs(rb.velocity.x) / adjustedVariableFromVelocity ));
        KiRoll = Mathf.Lerp(0.0f, 0.1f, Mathf.Clamp01( MathF.Abs(xError) / adjustedVariableFromVelocity ));
        KdRoll = Mathf.Lerp(4.0f, 3.0f, Mathf.Clamp01( MathF.Abs(rb.velocity.x) / adjustedVariableFromVelocity ));
        
        // Apply the PID control to the pitch, only if the drone is not shutting down
        if(!droneDynamics.flagDroneShutdown) droneDynamics.pitch = pitchPID.Compute(zError, KpPitch, KiPitch, KdPitch);

        // Apply the PID control to the roll, only if the drone is not shutting down
        if(!droneDynamics.flagDroneShutdown) droneDynamics.roll = rollPID.Compute(xError, KpRoll, KiRoll, KdRoll);

        // Reset the PID control to the pitch if the error is less than 0.5
        if(zError < 0.5f) pitchPID.Reset();

        // Reset the PID control to the roll if the error is less than 0.5
        if(xError < 0.5f) rollPID.Reset();

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to apply the yaw PID control:

    void ApplyYawPIDControl()
    {
        
        // Get the current yaw of the drone
        currentYaw = transform.localEulerAngles.y;

        // Calculate the error between the target orientation and the current yaw
        yawError = targetOrientation - currentYaw;

        // Normalize the yaw error
        if (yawError > 180) yawError -= 360;
        if (yawError < -180) yawError += 360;

        // Set the PID control parameters for yaw, may need to be optimized
        KpYaw = 0.4f;
        KiYaw = 0.0f;
        KdYaw = Mathf.Lerp(0.18f, 0.28f, Mathf.Clamp01( MathF.Abs(yawError) / adjustedVariableFromVelocity ));

        // Apply the PID control to the yaw, only if the drone is not shutting down
        if(!droneDynamics.flagDroneShutdown) droneDynamics.yaw = yawPID.Compute(yawError, KpYaw, KiYaw, KdYaw);

        // Reset the PID control to the yaw if the error is less than 0.5
        if(yawError < 0.5f) yawPID.Reset();

    }

}
