using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace sugi.cc
{


    public class MaterialPropertySetting : MonoBehaviour
    {
        [Header("sync materialProperties server to client")]
        public bool sync;
        public Material[] targetMaterials;
        public string fileDirectryPath = "MaterialSettings/";
        string GetFilePath(string matName) { return fileDirectryPath + matName + ".json"; }

        [SerializeField]
        Setting[] settings;
#if UNITY_EDITOR
        public void SetPropertiesFromMaterial()
        {
            targetMaterials = targetMaterials.OrderBy(tm => tm.name).ToArray();
            InitializeSettings();
            for (var i = 0; i < settings.Length; i++)
            {
                var targetMat = targetMaterials[i];
                var setting = settings[i];
                setting.name = targetMat.name;
                var s = targetMat.shader;
                var pCount = ShaderUtil.GetPropertyCount(s);

                var fPropNames = Enumerable.Range(0, pCount).Where(idx => ShaderUtil.GetPropertyType(s, idx) == ShaderUtil.ShaderPropertyType.Float || ShaderUtil.GetPropertyType(s, idx) == ShaderUtil.ShaderPropertyType.Range).Select(idx => ShaderUtil.GetPropertyName(s, idx)).ToArray();
                var cPropNames = Enumerable.Range(0, pCount).Where(idx => ShaderUtil.GetPropertyType(s, idx) == ShaderUtil.ShaderPropertyType.Color).Select(idx => ShaderUtil.GetPropertyName(s, idx)).ToArray();
                var vPropNames = Enumerable.Range(0, pCount).Where(idx => ShaderUtil.GetPropertyType(s, idx) == ShaderUtil.ShaderPropertyType.Vector).Select(idx => ShaderUtil.GetPropertyName(s, idx)).ToArray();

                setting.floatProperties = fPropNames.Select(name => new StringFloatPair(name, targetMat.GetFloat(name))).ToArray();
                setting.colorProperties = cPropNames.Select(name => new StringColorPair(name, targetMat.GetColor(name))).ToArray();
                setting.vectorProperties = vPropNames.Select(name => new StringVectorPair(name, targetMat.GetVector(name))).ToArray();
                setting.texNames = Enumerable.Range(0, pCount).Where(idx => ShaderUtil.GetPropertyType(s, idx) == ShaderUtil.ShaderPropertyType.TexEnv).Select(idx => ShaderUtil.GetPropertyName(s, idx)).ToArray();

                setting.filePath = GetFilePath(targetMat.name);
                setting.Save();
            }
        }
        public void GetPropertiesFromJson()
        {
            InitializeSettings();
            for (var i = 0; i < settings.Length; i++)
            {
                var setting = settings[i];
                var targetMat = targetMaterials[i];
                setting.targetMat = targetMat;
                setting.LoadSettingFromFile(GetFilePath(targetMat.name));
            }
        }
#endif
        //property for use oscMessageEvent
        //use pass "/material/save"
        [Osc("/material/save")]
        public void SaveSettings(object[] msgs = null)
        {
            foreach (var setting in settings)
                setting.Save();
        }

        #region use pass "/material/float" s,s,f
        [Osc("/material/float")]
        public void SetFloatFromOsc(object[] msgs)
        {
            if (msgs.Length < 3) return;
            var matName = (string)msgs[0];
            var propName = (string)msgs[1];
            var value = (float)msgs[2];

            SetFloat(matName, propName, value);
        }
        public void SetFloat(string matName, string propName, float value)
        {
            var targetMat = targetMaterials.Where(mat => mat.name == matName).FirstOrDefault();
            if (targetMat == null) return;
            if (!targetMat.HasProperty(propName)) return;
            targetMat.SetFloat(propName, value);

            var setting = settings.Where(b => b.name == matName).FirstOrDefault();
            if (setting == null) return;
            var pairIdx = setting.floatProperties.Select((pair, idx) => new { pair, idx }).Where(b => b.pair.propName == propName).FirstOrDefault();
            if (pairIdx == null) return;
            setting.floatProperties[pairIdx.idx].value = value;
        }
        #endregion

        #region use pass "/material/color" s,s,f,f,f,f
        [Osc("/material/color")]
        public void SetColorFromOsc(object[] msgs)
        {
            if (msgs.Length < 6) return;
            var matName = (string)msgs[0];
            var propName = (string)msgs[1];
            var value = new Color((float)msgs[2], (float)msgs[3], (float)msgs[4], (float)msgs[5]);

            SetColor(matName, propName, value);
        }
        public void SetColor(string matName, string propName, Color value)
        {
            var targetMat = targetMaterials.Where(mat => mat.name == matName).FirstOrDefault();
            if (targetMat == null) return;
            if (!targetMat.HasProperty(propName)) return;
            targetMat.SetColor(propName, value);

            var setting = settings.Where(b => b.name == matName).FirstOrDefault();
            if (setting == null) return;
            var pairIdx = setting.colorProperties.Select((pair, idx) => new { pair, idx }).Where(b => b.pair.propName == propName).FirstOrDefault();
            if (pairIdx == null) return;
            setting.colorProperties[pairIdx.idx].value = value;
        }
        #endregion

        #region use pass "/material/vector" s,s,f,f,f,f
        [Osc("/material/vector")]
        public void SetVectorFromOsc(object[] msgs)
        {
            if (msgs.Length < 6) return;
            var matName = (string)msgs[0];
            var propName = (string)msgs[1];
            var value = new Vector4((float)msgs[2], (float)msgs[3], (float)msgs[4], (float)msgs[5]);

            SetVector(matName, propName, value);
        }
        public void SetVector(string matName, string propName, Vector4 value)
        {
            var targetMat = targetMaterials.Where(mat => mat.name == matName).FirstOrDefault();
            if (targetMat == null) return;
            if (!targetMat.HasProperty(propName)) return;
            targetMat.SetVector(propName, value);

            var setting = settings.Where(b => b.name == matName).FirstOrDefault();
            if (setting == null) return;
            var pairIdx = setting.vectorProperties.Select((pair, idx) => new { pair, idx }).Where(b => b.pair.propName == propName).FirstOrDefault();
            if (pairIdx == null) return;
            setting.vectorProperties[pairIdx.idx].value = value;
        }
        #endregion

        void InitializeSettings()
        {
            if (settings.Length != targetMaterials.Length)
            {
                settings = new Setting[targetMaterials.Length];
                for (var i = 0; i < settings.Length; i++)
                    settings[i] = new Setting();
            }
        }

        void Start()
        {
            if (OscController.Instance != null)
                OscController.Instance.AddCallbacks(this);
            InitializeSettings();
            for (var i = 0; i < settings.Length; i++)
            {
                var setting = settings[i];
                var targetMat = targetMaterials[i];
                setting.targetMat = targetMat;
                SettingManager.AddSettingMenu(setting, GetFilePath(targetMat.name));
                if (sync) setting.SetSyncable();
            }

        }

        [System.Serializable]
        public class Setting : SettingManager.Setting
        {
            public string name;
            public StringFloatPair[] floatProperties;
            public StringColorPair[] colorProperties;
            public StringVectorPair[] vectorProperties;
            public Material targetMat { private get; set; }

            List<string> floatFieldStrs = new List<string>();
            List<string[]> colorFieldStrs = new List<string[]>();
            List<string[]> vectorFieldStrs = new List<string[]>();
            string floatSizeStr;
            string colorSizeText;
            string vectorSizeText;

            public string[] texNames;
            int selectedIdx;

            public override void OnGUIFunc()
            {
                GUILayout.BeginVertical();
                DrawFloatFields();
                DrawColorFields();
                DrawVectorFields();
                ViewTexture();
                GUILayout.EndVertical();

                SetProps();
            }

            protected override void OnLoad()
            {
                SetProps();
                ResetFieldStrings();
            }

            void ResetFieldStrings()
            {
                floatSizeStr = floatProperties.Length.ToString();
                colorSizeText = colorProperties.Length.ToString();
                vectorSizeText = vectorProperties.Length.ToString();
                floatFieldStrs.Clear();
                colorFieldStrs.Clear();
                vectorFieldStrs.Clear();
                for (var i = 0; i < floatProperties.Length; i++)
                    floatFieldStrs.Add(floatProperties[i].value.ToString());
                for (var i = 0; i < colorProperties.Length; i++)
                    colorFieldStrs.Add(Enumerable.Range(0, 4).Select(cIdx => colorProperties[i].value[cIdx].ToString()).ToArray());
                for (var i = 0; i < vectorProperties.Length; i++)
                    vectorFieldStrs.Add(Enumerable.Range(0, 4).Select(vIdx => vectorProperties[i].value[vIdx].ToString()).ToArray());
            }

            void SetProps()
            {
                foreach (var fProp in floatProperties)
                    targetMat.SetFloat(fProp.propName, fProp.value);
                foreach (var cProp in colorProperties)
                    targetMat.SetColor(cProp.propName, cProp.value);
                foreach (var vProp in vectorProperties)
                    targetMat.SetVector(vProp.propName, vProp.value);
                name = targetMat.name;
            }
            void Indent(int size, System.Action guiFunc)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(size);
                GUILayout.BeginVertical();
                guiFunc();
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            void DrawFloatFields()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Size of Float Properties: ");
                floatSizeStr = GUILayout.TextField(floatSizeStr, GUILayout.Width(120));
                GUILayout.EndHorizontal();
                var size = floatProperties.Length;
                if (int.TryParse(floatSizeStr, out size))
                    if (floatProperties.Length != (size = Mathf.Max(0, size)))
                    {
                        floatProperties = Helper.ResizeArray(floatProperties, size);
                        ResetFieldStrings();
                    }
                Indent(32, delegate ()
                {
                    for (var i = 0; i < floatProperties.Length; i++)
                    {
                        var prop = floatProperties[i];
                        prop.propName = prop.propName == null ? "" : prop.propName;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("propName:");
                        floatProperties[i].propName = GUILayout.TextField(prop.propName, GUILayout.Width(240));
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("value:");
                        floatFieldStrs[i] = GUILayout.TextField(floatFieldStrs[i], GUILayout.Width(120));
                        float.TryParse(floatFieldStrs[i], out floatProperties[i].value);
                        GUILayout.EndHorizontal();
                    }
                });
            }
            void DrawColorFields()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Size of Color Properties: ");
                colorSizeText = GUILayout.TextField(colorSizeText, GUILayout.Width(120));
                GUILayout.EndHorizontal();
                var size = colorProperties.Length;
                if (int.TryParse(colorSizeText, out size))
                    if (colorProperties.Length != (size = Mathf.Max(0, size)))
                    {
                        colorProperties = Helper.ResizeArray(colorProperties, size);
                        ResetFieldStrings();
                    }
                Indent(32, delegate ()
                {
                    for (var i = 0; i < colorProperties.Length; i++)
                    {
                        var prop = colorProperties[i];
                        prop.propName = prop.propName == null ? "" : prop.propName;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("propName:");
                        colorProperties[i].propName = GUILayout.TextField(prop.propName, GUILayout.Width(240));
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Color:");
                        GUI.contentColor = Color.white;
                        colorFieldStrs[i][0] = GUILayout.TextField(colorFieldStrs[i][0], GUILayout.Width(48));
                        float.TryParse(colorFieldStrs[i][0], out colorProperties[i].value.r);
                        colorFieldStrs[i][1] = GUILayout.TextField(colorFieldStrs[i][1], GUILayout.Width(48));
                        float.TryParse(colorFieldStrs[i][1], out colorProperties[i].value.g);
                        colorFieldStrs[i][2] = GUILayout.TextField(colorFieldStrs[i][2], GUILayout.Width(48));
                        float.TryParse(colorFieldStrs[i][2], out colorProperties[i].value.b);
                        colorFieldStrs[i][3] = GUILayout.TextField(colorFieldStrs[i][3], GUILayout.Width(48));
                        float.TryParse(colorFieldStrs[i][3], out colorProperties[i].value.a);
                        var color = colorProperties[i].value;
                        color.a = 1f;
                        GUI.contentColor = color;
                        GUILayout.Label("●▲■");
                        GUI.contentColor = Color.white;
                        GUILayout.EndHorizontal();
                    }
                });
            }
            void DrawVectorFields()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Size of Vector Properties: ");
                vectorSizeText = GUILayout.TextField(vectorSizeText, GUILayout.Width(120));
                GUILayout.EndHorizontal();
                var size = vectorProperties.Length;
                if (int.TryParse(vectorSizeText, out size))
                    if (vectorProperties.Length != (size = Mathf.Max(0, size)))
                    {
                        vectorProperties = Helper.ResizeArray(vectorProperties, size);
                        ResetFieldStrings();
                    }
                Indent(32, delegate ()
                {
                    for (var i = 0; i < vectorProperties.Length; i++)
                    {
                        var prop = vectorProperties[i];
                        prop.propName = prop.propName == null ? "" : prop.propName;
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("propName:");
                        vectorProperties[i].propName = GUILayout.TextField(prop.propName, GUILayout.Width(240));
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Vector:");
                        vectorFieldStrs[i][0] = GUILayout.TextField(vectorFieldStrs[i][0], GUILayout.Width(48));
                        float.TryParse(vectorFieldStrs[i][0], out vectorProperties[i].value.x);
                        vectorFieldStrs[i][1] = GUILayout.TextField(vectorFieldStrs[i][1], GUILayout.Width(48));
                        float.TryParse(vectorFieldStrs[i][1], out vectorProperties[i].value.y);
                        vectorFieldStrs[i][2] = GUILayout.TextField(vectorFieldStrs[i][2], GUILayout.Width(48));
                        float.TryParse(vectorFieldStrs[i][2], out vectorProperties[i].value.z);
                        vectorFieldStrs[i][3] = GUILayout.TextField(vectorFieldStrs[i][3], GUILayout.Width(48));
                        float.TryParse(vectorFieldStrs[i][3], out vectorProperties[i].value.w);
                        GUILayout.EndHorizontal();
                    }
                });
            }
            void ViewTexture()
            {
                if (texNames.Length < 1)
                    return;
                GUILayout.Space(16);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Texture Property Name:");
                selectedIdx = GUILayout.SelectionGrid(selectedIdx, texNames, Mathf.Min(5, texNames.Length));
                var texName = texNames[selectedIdx];
                GUILayout.EndHorizontal();
                var tex = targetMat.GetTexture(texName);
                if (tex != null)
                {
                    GUILayout.Label("Texture Name: " + tex.name);
                    GUILayout.Label(tex, GUILayout.Width(Mathf.Min(tex.width, 640)), GUILayout.Height(Mathf.Min(tex.height, 480)));
                }
                else
                    GUILayout.Label("Texture Not Assigned");
            }
        }
    }
}