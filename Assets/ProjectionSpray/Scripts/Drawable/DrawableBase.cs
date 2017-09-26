using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using sugi.cc;

public abstract class DrawableBase : RendererBehaviour
{

    static List<DrawableBase> AllDrawableList;
    public static IEnumerable<DrawableBase> ReadyToDraws { get { return AllDrawableList.Where(d => d.initialized); } }

    const string PropCanvasTex = "_Canvas";
    const string PropGuidTex = "_Guid";

    public Setting setting;
    public Color clearColor = Color.black;
    public Texture originTex;

    public Material updater;
    public Bounds bounds { get { return renderer.bounds; } }

    RenderTexture canvas;
    RenderTexture guid;

    RenderTexture[] pingPongCanvasRts;
    RenderTexture[] pingPongGuidRts;

    public Texture drawingTex { get { return canvas; } }
    public bool initialized { get; private set; }

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
        if (setting == null)
            setting = new Setting();
        SettingManager.AddSettingMenu(setting, string.Format("DrawableObjectSettings/{0}.json", name));
        Init();
    }

    protected virtual void Init()
    {
        canvas = Helper.CreateRenderTexture(setting.texWidth, setting.texHeight);
        canvas.wrapMode = TextureWrapMode.Clamp;
        guid = Instantiate(canvas);
        guid.Create();

        pingPongCanvasRts = Helper.CreateRts(canvas, pingPongCanvasRts);
        pingPongGuidRts = Helper.CreateRts(guid, pingPongGuidRts);

        Clear();

        renderer.SetTexture(PropCanvasTex, canvas);
        renderer.SetTexture(PropGuidTex, guid);
        initialized = true;
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
    public class Setting : SettingManager.Setting
    {
        public int texWidth = 512;
        public int texHeight = 512;
    }
}
