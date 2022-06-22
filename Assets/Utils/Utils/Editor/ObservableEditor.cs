#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
namespace _Scripts.Utils.Editor
{
    [CustomPropertyDrawer(typeof(Observable<float>))]
    public class ObservableFloatEditor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var firstHalf = position;
            firstHalf.width *= 0.35f;
            EditorGUI.LabelField(firstHalf, label.text);
            firstHalf.x += firstHalf.width;
            firstHalf.width = 0.65f * position.width;
            GUI.backgroundColor = ColorUtils.FromHex("#B0BEC5");
            EditorGUI.PropertyField(firstHalf, property.FindPropertyRelative("_value"), GUIContent.none);
        }
    }

    [CustomPropertyDrawer(typeof(Observable<int>))]
    public class ObservableIntEditor : ObservableFloatEditor
    {
    }

    [CustomPropertyDrawer(typeof(Observable<uint>))]
    public class ObservableUIntEditor : ObservableFloatEditor
    {
    }

    [CustomPropertyDrawer(typeof(Observable<bool>))]
    public class ObservableBoolEditor : ObservableFloatEditor
    {
    }
}
#endif