var target : Vector3;
var damping = 6.0;
var smooth = true;

function LateUpdate () {
			// Look at and dampen the rotation
			var rotation = Quaternion.LookRotation(target - transform.position);
			transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
}
