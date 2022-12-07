using UnityEngine;
using System.Collections;

public class ShadowScript : MonoBehaviour {

    public Transform obj;
    
    private GameObject plane;
    private RenderTexture mTex = null;

    void Start()
    {
        plane = transform.FindChild("Plane").gameObject;
        Camera ShadowCamera = transform.FindChild("Camera").GetComponent<Camera>();

        if (!obj)
            obj = transform.parent;
        mTex = new RenderTexture(256, 256, 0);
        mTex.name = Random.Range(0, 100).ToString();
        ShadowCamera.targetTexture = mTex;
    }

    void Update()
    {
        plane.renderer.material.mainTexture = mTex;
    }
}
