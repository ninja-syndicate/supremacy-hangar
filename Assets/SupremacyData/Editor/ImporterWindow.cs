using System;
using UnityEditor;
using UnityEngine;

namespace SupremacyData.Editor
{
    public class ImporterWindow : UnityEditor.EditorWindow
    {
        [MenuItem("Supremacy/Data/Importer")]
        public static void Spawn()
        {
            var window = EditorWindow.GetWindow<ImporterWindow>();
            window.Show();
        }

        public void OnGUI()
        {
            GUILayout.Label("I AM WINDOW");
        }
    }
}