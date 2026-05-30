using UnityEngine;
using UnityEditor;

public class UnlockGameObject : MonoBehaviour
{
    // This adds a clickable command to the top menu bar in Unity
    [MenuItem("Tools/Unlock Selected Object")]
    public static void Unlock()
    {
        // Check if you have actually selected an object in the Hierarchy
        if (Selection.activeGameObject != null)
        {
            // Reset the flags back to completely normal
            Selection.activeGameObject.hideFlags = HideFlags.None;
            
            // Tell Unity to refresh the UI and mark the scene as changed
            EditorUtility.SetDirty(Selection.activeGameObject);
            
            Debug.Log($"[Unlocked] {Selection.activeGameObject.name} is now fully editable!");
        }
        else
        {
            Debug.LogWarning("Please select the locked GameObject in the Hierarchy first.");
        }
    }
}
