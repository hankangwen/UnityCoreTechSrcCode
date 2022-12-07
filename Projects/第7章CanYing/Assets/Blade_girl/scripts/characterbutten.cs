using UnityEngine;
using System.Collections;

public class characterbutten : MonoBehaviour
{

	public GameObject frog;
	
	
	
	private Rect FpsRect ;
	private string frpString;
	
	private GameObject instanceObj;
	public GameObject[] gameObjArray=new GameObject[9];
	public AnimationClip[] AniList  = new AnimationClip[9];
	
	float minimum = 2.0f;
	float maximum = 50.0f;
	float touchNum = 0f;
	string touchDirection ="forward"; 
	private GameObject blade_girl;
	
	// Use this for initialization
	void Start () {
		
		//frog.animation["dragon_03_ani01"].blendMode=AnimationBlendMode.Blend;
		//frog.animation["dragon_03_ani02"].blendMode=AnimationBlendMode.Blend;
		//Debug.Log(frog.GetComponent("dragon_03_ani01"));
		
		//Instantiate(gameObjArray[0], gameObjArray[0].transform.position, gameObjArray[0].transform.rotation);
	}
	
 void OnGUI() {
	  if (GUI.Button(new Rect(20, 20, 70, 40),"Idle")){
		 frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Idle");
	  }
	    if (GUI.Button(new Rect(90, 20, 70, 40),"Walk")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Walk");
	  }
		  if (GUI.Button(new Rect(160, 20, 70, 40),"L_Walk")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("L_Walk");
	  }
		  if (GUI.Button(new Rect(230, 20, 70, 40),"R_Walk")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("R_Walk");
	  }
		  if (GUI.Button(new Rect(300, 20, 70, 40),"B_Walk")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("B_Walk");
	  }
	     if (GUI.Button(new Rect(370, 20, 70, 40),"Talk")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Talk");
	  } 
		if (GUI.Button(new Rect(440, 20, 70, 40),"Run")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Run00");
	  }  
		if (GUI.Button(new Rect(510, 20, 70, 40),"L_Run")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("L_Run00");
	  }  
		if (GUI.Button(new Rect(580, 20, 70, 40),"R_Run")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("R_Run00");
	  }  
		if (GUI.Button(new Rect(650, 20, 70, 40),"B_Run")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("B_Run00");
	 }  
			if (GUI.Button(new Rect(720, 20, 70, 40),"Jump")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Jump_NoBlade");
	  } 
		if (GUI.Button(new Rect(20, 65, 70, 40),"Draw Blade")){
		  frog.animation.wrapMode= WrapMode.Once;
		  	frog.animation.CrossFade("DrawBlade");
	  } 
		if (GUI.Button(new Rect(90, 65, 70, 40),"AtkStandy")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("AttackStandy");
	  } 
		if (GUI.Button(new Rect(160, 65, 70, 40),"Attack00")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Attack00");
	  } 
		if (GUI.Button(new Rect(230, 65, 70, 40),"Attack01")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Attack");
			
		}	
		if (GUI.Button(new Rect(300, 65, 70, 40),"Block")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Block");
	  } 
			if (GUI.Button(new Rect(370, 65, 70, 40),"Attack02")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Attack01");
	  } 
			if (GUI.Button(new Rect(440, 65, 70, 40),"Combo")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("ComboAttack");
	  }
			if (GUI.Button(new Rect(510, 65, 70, 40),"Skill")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Skill");
	  }
			if (GUI.Button(new Rect(580, 65, 70, 40),"M_Avoid")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("M_Avoid");
	  }
			if (GUI.Button(new Rect(650, 65, 70, 40),"L_Avoid")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("L_Avoid");
	  }
			if (GUI.Button(new Rect(720, 65, 70, 40),"R_Avoid")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("R_Avoid");
	  }
			if (GUI.Button(new Rect(20, 110, 70, 40),"Buff")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Buff");
	  }
		if (GUI.Button(new Rect(90, 110, 70, 40),"Run01")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Run");
	  }
		if (GUI.Button(new Rect(160, 110, 70, 40),"L_Run01")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("L_Run");
	  }
		if (GUI.Button(new Rect(230, 110, 70, 40),"R_Run01")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("R_Run");
	  }
		if (GUI.Button(new Rect(300, 110, 70, 40),"B_Run01")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("B_Run");
	  }
		
		if (GUI.Button(new Rect(370, 110, 70, 40),"Jump01")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("jump");
	  }
		if (GUI.Button(new Rect(440, 110, 70, 40),"PickUp")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Pickup");
	  }
		
			if (GUI.Button(new Rect(510, 110, 70, 40),"Damage")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Damage");
	  }
			if (GUI.Button(new Rect(580, 110, 70, 40),"Death")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Death");
	  }
		if (GUI.Button(new Rect(650, 110, 120, 40),"GangnamStyle")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("GanamStyle");
	  }
				if (GUI.Button(new Rect(620, 470, 120, 40),"Ver 2.1")){
		  frog.animation.wrapMode= WrapMode.Loop;
		  	frog.animation.CrossFade("Idle");
	  }
 }
	
	// Update is called once per frame
	void Update () {
		
		//if(Input.GetMouseButtonDown(0)){
		
			//touchNum++;
			//touchDirection="forward";
		 // transform.position = new Vector3(0, 0,Mathf.Lerp(minimum, maximum, Time.time));
			//Debug.Log("touchNum=="+touchNum);
		//}
		/*
		if(touchDirection=="forward"){
			if(Input.touchCount>){
				touchDirection="back";
			}
		}
	*/
		 
		//transform.position = Vector3(Mathf.Lerp(minimum, maximum, Time.time), 0, 0);
	if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
		//frog.transform.Rotate(Vector3.up * Time.deltaTime*30);
	}
	
}
