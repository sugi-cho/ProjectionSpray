using System.Linq;
using UnityEngine;

using sugi.cc;


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
                _cam.targetTexture = depthOutput = Helper.CreateRenderTexture(depthResolution, depthResolution, depthOutput, RenderTextureFormat.RFloat);
                _cam.SetReplacementShader(depthRendererShader, "RenderType");
                _cam.enabled = false;
            }
            return _cam;
        }
    }
    Camera _cam;
    [SerializeField] RenderTexture depthOutput;
    Matrix4x4 ProjectionOffsetMatrix = Matrix4x4.TRS(Vector3.one * .5f, Quaternion.identity, Vector3.one * .5f);
    Matrix4x4 projectionMatrix;

    public Shader depthRendererShader;
    public int depthResolution = 1024;
    public float sprayAngle = 30f;
    public float splayDistance = 1f;

    protected override void UpdateDrawTarget()
    {
        drawTargetList.Clear();
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        drawTargetList.AddRange(
            DrawableBase.ReadyToDraws.Where(d => GeometryUtility.TestPlanesAABB(planes, d.bounds))
            );
    }

    protected override void UpdateDrawerProp()
    {
        base.UpdateDrawerProp();

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
    }

    private void OnDestroy()
    {
        if (depthOutput != null)
            depthOutput.Release();
        depthOutput = null;
    }
}
