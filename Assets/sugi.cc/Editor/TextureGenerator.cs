using UnityEngine;
using UnityEditor;

namespace sugi.cc
{
    [CreateAssetMenu]
    public class TextureGenerator : CustomEditorWindow.WindowItem
    {
        public Material materialForTexGen;
        int texWidth = 512;
        int texHeight = 512;
        RenderTextureFormat format = RenderTextureFormat.ARGBHalf;
        string path { get { return AssetDatabase.GetAssetPath(this); } }

        public override void OnGUI()
        {
            materialForTexGen = EditorGUILayout.ObjectField(materialForTexGen, typeof(Material), allowSceneObjects: false) as Material;
            texWidth = EditorGUILayout.IntField("texture width:", texWidth);
            texHeight = EditorGUILayout.IntField("texture height:", texHeight);
            format = (RenderTextureFormat)EditorGUILayout.EnumPopup(format);

            if (materialForTexGen != null)
                if (GUILayout.Button("Create Texture"))
                    CreateTexture();

            GUILayout.Space(16);
            GUILayout.Label("Generated Textures");
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var tex = o as Texture;
                if (tex != null)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(tex, GUILayout.Width(64), GUILayout.Height(64));
                    GUILayout.Label(tex.name);
                    if (GUILayout.Button("Delete Texture", GUILayout.Width(128)))
                    {
                        DestroyImmediate(tex, true);
                        AssetDatabase.ImportAsset(path);
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }
        void CreateTexture()
        {
            var rt = Helper.CreateRenderTexture(texWidth, texHeight, null, format);
            Graphics.Blit(null, rt, materialForTexGen);
            var tex2d = RenderTextureToTexture2D.Convert(rt);
            tex2d.name = string.Format("{0}_{1}_{2}.asset", materialForTexGen.name, texWidth, texHeight);

            AssetDatabase.AddObjectToAsset(tex2d, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            AssetDatabase.Refresh();

            Selection.activeObject = tex2d;
            rt.Release();
            rt = null;
        }
    }
}