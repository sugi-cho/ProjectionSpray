using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace sugi.cc
{
    public static class Helper
    {
        public static void LoadJsonFile<T>(T overwriteTarget, string filePath = "appData.json")
        {
            var path = Path.Combine(Application.persistentDataPath, filePath);
            if (File.Exists(path))
                JsonUtility.FromJsonOverwrite(File.ReadAllText(path), overwriteTarget);
            else
                SaveJsonFile(overwriteTarget, filePath);
        }

        public static void SaveJsonFile<T>(T obj, string filePath = "appData.json")
        {
            var json = JsonUtility.ToJson(obj);
            var path = Path.Combine(Application.persistentDataPath, filePath);
            var dPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dPath))
                Directory.CreateDirectory(dPath);

            using (var writer = new StreamWriter(path))
                writer.Write(json);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        /// <summary>
        /// this function create rendertexture.
        /// </summary>
        /// <param name="s">source RenderTexture</param>
        /// <param name="rt">if output RenderTexture is not null, output will be Released.</param>
        /// <param name="downSample">down sample</param>
        /// <returns></returns>
        public static RenderTexture CreateRenderTexture(RenderTexture s, RenderTexture rt = null, int downSample = 0)
        {
            rt = CreateRenderTexture(s.width >> downSample, s.height >> downSample, rt, s.format);
            rt.filterMode = s.filterMode;
            rt.wrapMode = s.wrapMode;
            return rt;
        }

        public static RenderTexture CreateRenderTexture(int width, int height, RenderTexture rt = null, RenderTextureFormat format = RenderTextureFormat.ARGBHalf)
        {
            if (rt != null)
                ReleaseRenderTexture(rt);
            rt = new RenderTexture(width, height, 16, format);
            rt.wrapMode = TextureWrapMode.Repeat;
            rt.filterMode = FilterMode.Bilinear;
            rt.Create();
            rt.name = "helper.createRenderTexture";
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            return rt;
        }

        public static RenderTexture[] CreateRts(RenderTexture source, RenderTexture[] rts = null, int downSample = 0)
        {
            return Enumerable.Range(0, 2).Select(b =>
            {
                var rt = CreateRenderTexture(source, rts != null && rts.Length == 2 ? rts[b] : null, downSample);
                rt.wrapMode = source.wrapMode;
                rt.name = "helper.createRts." + b;
                return rt;
            }).ToArray();
        }

        /// <summary>
        /// return true if invalid target.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="downSample"></param>
        /// <returns></returns>
        public static bool CheckRtSize(Texture source, Texture target, int downSample = 0)
        {
            return target == null || target.width != source.width >> downSample || target.height != source.height >> downSample;
        }

        public static void ReleaseRenderTexture(RenderTexture rt)
        {
            if (rt == null || RenderTexture.active == rt)
                return;
            rt.Release();
            Object.Destroy(rt);
        }

        public static Vector3 RandomMinMaxPoint(Vector3 pointMin, Vector3 pointMax)
        {
            return new Vector3(
                Random.Range(pointMin.x, pointMax.x),
                Random.Range(pointMin.y, pointMax.y),
                Random.Range(pointMin.z, pointMax.z)
            );
        }
        public static T[] ResizeArray<T>(T[] array, int size)
        {
            var last = array.LastOrDefault();
            if (size < array.Length)
                array = array.Where((val, idx) => idx < size).ToArray();
            else
                array = MargeArray(array, Enumerable.Repeat(last, size - array.Length).ToArray());
            return array;
        }

        public static T[] MargeArray<T>(T[] array1, T[] array2)
        {
            var array = new T[array1.Length + array2.Length];
            System.Array.Copy(array1, array, array1.Length);
            System.Array.Copy(array2, 0, array, array1.Length, array2.Length);
            array1 = array2 = null;
            return array;
        }

        public static T[] MargeArray<T>(T[] array1, T[] array2, int length)
        {
            var array = new T[array1.Length + length];
            System.Array.Copy(array1, array, array1.Length);
            System.Array.Copy(array2, 0, array, array1.Length, length);
            array1 = array2 = null;
            return array;
        }

        public static T[] MargeArray<T>(T[] array1, T[] array2, int length1, int length2)
        {
            System.Array.Copy(array2, 0, array1, length1, length2);
            return array1;
        }

        public static ComputeBuffer CreateComputeBuffer<T>(int count)
        {
            return new ComputeBuffer(count, Marshal.SizeOf(typeof(T)));
        }

        public static ComputeBuffer CreateComputeBuffer<T>(T[] array, bool setData = false)
        {
            var buffer = CreateComputeBuffer<T>(array.Length);
            if (setData)
                buffer.SetData(array);
            return buffer;
        }
        public static Mesh GetPrimitiveMesh(PrimitiveType type)
        {
            if (primitiveMeshMap.ContainsKey(type))
                return primitiveMeshMap[type];
            var go = GameObject.CreatePrimitive(type);
            primitiveMeshMap.Add(type, go.GetComponent<MeshFilter>().sharedMesh);
            Object.Destroy(go);
            return primitiveMeshMap[type];
        }
        static Dictionary<PrimitiveType, Mesh> primitiveMeshMap = new Dictionary<PrimitiveType, Mesh>();

        /// <summary>
        /// Determines if the given point is inside the polygon http://stackoverflow.com/questions/4243042/c-sharp-point-in-polygon
        /// </summary>
        /// <param name="polygon">the vertices of polygon</param>
        /// <param name="testPoint">the given point</param>
        /// <returns>true if the point is inside the polygon; otherwise, false</returns>
        public static bool IsInsidePolygon(Vector2[] polygon, Vector2 testPoint)
        {
            var result = false;
            var minX = polygon.Min(p => p.x);
            var maxX = polygon.Max(p => p.x);
            var minY = polygon.Min(p => p.y);
            var maxY = polygon.Max(p => p.y);
            if (testPoint.x < minX || maxX < testPoint.x || testPoint.y < minY || maxY < testPoint.y)
                return result;
            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++)
            {
                if (polygon[i].y < testPoint.y && polygon[j].y >= testPoint.y || polygon[j].y < testPoint.y && polygon[i].y >= testPoint.y)
                {
                    if (polygon[i].x + (testPoint.y - polygon[i].y) / (polygon[j].y - polygon[i].y) * (polygon[j].x - polygon[i].x) < testPoint.x)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }
    }

    // from nobnak https://github.com/nobnak/GaussianBlurUnity
    static class Gaussian
    {
        #region gaussianMat

        static Material gaussianMat
        {
            get
            {
                if (_gaussianMat == null)
                    _gaussianMat = new Material(Shader.Find("Hidden/Gaussian"));
                return _gaussianMat;
            }
        }

        static Material _gaussianMat;

        #endregion

        public static RenderTexture GetDdownSampledRt(Texture s, RenderTexture blitTarget, int lod, Material blitMat)
        {
            if (Helper.CheckRtSize(s, blitTarget, lod))
                blitTarget = Helper.CreateRenderTexture(s.width >> lod, s.height >> lod, blitTarget);
            var ds = DownSample(s, lod, gaussianMat);
            if (blitMat != null)
                Graphics.Blit(ds, blitTarget, blitMat);
            else
                Graphics.Blit(ds, blitTarget);

            RenderTexture.ReleaseTemporary(ds);
            return blitTarget;
        }

        public static RenderTexture GaussianFilter(RenderTexture s, RenderTexture d, int nIterations = 3, int lod = 1)
        {
            var ds = DownSample(s, lod, gaussianMat);
            Blur(ds, d, nIterations, gaussianMat);
            RenderTexture.ReleaseTemporary(ds);
            return d;
        }

        static void Blur(RenderTexture src, RenderTexture dst, int nIterations, Material gaussianMat)
        {
            var tmp0 = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
            var tmp1 = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
            var iters = Mathf.Clamp(nIterations, 0, 10);
            Graphics.Blit(src, tmp0);
            for (var i = 0; i < iters; i++)
            {
                for (var pass = 1; pass < 3; pass++)
                {
                    tmp1.DiscardContents();
                    tmp0.filterMode = FilterMode.Bilinear;
                    Graphics.Blit(tmp0, tmp1, gaussianMat, pass);
                    var tmpSwap = tmp0;
                    tmp0 = tmp1;
                    tmp1 = tmpSwap;
                }
            }
            Graphics.Blit(tmp0, dst);
            RenderTexture.ReleaseTemporary(tmp0);
            RenderTexture.ReleaseTemporary(tmp1);
        }

        static RenderTexture DownSample(Texture src, int lod, Material gaussianMat)
        {
            var dst = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGBHalf);
            src.filterMode = FilterMode.Bilinear;
            Graphics.Blit(src, dst);

            for (var i = 0; i < lod; i++)
            {
                var tmp = RenderTexture.GetTemporary(dst.width >> 1, dst.height >> 1, 0, RenderTextureFormat.ARGBHalf);
                dst.filterMode = FilterMode.Bilinear;
                Graphics.Blit(dst, tmp, gaussianMat, 0);
                RenderTexture.ReleaseTemporary(dst);
                dst = tmp;
            }
            return dst;
        }
    }
    [System.Serializable]
    public class CoonsCurve
    {
        public Vector3 position0;
        public Vector3 position1;
        public Vector3 velocity0;
        public Vector3 velocity1;

        private Vector3 a, b, c, d;

        public CoonsCurve(Vector3 p0, Vector3 p1, Vector3 v0, Vector3 v1)
        {
            SetVertices(p0, p1, v0, v1);
            position0 = p0;
            position1 = p1;
            velocity0 = v0;
            velocity1 = v1;
        }

        public void SetVertices()
        {
            var p0 = position0;
            var p1 = position1;
            var v0 = velocity0;
            var v1 = velocity1;
            SetVertices(p0, p1, v0, v1);
        }
        public void SetVertices(Vector3 p0, Vector3 p1, Vector3 v0, Vector3 v1)
        {
            a = 2 * p0 - 2 * p1 + v0 + v1;
            b = -3 * p0 + 3 * p1 - 2 * v0 - v1;
            c = v0;
            d = p0;

            position0 = p0;
            position1 = p1;
            velocity0 = v0;
            velocity1 = v1;
        }
        public Vector3 Interpolate(float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return a * t3 + b * t2 + c * t + d;
        }
    }
    [System.Serializable]
    public class CoonsChain
    {
        public Vector3[] positions;
        public Vector3[] velocities;

        CoonsCurve[] curves;

        public Vector3 Interpolate(float t)
        {
            if (curves == null)
                return Vector3.zero;

            var idx = Mathf.FloorToInt(t * curves.Length);
            if (idx == curves.Length)
                idx--;
            t = t * curves.Length - idx;
            Gizmos.color = Color.Lerp(Color.red, Color.blue, t);
            return curves[idx].Interpolate(t);
        }
        public void UpdateCurves()
        {
            if (positions.Length != velocities.Length || positions.Length < 2)
                return;
            if (curves != null && curves.Length != positions.Length - 1)
                curves = null;
            if (curves == null)
                curves = new CoonsCurve[positions.Length - 1];
            for (var i = 0; i < curves.Length; i++)
            {
                if (curves[i] == null)
                    curves[i] = new CoonsCurve(positions[i], positions[i + 1], velocities[i], velocities[i + 1]);
                else
                    curves[i].SetVertices(positions[i], positions[i + 1], velocities[i], velocities[i + 1]);
            }
        }
        public void OnDrawGizmos(float dt = 0.05f) {
            UpdateCurves();
            for (var t = 0f; t < 1f; t += dt)
                Gizmos.DrawLine(Interpolate(t), Interpolate(t + dt));
        }
    }
    [System.Serializable]
    public class MaterialProperties
    {
        public StringTexturePair[] textureProps;
        public StringMatrixPair[] matrixProps;
        public StringColorPair[] colorProps;
        public StringVectorPair[] vectorProps;
        public StringFloatPair[] floatProps;
        public StringIntPair[] intProps;
    }

}