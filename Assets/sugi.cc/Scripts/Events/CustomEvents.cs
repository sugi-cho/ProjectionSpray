using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace sugi.cc
{
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }
    [System.Serializable]
    public class IntEvent : UnityEvent<int> { }
    [System.Serializable]
    public class FloatEvent : UnityEvent<float> { }
    [System.Serializable]
    public class FloatArrayEvent : UnityEvent<float[]> { }
    [System.Serializable]
    public class TextureEvent : UnityEvent<Texture> { }
    [System.Serializable]
    public class Texture2DEvent : UnityEvent<Texture2D> { }
    [System.Serializable]
    public class RenderTextureEvent : UnityEvent<RenderTexture> { }
    [System.Serializable]
    public class Matrix4x4Event : UnityEvent<Matrix4x4> { }

    #region set to property
    [System.Serializable]
    public class StringValueEvent : UnityEvent<StringObjectPair> { }
    [System.Serializable]
    public class StringFloatEvent : UnityEvent<StringFloatPair> { }
    [System.Serializable]
    public class StringColorEvent : UnityEvent<StringColorPair> { }
    [System.Serializable]
    public class StringVectorEvent : UnityEvent<StringVectorPair> { }
    [System.Serializable]
    public class StringTextureEvent : UnityEvent<StringTexturePair> { }
    [System.Serializable]
    public class StringMatrixEvent : UnityEvent<StringMatrixPair> { }
    #endregion

    [System.Serializable]
    public class MeshEvent : UnityEvent<Mesh> { }
    [System.Serializable]
    public class ColorEvent : UnityEvent<Color> { }
}