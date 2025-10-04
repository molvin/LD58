using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(Pawn))]
public class PawnEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Pawn pawn = (Pawn)target;

        var types = typeof(PawnPrototype).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(PawnPrototype))).ToArray();
        string[] options = types.Select(t => t.Name).ToArray();

        int selectedIndex = 0;
        if (pawn.Prototype != null)
        {
            selectedIndex = System.Array.IndexOf(options, pawn.Prototype);
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }
        }

        // Dropdown menu
        selectedIndex = EditorGUILayout.Popup("Select Prototype", selectedIndex, options);

        // Set the selected option back
        pawn.Prototype = options[selectedIndex];

        // Save changes
        if (GUI.changed)
        {
            EditorUtility.SetDirty(pawn);
        }
    }
}
