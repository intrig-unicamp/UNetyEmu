using UnityEngine; // Unity Engine library to use in PropertyAttribute classes

// Class to control the Info Panel in the UI by clicking the Info Button
public class InfoPanelController : MonoBehaviour
{

    // -----------------------------------------------------------------------------------------------------
    // Public variables that appear in the Inspector:

    public GameObject infoPanel; // Reference to the Info Panel

    // -----------------------------------------------------------------------------------------------------
    // Method to toggle the Info Panel by clicking the Info Button:

    public void TogglePanel()
    {
        infoPanel.SetActive(!infoPanel.activeSelf); // Toggle the Info Panel
    }

}
