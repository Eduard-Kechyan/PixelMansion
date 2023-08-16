using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Merge
{
    [CustomPropertyDrawer(typeof(SortingLayerAttribute))]
    public class SortingLayerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            int sortingLayerCount = SortingLayer.layers.Length;
            string[] sortingLayerNames = new string[sortingLayerCount];

            // Save the sorting layer names to an array
            for (int i = 0; i < sortingLayerCount; i++)
            {
                sortingLayerNames[i] = SortingLayer.layers[i].name;
            }

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.HelpBox(position, property.name + "{0} in not a string but has the SortingLayer attribute!", MessageType.Error);

                property.stringValue = "Default";
            }
            else if (sortingLayerNames.Length == 0)
            {
                EditorGUI.HelpBox(position, property.name + "There are no Sorting Layers!", MessageType.Error);

                property.stringValue = "Default";
            }
            else if (sortingLayerNames != null)
            {
                EditorGUI.BeginProperty(position, label, property);

                string oldName = property.stringValue;

                int oldLayerIndex = -1;

                for (int i = 0; i < sortingLayerCount; i++)
                {
                    if (sortingLayerNames[i].Equals(oldName))
                    {
                        oldLayerIndex = i;
                    }
                }

                int newLayerIndex = EditorGUI.Popup(position, label.text, oldLayerIndex, sortingLayerNames);

                if (newLayerIndex != oldLayerIndex)
                {
                    property.stringValue = sortingLayerNames[newLayerIndex];
                }

                EditorGUI.EndProperty();
            }
        }
    }
}