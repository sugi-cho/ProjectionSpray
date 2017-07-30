using System.Collections.Generic;
using UnityEngine;

public abstract class DrawableBase : MonoBehaviour {
    new protected Renderer renderer;
    public static List<DrawableBase> AllDrawableList { get; private set; }
    const string PropCanvasTex = "_Canvas";
    const string PropGuidTex = "_Guid";

    public Setting setting;
    public Color clearColor = Color.gray;
    public Texture originTex;

    public Material updater;
    public Bounds bounds { get { return renderer.bounds; } }

    RenderTexture canvas;
    RenderTexture guid;

    RenderTexture[] pingPongCanvasRts;
    RenderTexture[] pingPongGuidRts;

    public Texture drawingTex { get { return canvas; } }

    private void OnEnable()
    {
        if (AllDrawableList == null)
            AllDrawableList = new List<DrawableBase>();
        AllDrawableList.Add(this);
    }
    private void OnDisable()
    {
        AllDrawableList.Remove(this);
    }

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        Init();
    }

    RenderTexture CreateRT()
    {
        var rt = new RenderTexture(setting.texWidth, setting.texHeight, 0, RenderTextureFormat.ARGBHalf);
        rt.Create();
        return rt;
    }
    protected virtual void Init ()
    {
        canvas = CreateRT();
        canvas.wrapMode = TextureWrapMode.Clamp;
        guid = Instantiate(canvas);
        guid.Create();

        pingPongCanvasRts = new[] { CreateRT(), CreateRT() };
        pingPongGuidRts = new[] { CreateRT(), CreateRT() };

        Clear();

        var mpb = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(mpb);
        mpb.SetTexture(PropCanvasTex, canvas);
        mpb.SetTexture(PropGuidTex, guid);
        renderer.SetPropertyBlock(mpb);
    }
    public void Clear()
    {
        if (originTex != null)
            Graphics.Blit(originTex, canvas);
        else
        {
            RenderTexture.active = canvas;
            GL.Clear(true, true, clearColor);
        }
        Graphics.CopyTexture(canvas, pingPongCanvasRts[0]);
    }
    public void ClearGuid()
    {
        RenderTexture.active = guid;
        GL.Clear(true, true, Color.clear);
        Graphics.CopyTexture(guid, pingPongGuidRts[0]);
    }

    public virtual void Draw(DrawerBase d)
    {
        d.DrawRts(pingPongCanvasRts, d.drawPass);
    }
    public virtual void DrawGuid(DrawerBase d)
    {
        d.DrawRts(pingPongGuidRts, d.guidPass);
    }

    public void Apply()
    {
        if (updater != null)
            UpdateTexture(pingPongCanvasRts, pingPongGuidRts);

        Graphics.CopyTexture(pingPongCanvasRts[0], canvas);
        Graphics.CopyTexture(pingPongGuidRts[0], guid);
    }

    protected virtual void UpdateTexture(RenderTexture[] pingPongCanvasRts, RenderTexture[] pingPongGuidRts) { }

    [System.Serializable]
    public class Setting 
    {
        public int texWidth = 1024;
        public int texHeight = 1024;
    }
}
