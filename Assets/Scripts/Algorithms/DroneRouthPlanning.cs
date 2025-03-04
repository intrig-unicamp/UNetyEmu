using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to perform the drone route planning
public class DroneRouthPlanning : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:
    
    [ReadOnly] public bool flagTargetReached; // Flag to indicate that the target has been reached
    [ReadOnly] public float permissibleErrorFromPIDController; // Permissible error from the PID controller
    public string routhTo; // Variable used in the switch statement to determine the route to follow

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    private int flagCurrentTarget; // Flag to indicate the current target

    private GetObjectFeatures getObjectFeatures; // Get the object features
    private string objectID; // Object ID

    private Vector3 finalTargetPosition; // Variable to store the final target position
    private float finalTargetOrientation; // Variable to store the final target orientation

    private GameObject targetObj; // Sphere target object related to the drone

    private DroneCurrentState.DroneStep currentStep; // Current step of the drone
    private DroneCurrentState droneCurrentState; // Current state of the drone

    private float speedRotationRouthToTarget = 1f; // Rotation speed
    private float accelerationRouthToTarget = 1.5f; // Acceleration factor
    private float decelerationRouthToTarget = 3.0f; // Deceleration factor

    private float speedRotationRouthToTargetPickDeliver = 1f; // Rotation speed
    private float accelerationRouthToTargetPickDeliver = 0.5f; // Acceleration factor
    private float decelerationRouthToTargetPickDeliver = 0.5f; // Deceleration factor

    private float speedRotationRouthToTargetLand = 1f; // Rotation speed
    private float accelerationRouthToTargetLand = 2.0f; // Acceleration factor
    private float decelerationRouthToTargetLand = 0.5f; // Deceleration factor

    private float speedRotationRouthToTargetThroughCheckPoint = 1f; // Rotation speed
    private float accelerationRouthToTargetThroughCheckPoint = 1.0f; // Acceleration factor
    
    private float currentSpeed; // Current object speed
    private float calculatedDecelerationDistance; // Calculated distance to start deceleration

    private float distanceToTarget; // Distance to the target
    private Quaternion finalRotation; // Final rotation of the object
    private Vector3 errorTarget; // Error between the final target position and the current position of the drone

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the object features and the object ID
        getObjectFeatures = GetComponent<GetObjectFeatures>();
        objectID = getObjectFeatures.objectID;
        
        // Initialize the current target flag
        flagCurrentTarget = 0;

        // Initialize the final target position and orientation
        finalTargetPosition = transform.position;
        finalTargetOrientation = transform.eulerAngles.y;

        // Get the sphere target object related to the drone
        targetObj = GameObject.Find("ID" + objectID + "Target");
        
        // Initialize the current step
        droneCurrentState = GetComponent<DroneCurrentState>();

        // Initialize the target reached flag
        flagTargetReached = false;

        // Initialize the permissible error from the PID controller (in meters)
        permissibleErrorFromPIDController = 0.5f;

        // Initialize the switch statement variable
        routhTo = "";

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // Switch statement to determine the route to follow
        switch (routhTo)
        {
            
            // Route to the target
            case "RouthToTarget":
                getCurrentStep();
                RouthToTarget(speedRotationRouthToTarget, accelerationRouthToTarget, decelerationRouthToTarget);
                break;

            // Route to the target through a checkpoint
            case "RouthToTargetThroughCheckPoint":
                getCurrentStep();
                RouthToTargetThroughCheckPoint(speedRotationRouthToTargetThroughCheckPoint, accelerationRouthToTargetThroughCheckPoint);
                break;

            // Route to the target to pick up and deliver
            case "RouthToTargetPickDeliver":
                getCurrentStep();
                RouthToTarget(speedRotationRouthToTargetPickDeliver, accelerationRouthToTargetPickDeliver, decelerationRouthToTargetPickDeliver);
                break;

            // Route to the target to land
            case "RouthToTargetLand":
                getCurrentStep();
                RouthToTarget(speedRotationRouthToTargetLand, accelerationRouthToTargetLand, decelerationRouthToTargetLand);
                break;

            // Default case
            case "":
                break;

        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to get the current step of the drone:

    void getCurrentStep()
    {
        
        // If there are steps in the internal route planning
        if(droneCurrentState.internalRoutePlanning.Count > 0)
        {
            
            // Get the current step
            currentStep = droneCurrentState.internalRoutePlanning[droneCurrentState.currentStepInternalIndex];

            // If the current target is reached, go to the next step
            if(flagCurrentTarget == 0)
            {
                // Set the final target position and orientation
                finalTargetPosition = currentStep.targetPos;

                // Set the final target orientation
                finalTargetOrientation = currentStep.targetOri;

                // Reset the flag of the target reached
                flagCurrentTarget = 1;
                
            }
            
        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to move the sphere target object related to the drone:

    void RouthToTarget(float speedRotation, float accelerationt, float deceleration)
    {
        
        // Calculates the distance from the sphere object to the target
        distanceToTarget = Vector3.Distance(targetObj.transform.position, finalTargetPosition);
        
        // Calculate the distance needed to decelerate to velocity = 0, right on target. Uniformly accelerated motion
        calculatedDecelerationDistance = (currentSpeed * currentSpeed) / (2 * deceleration);

        // Acceleration phase
        if (distanceToTarget > calculatedDecelerationDistance)
        {
            // Increases speed up to maximum speed
            currentSpeed = Mathf.Min(currentSpeed + accelerationt * Time.deltaTime, currentStep.maxVelocity);
        }
        else // Deceleration phase
        {
            // Decreases speed to 0 when the target is reached
            currentSpeed = Mathf.Max(currentSpeed - deceleration * Time.deltaTime, 0f);
        }

        // Move the sphere object towards the target position
        targetObj.transform.position = Vector3.MoveTowards(targetObj.transform.position, finalTargetPosition, currentSpeed * Time.deltaTime);

        // Get the target orientation
        finalRotation = Quaternion.Euler(
            targetObj.transform.eulerAngles.x, 
            finalTargetOrientation, 
            targetObj.transform.eulerAngles.z
        );

        // Rotate the sphere object towards the target orientation
        targetObj.transform.rotation = Quaternion.Slerp(targetObj.transform.rotation, finalRotation, speedRotation * Time.deltaTime);

        // If the distance to the target is less than 0.5 and the speed is 0
        if (distanceToTarget < 0.5f && currentSpeed == 0)
        {
            
            // Calculate the error between the final target position and the current position of the drone
            errorTarget = finalTargetPosition - transform.position; 

            // If the error is less than the permissible error from the PID controller
            if(errorTarget.magnitude < permissibleErrorFromPIDController)
            {
                flagTargetReached = true; // Set the flag of the target reached
                flagCurrentTarget = 0; // Reset the flag of the current target
            } 
            
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to move the drone to the target through a checkpoint:

    void RouthToTargetThroughCheckPoint(float speedRotation, float accelerationt)
    {
        
        // Calculates the distance from the sphere object to the target
        distanceToTarget = Vector3.Distance(targetObj.transform.position, finalTargetPosition);
        
        // Increases speed up to maximum speed (does not need to reduce speed to zero)
        currentSpeed = Mathf.Min(currentSpeed + accelerationt * Time.deltaTime, currentStep.maxVelocity);

        // Move the sphere object towards the target position
        targetObj.transform.position = Vector3.MoveTowards(targetObj.transform.position, finalTargetPosition, currentSpeed * Time.deltaTime);

        // Get the target orientation
        finalRotation = Quaternion.Euler(
            targetObj.transform.eulerAngles.x, 
            finalTargetOrientation, 
            targetObj.transform.eulerAngles.z
        );

        // Rotate the sphere object towards the target orientation
        targetObj.transform.rotation = Quaternion.Slerp(targetObj.transform.rotation, finalRotation, speedRotation * Time.deltaTime);

        // If the distance to the target is less than 0.5
        if (distanceToTarget < 0.5f)
        {
            
            // Calculate the error between the final target position and the current position of the drone
            errorTarget = finalTargetPosition - transform.position; 

            // If the error is less than the permissible error from the PID controller
            if(errorTarget.magnitude < permissibleErrorFromPIDController)
            {
                flagTargetReached = true; // Set the flag of the target reached
                flagCurrentTarget = 0; // Reset the flag of the current target
            } 
            
        }
        
    }

} 
