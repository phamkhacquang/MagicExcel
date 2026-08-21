using UnityEditor;
using UnityEngine;

namespace MagicExcel.Editor {
    [CustomPropertyDrawer(typeof(XorInt))]
    [CustomPropertyDrawer(typeof(XorFloat))]
    [CustomPropertyDrawer(typeof(XorDouble))]
    [CustomPropertyDrawer(typeof(XorLong))]
    public class XorDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.LabelField(position, label.text, property.boxedValue.ToString());
        }
    }
}