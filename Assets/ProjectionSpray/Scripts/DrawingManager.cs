using System.Collections.Generic;
using UnityEngine;

public class DrawingManager : MonoBehaviour
{

    DrawerBase drawer;
    DrawableBase[] drawables;

    // Use this for initialization
    void Start()
    {
        drawer = FindObjectOfType<DrawerBase>();
        drawables = FindObjectsOfType<DrawableBase>();
    }

    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            drawer.drawing = true;
            drawer.color = Color.HSVToRGB((Time.time * 0.1f % 1f), 1f, 1f);
            drawer.transform.LookAt(hit.point);
        }

        drawer.drawing = Input.GetMouseButton(0);


        foreach (var drawable in drawables)
            drawable.ClearGuid();

        drawer.Draw();
        drawer.DrawGuid();

        foreach (var drawable in drawables)
            drawable.Apply();
    }

    private void OnGUI()
    {
        var width = Screen.width * 0.2f;
        var height = Screen.height;
        var rect = new Rect(0, 0, width, height);

        GUI.backgroundColor = Color.gray;
        for (var i = 0; i < 2; i++)
        {
            var co = (CanvasObject)drawables[i];
            rect.x = (Screen.width - width) * i;

            GUILayout.BeginArea(rect, (GUIStyle)"box");
            GUILayout.Label(co.name);
            GUILayout.Label("position texture");
            GUILayout.Label(co.posTex, GUILayout.Width(width), GUILayout.Height(width));
            GUILayout.Label("canvas texture");
            GUILayout.Label(co.drawingTex, GUILayout.Width(width), GUILayout.Height(width));
            GUILayout.EndArea();
        }
    }
}
