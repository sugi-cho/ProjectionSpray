using UnityEngine;

namespace sugi.cc
{
    public abstract class RendererBehaviour : MonoBehaviour
    {
        public new Renderer renderer { get { if (_r == null) _r = GetComponentInChildren<Renderer>(); return _r; } }
        Renderer _r;
        public Renderer[] renderers { get { if (_rs == null) _rs = GetComponentsInChildren<Renderer>(); return _rs; } }
        Renderer[] _rs;
    }
}