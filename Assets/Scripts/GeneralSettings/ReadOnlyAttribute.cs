using UnityEngine; // Unity Engine library to use in PropertyAttribute classes

// Select Unity Editor:
#if UNITY_EDITOR
using UnityEditor;
#endif

// Defines the ReadOnly attribute:
public class ReadOnlyAttribute : PropertyAttribute { }

// Creates a CustomPropertyDrawer for ReadOnly, which is what makes it “opaque” and not editable in the Inspector:
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;  // Disable editing for a moment
        EditorGUI.PropertyField(position, property, label); // Draw the property for ReadOnly
        GUI.enabled = true;   // Enable editing for other variables
    }
}
#endif
