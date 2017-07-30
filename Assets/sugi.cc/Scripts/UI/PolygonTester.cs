using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace sugi.cc
{
    public class PolygonTester : MonoBehaviour
    {
        public Vector2[] polygon;

        public bool IsInsidePolygon(Vector2 point) { return Helper.IsInsidePolygon(polygon, point); }

        private void OnDrawGizmos()
        {
            var test = Input.mousePosition;
            test.x /= Screen.width;
            test.y /= Screen.height;
            if (IsInsidePolygon(test))
                Gizmos.color = Color.red;

            for (var i = 0; i < polygon.Length; i++)
            {
                var i1 = (i + 1) % polygon.Length;
                var vert0 = (Vector3)polygon[i] + Vector3.forward;
                vert0 = Camera.main.ViewportToWorldPoint(vert0);
                var vert1 = (Vector3)polygon[i1] + Vector3.forward;
                vert1 = Camera.main.ViewportToWorldPoint(vert1);
                Gizmos.DrawLine(vert0, vert1);
            }
            var vert = (Vector3)test + Vector3.forward;
            vert = Camera.main.ViewportToWorldPoint(vert);
            Gizmos.DrawSphere(vert, 0.1f);
        }
    }
}