using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CanYing : MonoBehaviour {

    public float interval = 0.1f;
    public float lifeCycle = 2.0f;

    float lastCombinedTime = 0.0f;

    MeshFilter[] meshFilters = null;

    MeshRenderer[] meshRenderers = null;

    SkinnedMeshRenderer[] skinedMeshRenderers = null;

    List<GameObject> objs = new List<GameObject>();

	// Use this for initialization
	void Start () 
    {
        meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
        skinedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
	}

    void OnDisable()
    {
        foreach (GameObject go in objs)
        {
            DestroyImmediate(go);
        }
        objs.Clear();
        objs = null;
    }
	// Update is called once per frame
	void Update () 
    {
        if (Time.time - lastCombinedTime > interval)
        {
            lastCombinedTime = Time.time;

            for (int i = 0; skinedMeshRenderers != null && i < skinedMeshRenderers.Length; ++i)
            {
                Mesh mesh = new Mesh();

                skinedMeshRenderers[i].BakeMesh(mesh);

                GameObject go = new GameObject();

                go.hideFlags = HideFlags.HideAndDontSave;
                MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.mesh = mesh;

                MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                meshRenderer.material = skinedMeshRenderers[i].material;

                InitFadeInObj(go, skinedMeshRenderers[i].transform.position,
                    skinedMeshRenderers[i].transform.rotation, lifeCycle);
            }
            for (int i = 0; meshFilters != null && i < meshFilters.Length; ++i)
            {
                GameObject go = Instantiate(meshFilters[i].gameObject) as GameObject;
                InitFadeInObj(go, meshFilters[i].transform.position, meshFilters[i].transform.rotation, lifeCycle);
            }
        }
	}

    private void InitFadeInObj(GameObject go, Vector3 position, Quaternion rotation, float lifeCycle)
    {
        go.hideFlags = HideFlags.HideAndDontSave;
        go.transform.position = position;
        go.transform.rotation = rotation;

        FadInOut fi = go.AddComponent<FadInOut>();
        fi.lifeCycle = lifeCycle;
        objs.Add(go);
    }
}
