using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Wind))]


public class MistController : MonoBehaviour
{

	public GameObject mist;
	public GameObject mistLow;
	private Wind wind;
	private Transform player;
	
	void OnEnable ()
	{
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
		wind  = gameObject.GetComponent<Wind>();
		StartCoroutine(AddMist());
	}

	IEnumerator AddMist ()
	{
		while(true){
			Vector3 pos = new Vector3(player.position.x + Random.Range(-30, 30), player.position.y + 5, player.position.z + Random.Range(-30, 30));
			if(wind.humidity >= 0.7f){
			    GameObject mistParticles = Instantiate(mist, pos, new Quaternion(0,0,0,0)) as GameObject;
				mistParticles.transform.parent = player;
			}else if(wind.humidity > 0.4f){
			    GameObject mistParticles = Instantiate(mist, pos, new Quaternion(0,0,0,0)) as GameObject;
			    mistParticles.transform.parent = player;
				yield return new WaitForSeconds(0.5f);
			}else{
			    GameObject mistParticles = Instantiate(mistLow, pos, new Quaternion(0,0,0,0)) as GameObject;
				mistParticles.transform.parent = player;
			    yield return new WaitForSeconds(1f);
			}
			yield return new WaitForSeconds(0.5f);
			
		}
	}
}
