
using UnityEngine;
using System;
using GameDefine;
using Game.GameData;
using System.Collections;
using Game.GameEntity;
using MO.MOBA.Tools;
using System.Collections.Generic;
using Game.GuideDate;
using Game.GameState;
using Game.Network;
using Game.Effect;
using Game.Resource;
using Game.View;
using Game;
using Game.Ctrl;
using Game.Model;

public class GameLogic : MonoBehaviour {

	public e_BattleState Battle_State {
		private set;
		get;
	}

	public static GameLogic Instance{
		set;get;
	}


	private bool IsCutLine = false;

    public bool IsInitialize = false;

    public bool IsQuickBattle = false;

    public List<string> ipList = new List<string>();

  //  public List<string> ServerIpList = new List<string>();
    public string LoginServerAdress = "10.10.40.29";
    public int LoginServerPort = 49996;

    public Game.AudioManager AudioPlay
    {
        get;
        private set;
    }

    public bool SkipNewsGuide = false;

	void Awake(){
		if (Instance != null) {
			Destroy(this.gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad (this.gameObject);
        Application.runInBackground = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        WindowManager.Instance.ChangeScenseToLogin(EScenesType.EST_None);
    }

	// Use this for initialization
	void Start () {
 		new PlayerManager ();
 		new NpcManager(); 
        NetworkManager.Instance.Close();

        ////读取游戏配置信息
        //GameConfig.Instance.Init();

        GameStateManager.Instance.EnterDefaultState();

        //初始化逻辑对象
        //CGLCtrl_GameLogic logini = CGLCtrl_GameLogic.Instance;

        //预加载，减少进入游戏资源加载卡顿
        ConfigReader.Init();
        GameMethod.FileRead();
        
        //预加载特效信息
        ReadPreLoadConfig.Instance.Init();
        //需要释放的资源信息
        ReadReleaseResourceConfig.Instance.Init();
       
	}

    void OnDestroy()
    {
       
    }

    
  
	// Update is called once per frame
	void Update ()
    {
        //更新buff
       Game.Skill.BuffManager.Instance.Update();
		//更新特效
        Game.Effect.EffectManager.Instance.UpdateSelf();      
        //更新提示消失
        MsgInfoManager.Instance.Update();
        //场景声音更新
        SceneSoundManager.Instance.Update();
        //声音更新
        Game.AudioManager.Instance.OnUpdate();
        //更新游戏状态机
        GameStateManager.Instance.Update(Time.deltaTime);
        //更新网络模块
        NetworkManager.Instance.Update(Time.deltaTime);
        //更新界面引导
         IGuideTaskManager.Instance().OnUpdate();
        //小地图更新
        MiniMapManager.Instance.Update();

        //UI更新
        WindowManager.Instance.Update(Time.deltaTime);

        //特效后删除机制 
        Game.Effect.EffectManager.Instance.HandleDelete();

        //GameObjectPool更新
        GameObjectPool.Instance.OnUpdate();

        //游戏时间设置
        GameTimeData.Instance.OnUpdate();
	}

 
	void OnEnable()
	{
        //event
        EventCenter.AddListener(EGameEvent.eGameEvent_ConnectServerSuccess, GameConnectServerSuccess);
        EventCenter.AddListener(EGameEvent.eGameEvent_ConnectServerFail, OpenConnectUI);
        EventCenter.AddListener(EGameEvent.eGameEvent_ReconnectToBatttle, OpenConnectUI);
        EventCenter.AddListener(EGameEvent.eGameEvent_BeginWaiting, OpenWaitingUI);   

        if (PlayerPrefs.HasKey(UIGameSetting.voiceKey))
        {
            int vKey = PlayerPrefs.GetInt(UIGameSetting.voiceKey);
            bool state = (vKey == 1) ? true : false;
            AudioManager.Instance.EnableVoice(state);
        }
        if (PlayerPrefs.HasKey(UIGameSetting.soundKey)) {
            int sKey = PlayerPrefs.GetInt(UIGameSetting.soundKey);
            bool state = (sKey == 1) ? true : false;
            AudioManager.Instance.EnableSound(state);
        }       
	}

	void OnDisable()
	{
        EventCenter.RemoveListener(EGameEvent.eGameEvent_ConnectServerSuccess, GameConnectServerSuccess);
        EventCenter.RemoveListener(EGameEvent.eGameEvent_ConnectServerFail, OpenConnectUI);
        EventCenter.RemoveListener(EGameEvent.eGameEvent_ReconnectToBatttle, OpenConnectUI);
        EventCenter.RemoveListener(EGameEvent.eGameEvent_BeginWaiting, OpenWaitingUI);   
	}

    //游戏退出前执行（玩家强行关闭游戏）
    void OnApplicationQuit()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || SKIP_SDK
#else
         SdkConector.Quit();
#endif

        Debug.Log("游戏退出前执行了OnAppliactionQuit");
        /*
	//	PlatformManager.GetSingleton ().OnAction (EActionType.eA_Logout,null,null,null);
        #region  talkingdata
        CEvent eve = new CEvent(EGameEvent.eGameEvent_TalkgameAction);
        eve.AddParam("type", EActionType.eA_Logout); 
        EventCenter.SendEvent(eve);
        #endregion 
        */

        NetworkManager.Instance.Close();
    }

    public void OpenConnectUI()
    {
        PlayerManager.Instance.CleanPlayerWhenGameOver();
        EntityManager.Instance.DestoryAllEntity();
        EffectManager.Instance.DestroyAllEffect();
        GameLogic.Instance.IsInitialize = true;
	}

    private void OpenWaitingUI()
    {
        if (WaitingInterface.Instance == null)
        {
            GameUI.Instance.OnOpenUIPathCamera(GameDefine.GameConstDefine.WaitingUI);
        }
    }

    /// <summary>
    /// 连接服务器成功
    /// </summary>
    private void GameConnectServerSuccess()
    {
        StopCoroutine("PingToServer");

        StartCoroutine("PingToServer");
    }

    private IEnumerator PingToServer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            CGLCtrl_GameLogic.Instance.EmsgToss_AskPing();
        }
    }

    public void PlayEnd()
    {
//         EntityManager.AllEntitys.Clear();
//         if (PlayerManager.Instance.LocalPlayer != null)
//         {
//             PlayerManager.Instance.AccountDic.Clear();
//             PlayerManager.Instance.LocalPlayer.AbsorbMonsterType = null;
//         }
//         
//         Game.AudioManager.Instance.StopHeroAudio();
    }

	public  void PlayStart()
    {
//         Int32 state = 0;
//         if (PlayerManager.Instance.LocalAccount.ObType == ObPlayerOrPlayer.PlayerObType)
//         {
//             state = 1;
//         }
//         GameMapObjs GameBuilding = GameObject.FindObjectOfType(typeof(GameMapObjs)) as GameMapObjs;
//         EntityManager.ClearHomeBase();
//         if (GameBuilding != null)
//         {
//             for (int id = 0; id < GameBuilding.transform.childCount; id++)
//             {
//                 Transform child = GameBuilding.transform.GetChild(id);
//                 int objId = 0;
//                 try
//                 {
//                     objId = Convert.ToInt32(child.name);
//                 }
//                 catch (Exception e)
//                 {
//                     Debug.LogError(e.ToString());
//                     continue;
//                 }
// 
//                 int infoId = GetMapObjIndex(objId);
//                 if (ConfigReader.MapObjXmlInfoDict.ContainsKey(infoId))
//                 {
//                     MapObjConfigInfo configInfp = ConfigReader.MapObjXmlInfoDict[infoId];
//                     int type = configInfp.eObjectTypeID;
//                     int index = configInfp.un32ObjIdx;
//                     int camp = configInfp.n32Camp;
//                     UInt64 sGUID = (UInt64)index;
//                     EntityManager.HandleDelectEntity(sGUID);
//                     Ientity item = NpcManager.Instance.HandleCreateEntity(sGUID, (EntityCampType)camp);
//                     item.MapObgId = objId;
//                     item.realObject = child.gameObject;
//                     item.objTransform = child.gameObject.transform;
//                     item.GameObjGUID = sGUID;
//                     item.NpcGUIDType = type;
//                     item.ObjTypeID = (uint)type;
//                     item.entityType = (EntityType)ConfigReader.GetNpcInfo(type).NpcType;
//                     item.SetHp(1);
//                     item.SetHpMax(1);
//                     EntityManager.Instance.SetCommonProperty(item, type);
//                     item.RealEntity = EntityManager.AddBuildEntityComponent(item);
//                     NpcManager.Instance.AddEntity(sGUID, item);
//                     EntityManager.AddHomeBase(item);
//                     GuideBuildingTips.Instance.AddBuildingTips(item);
//                 }
//             }
//         } 
       // LoadBaseDate.Instance().LoadBase();
	}

    private int GetMapObjIndex(int objId){
//         foreach (var item in ConfigReader.MapObjXmlInfoDict.Values) {
//             if (item.un32ObjIdx == objId && (int)GameUserModel.Instance.GameMapID == item.un32MapID)
//             {
//                 return item.un32Id;
//             }
//         }
         return -1;
     }
}
