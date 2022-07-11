using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SupremacyHangar.Editor
{
    [CustomEditor(typeof(Runtime.Silo.ComposableSockets))]
    public class ComposableSocketEditor : UnityEditor.Editor
    {
        private SerializedProperty weaponSockets;
        private SerializedProperty utilitySockets;
        private SerializedProperty powerCoreSocket;

        private Runtime.Silo.ComposableSockets selfReference;
        public void OnEnable()
        {
            weaponSockets = serializedObject.FindProperty("WeaponSockets");
            utilitySockets = serializedObject.FindProperty("UtilitySockets");
            powerCoreSocket = serializedObject.FindProperty("PowerCoreSocket");

            selfReference = target as Runtime.Silo.ComposableSockets;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            ImportSocketButton();

            EditorGUILayout.PropertyField(weaponSockets);
            EditorGUILayout.PropertyField(utilitySockets);
            EditorGUILayout.PropertyField(powerCoreSocket);

            serializedObject.ApplyModifiedProperties();
        }

        private void ImportSocketButton()
        {
            if (GUILayout.Button("Import Sockets"))
            {
                selfReference.WeaponSockets.Clear();
                selfReference.UtilitySockets.Clear();

                ImportSockets(selfReference.transform);

                selfReference.WeaponSockets = selfReference.WeaponSockets.OrderBy(go => go.name).ToList();
                selfReference.UtilitySockets = selfReference.UtilitySockets.OrderBy(go => go.name).ToList();
            }
        }

        private void ImportSockets(Transform myTransform)
        {
            for (int i = 0; i < myTransform.childCount; i++)
            {
                Transform child = myTransform.GetChild(i);
                if (child.childCount > 0)
                    ImportSockets(myTransform.GetChild(i));

                if (child.name.Contains("weapon", StringComparison.CurrentCultureIgnoreCase))
                    selfReference.WeaponSockets.Add(child);
                else if (child.name.Contains("utility", StringComparison.CurrentCultureIgnoreCase))
                    selfReference.UtilitySockets.Add(child);
                else if (child.name.Contains("powercore", StringComparison.CurrentCultureIgnoreCase))
                    selfReference.PowerCoreSocket = child;
            }
        }
    }
}
