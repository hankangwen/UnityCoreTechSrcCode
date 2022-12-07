using UnityEngine;
using System.Collections;

public class Avatar_Btn : MonoBehaviour {

    int count = 0;

    public void OnClick()
    {
        string name = this.gameObject.name;
        switch (name)
        {
            case "coat":
                if (count == 0)
                {
                    AvatarSys.instance.ChangeMesh("coat", "001");
                    count = 1;
                }
                else
                {
                    AvatarSys.instance.ChangeMesh("coat", "003");
                    count = 0;
                }
                break;
            case "hair":
                if (count == 0)
                {
                    AvatarSys.instance.ChangeMesh("hair", "001");
                    count = 1;
                }
                else
                {
                    AvatarSys.instance.ChangeMesh("hair", "003");
                    count = 0;
                }
                break;
            case "hand":
                if (count == 0)
                {
                    AvatarSys.instance.ChangeMesh("hand", "001");
                    count = 1;
                }
                else
                {
                    AvatarSys.instance.ChangeMesh("hand", "003");
                    count = 0;
                }
                break;
            case "head":
                if (count == 0)
                {
                    AvatarSys.instance.ChangeMesh("head", "001");
                    count = 1;
                }
                else
                {
                    AvatarSys.instance.ChangeMesh("head", "003");
                    count = 1;
                }
                break;
            case "pant":
                if (count == 0)
                {
                    AvatarSys.instance.ChangeMesh("pant", "001");
                    count = 1;
                }
                else
                {
                    AvatarSys.instance.ChangeMesh("pant", "003");
                    count = 0;
                }
                break;
            case "foot":
                if (count == 0)
                {
                    AvatarSys.instance.ChangeMesh("foot", "001");
                    count = 1;
                }
                else
                {
                    AvatarSys.instance.ChangeMesh("foot", "003");
                    count = 0;
                }
                break;
        }
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
