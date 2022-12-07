using UnityEngine;
using System.Collections;

public class Shadow : MonoBehaviour {

    public RenderTexture tex;

    void Start()
    {
        GameObject.Find("Camera").GetComponent<Camera>().targetTexture = tex;
    }

    void Update()
    {
        renderer.material.mainTexture = tex;
    }
	
}

