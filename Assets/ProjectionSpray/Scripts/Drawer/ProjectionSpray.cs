using UnityEngine;


public class ProjectionSpray : DrawerBase
{
    new Camera camera
    {
        get
        {
            if (_cam == null)
            {
                _cam = GetComponent<Camera>();
                if (_cam == null)
                    _cam = gameObject.AddComponent<Camera>();
                _cam.clearFlags = CameraClearFlags.Nothing;
                _cam.nearClipPlane = 0.01f;
                depthOutput = new RenderTexture(dpthResolution, dpthResolution, 0, RenderTextureFormat.RFloat);
                depthOutput.Create();
                _cam.targetTexture = depthOutput;
                _cam.SetReplacementShader(depthRendererShader, "RenderType");
            }
            return _cam;
        }
    }
    Camera _cam;
    RenderTexture depthOutput;
    Matrix4x4 ProjectionOffsetMatrix = Matrix4x4.TRS(Vector3.one * .5f, Quaternion.identity, Vector3.one * .5f);
    Matrix4x4 projectionMatrix;

    public Shader depthRendererShader;
    public int dpthResolution = 1024;
    public float sprayAngle = 30f;
    public float splayDistance = 1f;

    protected override void UpdateDrawTarget()
    {
        drawTargetList.Clear();
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        drawTargetList.AddRange(
            DrawableBase.AllDrawableList.FindAll(d => GeometryUtility.TestPlanesAABB(planes, d.bounds))
            );
    }

    public override void DrawRts(RenderTexture[] rts, int pass)
    {
        RenderTexture.active = depthOutput;
        GL.Clear(true, true, Color.red * camera.farClipPlane);
        camera.fieldOfView = sprayAngle;
        camera.farClipPlane = splayDistance;
        camera.Render();
        projectionMatrix = ProjectionOffsetMatrix * camera.projectionMatrix * camera.worldToCameraMatrix;

        drawMat.SetFloat("_Dst", splayDistance);
        drawMat.SetTexture("_Depth", depthOutput);
        drawMat.SetMatrix("_ProjectionMatrix", projectionMatrix);
        drawMat.SetMatrix("_MatrixW2D", transform.worldToLocalMatrix);

        base.DrawRts(rts, pass);
    }

    private void OnDestroy()
    {
        if (depthOutput != null)
            depthOutput.Release();
        depthOutput = null;
    }
}
