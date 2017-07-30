using UnityEngine;

namespace sugi.cc
{
    public class Projection : MonoBehaviour
    {
        public int camDepth = 100;
        public Color bgColor;
        public Rect projectionArea;
        public Rect trimArea;
        public QuadWarp quadWarpProps;
        public Vector4 overlapProps; //overlap left,right,bottom,up
        public int numDivisions = 10;
        public float gamma = 1.0f / 2.2f;

        public Texture texture;

        static Material projectionMat { get { if (_mat == null) _mat = new Material(Shader.Find("Hidden/Projection")); return _mat; } }
        static Material _mat;

        void Start()
        {
            Setup();
        }

        Camera cam
        {
            get
            {
                if (_cam != null) return _cam;
                _cam = GetComponent<Camera>();
                if (_cam == null) _cam = gameObject.AddComponent<Camera>();
                return _cam;
            }
        }
        Camera _cam;

        public void Setup()
        {
            cam.rect = projectionArea;
            cam.clearFlags = CameraClearFlags.Color;
            cam.backgroundColor = bgColor;
            cam.cullingMask = 0;
            cam.depth = camDepth;
        }

        void OnPostRender()
        {
            projectionMat.mainTexture = texture;
            projectionMat.SetVector("_Prop1", quadWarpProps.prop1);
            projectionMat.SetVector("_Prop2", quadWarpProps.prop2);
            projectionMat.SetVector("_Prop3", overlapProps);
            projectionMat.SetFloat("_Gamma", gamma);
            projectionMat.DrawFullScreenQuadNxN(numDivisions, trimArea.xMin, trimArea.yMin, trimArea.xMax, trimArea.yMax);
        }


        [System.Serializable]
        public struct QuadWarp
        {
            public Vector2 bottomLeft;
            public Vector2 bottomRight;
            public Vector2 upperLeft;
            public Vector2 upperRight;

            public Vector4 prop1 { get { return new Vector4(bottomLeft.x, bottomLeft.y, bottomRight.x, bottomRight.y); } }
            public Vector4 prop2 { get { return new Vector4(upperLeft.x, upperLeft.y, upperRight.x, upperRight.y); } }
        }
    }
}