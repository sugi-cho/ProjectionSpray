using UnityEngine;
using System.Collections;

namespace sugi.cc
{
    public static class Extensions
    {
        #region MaterialPropertyBlock mpBlock

        static MaterialPropertyBlock mpBlock
        {
            get
            {
                if (_mpBlock == null)
                    _mpBlock = new MaterialPropertyBlock();
                return _mpBlock;
            }
        }

        static MaterialPropertyBlock _mpBlock;

        #endregion

        public static void Invoke(this MonoBehaviour behaviour, System.Action method, float time)
        {
            behaviour.Invoke(method.Method.Name, time);
        }
        public static void InvokeRepeating(this MonoBehaviour behaviour, System.Action method, float time, float repeatRate)
        {
            behaviour.InvokeRepeating(method.Method.Name, time, repeatRate);
        }

        public static void CancelInvoke(this MonoBehaviour behaviour, System.Action method)
        {
            behaviour.CancelInvoke(method.Method.Name);
        }

        #region call method with delay by using coroutine
        static IEnumerator DelayMethodCoroutine(float waitTime, System.Action method)
        {
            yield return new WaitForSeconds(waitTime);
            method();
        }
        public static void CallMethodDelayed(this MonoBehaviour behaviour, float time, System.Action method)
        {
            behaviour.StartCoroutine(DelayMethodCoroutine(time, method));
        }
        public static void CallMethodDelayed<T>(this MonoBehaviour behaviour, float time, System.Action<T> method, T param)
        {
            behaviour.StartCoroutine(DelayMethodCoroutine(time, () => { method(param); }));
        }
        public static void CallMethodDelayed<T1, T2>(this MonoBehaviour behaviour, float time, System.Action<T1, T2> method, T1 param1, T2 param2)
        {
            behaviour.StartCoroutine(DelayMethodCoroutine(time, () => { method(param1, param2); }));
        }
        public static void CallMethodDelayed<T1, T2, T3>(this MonoBehaviour behaviour, float time, System.Action<T1, T2, T3> method, T1 param1, T2 param2, T3 param3)
        {
            behaviour.StartCoroutine(DelayMethodCoroutine(time, () => { method(param1, param2, param3); }));
        }
        public static void CallMethodDelayed<T1, T2, T3, T4>(this MonoBehaviour behaviour, float time, System.Action<T1, T2, T3, T4> method, T1 param1, T2 param2, T3 param3, T4 param4)
        {
            behaviour.StartCoroutine(DelayMethodCoroutine(time, () => { method(param1, param2, param3, param4); }));
        }
        #endregion

        public static void ApplyGaussianFilter(this RenderTexture s, int nIterations = 3, int lod = 1, RenderTexture d = null)
        {
            Gaussian.GaussianFilter(s, d == null ? s : d, nIterations, lod);
        }
        public static RenderTexture GetDownSampled(this Texture s, RenderTexture output, int ds = 1, Material blitMat = null)
        {
            return Gaussian.GetDdownSampledRt(s, output, ds, blitMat);
        }

        /**
         * use NxN texture
         **/
        public static void DrawTexture(this RenderTexture canvas, Vector2 centerUV, float size, Texture tex, Material drawMat = null, int pass = 0)
        {
            var pos = new Vector2(centerUV.x * canvas.width, centerUV.y * canvas.height);
            size *= canvas.height;
            var rect = Rect.MinMaxRect(pos.x - size / 2f, pos.y - size / 2f, pos.x + size / 2f, pos.y += size / 2f);
            var projMat = Matrix4x4.Ortho(0f, canvas.width, 0f, canvas.height, -1f, 1f);

            GL.PushMatrix();
            GL.LoadIdentity();
            GL.LoadProjectionMatrix(projMat);
            RenderTexture.active = canvas;
            Graphics.DrawTexture(rect, tex, drawMat, pass);
            RenderTexture.active = null;
            GL.PopMatrix();
        }

        public static void DrawFullscreenQuad(this Material mat, int pass = 0, float z = 1.0f)
        {
            if (mat != null)
                mat.SetPass(pass);
            GL.Begin(GL.QUADS);
            GL.Vertex3(-1.0f, -1.0f, z);
            GL.Vertex3(1.0f, -1.0f, z);
            GL.Vertex3(1.0f, 1.0f, z);
            GL.Vertex3(-1.0f, 1.0f, z);

            GL.Vertex3(-1.0f, 1.0f, z);
            GL.Vertex3(1.0f, 1.0f, z);
            GL.Vertex3(1.0f, -1.0f, z);
            GL.Vertex3(-1.0f, -1.0f, z);
            GL.End();
        }
        public static void DrawFullscreenQuad(this Material mat, Color col, int pass = 0, float z = 1.0f)
        {
            if (mat != null)
                mat.SetPass(pass);
            GL.Color(col);
            GL.Begin(GL.QUADS);
            GL.Vertex3(-1.0f, -1.0f, z);
            GL.Vertex3(1.0f, -1.0f, z);
            GL.Vertex3(1.0f, 1.0f, z);
            GL.Vertex3(-1.0f, 1.0f, z);

            GL.Vertex3(-1.0f, 1.0f, z);
            GL.Vertex3(1.0f, 1.0f, z);
            GL.Vertex3(1.0f, -1.0f, z);
            GL.Vertex3(-1.0f, -1.0f, z);
            GL.End();
        }

