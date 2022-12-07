#pragma strict

function Start () {
   animation.wrapMode = WrapMode.Loop;

   animation["Attack"].wrapMode = WrapMode.Once;
   animation["Defend"].wrapMode = WrapMode.Once;
   animation["Jump"].wrapMode = WrapMode.Once;

   animation["Attack"].layer = 1;
   animation["Defend"].layer = 1;
   animation["Jump"].layer = 1;

   animation.Stop();
}

function Update (){
   if (Input.GetAxis("Vertical") > 0.1)
      animation.CrossFade("Run");

   else if (Input.GetAxis("Vertical") < -0.1)
      animation.CrossFade("Backstep");

   else if (Input.GetAxis("Horizontal") > 0.1)
      animation.CrossFade("StepRight");

   else if (Input.GetAxis("Horizontal") < -0.1)
      animation.CrossFade("StepLeft");

      else
      animation.CrossFade("Idle");

   if (Input.GetButtonDown ("Fire1"))
      animation.CrossFade("Attack");

   if (Input.GetButtonDown ("Fire2"))
      animation.CrossFade("Defend");

   if (Input.GetButtonDown ("Jump"))
      animation.CrossFade("Jump");
} 