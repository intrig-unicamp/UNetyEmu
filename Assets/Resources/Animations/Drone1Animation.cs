using UnityEngine; // Library to use MonoBehaviour classes

// Class to control the animations of the small drone
public class Drone1Animation : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Variables to identify the drone's state
    [ReadOnly] public bool isPickingUpPackage = false;
    [ReadOnly] public bool isDeliveringPackage = false;

    // Speed of package holders animation
    public float packageHolderAnimationSpeed = 3f;

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Animation features
    private Animator prefabDrone1Animation; // Animator component of the drone
    private float propellerAnimationFPS = 100f; // Frames per second of the propeller animation
    private float smoothSpeed = 0.01f; // Smooth speed of the animation
    private float rotationSpeed; // Propeller rotation speed
    private float propellerMaxRPM; // Maximum RPM of the propeller

    // Variables to obtain the drone's forces and torques
    private float droneCurrentTotalForceApplied;
    private float droneCurrentYawTorque;
    
    // Variables to obtain the drone's forces and torques
    DroneDynamics droneDynamics;
    GetDroneFeatures getDroneFeatures;

    // Variables to calculate the propeller rotation speed
    private float propellerMaxRPS;
    private float currentFPS;
    private float targetFPS;
    
    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the DroneDynamics and GetDroneFeatures components of the drone
        droneDynamics = GetComponent<DroneDynamics>();
        getDroneFeatures = GetComponent<GetDroneFeatures>();
        
        // Get the Animator component of the drone
        prefabDrone1Animation = GetComponent<Animator>(); 

        // Set the initial rotation speed of the propeller
        rotationSpeed = 0f; 

    }

    // -----------------------------------------------------------------------------------------------------
    // Update is called once per frame:

    void Update()
    {
        
        // If the droneDynamics and getDroneFeatures are not null, get the animation rotation speed
        if( (droneDynamics != null) && (getDroneFeatures != null) )
        {
            if(prefabDrone1Animation != null) GetAnimationRotationSpeed();
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to get the animation rotation speed of the propeller:

    void GetAnimationRotationSpeed(){

        propellerMaxRPM = getDroneFeatures.propellerMaxRPM; // Get the maximum RPM of the propeller
        droneCurrentTotalForceApplied = droneDynamics.totalForce.magnitude; // Get the total force applied to the drone
        droneCurrentYawTorque = droneDynamics.yaw; // Get the yaw torque applied to the drone

        // Calculate the target frames per second of the propeller animation
        propellerMaxRPS = propellerMaxRPM / 60;
        currentFPS = Mathf.Round( 1.0f / Time.deltaTime );
        targetFPS = propellerAnimationFPS / propellerMaxRPS;

        // Calculate the rotation speed of the propeller
        rotationSpeed = (droneCurrentTotalForceApplied + droneCurrentYawTorque) * smoothSpeed * ( currentFPS / targetFPS );

        // Call the method to animate the propeller rotation speed
        rotationPropellerSpeed();

        // If the drone is picking up a package, call the method to animate the package holder
        if(isPickingUpPackage) pickUpPackageAnimation();

        // If the drone is delivering a package, call the method to animate the package holder
        if(isDeliveringPackage) deliverPackageAnimation();
        
    }

    // -----------------------------------------------------------------------------------------------------
    // Method to animate the propeller rotation speed:

    void rotationPropellerSpeed(){

        // Set the rotation speed of the propeller
        prefabDrone1Animation.SetFloat("RotationSpeed", rotationSpeed);

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to animate the package holder when delivering a package:

    void deliverPackageAnimation(){

        // Set the animation time of the first package holder
        prefabDrone1Animation.Play("PickUpPackageSS_1_2_Open", -1, -2.5f);
        prefabDrone1Animation.Play("PickUpPackageSS_1_3_Open", -1, -1.1f);
        prefabDrone1Animation.Play("PickUpPackageSS_1_4_Open", -1, -0.7f);
        prefabDrone1Animation.Play("PickUpPackageSS_1_5_Open", -1, -0.2f);
        prefabDrone1Animation.Play("PickUpPackageSS_1_6_Open", -1, 0f);

        // Set the animation time of the second package holder
        prefabDrone1Animation.Play("PickUpPackageSS_2_2_Open", -1, -2.5f);
        prefabDrone1Animation.Play("PickUpPackageSS_2_3_Open", -1, -1.1f);
        prefabDrone1Animation.Play("PickUpPackageSS_2_4_Open", -1, -0.7f);
        prefabDrone1Animation.Play("PickUpPackageSS_2_5_Open", -1, -0.2f);
        prefabDrone1Animation.Play("PickUpPackageSS_2_6_Open", -1, 0f);

        // Set the animation time of the third package holder
        prefabDrone1Animation.Play("PickUpPackageSS_3_2_Open", -1, -2.5f);
        prefabDrone1Animation.Play("PickUpPackageSS_3_3_Open", -1, -1.1f);
        prefabDrone1Animation.Play("PickUpPackageSS_3_4_Open", -1, -0.7f);
        prefabDrone1Animation.Play("PickUpPackageSS_3_5_Open", -1, -0.2f);
        prefabDrone1Animation.Play("PickUpPackageSS_3_6_Open", -1, 0f);

        // Set the animation time of the fourth package holder
        prefabDrone1Animation.Play("PickUpPackageSS_4_2_Open", -1, -2.5f);
        prefabDrone1Animation.Play("PickUpPackageSS_4_3_Open", -1, -1.1f);
        prefabDrone1Animation.Play("PickUpPackageSS_4_4_Open", -1, -0.7f);
        prefabDrone1Animation.Play("PickUpPackageSS_4_5_Open", -1, -0.2f);
        prefabDrone1Animation.Play("PickUpPackageSS_4_6_Open", -1, 0f);

        // Set the speed of the package holder animation
        prefabDrone1Animation.speed = packageHolderAnimationSpeed;

        // Set the trigger to open the package holder
        prefabDrone1Animation.SetTrigger("TrOpen");

        // Finish the animation
        isDeliveringPackage = false;

    } 

    // -----------------------------------------------------------------------------------------------------
    // Method to animate the package holder when picking up a package:

    void pickUpPackageAnimation(){

        // Set the speed of the package holder animation
        prefabDrone1Animation.speed = packageHolderAnimationSpeed;

        // Set the trigger to close the package holder
        prefabDrone1Animation.SetTrigger("TrClose");

        // Finish the animation
        isPickingUpPackage = false;
        
    }

}