        public static void DrawFullScreenQuadNxN(this Material mat, int numDivisions = 10, float uvMinX = 0, float uvMinY = 0, float uvMaxX = 1f, float uvMaxY = 1f, int pass = 0, float z = 1.0f)
        {
            //上のDrawQuadの、分割数をいじれるバージョン。たくさんオプションがあり、UV付きで、UVの範囲も指定できる（minXY,maxXYの、Rectの範囲のuv）
            if (mat != null)
                mat.SetPass(pass);
            var delta = 1f / numDivisions;
            GL.Begin(GL.QUADS);
            for (var y = 0; y < numDivisions; y++)
                for (int x = 0; x < numDivisions; x++)
                {
                    var vx0 = 2f * delta * x - 1f;
                    var vy0 = 2f * delta * y - 1f;
                    var tx0 = delta * x;
                    var ty0 = delta * y;
                    GL.MultiTexCoord2(0, Mathf.Lerp(uvMinX, uvMaxX, tx0), Mathf.Lerp(uvMinY, uvMaxY, ty0));
                    GL.MultiTexCoord2(1, tx0, ty0);
                    GL.Vertex3(vx0, vy0, z);

                    GL.MultiTexCoord2(0, Mathf.Lerp(uvMinX, uvMaxX, tx0 + delta), Mathf.Lerp(uvMinY, uvMaxY, ty0));
                    GL.MultiTexCoord2(1, tx0 + delta, ty0);
                    GL.Vertex3(vx0 + 2f * delta, vy0, z);

                    GL.MultiTexCoord2(0, Mathf.Lerp(uvMinX, uvMaxX, tx0 + delta), Mathf.Lerp(uvMinY, uvMaxY, ty0 + delta));
                    GL.MultiTexCoord2(1, tx0 + delta, ty0 + delta);
                    GL.Vertex3(vx0 + 2f * delta, vy0 + 2f * delta, z);

                    GL.MultiTexCoord2(0, Mathf.Lerp(uvMinX, uvMaxX, tx0), Mathf.Lerp(uvMinY, uvMaxY, ty0 + delta));
                    GL.MultiTexCoord2(1, tx0, ty0 + delta);
                    GL.Vertex3(vx0, vy0 + 2f * delta, z);
                }
            GL.End();
        }

        public static MaterialPropertyBlock GetPropertyBlock(this Renderer renderer)
        {
            renderer.GetPropertyBlock(mpBlock);
            return mpBlock;
        }

        #region SetProps to Renderer & Material
        public static void SetPropertis(this Renderer r, MaterialProperties props)
        {
            foreach (var pair in props.textureProps)
                r.SetTexture(pair.propName, pair.value);
            foreach (var pair in props.matrixProps)
                r.SetMatrix(pair.propName, pair.value);
            foreach (var pair in props.colorProps)
                r.SetColor(pair.propName, pair.value);
            foreach (var pair in props.vectorProps)
                r.SetVector(pair.propName, pair.value);
            foreach (var pair in props.floatProps)
                r.SetFloat(pair.propName, pair.value);
            foreach (var pair in props.intProps)
                r.SetFloat(pair.propName, pair.value);
        }
        public static void SetPropertis(this Material mat, MaterialProperties props)
        {
            foreach (var pair in props.textureProps)
                mat.SetTexture(pair.propName, pair.value);
            foreach (var pair in props.matrixProps)
                mat.SetMatrix(pair.propName, pair.value);
            foreach (var pair in props.colorProps)
                mat.SetColor(pair.propName, pair.value);
            foreach (var pair in props.vectorProps)
                mat.SetVector(pair.propName, pair.value);
            foreach (var pair in props.floatProps)
                mat.SetFloat(pair.propName, pair.value);
            foreach (var pair in props.intProps)
                mat.SetInt(pair.propName, pair.value);
        }
        #endregion

        #region SetPropertyToRender

        public static void SetColor(this Renderer r, string name, Color value)
        {
            r.GetPropertyBlock().SetColor(name, value);
            r.SetPropertyBlock(mpBlock);
        }

        public static void SetFloat(this Renderer r, string name, float value)
        {
            r.GetPropertyBlock().SetFloat(name, value);
            r.SetPropertyBlock(mpBlock);
        }

        public static void SetMatrix(this Renderer r, string name, Matrix4x4 value)
        {
            r.GetPropertyBlock().SetMatrix(name, value);
            r.SetPropertyBlock(mpBlock);
        }

        public static void SetTexture(this Renderer r, string name, Texture value)
        {
            r.GetPropertyBlock().SetTexture(name, value);
            r.SetPropertyBlock(mpBlock);
        }

        public static void SetVector(this Renderer r, string name, Vector4 value)
        {
            r.GetPropertyBlock().SetVector(name, value);
            r.SetPropertyBlock(mpBlock);
        }

        #endregion

        public static T GetRandom<T>(this T[] array)
        {
            return array[array.GetRandomIndex()];
        }

        public static void Swap<T>(this T[] array)
        {
            var tmp = array[0];
            array[0] = array[1];
            array[1] = tmp;
        }

        public static int GetRandomIndex(this System.Array array)
        {
            return Random.Range(0, array.Length);
        }

        public static Vector2 GetRandomPoint(this Rect rect)
        {
            var x = Random.Range(rect.xMin, rect.xMax);
            var y = Random.Range(rect.yMin, rect.yMax);
            return new Vector2(x, y);
        }

        public static Vector3 GetRandomPoint(this Bounds bounds)
        {
            var x = Random.Range(-bounds.extents.x, bounds.extents.x);
            var y = Random.Range(-bounds.extents.y, bounds.extents.y);
            var z = Random.Range(-bounds.extents.z, bounds.extents.z);
            return bounds.center + new Vector3(x, y, z);
        }

        public static RenderTexture[] GetPositionNormalTexture(this Mesh mesh, int width = 512, int height = 512) { return MeshInfoTexture.GeneratePositionNormalTexture(mesh, width, height); }
    }
}