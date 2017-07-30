using UnityEngine;
using UnityEditor;
using System.Collections;

namespace sugi.cc
{
    [CanEditMultipleObjects, CustomEditor(typeof(MaterialPropertySetting))]
    public class MaterialPropertySettingEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            var setting = (MaterialPropertySetting)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Set Properties From TargetMat"))
                setting.SetPropertiesFromMaterial();
            if (GUILayout.Button("Get Properties From Json"))
                setting.GetPropertiesFromJson();
        }
    }
}