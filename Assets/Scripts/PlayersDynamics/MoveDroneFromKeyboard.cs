using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to move the drone from the keyboard
public class MoveDroneFromKeyboard : MonoBehaviour
{
    
    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    [Header("Speed of movement and rotation")]
    public float moveSpeed = 5f; // Speed of movement
    public float rotationSpeed = 100f; // Speed of rotation

    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    private GetObjectFeatures getObjectFeatures; // GetObjectFeatures script of the GameObject
    private GameObject targetObj; // Sphere target to move the drone
    private string objectID; // ID of the GameObject to find the target

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get the GetObjectFeatures script of the GameObject
        getObjectFeatures = GetComponent<GetObjectFeatures>();
        objectID = getObjectFeatures.objectID;
        
        // Get the target object to move the drone
        targetObj = GameObject.Find("ID" + objectID + "Target");

    }

    void Update()
    {
        
        // Movement relative to the object's orientation (local space)
        if (Input.GetKey(KeyCode.U)) // Move forward
        {
            targetObj.transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.J)) // Move backward
        {
            targetObj.transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.H)) // Move left
        {
            targetObj.transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.K)) // Move right
        {
            targetObj.transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }

        // Vertical movement (global)
        if (Input.GetKey(KeyCode.UpArrow)) // Move up
        {
            targetObj.transform.Translate(Vector3.up * moveSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.DownArrow)) // Move down
        {
            targetObj.transform.Translate(Vector3.down * moveSpeed * Time.deltaTime, Space.World);
        }

        // Rotation with left/right arrows
        if (Input.GetKey(KeyCode.LeftArrow)) // Rotate left
        {
            targetObj.transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.RightArrow)) // Rotate right
        {
            targetObj.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
        
    }

}
