using UnityEngine;
using UnityEditor;

namespace sugi.cc
{
    public class CustomEditorWindow : EditorWindow
    {
        [MenuItem("Window/sugi.cc.window")]
        public static void ShowWindow()
        {
            var window = GetWindow<CustomEditorWindow>();
            window.Show();
        }

        SerializedObject serializedObject { get { if (_so == null) _so = new SerializedObject(this); return _so; } }
        SerializedObject _so;
        public WindowItem item;

        private void OnGUI()
        {
            serializedObject.Update();

            var prop = serializedObject.FindProperty("item");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(prop, true);

            var selection = Selection.activeObject as WindowItem;
            if (selection != null)
                item = selection;

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (item != null)
                item.OnGUI();
        }

        public abstract class WindowItem : ScriptableObject
        {
            protected SerializedObject serializedObject { get { if (_so == null) _so = new SerializedObject(this); return _so; } }
            SerializedObject _so;
            public abstract void OnGUI();
        }
    }
}