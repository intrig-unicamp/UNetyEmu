using UnityEngine; // Unity Engine library to use in MonoBehaviour classes

// Class to add a read component about the object's features, according to the data in the setupObjects.json file:
public class GetObjectFeatures : MonoBehaviour
{
    // Object General Features:
    [Header("Object General Features")]
    [ReadOnly] public string group;
    [ReadOnly] public string objectID;
    [ReadOnly] public string prefabName;
    
}
