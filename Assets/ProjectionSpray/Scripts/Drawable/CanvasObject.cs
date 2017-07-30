using UnityEngine;

using sugi.cc;

public class CanvasObject : DrawableBase
{
    public Mesh mesh;

    [SerializeField] RenderTexture positionInfoTex;
    [SerializeField] RenderTexture normalInfoTex;

    public Texture posTex { get { return positionInfoTex; } }
    public Texture normTex { get { return normalInfoTex; } }
    
    protected override void Init()
    {
        base.Init();

        if (mesh == null)
            mesh = GetComponent<MeshFilter>().sharedMesh;
        var texes = mesh.GetPositionNormalTexture(setting.texWidth, setting.texHeight);
        positionInfoTex = texes[0];
        normalInfoTex = texes[1];
    }
    
    public override void Draw(DrawerBase d)
    {
        d.SetCanvasObjectInfo(positionInfoTex, normalInfoTex, transform.localToWorldMatrix);
        d.SetDrawerTransform();

        base.Draw(d);
    }

    public override void DrawGuid(DrawerBase d)
    {
        d.SetCanvasObjectInfo(positionInfoTex, normalInfoTex, transform.localToWorldMatrix);
        d.SetDrawerTransform();

        base.DrawGuid(d);
    }

    protected override void UpdateTexture(RenderTexture[] pingPongCanvasRts, RenderTexture[] pingPongGuidRts)
    {
        updater.SetTexture("_OPosTex", positionInfoTex);
        Graphics.Blit(pingPongCanvasRts[0], pingPongCanvasRts[1], updater);
        Graphics.Blit(pingPongGuidRts[0], pingPongGuidRts[1], updater);
        pingPongCanvasRts.Swap();
        pingPongGuidRts.Swap();
    }
}
