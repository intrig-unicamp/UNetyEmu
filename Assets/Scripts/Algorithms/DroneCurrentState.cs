using System; // Library to use serializable classes
using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using System.Collections.Generic; // Library to use lists
using System.Collections; // Library to use in IEnumerator class

// Class to identify the current state of the drone
public class DroneCurrentState : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Initialize the current step internal index and the internal route planning list
    [ReadOnly] public int currentStepInternalIndex;
    [ReadOnly] public List<DroneStep> internalRoutePlanning;
    [ReadOnly] public string currentStateString;

    // Initialize the permissible error to get the target from the PID controller (changes the value in DroneRouthPlanning)
    public float permissibleErrorToGetTarget;

    // Class to store the drone's state, position, orientation, and velocity:
    [Serializable] public class DroneStep
    {
        
        // Variables of this class:
        public DroneState state;
        public Vector3 targetPos;
        public float targetOri;
        public float maxVelocity;

        // Constructor of this class:
        public DroneStep(DroneState state, Vector3 targetPos, float targetOri, float maxVelocity)
        {
            this.state = state;
            this.targetPos = targetPos;
            this.targetOri = targetOri;
            this.maxVelocity = maxVelocity;
        }

    }

    // Class to enumerate the possible states of the drone:
    [Serializable] public enum DroneState { 
        StandBy,
        TakeOff, 
        MoveToPickupPackage,
        MoveToCheckPoint,
        MoveToDelivery,
        Land,
        PickUpPackage,
        DeliverPackage,
        ReturnToHub
    };
    
    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Get the DroneRouthPlanning component to set the routh planning to the drone
    private DroneRouthPlanning droneRouthPlanning;

    // Initialize the current state of the drone
    private DroneState currentState;

    // Initiate flags related to time pauses
    private int flagTime2;
    private int flagTime4;
    private bool coroutineFlagTime2 = false;
    private bool coroutineFlagTime4 = false;

    // Get the Drone1Animation and Drone2Animation components to set the animations of the drone
    private Drone1Animation drone1Animation;
    private Drone2Animation drone2Animation;
    private int flagAnimation;

    // Get the DroneJoinedToPackage component to attach and deliver the package
    private DroneJoinedToPackage droneJoinedToPackage;
    private float distanceToPackage;
    
    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Initialize the internal route planning list and the current step internal index
        internalRoutePlanning = new List<DroneStep>{};
        currentStepInternalIndex = 0;

        // Set the current state of the drone to StandBy
        currentState = DroneState.StandBy;
        currentStateString = currentState.ToString();
        
        // Get the DroneRouthPlanning component to set the routh to the drone
        droneRouthPlanning = GetComponent<DroneRouthPlanning>();

        // Get the DroneJoinedToPackage component
        droneJoinedToPackage = GetComponent<DroneJoinedToPackage>();

        // Initialize the flags related to time pauses
        flagTime2 = 1;
        flagTime4 = 1;
        
        // Get the Drone1Animation and Drone2Animation components
        drone1Animation = GetComponent<Drone1Animation>();
        drone2Animation = GetComponent<Drone2Animation>();

        // Initialize the flag for the pick up and deliver animations
        flagAnimation = 0;

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals, ideal for physical changes in objects:

    void FixedUpdate()
    {
        
        // Switch to define the current state of the drone
        switch (currentState)
        {
            
            // If the current state is StandBy
            case DroneState.StandBy:
                StandBy();
                break;

            // If the current state is TakeOff
            case DroneState.TakeOff:
                TakeOff();
                break;

            // If the current state is MoveToCheckPoint
            case DroneState.MoveToCheckPoint:
                MoveToCheckPoint();
                break;

            // If the current state is MoveToPickupPackage
            case DroneState.MoveToPickupPackage:
                MoveToPickupPackage();
                break;

            // If the current state is MoveToDelivery
            case DroneState.MoveToDelivery:
                MoveToDelivery();
                break;

            // If the current state is Land
            case DroneState.Land:
                Land();
                break;

            // If the current state is PickUpPackage
            case DroneState.PickUpPackage:
                PickUpPackage();
                break;

            // If the current state is DeliverPackage
            case DroneState.DeliverPackage:
                DeliverPackage();
                break;

            // If the current state is ReturnToHub
            case DroneState.ReturnToHub:
                ReturnToHub();
                break;
            
        }

        // Update the current state string variable
        currentStateString = currentState.ToString();

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to add a new step to the internal route planning list:

    void StartNextStep()
    {
        
        // If the internal route planning list is not empty
        if(internalRoutePlanning.Count > 0)
        {
            
            // If the current step internal index is less than the internal route planning list count
            if(currentStepInternalIndex < internalRoutePlanning.Count)
            {
               
                // Get the next step from the internal route planning list
                DroneStep nextStep = internalRoutePlanning[currentStepInternalIndex];

                // Set the current state of the drone to the next step state
                currentState = nextStep.state;

            }
            else
            {
                currentState = DroneState.StandBy; // Set the current state of the drone to StandBy
            }

        }
        else
        {
            currentStepInternalIndex = 0; // There are no steps in the internal route planning list
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to StandBy:

    void StandBy()
    {
    
        // Set the internal route planning list with the steps to follow
        droneRouthPlanning.routhTo = "";

        // Add the steps to the internal route planning list
        StartNextStep();
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to TakeOff:

    void TakeOff()
    {
        
        // Set the permissible error for TakeOff
        permissibleErrorToGetTarget = 0.5f;

        // If the target is not reached
        if(droneRouthPlanning.flagTargetReached == false)
        {
            
            // Set the TakeOff routh planning to the drone
            droneRouthPlanning.routhTo = "RouthToTarget";

            // Set the permissible error from the PID controller
            droneRouthPlanning.permissibleErrorFromPIDController = permissibleErrorToGetTarget;

        } 
        
        // If the target is reached
        if(droneRouthPlanning.flagTargetReached == true)
        {
            
            // Reset the flag target reached
            droneRouthPlanning.flagTargetReached = false;

            // Increment the current step internal index
            currentStepInternalIndex++;

            // Add the steps to the internal route planning list
            StartNextStep();
            
        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to MoveToCheckPoint:

    void MoveToCheckPoint()
    {
        
        // Set the permissible error for MoveToCheckPoint
        permissibleErrorToGetTarget = 5f;

        // If the target is not reached
        if(droneRouthPlanning.flagTargetReached == false)
        {
            
            // Set the MoveToCheckPoint routh planning to the drone
            droneRouthPlanning.routhTo = "RouthToTargetThroughCheckPoint";

            // Set the permissible error from the PID controller
            droneRouthPlanning.permissibleErrorFromPIDController = permissibleErrorToGetTarget;

        } 
        
        // If the target is reached
        if(droneRouthPlanning.flagTargetReached == true)
        {
            
            // Reset the flag target reached
            droneRouthPlanning.flagTargetReached = false;

            // Increment the current step internal index
            currentStepInternalIndex++;

            // Add the steps to the internal route planning list
            StartNextStep();
            
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to MoveToPickupPackage:

    void MoveToPickupPackage()
    {
        
        // Set the permissible error for MoveToPickupPackage
        permissibleErrorToGetTarget = 0.5f;

        // If the target is not reached
        if(droneRouthPlanning.flagTargetReached == false)
        {
            
            // Set the MoveToPickupPackage routh planning to the drone
            droneRouthPlanning.routhTo = "RouthToTarget";

            // Set the permissible error from the PID controller
            droneRouthPlanning.permissibleErrorFromPIDController = permissibleErrorToGetTarget;

        } 
        
        // If the target is reached
        if(droneRouthPlanning.flagTargetReached == true)
        {
            
            // Reset the flag target reached
            droneRouthPlanning.flagTargetReached = false;

            // Increment the current step internal index
            currentStepInternalIndex++;

            // Add the steps to the internal route planning list
            StartNextStep();
            
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to MoveToDelivery:

    void MoveToDelivery()
    {
        
        // Set the permissible error for MoveToDelivery
        permissibleErrorToGetTarget = 0.5f;

        // If the target is not reached
        if(droneRouthPlanning.flagTargetReached == false)
        {
            
            // Set the MoveToDelivery routh planning to the drone
            droneRouthPlanning.routhTo = "RouthToTarget";

            // Set the permissible error from the PID controller
            droneRouthPlanning.permissibleErrorFromPIDController = permissibleErrorToGetTarget;

        } 
        
        // If the target is reached
        if(droneRouthPlanning.flagTargetReached == true)
        {
            
            // Reset the flag target reached
            droneRouthPlanning.flagTargetReached = false;

            // Increment the current step internal index
            currentStepInternalIndex++;

            // Add the steps to the internal route planning list
            StartNextStep();
            
        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to Land:

    void Land()
    {
        
        // Set the permissible error for Land
        permissibleErrorToGetTarget = 0.5f;

        // If the target is not reached
        if(droneRouthPlanning.flagTargetReached == false)
        {
            
            // Set the Land routh planning to the drone
            droneRouthPlanning.routhTo = "RouthToTargetLand";

            // Set the permissible error from the PID controller
            droneRouthPlanning.permissibleErrorFromPIDController = permissibleErrorToGetTarget;

        } 
        
        // If the target is reached
        if(droneRouthPlanning.flagTargetReached == true)
        {
            
            // Reset the flag target reached
            droneRouthPlanning.flagTargetReached = false;

            // Increment the current step internal index
            currentStepInternalIndex++;

            // Add the steps to the internal route planning list
            StartNextStep();
            
        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to PickUpPackage:

    void PickUpPackage()
    {
        
        // Set the permissible error for PickUpPackage
        permissibleErrorToGetTarget = 0.1f;

        // If the target is not reached
        if(droneRouthPlanning.flagTargetReached == false)
        {
            
            // Set the PickUpPackage routh planning to the drone
            droneRouthPlanning.routhTo = "RouthToTargetPickDeliver";

            // Set the permissible error from the PID controller
            droneRouthPlanning.permissibleErrorFromPIDController = permissibleErrorToGetTarget;

        }
        
        // If the target is reached
        if(droneRouthPlanning.flagTargetReached == true)
        {
            
            // Start a coroutin for waiting 2 seconds to activate the package pickup animation
            if(!coroutineFlagTime2)
            {
                coroutineFlagTime2 = true; // Set the flag to true
                StartCoroutine(WaitAndAct2()); // Start the coroutine
            }

            // Start a coroutin for waiting 4 seconds to move to the next state
            if(!coroutineFlagTime4)
            {
                coroutineFlagTime4 = true; // Set the flag to true
                StartCoroutine(WaitAndAct4()); // Start the coroutine
            }

            // If 2 seconds have passed and the animation has not been activated
            if( (flagTime2 == 0) && (flagAnimation == 0) ) droneAttachToPackage();

            // If 4 seconds have passed
            if(flagTime4 == 0)
            {
                
                // Reset the flag target reached
                droneRouthPlanning.flagTargetReached = false;

                // Increment the current step internal index
                currentStepInternalIndex++;

                // Add the steps to the internal route planning list
                StartNextStep();

                // Reset the flags and the coroutines for the time pauses
                flagAnimation = 0;
                flagTime2 = 1;
                flagTime4 = 1;
                coroutineFlagTime2 = false;
                coroutineFlagTime4 = false;

            }
            
        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to DeliverPackage:

    void DeliverPackage()
    {
        
        // Set the permissible error for DeliverPackage
        permissibleErrorToGetTarget = 0.1f;

        // If the target is not reached
        if(droneRouthPlanning.flagTargetReached == false)
        {
            
            // Set the DeliverPackage routh planning to the drone
            droneRouthPlanning.routhTo = "RouthToTargetPickDeliver";
            
            // Set the permissible error from the PID controller
            droneRouthPlanning.permissibleErrorFromPIDController = permissibleErrorToGetTarget;

        } 
        
        // If the target is reached
        if(droneRouthPlanning.flagTargetReached == true)
        {
            
            // Start a coroutin for waiting 2 seconds to activate the package pickup animation
            if(!coroutineFlagTime2)
            {
                coroutineFlagTime2 = true; // Set the flag to true
                StartCoroutine(WaitAndAct2()); // Start the coroutine
            }

            // Start a coroutin for waiting 4 seconds to move to the next state
            if(!coroutineFlagTime4)
            {
                coroutineFlagTime4 = true; // Set the flag to true
                StartCoroutine(WaitAndAct4()); // Start the coroutine
            }

            // If 2 seconds have passed and the animation has not been activated
            if( (flagTime2 == 0) && (flagAnimation == 0) ) droneDeliverPackage();

            // If 4 seconds have passed
            if(flagTime4 == 0)
            {
                
                // Reset the flag target reached
                droneRouthPlanning.flagTargetReached = false;

                // Increment the current step internal index
                currentStepInternalIndex++;

                // Add the steps to the internal route planning list
                StartNextStep();

                // Reset the flags and the coroutines for the time pauses
                flagAnimation = 0;
                flagTime2 = 1;
                flagTime4 = 1;
                coroutineFlagTime2 = false;
                coroutineFlagTime4 = false;

            }
            
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set th current state of the drone to ReturnToHub:

    void ReturnToHub()
    {
        
        // Set the permissible error for ReturnToHub
        permissibleErrorToGetTarget = 0.25f;

        // If the target is not reached
        if(droneRouthPlanning.flagTargetReached == false)
        {
            
            // Set the ReturnToHub routh planning to the drone
            droneRouthPlanning.routhTo = "RouthToTarget";

            // Set the permissible error from the PID controller
            droneRouthPlanning.permissibleErrorFromPIDController = permissibleErrorToGetTarget;

        } 
        
        // If the target is reached
        if(droneRouthPlanning.flagTargetReached == true)
        {
            
            // Reset the flag target reached
            droneRouthPlanning.flagTargetReached = false;

            // Increment the current step internal index
            currentStepInternalIndex++;

            // Add the steps to the internal route planning list
            StartNextStep();
            
        }
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to start a coroutine for waiting 2 seconds:
    
    IEnumerator WaitAndAct2()
    {
        flagTime2 = 1; // Set the flag to 1 to wait
        yield return new WaitForSeconds(2); // Wait for 2 seconds
        flagTime2 = 0; // Set the flag to 0 to continue
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to start a coroutine for waiting 4 seconds:

    IEnumerator WaitAndAct4()
    {
        flagTime4 = 1; // Set the flag to 1 to wait
        yield return new WaitForSeconds(4); // Wait for 4 seconds
        flagTime4 = 0; // Set the flag to 0 to continue
    }

    // -----------------------------------------------------------------------------------------------------
    // Class to attach the package GameObject to the drone using the DroneJoinedToPackage component:

    void droneAttachToPackage()
    {

        // Calculate the distance between the drone and the package
        distanceToPackage = Vector3.Distance(transform.position, droneJoinedToPackage.objectPackage.transform.position);
        
        // If the distance is less than 0.1f
        if(distanceToPackage < 0.1f)
        {
            droneJoinedToPackage.isPackageAttached = true; // Set the package attached flag to true
            pickUpAnimation(); // Call the pick up animation method
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to deliver the package using the DroneJoinedToPackage component:

    void droneDeliverPackage()
    {
        
        // Set the package delivered flag to true and call the deliver animation method
        droneJoinedToPackage.isPackageDelivered = true; 
        deliverAnimation();

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set the pick up animation of the drone:

    void pickUpAnimation()
    {

        // Set the pick up package flag to true for the type of the Drone1
        if(drone1Animation != null) drone1Animation.isPickingUpPackage = true;

        // Set the pick up package flag to true for the type of the Drone2
        if(drone2Animation != null) drone2Animation.isPickingUpPackage = true;

        // Reset the flag of the animation
        flagAnimation = 1;

    }

    // -----------------------------------------------------------------------------------------------------
    // Class to set the deliver animation of the drone:

    void deliverAnimation()
    {

        // Set the deliver package flag to true for the type of the Drone1
        if(drone1Animation != null) drone1Animation.isDeliveringPackage = true;

        // Set the deliver package flag to true for the type of the Drone2
        if(drone2Animation != null) drone2Animation.isDeliveringPackage = true;

        // Reset the flag of the animation
        flagAnimation = 1;

    }

} 
