using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace sugi.cc
{
    public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get { if (_Instance == null) _Instance = FindObjectOfType<T>(); return _Instance; } }
        static T _Instance;
    }
}