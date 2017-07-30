using UnityEngine;

namespace sugi.cc
{
    public class MeshInfoTexture : MonoBehaviour
    {
        public static RenderTexture[] GeneratePositionNormalTexture(Mesh mesh, int width = 512, int height=512)
        {
            var texes = new RenderTexture[2];
            for (var i = 0; i < 2; i++)
            {
                var tex = texes[i] = Helper.CreateRenderTexture(width, height, texes[0], RenderTextureFormat.ARGBFloat);
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

        public Setting setting;
        public Mesh targetMesh;

        public TextureEvent onCreatePositionTex;
        public TextureEvent onCreateNormalTex;

        RenderTexture[] generatedTexes;

        // Use this for initialization
        void Start()
        {
            SettingManager.AddSettingMenu(setting, SettingFolderPath + targetMesh.name + "_infoTex.json");
            SettingManager.AddExtraGuiFunc(ViewTexture);
            generatedTexes = GeneratePositionNormalTexture(targetMesh, setting.texWidth, setting.texHeight);
            onCreatePositionTex.Invoke(generatedTexes[0]);
            onCreateNormalTex.Invoke(generatedTexes[1]);
        }

        #region GUI view texture
        bool showInfo;
        string[] labels = new[] { "position texture", "normal texture" };
        int selectingIdx = 0;
        void ViewTexture()
        {
            GUILayout.BeginVertical("box");
            if (showInfo = GUILayout.Toggle(showInfo, targetMesh.name + ".meshInfoTexture"))
            {
                selectingIdx = GUILayout.SelectionGrid(selectingIdx, labels, 2);
                var tex = generatedTexes[selectingIdx];
                if (tex != null)
                    GUILayout.Label(tex, GUILayout.MaxWidth(512), GUILayout.MaxHeight(512));
            }
            GUILayout.EndVertical();
        }
        #endregion

        [System.Serializable]
        public class Setting : SettingManager.Setting
        {
            public int texWidth = 512;
            public int texHeight = 512;
        }
    }
}