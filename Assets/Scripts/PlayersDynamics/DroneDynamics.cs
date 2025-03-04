using System; // Library to use DateTime class
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Main class to control the drone's movements, take images from the depth camera, and show the detection rays:
public class DroneDynamics : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Input values for the drone's movements
    [Header("Input Values")]
    [Range(0f, 1f)] public float throttle = 0f;
    [Range(-45f, 45f)] public float pitch = 0f;
    [Range(-45f, 45f)] public float roll = 0f;
    [Range(-5f, 5f)] public float yaw = 0f;

    // Battery features
    [Header("Battery Features")]
    public float batteryLevel = 100f; // Initial battery level of the drone in percentage (just to initialize the variable)
    public float simulationBatterySpeedFactor = 0.0001f; // Speed factor to simulate the battery consumption
    public float idlePowerConsumptionPerSecond = 0.01f; // Power consumption of the drone when idle
    
    // Total force applied to the drone
    [Header("Total Force")]
    public Vector3 totalForce;

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:
    
    // Variables for a PID controller to stabilize the drone
    private float KpS = 1f; // Initial value, may need to be optimized
    private float KiS = 0.0f; // Initial value, may need to be optimized
    private float KdS = 0.5f; // Initial value, may need to be optimized 
    private Vector3 previousError = Vector3.zero; // Initial value for the previous error
    private Vector3 integralError = Vector3.zero; // Initial value for the integral error
    private Vector3 totalTorque; // Total torque applied to the drone
    private Vector3 yawTorque; // Torque applied to the drone for yaw control

    // Get the drone's features from the GetDroneFeatures script
    private GetDroneFeatures getDroneFeatures;
    private float unladenWeight = 14f; // Initial weight of the drone without any payload (just to initialize the variable)
    private float maxThrust;
    private float maxSpeedManufacturer;
    private float batteryCapacity;
    private float batteryVoltage; 
    private float maxFlightTime;
    private float maximumTiltAngle;

    // Power required to hover the drone
    private float hoverPower;
    private float energyTotal;
    
    // Rigidbody component of the drone
    private Rigidbody rb;
    private float droneWeight;
    private float gravity;

    // Time active of the drone
    private float timeActive = 0f;

    // Variables for the drone's forces and torques
    private Vector3 upwardForce;
    private Vector3 forwardForce;
    private Vector3 lateralForce;
    private Vector3 forwardTorque;
    private Vector3 lateralTorque;

    // Variables to deal with the drone's rotation
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Vector3 error;

    // Variables for the PID controller to stabilize the drone
    private Vector3 proportional;
    private Vector3 integral;
    private Vector3 derivative;
    private Vector3 pidOutput;
    private Vector3 correctionTorque;

    // Variables for the battery consumption of the drone
    private float thrustUsed;
    private float powerConsumption;
    private float timeBasedConsumption;
    private float batteryUsed;
    private float batteryUsed_percent;

    // Flag to shutdown the drone
    public bool flagDroneShutdown = false;

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the Rigidbody component of the drone
        rb = GetComponent<Rigidbody>();

        // Get the drone's features from the GetDroneFeatures script
        getDroneFeatures = GetComponent<GetDroneFeatures>();

        // If the GetDroneFeatures script exists, get the drone's features
        if (getDroneFeatures != null)
        {
            maxThrust = getDroneFeatures.maxThrust; // Get the maximum thrust of the drone
            maxSpeedManufacturer = getDroneFeatures.maxSpeedManufacturer; // Get the maximum speed of the drone
            maximumTiltAngle = getDroneFeatures.maximumTiltAngle; // Get the maximum tilt angle of the drone
            batteryLevel = getDroneFeatures.batteryStartPercentage; // Get the initial battery level of the drone
            batteryCapacity = getDroneFeatures.maxBatteryCapacity; // Get the maximum battery capacity of the drone
            batteryVoltage = getDroneFeatures.batteryVoltage; // Get the battery voltage of the drone
            unladenWeight = getDroneFeatures.unladenWeight; // Get the unladen weight of the drone
            maxFlightTime = getDroneFeatures.approxMaxFlightTime * 60f; // Get the maximum flight time of the drone in seconds
        }

        // Get the gravity of Unity
        gravity = Mathf.Abs(Physics.gravity.y);

        // Calculate the weight of the drone using the gravity of Unity
        droneWeight = unladenWeight * gravity;

        // Calculate the power required to hover the drone
        energyTotal = (batteryCapacity * batteryVoltage) / 1000f;
        hoverPower = energyTotal / (maxFlightTime / 3600f);

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // Check the battery level of the drone
        CheckBatteryLevel();

        // If the drone is not shutdown, apply the forces and torques to the drone
        if(!flagDroneShutdown)
        {

            // If the GetDroneFeatures script does not exist, exit the method
            if(getDroneFeatures == null) return;

            // Apply the forces to the motors of the drone
            ApplyForcesTorques();

            // Apply the PID controller to stabilize the drone
            ApplyPIDStabilization();

            // Limit the speed of the drone
            if(maxSpeedManufacturer > 0.0f) rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeedManufacturer);

            // Update the battery consumption of the drone
            UpdateBatteryConsumption();

        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // OnCollisionEnter is called when this collider/rigidbody has begun touching another rigidbody/collider:

    void OnCollisionStay(Collision collision)
    {
        
        // If the drone is shutdown, stop the drone's movements
        if(flagDroneShutdown)
        {

            throttle = 0f; // Stop the throttle
            pitch = 0f; // Stop the pitch
            roll = 0f; // Stop the roll
            yaw = 0f; // Stop the yaw

            totalForce = Vector3.zero; // Stop the total force
            totalTorque = Vector3.zero; // Stop the total torque
            yawTorque = Vector3.zero; // Stop the yaw torque

        }
                
        // If the drone collides with the Ground or DronePads, stop the drone's movements:
        if ( collision.gameObject.CompareTag("DronePadStart") || collision.gameObject.CompareTag("DronePadCustomer") ||
            collision.gameObject.CompareTag("DronePadBatteryRecharge") || collision.gameObject.CompareTag("DronePadPackage") ||
            collision.gameObject.CompareTag("Ground") )
        {
            pitch = 0f; // Stop the pitch movement
            roll = 0f; // Stop the roll movement
            yaw = 0f; // Stop the yaw movement
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to apply the forces to the motors of the drone:

    void ApplyForcesTorques()
    {
        
        // Limit the values of the inputs
        throttle = Mathf.Clamp(throttle, 0f, 1f);
        pitch = Mathf.Clamp(pitch, -maximumTiltAngle, maximumTiltAngle);
        roll = Mathf.Clamp(roll, -maximumTiltAngle, maximumTiltAngle);

        // Get the forces to the motors of the drone
        upwardForce = Vector3.up * throttle * maxThrust;
        forwardForce = Vector3.forward * (pitch / maximumTiltAngle) * maxThrust * 0.5f;
        lateralForce = Vector3.right * (roll / maximumTiltAngle) * maxThrust * 0.5f;
        
        // Sum the forces to get the total force applied to the drone
        totalForce = upwardForce + forwardForce + lateralForce;
        if(totalForce.magnitude > maxThrust) totalForce = totalForce.normalized * maxThrust; // Limit the total force to the maximum thrust
        if(throttle < 0.05f) totalForce = Vector3.zero; // Stop the drone if the throttle is less than 0.05f

        // Apply the total force to the drone
        rb.AddForce(totalForce);

        // Get the torques to the motors of the drone
        forwardTorque = Vector3.right * pitch * 0.5f; // 0.5 is a factor to reduce the torque applied
        lateralTorque = - Vector3.forward * roll * 0.5f; // 0.5 is a factor to reduce the torque applied

        // Sum the torques to get the total torque applied to the drone
        totalTorque = forwardTorque + lateralTorque;

        // Apply the total torque to the drone
        rb.AddTorque(totalTorque);

        // Get the torque for yaw control
        yawTorque = Vector3.up * yaw;

        // Limit the yaw torque to the maximum thrust
        if(yawTorque.magnitude > (maxThrust * 0.01f)) yawTorque = yawTorque.normalized * (maxThrust * 0.01f);

        // Apply the yaw torque to the drone
        rb.AddTorque(yawTorque);

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to apply the PID controller to stabilize the drone: 

    void ApplyPIDStabilization()
    {
        
        // Get the current rotation of the drone
        currentRotation = transform.localEulerAngles;

        // Get the target rotation error of the drone
        targetRotation = new Vector3(0f, currentRotation.y, 0f);
        error = targetRotation - currentRotation;

        // Limit the error values to -180 and 180 degrees
        if (error.x > 180) error.x -= 360;
        if (error.x < -180) error.x += 360;
        if (error.z > 180) error.z -= 360;
        if (error.z < -180) error.z += 360;
        if (error.y > 180) error.y -= 360;
        if (error.y < -180) error.y += 360;

        // Proportional
        proportional = KpS * error;

        // Integral
        integralError += error * Time.fixedDeltaTime;
        integral = KiS * integralError;

        // Derivative
        derivative = KdS * (error - previousError) / Time.fixedDeltaTime;
        previousError = error;

        // PID output
        pidOutput = proportional + integral + derivative;

        // Apply torque to correct pitch (X axis), roll (Z axis), and yaw (Y axis)
        correctionTorque = new Vector3(pidOutput.x, pidOutput.y, pidOutput.z);

        // Apply the correction torque to the drone
        rb.AddRelativeTorque(correctionTorque);
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Method to update the battery consumption of the drone:

    void UpdateBatteryConsumption()
    {
        
        // Get the total force applied to the drone
        thrustUsed = totalForce.magnitude;
        
        // Calculate the power consumption of the drone
        powerConsumption = hoverPower * (thrustUsed / droneWeight);

        // If the drone is idle, increase the power consumption
        timeActive += Time.deltaTime;

        // Calculate the time-based consumption of the drone
        timeBasedConsumption = timeActive * idlePowerConsumptionPerSecond;

        // Sum the power consumption of the drone
        powerConsumption += timeBasedConsumption;

        // Calculate the battery used in the simulation
        batteryUsed = (powerConsumption / batteryVoltage) * 3600 * Time.deltaTime * simulationBatterySpeedFactor;

        // Calculate the battery used in percentage
        batteryUsed_percent = (batteryUsed / batteryCapacity) * 100f;
        
        // Update the battery level of the drone
        batteryLevel = Mathf.Clamp(batteryLevel - batteryUsed_percent, 0f, 100f);
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to check the battery level of the drone:

    void CheckBatteryLevel()
    {
        
        // If the battery level is less than 0, stop the drone
        if(batteryLevel <= 1f)
        {
            
            // Set the flag to shutdown the drone
            flagDroneShutdown = true;

            throttle = 0f; // Stop the throttle
            pitch = 0f; // Stop the pitch
            roll = 0f; // Stop the roll
            yaw = 0f; // Stop the yaw

            totalForce = Vector3.zero; // Stop the total force
            totalTorque = Vector3.zero; // Stop the total torque
            yawTorque = Vector3.zero; // Stop the yaw torque

        }

    }

}
