using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using sugi.cc;

public class CanvasObjectBuilder : SingletonMonoBehaviour<CanvasObjectBuilder> {

    public Material objMat;
    public Material updateMat;

    public void Build(GameObject go)
    {
        var canvasObj = go.AddComponent<CanvasObject>();
        canvasObj.updater = updateMat;
        go.GetComponent<Renderer>().sharedMaterial = objMat;
    }

    private void Update()
    {
        //for debug

        if (Input.GetMouseButtonDown(0))
        {
            var pos = Input.mousePosition;
            pos.z = 10f;
            pos = Camera.main.ScreenToWorldPoint(pos);
            var newGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            newGo.transform.position = pos;
            Build(newGo);
        }
    }
}
