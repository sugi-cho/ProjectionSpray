using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using sugi.cc;

public class Bomb : DrawerBase
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
                _cam.clearFlags = CameraClearFlags.SolidColor;
                _cam.backgroundColor = Color.red * _cam.farClipPlane;
                _cam.nearClipPlane = 0.01f;
                _cam.SetReplacementShader(depthRendererShader, "RenderType");
                _cam.enabled = false;

                depthOutput = new RenderTexture(depthResolution, depthResolution, 0, RenderTextureFormat.RFloat);
                depthOutput.dimension = UnityEngine.Rendering.TextureDimension.Cube;
                depthOutput.Create();
                var r = GetComponent<Renderer>();
                if (r != null)
                    r.SetTexture("_Cube", depthOutput);
            }
            return _cam;
        }
    }
    Camera _cam;
    [SerializeField] RenderTexture depthOutput;

    public Shader depthRendererShader;
    public int depthResolution = 1024;
    public float bombDistance = 10f;

    protected override void UpdateDrawTarget()
    {
        drawTargetList.Clear();
        drawTargetList.AddRange(
            Physics.OverlapSphere(transform.position, bombDistance)
            .Select(c => c.GetComponent<DrawableBase>())
            .Where(d => d != null && d.initialized)
            );
    }

    protected override void UpdateDrawerProp()
    {
        base.UpdateDrawerProp();

        camera.RenderToCubemap(depthOutput);
        drawMat.SetFloat("_Dst", bombDistance);
        drawMat.SetTexture("_Depth", depthOutput);
    }

    private void OnDestroy()
    {
        if (depthOutput != null)
            depthOutput.Release();
        depthOutput = null;
    }
}
