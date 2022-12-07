﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic; 
using Game.GameEntity;

public class XueTiaoMonster : XueTiaoUI 
{	
    void Awake()
    {
        hpSprite = transform.FindChild("Control - Hp/Foreground").GetComponent<UISprite>();
        labelCost = transform.FindChild("CP").GetComponent<UILabel>();
    }
	public override void SetXueTiaoInfo ()
	{
		base.SetXueTiaoInfo ();
		NpcConfigInfo info = ConfigReader.GetNpcInfo(xueTiaoOwner.NpcGUIDType);
		if(info.NpcCanControl == CanControl){
			int cp = (int)info.NpcConsumeCp;
			labelCost.text = "CP "+ cp.ToString();
			labelCost.gameObject.SetActive(true); 
		}
		else{
			labelCost.gameObject.SetActive(false); 
		}
	}

	public override void ResetXueTiaoValue ()
	{
		base.ResetXueTiaoValue();
		labelCost.text = null;
		labelCost.gameObject.SetActive(false);
	}

    public override void IsXueTiaoCpVib(bool isVis)
    {
        base.IsXueTiaoCpVib(isVis);
    }
}
