using UnityEngine;
using System.Collections;

namespace sugi.cc
{
    #region string pairs structs
    [System.Serializable]
    public struct StringObjectPair
    {
        public string propName;
        public object value;
        public StringObjectPair(string s, object o)
        {
            propName = s;
            value = o;
        }
    }
    [System.Serializable]
    public struct StringIntPair
    {
        public string propName;
        public int value;
        public StringIntPair(string s, int i)
        {
            propName = s;
            value = i;
        }
    }
    [System.Serializable]
    public struct StringFloatPair
    {
        public string propName;
        public float value;
        public StringFloatPair(string s, float f)
        {
            propName = s;
            value = f;
        }
    }
    [System.Serializable]
    public struct StringColorPair
    {
        public string propName;
        public Color value;
        public StringColorPair(string s, Color c)
        {
            propName = s;
            value = c;
        }
    }
    [System.Serializable]
    public struct StringVectorPair
    {
        public string propName;
        public Vector4 value;
        public StringVectorPair(string s, Vector4 v)
        {
            propName = s;
            value = v;
        }
    }
    [System.Serializable]
    public struct StringTexturePair
    {
        public string propName;
        public Texture value;
        public StringTexturePair(string s, Texture t)
        {
            propName = s;
            value = t;
        }
    }
    [System.Serializable]
    public struct StringMatrixPair
    {
        public string propName;
        public Matrix4x4 value;
        public StringMatrixPair(string s, Matrix4x4 m)
        {
            propName = s;
            value = m;
        }
    }
    #endregion

    [System.Serializable]
    public struct RandomOp
    {
        public float minVal;
        public float maxVal;
        public float randomValue { get { return Random.Range(minVal, maxVal); } }
        public RandomOp(float min, float max)
        {
            minVal = min;
            maxVal = max;
        }
    }
}