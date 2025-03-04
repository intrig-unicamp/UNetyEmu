using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to add a read component about the communication's features, according to the data in the setupObjects.json file:
public class GetCommunicationFeatures : MonoBehaviour
{
    // Communication Features:
    [Header("Communication Features")]
    [ReadOnly] public string scriptName;
}
