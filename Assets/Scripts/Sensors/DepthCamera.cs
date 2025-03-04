using UnityEngine; // Unity Engine library to use in MonoBehaviour classes
using System.IO; // System library to use in File class
using System.Collections; // System library to use in IEnumerator class

// Class that calls the material and shader used to calculate the depth of the image captured by the camera:
public class DepthCamera : MonoBehaviour
{

    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    public bool captureImage = false;  // Variable to activate image capture
    
    // -----------------------------------------------------------------------------------------------------
    // Private variables of this class:

    private string objectName; // Name of the GameObject parent of the camera
    private Transform parentTransform; // Transform of the parent GameObject of the camera
    private GameObject targetGameObject; // Target GameObject of the parent GameObject
    private GetObjectFeatures getObjectFeatures; // GetObjectFeatures script of the GameObject
    private string objectID; // ID of the GameObject

    private Camera depthCamera; // Set the depth texture mode to Depth
    private Material material; // Material to apply the shader to the camera
    private Shader shader; // Shader to calculate the depth of the image
    private RenderTexture renderTexture; // RenderTexture to the depth camera
    private Texture2D depthImage; // Texture to read the pixels of the depth image

    private byte[] imageBytes; // Bytes of the depth image
    private string folderPath; // Folder Path to save the depth images
    private string filePath; // File Path to save the depth images

    // -----------------------------------------------------------------------------------------------------
    // Start is called before the first frame update:

    void Start()
    {
        
        // Get to the transform of the parent GameObject
        parentTransform = transform.parent;

        // Get the name of the parent GameObject
        objectName = parentTransform.gameObject.name;

        // Get the GetObjectFeatures script of the parent GameObject
        getObjectFeatures = parentTransform.gameObject.GetComponent<GetObjectFeatures>();

        // Get the ID of the parent GameObject
        objectID = getObjectFeatures.objectID;
        
        // Call the method to apply the shader and the material to the depth camera
        ApplyShaderMaterial(); 

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to apply the shader and the material to the depth camera:

    private void ApplyShaderMaterial()
    {
        
        // Create a new material with the shader to calculate the depth of the image
        shader = Shader.Find("Custom/DepthCameraShader");
        material = new Material(shader);

        // Get the camera component of the GameObject
        depthCamera = GetComponent<Camera>();

        // Set the depth texture mode to Depth
        depthCamera.depthTextureMode = DepthTextureMode.Depth;

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to render the image with the depth shader:

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
        // Apply the shader to the image
        Graphics.Blit(source, destination, material, 0);

    }

    // -----------------------------------------------------------------------------------------------------
    // FixedUpdate is called at fixed time intervals:

    void FixedUpdate()
    {

        // If the variable captureImage is true, call the method to capture the depth image
        if (captureImage)
        {
            StartCoroutine(CaptureDepthImageCoroutine());
            captureImage = false; // Set the variable to false to avoid multiple captures
        }

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to capture the depth image:

    private IEnumerator CaptureDepthImageCoroutine()
    {

        // Find the target GameObject of the parent GameObject
        targetGameObject = GameObject.Find("ID"+objectID+"Target");

        // If the target GameObject exists, deactivate it
        if (targetGameObject != null) targetGameObject.SetActive(false);
        
        // Activate the depth camera
        depthCamera.enabled = true;

        // Get the RenderTexture to the depth camera
        renderTexture = depthCamera.targetTexture;

        // Ensure that the RenderTexture is active
        RenderTexture.active = renderTexture;

        // Render depth camera
        depthCamera.Render();

        // Create a new texture to read the pixels using 24 bit depth
        depthImage = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);

        // Read pixels from active RenderTexture
        depthImage.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);

        // Apply the changes to the texture
        depthImage.Apply();

        // Set the RenderTexture to null for a new image capture
        RenderTexture.active = null;

        // Deactivate the depth camera for a new image capture
        depthCamera.enabled = false;

        // If the target GameObject exists, activate it
        if (targetGameObject != null) targetGameObject.SetActive(true);

        // Convert the texture to a PNG image
        imageBytes = depthImage.EncodeToPNG();

        // Save the image on the disk in a separate Coroutine
        yield return StartCoroutine(SaveDepthImageCoroutine(imageBytes));

    }

    // -----------------------------------------------------------------------------------------------------
    // Method to save the depth image on the disk

    private IEnumerator SaveDepthImageCoroutine(byte[] imageBytes)
    {

        // Call the method to save the image
        folderPath = Path.Combine(Application.dataPath, "DepthCameraImages", objectName);

        // Create the folder if it does not exist
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            yield return null;
        }

        // Save the image in the folder with the current date and time
        filePath = Path.Combine(folderPath, "DepthImage_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png");
        File.WriteAllBytes(filePath, imageBytes);

        // Log the path of the saved image
        Debug.Log("Depth image saved in: " + filePath);

        // Wait for the image to be saved
        yield return null;

    }

}
