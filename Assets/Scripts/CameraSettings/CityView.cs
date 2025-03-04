using UnityEngine; // Library to use in MonoBehaviour classes

// Class to control the city view camera
public class CityView : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    public bool cityView = false; // Flag to control the city view mode

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    private Camera cam; // Reference to the camera component

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the camera component
        cam = GetComponent<Camera>();
        cam.enabled = false;

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at a fixed interval:

    void FixedUpdate()
    {
        
        // Enable or disable the camera according to the city view mode
        if(cityView) cam.enabled = true;
        else cam.enabled = false;

    }

}
