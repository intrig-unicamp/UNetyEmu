using System; // Library to use DateTime class
using UnityEngine; // Library to use MonoBehaviour classes
using TMPro; // Library to use TextMeshProUGUI class
using UnityEngine.UI; // Library to use Image class

// Class to display all the UI elements in the scene
public class UIAllDisplay : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    // Variables to display the cardinal points
    public RectTransform cardinalPointsTransform;
    
    // Variable to display the time
    public TextMeshProUGUI timeText;
    
    // Variables to display the battery level
    public TextMeshProUGUI batteryText;
    public Image batteryImage;

    // Variables to display the wifi signal
    public Image wifi1White;
    public Image wifi2White;
    public Image wifi3White;
    public Image wifi4White;
    public Image wifi1Gray;
    public Image wifi2Gray;
    public Image wifi3Gray;
    public Image wifi4Gray;
    
    // Variables to display the speed
    public TextMeshProUGUI speedText;
    public RectTransform speedScaleGraphTransform;
    
    // Variables to display the altitude
    public TextMeshProUGUI altitudeText;
    public RectTransform altitudeScaleGraphTransform;
    
    // Variables to display the player name
    public TextMeshProUGUI playerText;

    // Variables to display the player current status
    public TextMeshProUGUI currentStatusText;

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    // Variable to get the player's rotation in the Y axis
    private float playerRotationY;

    // Variable to get the current time
    private string timeString;

    // Variables to get the battery level
    private string batteryString;
    private float batteryLevel;
    private int batteryLevelPercentage;

    // Variables to get the wifi signal
    private float wifiSignal;
    private bool wifi1Flag;
    private bool wifi2Flag;
    private bool wifi3Flag;
    private bool wifi4Flag;

    // Variables to get the speed
    private string playerSpeed;
    private float speedScale;
    private float speedFactorMapped;
    private float speedMappedValue;
    private float maxSpeedScale;
    private float minSpeedScale;

    // Variables to get the altitude
    private string playerAltitude;
    private float altitudeScale;
    private float altitudeFactorMapped;
    private float altitudeMappedValue;
    private float maxAltitudeScale;
    private float minAltitudeScale;

    // Variables to get the player name
    private string playerName;

    // Variables to get the player current status
    private string playerCurrentStatus;

    // Variable to get the current time
    private DateTime now;

    // Variable to get the Main Camera object
    private GameObject mainCameraScript;

    // Variable to get the FollowCamera script of the main camera
    private FollowCamera followCamera;

    // Variable to get the player object
    private GameObject playerObject;

    // Variables to get the DroneDynamics, DroneCommunication, DroneCurrentState, and Rigidbody components of the player object
    private DroneDynamics droneDynamics;
    private DroneCommunication droneCommunication;
    private DroneCurrentState droneCurrentState;
    private Rigidbody rb;

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Set the initial values of the player's rotation in the Y axis
        playerRotationY = 0f;
        
        // Set the initial values of the battery level
        batteryString = "100";
        batteryLevel = 100f;
        batteryLevelPercentage = 1;

        // Set the initial values of the wifi signal
        wifiSignal = 1f;
        wifi1Flag = true;
        wifi2Flag = true;
        wifi3Flag = true;
        wifi4Flag = true;

        // Set the initial values of the altitude
        playerAltitude = "";
        minAltitudeScale = 442.9f; // Minimum position according to the height scale image
        maxAltitudeScale = -442.9f; // Maximum position according to the height scale image
        altitudeFactorMapped = 3.55f; // Factor to match the actual height values with those shown in the image
        altitudeMappedValue = minAltitudeScale;

        // Set the initial values of the speed
        playerSpeed = "";
        minSpeedScale = 442.9f; // Minimum position according to the speed scale image
        maxSpeedScale = -442.9f; // Maximum position according to the speed scale image
        speedFactorMapped = 17.9f; // Factor to match the actual speed values with those shown in the image
        speedMappedValue = minSpeedScale;

        // Set the initial value of the player name
        playerName = "";

        // Set the initial value of the player current status
        playerCurrentStatus = "";

        // Find the Main Camera object
        mainCameraScript = GameObject.Find("Main Camera");

        // Get the followCamera script of the main camera
        followCamera = mainCameraScript.GetComponent<FollowCamera>();

    }

    // -----------------------------------------------------------------------------------------------------
    // Update is called once per frame:

    void Update()
    {
        
        // Find the player object
        playerObject = GameObject.Find(followCamera.playerName);

        // Set the value of the current time
        now = DateTime.Now;
        timeString = now.ToString("HH:mm:ss");

        // If the cardinalPointsTransform is not null, update the rotation of the indicator
        if(cardinalPointsTransform != null)
        {
            cardinalPointsTransform.rotation = Quaternion.Euler(0, 0, playerRotationY);
        }
        
        // If the timeText is not null, update the time
        if (timeText != null)
        {
            timeText.text = timeString;
        }

        // If the batteryText and batteryImage are not null, update the battery level in the UI elements
        if ((batteryText != null) && (batteryImage != null))
        {
            batteryText.text = batteryString;
            batteryImage.fillAmount = batteryLevel / 100f; // Fill the battery image between 0 and 1
        }

        // If the wifiWhite and wifiGray are not null, update the wifi signal in the UI elements
        if ((wifi1White != null) && (wifi1Gray != null))
        {
            wifi1White.enabled = wifi1Flag;
            wifi1Gray.enabled = !wifi1Flag;
        }
        if ((wifi2White != null) && (wifi2Gray != null))
        {
            wifi2White.enabled = wifi2Flag;
            wifi2Gray.enabled = !wifi2Flag;
        }
        if ((wifi3White != null) && (wifi3Gray != null))
        {
            wifi3White.enabled = wifi3Flag;
            wifi3Gray.enabled = !wifi3Flag;
        }
        if ((wifi4White != null) && (wifi4Gray != null))
        {
            wifi4White.enabled = wifi4Flag;
            wifi4Gray.enabled = !wifi4Flag;
        }

        // If the altitudeText and altitudeScaleGraphTransform are not null, update the altitude in the UI elements
        if ((altitudeText != null) && (altitudeScaleGraphTransform != null))
        {
            altitudeText.text = playerAltitude;
            altitudeScaleGraphTransform.anchoredPosition = new Vector2(0, altitudeMappedValue);
        }

        // If the speedText and speedScaleGraphTransform are not null, update the speed in the UI elements
        if ((speedText != null) && (speedScaleGraphTransform != null))
        {
            speedText.text = playerSpeed;
            speedScaleGraphTransform.anchoredPosition = new Vector2(0, speedMappedValue);
        }

        // If the playerText and currentStatusText are not null, update the player name and current status in the UI elements
        if ((playerText != null) && (currentStatusText != null))
        {
            playerText.text = playerName;
            currentStatusText.text = playerCurrentStatus;
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at a fixed interval:

    void FixedUpdate()
    {
          
        // If the followCamera exists
        if (followCamera != null)
        {
            
            // Try to get the DroneDynamics component of the player object
            try
            {

                // Get the DroneDynamics component of the player object
                droneDynamics = playerObject.GetComponent<DroneDynamics>();

                // Get the battery level
                batteryLevel = droneDynamics.batteryLevel;

                // Get the battery level percentage
                batteryLevelPercentage = (int)batteryLevel;

                // Set the battery level string for the UI element
                batteryString = batteryLevelPercentage.ToString();

            }
            catch (Exception)
            {

                // Set the battery level string for the UI element
                batteryString = "";
                batteryLevel = 100f;

            }

            // Try to get the DroneCommunication component of the player object
            try
            {

                // Get the DroneCommunication component of the player object
                droneCommunication = playerObject.GetComponent<DroneCommunication>();

                // Get the wifi signal
                wifiSignal = droneCommunication.droneWifiSignal;
                
            }
            catch (Exception){}

            // Show the wifi signal in the UI elements
            if(wifiSignal < 0.1f) // no signal
            {
                wifi1Flag = false;
                wifi2Flag = false;
                wifi3Flag = false;
                wifi4Flag = false;
            }
            else if(wifiSignal < 0.25f) // low signal
            {
                wifi1Flag = true;
                wifi2Flag = false;
                wifi3Flag = false;
                wifi4Flag = false;
            }
            else if(wifiSignal < 0.5f) // medium signal
            {
                wifi1Flag = true;
                wifi2Flag = true;
                wifi3Flag = false;
                wifi4Flag = false;
            }
            else if(wifiSignal < 0.75f) // good signal
            {
                wifi1Flag = true;
                wifi2Flag = true;
                wifi3Flag = true;
                wifi4Flag = false;
            }
            else // excellent signal 
            {
                wifi1Flag = true;
                wifi2Flag = true;
                wifi3Flag = true;
                wifi4Flag = true;
            }
            
            // Try to get the DroneCurrentState component of the player object
            try
            {

                // Get the DroneCurrentState component of the player object
                droneCurrentState = playerObject.GetComponent<DroneCurrentState>();

                // Get the current status of the player
                playerCurrentStatus = droneCurrentState.currentStateString;
                
            }
            catch (Exception)
            {

                // Set the player current status like empty strings
                playerCurrentStatus = "";

            }

            // Try to get the rigidbody component of the player object
            try
            {

                // Get the rigidbody component of the player object
                rb = playerObject.GetComponent<Rigidbody>();

                // Get the speed of the player
                speedScale = rb.velocity.magnitude;

                // Set the speed string for the UI element
                playerSpeed = speedScale.ToString("F2");

                // Map the speed value to the speed scale image
                speedMappedValue = minSpeedScale - (speedScale * speedFactorMapped);
                if(speedMappedValue > minSpeedScale) speedMappedValue = minSpeedScale;
                if(speedMappedValue < maxSpeedScale) speedMappedValue = maxSpeedScale;
                
            }
            catch (Exception)
            {

                // Set the speed strings like empty strings
                playerSpeed = "";
                
                // Set the speed scale image values 
                speedMappedValue = minSpeedScale;
                
            }

            // Try to get the player object and its components
            try
            {
                  
                // Set the player's rotation in the Y axis (the icon is rotated negatively)
                playerRotationY = - playerObject.transform.eulerAngles.y;
                
                // Get the altitude of the player
                altitudeScale = playerObject.transform.position.y;

                // Set the altitude string for the UI element
                playerAltitude = altitudeScale.ToString("F2");

                // Map the altitude value to the altitude scale image
                altitudeMappedValue = minAltitudeScale - (altitudeScale * altitudeFactorMapped);
                if(altitudeMappedValue > minAltitudeScale) altitudeMappedValue = minAltitudeScale;
                if(altitudeMappedValue < maxAltitudeScale) altitudeMappedValue = maxAltitudeScale;
                
                // Get the player name and current status
                playerName = playerObject.name;
                
                
            }
            catch (Exception)
            {

                // Set the player's rotation in the Y axis
                playerRotationY = 0f;
                
                // Set the altitude and speed strings like empty strings
                playerAltitude = "";
                
                // Set the altitude and speed scale image values
                altitudeMappedValue = minAltitudeScale;
                
                // Set the player name and current status like empty strings
                playerName = "";
                                
            }
            
        }
        
    }

}
