using UnityEngine;

namespace sugi.cc
{
    public class CameraTargetEvent : MonoBehaviour
    {
        public TextureEvent onCreateTarget;
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
        void Start()
        {
            var cam = GetComponent<Camera>();
            var width = cam.pixelWidth;
            var height = cam.pixelHeight;
            var targetTexture = Helper.CreateRenderTexture(width, height);
            onCreateTarget.Invoke(targetTexture);
            cam.targetTexture = targetTexture;
        }
    }
}