using UnityEngine;

public class MeshInfoTexture : MonoBehaviour
{
    public static RenderTexture[] GeneratePositionNormalTexture(Mesh mesh, int width = 512, int height = 512)
    {
        var texes = new RenderTexture[2];
        for (var i = 0; i < 2; i++)
        {
            var tex = texes[i] = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            RenderTexture.active = tex;
            GL.Clear(true, true, Color.clear);
        }
        var buffers = new[] { texes[0].colorBuffer, texes[1].colorBuffer };

        infoGen.SetPass(0);
        Graphics.SetRenderTarget(buffers, texes[0].depthBuffer);
        Graphics.DrawMeshNow(mesh, Matrix4x4.identity);

        return texes;
    }
    static Material infoGen { get { if (_infoGen == null) _infoGen = new Material(Shader.Find("Generator/mesh Info texture")); return _infoGen; } }
    static Material _infoGen;

    const string SettingFolderPath = "MeshInfoGeneratorSettings/";

    public Mesh targetMesh;

    RenderTexture[] generatedTexes;
}