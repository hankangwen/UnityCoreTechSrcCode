using UnityEngine;
using System.Collections;

/// <summary>
/// 主摄像机控制脚本
/// 绑定在Main Camera上
/// </summary>
public class RotateCameraControl : MonoBehaviour {

    private float _x = 0.0f;
    private float _y = 20.0f;

    public int xSpeed = 250;
    public int ySpeed = 120;
    public int yMinLimit = 5;
    public int yMaxLimit = 40;
    public int wheelSpeed = 8;
    public float distance = 2.5f;
    public float minDis = 2.5f;
    public float maxDis = 2.5f;

    private Quaternion _rotation;
    //private Vector3 _pos;

    private GameObject _playerTran;
    bool isInit = false;
    public GameObject PlayerTranTransform;

    private float _currentDistance;
    private float _desiredDistance;
    private float _correctedDistance;
    public float PlayerHeight = 1.85f;
	
	public static int camcount = 0;
	
    void Awake()
    {
        transform.name = "PlayerCamera";
// 		camera.farClipPlane = 20.0f;
// 		
// 		if(camcount == 6)
// 		{
// 			camcount = 0;
// 		}
    }

    void Start()
    {
        InvokeRepeating("init", 0f, 0.5f);
        _currentDistance = distance;
        _desiredDistance = distance;
        _correctedDistance = distance - 0.2f;
    }

    void init()
    {
       PlayerTranTransform = GameObject.FindWithTag("Player");
	   if (PlayerTranTransform)
	   {
	        _playerTran = PlayerTranTransform;
	        transform.rotation = Quaternion.Euler(_y, _x, 0);
	        transform.position = Quaternion.Euler(_y, _x, 0) * new Vector3(0, 0, 20) + _playerTran.transform.position;
	        isInit = true;
	        CancelInvoke("init");
	   }
    }

    // Update is called once per framei
    void Update()
    {
// 		if(camcount < 6)
// 		{
// 			camera.farClipPlane = (camcount +  1) * 20;
// 			
// 			camcount++;
// 		}
		
        if (!PlayerTranTransform)
        {
            PlayerTranTransform = GameObject.FindWithTag("Player");
        }
        if (!isInit)
        {
            return;
        }
        //
        if (Input.GetMouseButton(1))
        {
            _x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            _y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        _y = Algorithm.Clamp(_y, yMinLimit, yMaxLimit);
        //set camera _rotation
        _rotation = Quaternion.Euler(_y, _x, 0);

        //calculate the desired distance
		if(Input.GetAxis("Mouse ScrollWheel") != 0)
		{
			_desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * wheelSpeed;
		}
        
        _desiredDistance = Algorithm.Clamp(_desiredDistance, minDis, maxDis);
        _correctedDistance = _desiredDistance;

        // calculate desired camera position
        Vector3 position = _playerTran.transform.position - (_rotation * Vector3.forward * _desiredDistance + new Vector3(0, -PlayerHeight, 0));
        //_pos = _rotation * new Vector3(0.0f, 0.0f, -distance) + _playerTran.position;

        // check for collision using the true PlayerObj's desired registration point as set by user using height
        RaycastHit collisionHit;
        Vector3 truePlayerObjPosition = new Vector3(_playerTran.transform.position.x, _playerTran.transform.position.y + PlayerHeight, _playerTran.transform.position.z);

        // if there was a collision, correct the camera position and calculate the corrected distance
        bool isCorrected = false;
        if (Physics.Linecast(truePlayerObjPosition, position, out collisionHit))
        {
            position = collisionHit.point;
            _correctedDistance = Vector3.Distance(truePlayerObjPosition, position);
            isCorrected = true;
        }

        // For smoothing, lerp distance only if either distance wasn't corrected, or _correctedDistance is more than _currentDistance
        _currentDistance = isCorrected || _correctedDistance <= _currentDistance ? _correctedDistance : Mathf.Lerp(_currentDistance, _correctedDistance, Time.deltaTime * 10);

        // recalculate position based on the new _currentDistance
        position = _playerTran.transform.position - (_rotation * Vector3.forward * _currentDistance + new Vector3(0, -PlayerHeight - 0.05f, 0));

        transform.rotation = _rotation;
        transform.position = position;

    }
}
