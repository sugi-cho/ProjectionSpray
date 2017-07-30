using UnityEngine;
using UnityEditor;

using sugi.cc;

[CreateAssetMenu]
public class BluredTextureCreator : CustomEditorWindow.WindowItem
{
    public Texture source;
    public int texWidth;
    public int texHeight;

    [Header("GaussianBlur Props")]
    public int iterations = 1;
    public int downsample = 1;

    RenderTexture bTex;

    public override void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.Label(source,GUILayout.MaxHeight(256));
        if (GUILayout.Button("CreateBlurTex"))
            CreateBlurTex();
        if (bTex != null)
        {
            GUILayout.Label(bTex);
            if (GUILayout.Button("Save Tex"))
                SaveBlured();
        }
        GUILayout.EndVertical();
    }

    void CreateBlurTex()
    {
        bTex = Helper.CreateRenderTexture(texWidth, texHeight, bTex, RenderTextureFormat.ARGB32);
        Graphics.Blit(source, bTex);
        bTex.ApplyGaussianFilter(iterations, downsample);
    }

    void SaveBlured()
    {
        var tex = RenderTextureToTexture2D.Convert(bTex);
        var path = AssetDatabase.GetAssetPath(source);
        path = System.IO.Path.GetDirectoryName(path);
        path += string.Format("/{0}_blur.asset", source.name);
        AssetDatabase.CreateAsset(tex, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = tex;
    }
}
