using UnityEngine;
using System.Collections.Generic;

public class Buoyancy : MonoBehaviour
{
    public bool sink = false;  //下沉
    public float sinkForce = 3.0f; 
    
    private Ocean _ocean;
    private float _mag = 1f;
    private float _yPos = 0.1f;
    private List<Vector3> _blobs;
    private float _ax = 2.0f;
    private float _ay = 2.0f;
    private float _dampCoff = 0.2f; //阻尼系数
    private bool _engine = false;
    private List<float> _sinkForces;
    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = new Vector3(0.0f, -0.5f, 0.0f);

        Vector3 bounds = GetComponent<BoxCollider>().size;
        float length = bounds.z;
        float width = bounds.x;

        _blobs = new List<Vector3>();

        int i = 0;
        float xStep = 1.0f / (_ax - 1f);
        float yStep = 1.0f / (_ay - 1f);

        _sinkForces = new List<float>();

        float totalSink = 0;
        
        for (int x = 0; x < _ax;  x++)
        {
            for (int y = 0; y < _ay;  y++)
            {
                _blobs.Add(new Vector3((-0.5f + x * xStep) * width, 0.0f, (-0.5f + y * yStep) * length) 
                           + Vector3.up * yStep);
                float force = Random.Range(0f, 1f);
                force = force * force;
                totalSink += force;
                _sinkForces.Add(force);
                i++;
            }
        }
        //标准化浮力
        for (int j = 0; j < _sinkForces.Count; j++ )
        {
            _sinkForces[j] = _sinkForces[j] / totalSink * sinkForce;
        }
    }

    private void OnEnable()
    {
        if (_ocean == null)
            _ocean = GameObject.FindGameObjectWithTag("Ocean").GetComponent<Ocean>();
    }

    void FixedUpdate()
    {
        int index = 0;
        foreach (var blob in _blobs)
        {
            Vector3 wPos = transform.TransformPoint(blob);
            float damp = _rigidbody.GetPointVelocity(wPos).y;
            // Vector3 sinkForce = Vector3.zero;

            float buoyancy = _mag * (wPos.y);
            if (_ocean.enabled && !sink)
                buoyancy = _mag * (wPos.y - _ocean.GetWaterHeightAtLocation(wPos.x, wPos.y));

            if (sink)
                buoyancy = Mathf.Max(buoyancy, -3) + _sinkForces[index++];
            
            _rigidbody.AddForceAtPosition(-Vector3.up * (buoyancy + _dampCoff * damp), wPos);
        }
    }
    
    public void Sink(bool isActive)
    {
        sink = isActive;
    }
}