using System.Collections.Generic;
using UnityEngine;

public abstract class DrawerBase : MonoBehaviour
{
    const string PropPosition = "_Pos";
    const string PropRotation = "_Rot";
    const string PropColor = "_Color";
    const string PropEmission = "_Emission";
    const string PropPosTex = "_OPosTex";
    const string PropNormTex = "_ONormTex";
    const string PropObjectMatrix = "_MatrixO2W";

    public Material drawMat;
    public Vector3 pos;
    public Vector4 rot;
    public Color color;
    public float emission = 0f;
    public bool drawing;
    

    public List<DrawableBase> drawTargetList
    {
        get
        {
            if (_target == null)
                _target = new List<DrawableBase>();
            return _target;
        }
    }
    List<DrawableBase> _target;

    [Header("shader pass setting")]
    public int drawPass = 0;
    public int guidPass = 1;

    protected virtual void UpdateDrawTarget()
    {
        drawTargetList.Clear();
        drawTargetList.AddRange(DrawableBase.AllDrawableList);
    }

    public void Draw()
    {
        UpdateDrawTarget();
        if (!drawing) return;
        foreach (var d in drawTargetList)
            DrawTo(d);
    }
    public void DrawGuid()
    {
        if (drawing) return;
        foreach (var d in drawTargetList)
            DrawGuidTo(d);
    }

    public void DrawTo(DrawableBase drawable) { drawable.Draw(this); }

    public void DrawGuidTo(DrawableBase drawable) { drawable.DrawGuid(this); }

    public virtual void DrawRts(RenderTexture[] rts, int pass)
    {
        drawMat.SetColor(PropColor, color);
        drawMat.SetFloat(PropEmission, emission);

        if (pass < drawMat.passCount)
        {
            Graphics.Blit(rts[0], rts[1], drawMat, pass);
            Swap(rts);
        }
    }

    void Swap(RenderTexture[] rts)
    {
        var tmp = rts[0];
        rts[0] = rts[1];
        rts[1] = tmp;
    }

    public void SetDrawerTransform()
    {
        rot.x = transform.rotation.x;
        rot.y = transform.rotation.y;
        rot.z = transform.rotation.z;
        rot.w = transform.rotation.w;
        pos = transform.position;

        drawMat.SetVector(PropPosition, pos);
        drawMat.SetVector(PropRotation, rot);
    }

    public void SetCanvasObjectInfo(Texture posTex, Texture normTex, Matrix4x4 objMatrix)
    {
        drawMat.SetTexture(PropPosTex, posTex);
        drawMat.SetTexture(PropNormTex, normTex);
        drawMat.SetMatrix(PropObjectMatrix, objMatrix);
    }
}