using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
/// 作用：文件读取加载
/// Author:Jxw
/// Time:2015/10/22


#if UNITY_EDITOR 
#pragma warning disable 0649 // 检测到无法访问的代码
#endif
namespace BIEFramework.Manager {

    public static class FileManager {
        private static bool m_bInitAssetBundle;

        //静态数据
        private static List<ArmorInfo> armorInfoList = new List<ArmorInfo>();
        private static List<BumperInfo> bumperInfoList = new List<BumperInfo>();
        private static List<CarInfo> carInfoList = new List<CarInfo>();
        private static List<CarSkinInfo> carskinInfoList = new List<CarSkinInfo>();
        private static List<NitroInfo> nitroInfoList = new List<NitroInfo>();
        private static List<PerformanceInfo> performanceInfoList = new List<PerformanceInfo>();
        private static List<WeaponInfo> weaponInfoList = new List<WeaponInfo>();
        private static List<GuideGirlInfo> guideGirlInfoList = new List<GuideGirlInfo>();
        private static List<SkillBaseInfo> skillBaseInfoList = new List<SkillBaseInfo>();
		private static List<SkillModel> skillModelList=new List<SkillModel>();
        private static List<SkillInfo> skillInfoList = new List<SkillInfo>();
        private static List<NPCInfo> npcInfoList = new List<NPCInfo>();
        private static List<DefaultPlayerInfo> defaultPlayerInfoList = new List<DefaultPlayerInfo>();
        private static List<MapInfo> mapInfoList = new List<MapInfo>();
        private static List<TaskInfo> taskInfoList = new List<TaskInfo>();
        private static List<PropInfo> propInfoList = new List<PropInfo>();
        private static List<DailyReward> dailyRewardList = new List<DailyReward>();
        private static List<GoodInfo> goodList = new List<GoodInfo>();
        private static List<DailyTask> dailyTaskList = new List<DailyTask>();
        private static List<PopupInfo> popupInfolList = new List<PopupInfo>();
        //数据文件位置
        public const string dataFolder = "/data/"; 
        //玩家数据
        public static UserInfo mUserInfo;


        private static bool isDataInit = false;
        public static void Init() {
            if (isDataInit) {
                return;
            }
            isDataInit = true;
            InitBundle();
        }

        public static void InitBundle() {
            if (!m_bInitAssetBundle) {
                m_bInitAssetBundle = true;

                ParserFromTxtFile<ArmorInfo>(armorInfoList);
                ParserFromTxtFile<BumperInfo>(bumperInfoList);
                ParserFromTxtFile<CarInfo>(carInfoList);
                ParserFromTxtFile<CarSkinInfo>(carskinInfoList);
                ParserFromTxtFile<NitroInfo>(nitroInfoList);
                ParserFromTxtFile<PerformanceInfo>(performanceInfoList);
                ParserFromTxtFile<WeaponInfo>(weaponInfoList);
                ParserFromTxtFile<GuideGirlInfo>(guideGirlInfoList);
                ParserFromTxtFile<SkillModel>(skillModelList);
				InitSkillBaseList();
                ParserFromTxtFile<SkillInfo>(skillInfoList);
                ParserFromTxtFile<MapInfo>(mapInfoList);
                ParserFromTxtFile<TaskInfo>(taskInfoList);
                ParserFromTxtFile<NPCInfo>(npcInfoList);
                ParserFromTxtFile<DefaultPlayerInfo>(defaultPlayerInfoList);
				ParserFromTxtFile<PropInfo>(propInfoList);
                ParserFromTxtFile<GoodInfo>(goodList);
                ParserFromTxtFile<DailyReward>(dailyRewardList);
                ParserFromTxtFile<DailyTask>(dailyTaskList);
                ParserFromTxtFile<PopupInfo>(popupInfolList);
                //加载userinfo
                LoadUserInfo();//先读取用户记录，如果没有记录，则创建新用户
                if (mUserInfo == null) {
                    //mUserInfo = new UserInfo(SystemInfo.deviceUniqueIdentifier + "_0");
                    mUserInfo = new UserInfo(SystemInfo.deviceUniqueIdentifier + "_1000");
                    SGTool.jsonToModel(defaultPlayerInfoList[0], mUserInfo);
                } else if (mUserInfo.StorySave > 0 && mUserInfo.UserID == SystemInfo.deviceUniqueIdentifier + "_0") {
                    int tempStroySave = mUserInfo.StorySave;
                    mUserInfo = new UserInfo(SystemInfo.deviceUniqueIdentifier + "_1000");
                    SGTool.jsonToModel(defaultPlayerInfoList[1], mUserInfo);
                    mUserInfo.StorySave = tempStroySave;
                }
				UpgradeDataInit();
				InitGuideGetSkill();
                FileManager.InitCarConfig();
            }
        }
        public static void SaveUserInfo() {
            //if (BeginnerGuid.IsNeedStrength())
            //    return;
            if (mUserInfo == null)
                return;
            SetCarConfig();
            byte[] bt = SGTool.protobufEncode<UserInfo>(mUserInfo);
            string save = SGTool.bytesToString(bt);
            SGTool.saveStringDataToPrefs(mUserInfo.UserID, save);

            bt = SGTool.protobufEncode<UserSettings>(mUserInfo.UserSettings);
            //SGDebug.Log<UserSettings>(mUserInfo.UserSettings);
            save = SGTool.bytesToString(bt);
            SGTool.saveStringDataToPrefs(mUserInfo.UserID + "_settings", save);
        }

        private static void LoadUserInfo() {
            string savedInfo = SGTool.getStringDataFromPrefs(SystemInfo.deviceUniqueIdentifier + "_1000");
            if (!string.IsNullOrEmpty(savedInfo)) {
                byte[] bt = SGTool.stringToBytes(savedInfo);
                mUserInfo = SGTool.protobufDecode<UserInfo>(bt);
                LoadCarConfig();
            }

            savedInfo = SGTool.getStringDataFromPrefs(SystemInfo.deviceUniqueIdentifier + "_1000_settings");
            if (!string.IsNullOrEmpty(savedInfo)) {
                byte[] bt = SGTool.stringToBytes(savedInfo);
                mUserInfo.UserSettings = SGTool.protobufDecode<UserSettings>(bt);
                //SGDebug.Log<UserSettings>(mUserInfo.UserSettings);
            }
        }
		public static void SaveGuideConfig()
		{
			if (mUserInfo.GuideGetSkill != null) {
				byte[] bt = SGTool.protobufEncode<Dictionary<string,Skill>>(mUserInfo.GuideGetSkill);
				mUserInfo.GuideGetSkillStringInfo = SGTool.bytesToString(bt);
			}
		}
		public static void LoadGuideConfig()
		{
			if (!string.IsNullOrEmpty (mUserInfo.GuideGetSkillStringInfo)) {
			byte[] bt = SGTool.stringToBytes (mUserInfo.GuideGetSkillStringInfo);
			mUserInfo.GuideGetSkill = SGTool.protobufDecode<Dictionary<string,Skill>> (bt);
		}
		}
		public static void InitGuideGetSkill()
		{
			if (mUserInfo.GuideGetSkill == null) {
				mUserInfo.GuideGetSkill=new Dictionary<string, Skill>();
			}
		}


        public static void InitCarConfig()
        {
            foreach (CarInfo carInfo in FileManager.carInfoList)
            {
                CarConfig car = new CarConfig();
                car.CarId = carInfo.Id;
                if (car.CarId == mUserInfo.CurrentCar)
                {
                    car.LeftWp = int.Parse(mUserInfo.BoughtWeapon.Split(new char[] { ',' })[0]);
                    if (mUserInfo.BoughtWeapon.Split(',').Length > 1)
                    {
                        //car.RightWp = mUserInfo.BoughtWeapon.Split(new char[] { ',' })[RaceController.loadLevelIndex.Equals(RaceController.GuideLevel) ? 1 : 0];
                        car.RightWp = int.Parse(mUserInfo.BoughtWeapon.Split(new char[] { ',' })[0]);
                    }
                    else
                    {
                        car.RightWp = int.Parse(mUserInfo.BoughtWeapon.Split(new char[] { ',' })[0]);
                    }
					car.CarSkin = int.Parse(mUserInfo.BoughtCarSkin.Split(new char[] { ',' })[0]);
					car.WheelSkin = int.Parse(mUserInfo.BoughtWheelSkin.Split(new char[] { ',' })[0]);
                }
                else
                {
                    car.LeftWp = 101;
                    car.RightWp = 101;
                    car.CarSkin = carInfo.SkinId;
                    car.WheelSkin = carInfo.WheelSkin;
                }

                if (mUserInfo.CarConfigDic == null)
                {
                    mUserInfo.CarConfigDic = new Dictionary<int, CarConfig>();
                    // CarConfig defaultCar = GetDefaultCarConfig(_carId);
                    mUserInfo.CarConfigDic.Add(carInfo.Id, car);
                    //CarConfigDic.Add()
                }
                else
                {
                    if (!mUserInfo.CarConfigDic.ContainsKey(carInfo.Id))
                    {
                        mUserInfo.CarConfigDic.Add(carInfo.Id, car);
                    }

                }
            }
        }
        static void SetCarConfig() {
            foreach (CarConfig car in mUserInfo.CarConfigDic.Values) {
                byte[] bt = SGTool.protobufEncode<CarConfig>(car);
                string save = SGTool.bytesToString(bt);
                if (mUserInfo.CarConfigs == null) {
                    mUserInfo.CarConfigs = new Dictionary<int, string>();
                    mUserInfo.CarConfigs.Add(car.CarId, save);
                } else {
                    if (mUserInfo.CarConfigs.ContainsKey(car.CarId)) mUserInfo.CarConfigs[car.CarId] = save;
                    else mUserInfo.CarConfigs.Add(car.CarId, save);

                }
            }
        }
        static void LoadCarConfig() {
            foreach (string s in mUserInfo.CarConfigs.Values) {
                byte[] bt = SGTool.stringToBytes(s);
                CarConfig car = SGTool.protobufDecode<CarConfig>(bt);
                if (mUserInfo.CarConfigDic == null) {
                    mUserInfo.CarConfigDic = new Dictionary<int, CarConfig>();
                    mUserInfo.CarConfigDic.Add(car.CarId, car);
                } else {
                    if (mUserInfo.CarConfigDic.ContainsKey(car.CarId)) mUserInfo.CarConfigDic[car.CarId] = car;
                    else mUserInfo.CarConfigDic.Add(car.CarId, car);
                }
            }
        }
        public static CarConfig GetCarConfig(int carId)
        {
            return FileManager.mUserInfo.CarConfigDic[carId];
        }
        public static void ParserFromTxtFile<T>(List<T> list, bool bRefResource = false) {
            string asset = null;

            //获取文件路径
            string file = ((DataPathAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(DataPathAttribute))).fiePath;

            if (bRefResource) {
                asset = ((TextAsset)Resources.Load(file, typeof(TextAsset))).text;
            } else {
                asset = File.ReadAllText(Util.DataPath + file+".txt");
            }

            StringReader reader = null;

            try {
                bool isHeadLine = true;
                string[] headLine = null;
                string stext = string.Empty;
                reader = new StringReader(asset);
                while ((stext = reader.ReadLine()) != null) {
                    if (isHeadLine) {
                        headLine = stext.Split(',');
                        isHeadLine = false;
                    } else {
                        string[] data = stext.Split(',');
                        list.Add(CreateDataModule<T>(headLine.ToList(), data));
                    }
                }
            } catch (Exception exception) {
                Debug.Log("file:" + file + ",msg:" + exception.Message);
            } finally {
                if (reader != null) {
                    reader.Close();
                }

            }
        }

        private static T CreateDataModule<T>(List<string> headLine, string[] data) {
            T result = Activator.CreateInstance<T>();
            FieldInfo[] fis = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo fi in fis) {
                string column = headLine.Where(tempstr => tempstr == fi.Name).FirstOrDefault();
                if (!string.IsNullOrEmpty(column)) {
                    string baseValue = data[headLine.IndexOf(column)];
                    object setValueObj = null;
                    Type setValueType = fi.FieldType;
                    if (setValueType.Equals(typeof(short))) {
                        setValueObj = string.IsNullOrEmpty(baseValue.Trim()) ? (short)0 :  Convert.ToInt16(baseValue);
                    } else if (setValueType.Equals(typeof(int))) {
                        setValueObj = string.IsNullOrEmpty(baseValue.Trim()) ? 0 : Convert.ToInt32(baseValue);
                    } else if (setValueType.Equals(typeof(long))) {
                        setValueObj = string.IsNullOrEmpty(baseValue.Trim()) ? 0 : Convert.ToInt64(baseValue);
                    } else if (setValueType.Equals(typeof(float))) {
                        setValueObj = string.IsNullOrEmpty(baseValue.Trim()) ? 0 : Convert.ToSingle(baseValue);
                    } else if (setValueType.Equals(typeof(double))) {
                        setValueObj = string.IsNullOrEmpty(baseValue.Trim()) ? 0 : Convert.ToDouble(baseValue);
                    } else if (setValueType.Equals(typeof(bool))) {
                        setValueObj = string.IsNullOrEmpty(baseValue.Trim()) ? false : Convert.ToBoolean(baseValue);
                    } else if (setValueType.Equals(typeof(byte))) {
                        setValueObj = Convert.ToByte(baseValue);
                    } else {
                        setValueObj = baseValue;
                    }
                    fi.SetValue(result, setValueObj);
                }
            }
            return result;
        }
        public static void UpgradeDataInit() {
            char[] splitChar = new char[] { ',' };
            if (mUserInfo.Upgrades == null) mUserInfo.Upgrades = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(mUserInfo.BoughtCar)) {
                string[] cars = mUserInfo.BoughtCar.Split(splitChar);
                for (int i = 0; i < cars.Length; i++)
                    if (!mUserInfo.Upgrades.ContainsKey("car_" + cars[i]))
                        mUserInfo.Upgrades.Add("car_" + cars[i], "0,0,0,0");
            }
            string[] boughtWeapons = mUserInfo.BoughtWeapon.Split(splitChar);
            for (int i = 0; i < boughtWeapons.Length; i++) {
                if (!mUserInfo.Upgrades.ContainsKey("wp_" + boughtWeapons[i]))
                    mUserInfo.Upgrades.Add("wp_" + boughtWeapons[i], "0");
                //if (!mUserInfo.Upgrades.ContainsKey("rw_" + boughtWeapons[i]))
                //    mUserInfo.Upgrades.Add("rw_" + boughtWeapons[i], "0");
            }

            if (!string.IsNullOrEmpty(mUserInfo.BoughtGuide)) {
                string[] boughtGuides = mUserInfo.BoughtGuide.Split(splitChar);
                for (int i = 0; i < boughtGuides.Length; i++) {
                    if (!mUserInfo.Upgrades.ContainsKey("gd_" + boughtGuides[i]))
                        mUserInfo.Upgrades.Add("gd_" + boughtGuides[i], "1");
                }
            }

            if (mUserInfo.BoughtArmor != null) {
                string[] boughtArmors = mUserInfo.BoughtArmor.Split(splitChar);
                for (int i = 0; i < boughtArmors.Length; i++) {
                    if (!mUserInfo.Upgrades.ContainsKey("armor_" + boughtArmors[i]))
                        mUserInfo.Upgrades.Add("armor_" + boughtArmors[i], "0");
                }
            }
            if (mUserInfo.BoughtBumper != null) {
                string[] boughtBumpers = mUserInfo.BoughtBumper.Split(splitChar);
                for (int i = 0; i < boughtBumpers.Length; i++) {
                    if (!mUserInfo.Upgrades.ContainsKey("bp_" + boughtBumpers[i]))
                        mUserInfo.Upgrades.Add("bp_" + boughtBumpers[i], "0");
                }
            }

            for(int i = 0; i < carInfoList.Count; i++)
            {
                if (!mUserInfo.Upgrades.ContainsKey("engine_" + carInfoList[i].Engine))
                {
                    mUserInfo.Upgrades.Add("engine_" + carInfoList[i].Engine, "1");
                }

                if (!mUserInfo.Upgrades.ContainsKey("turbine_" + carInfoList[i].Turbine))
                {
                    mUserInfo.Upgrades.Add("turbine_" + carInfoList[i].Turbine, "1");
                }

                if (!mUserInfo.Upgrades.ContainsKey("drivetrain_" + carInfoList[i].Drivetrain))
                {
                    mUserInfo.Upgrades.Add("drivetrain_" + carInfoList[i].Drivetrain, "1");
                }

                if (!mUserInfo.Upgrades.ContainsKey("tire_" + carInfoList[i].Tire))
                {
                    mUserInfo.Upgrades.Add("tire_" + carInfoList[i].Tire, "1");
                }
                if (!mUserInfo.Upgrades.ContainsKey("nitrous_" + carInfoList[i].Nitrous))
                {
                    mUserInfo.Upgrades.Add("nitrous_" + carInfoList[i].Nitrous, "1");
                }

            }





        }
        public static ArmorInfo FindArmorInfoFromId(int id) {
            ArmorInfo data = null;
            data = armorInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In ArmorInfo. ID :" + id);
            }
            return data;
        }

        public static List<ArmorInfo> FindarmorInfoList() {
            return armorInfoList;
        }
        public static int GetCurrentArmorIndex()
        {
            return armorInfoList.FindIndex(obj => (obj.Id == FileManager.mUserInfo.CurrentArmor));
        }
        public static BumperInfo FindBumperInfoFromId(int id) {
            BumperInfo data = null;
            data = bumperInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In BumperInfo. ID :" + id);
            }
            return data;
        }

        public static List<BumperInfo> FindBumperInfoList() {
            return bumperInfoList;
        }
        public static int GetCurrentBumperIndex()
        {
            return bumperInfoList.FindIndex(obj => (obj.Id == FileManager.mUserInfo.CurrentBumper));
        }

        public static CarInfo FindCarInfoFromId(int id) {
            CarInfo data = null;
            data = carInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In CarInfo. ID :" + id);
            }
            return data;
        }
        public static int FindMaxCarRpm()
        {

            int maxval = carInfoList.Select(x => x.MaxRpm).Max();
            return maxval;
        }
        public static int FindMinCarRpm()
        {
            int minval = carInfoList.Select(x => x.MaxRpm).Min();
            return minval;
        }

        public static List<CarInfo> FindCarInfoList()
        {
            return carInfoList;
        }
        public static int GetCurrentCarIndex()
        {
            return carInfoList.FindIndex(obj => (obj.Id == FileManager.mUserInfo.CurrentCar));
        }
        public static CarSkinInfo FindCarSkinInfoFromId(int id) {
            CarSkinInfo data = null;
            data = carskinInfoList.Find(x => x.Id == id );
            if (data == null) {
                Debugger.Log("Error : Not Found In CarSkinInfo. ID :" + id);
            }
            return data;
        }

        public static List<CarSkinInfo> FindCarSkinInfoList() {
            return carskinInfoList;
        }
        public static List<CarSkinInfo> FindCarWheelSkinInfoList()
        {
            List<CarSkinInfo> wheelList = new List<CarSkinInfo>();
            for (int i = 0; i < carskinInfoList.Count; i++)
            {
                if (carskinInfoList[i].Id > 200)
                    wheelList.Add(carskinInfoList[i]);
            }
            return wheelList;
        }
        public static int GetWheelSkinIndex()
        {
            return FindCarWheelSkinInfoList().FindIndex(obj => (obj.Id == FileManager.mUserInfo.CurrentWheelSkin));
        }
        public static NitroInfo FindNitroInfoFromId(int id) {
            NitroInfo data = null;
            data = nitroInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In NitroInfo. ID :" + id);
            }
            return data;
        }
		public static List<NitroInfo> FindPlayerNitroInfoList()
		{
			List<NitroInfo> n = new List<NitroInfo> ();
			for (int i = 0; i < nitroInfoList.Count; i ++) {
				if(nitroInfoList[i].Id > 300)
					n.Add(nitroInfoList[i]);
			}
			return n;

		}
        public static int FindMinNitroBoostPower()
        {

            int minval = nitroInfoList.Select(x => x.BoostPower).Min();
            return minval;
        }
        public static int FindMaxNitroBoostPower()
        {
			int maxval = FindPlayerNitroInfoList().Select(x => x.BoostPower).Max();
            return maxval;
        }

        public static int FindMinNitroUpgradeBoostPower()
        {

			int minval = FindPlayerNitroInfoList().Select(x => x.UpgradeBoostPower).Min();
            return minval;
        }
        public static int FindMaxNitroUpgradeBoostPower()
        {
			int maxval = FindPlayerNitroInfoList().Select(x => x.UpgradeBoostPower).Max();
            return maxval;
        }
		public static float FindMinNitroCapacity()
        {
			float minval = FindPlayerNitroInfoList().Select(x => x.NitroCapacity).Min();
            return minval;
        }
		public static float FindMaxNitroCapacity()
        {
			float maxval = FindPlayerNitroInfoList().Select(x => x.NitroCapacity).Max();
            return maxval;
        }
        public static float FindMinUpgradeNitroCapacity()
        {
			float minval = FindPlayerNitroInfoList().Select(x => x.UpgradeNitroCapacity).Min();
            return minval;
        }
        public static float FindMaxUpgradeNitroCapacity()
        {
			float maxval = FindPlayerNitroInfoList().Select(x => x.UpgradeNitroCapacity).Max();
            return maxval;
        }
        public static List<NitroInfo> FindNitroInfoList()
        {
            return nitroInfoList;
        }
        public static PerformanceInfo FindPerformanceInfoFromId(int id) {
            PerformanceInfo data = null;
            data = performanceInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In PerformanceInfo. ID :" + id);
            }
            return data;
        }

        public static List<PerformanceInfo> FindPerformanceInfoList() {
            return performanceInfoList;
        }
        public static WeaponInfo FindWeaponInfoFromId(int id) {
            WeaponInfo data = null;
            data = weaponInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In WeaponInfo. ID :" + id);
            }
            return data;
        }

        public static List<WeaponInfo> FindWeaponInfoList() {
            return weaponInfoList;
        }
        public static GuideGirlInfo FindGuideGirlInfoFromId(int id) {
            GuideGirlInfo data = null;
            data = guideGirlInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In GuideGirlInfo. ID :" + id);
            }
            return data;
        }
        public static int GetCurrentLeftWeaponIndex()
        {
            return weaponInfoList.FindIndex(obj => (obj.Id == FileManager.mUserInfo.CurrentLeftWeapon));
        }
        public static int GetCurrentRightWeaponIndex()
        {
            return weaponInfoList.FindIndex(obj => (obj.Id == FileManager.mUserInfo.CurrentRightWeapon));
        }
        public static List<GuideGirlInfo> FindGuideGirlInfoList() {
            return guideGirlInfoList;
        }
        public static SkillInfo FindSkillInfoFromId(int id) {
            SkillInfo data = null;
            data = skillInfoList.Find(x => x.Id == id);
            //if (data == null) {
            //    Debugger.Log("Error : Not Found In SkillInfo. ID :" + id);
            //}
            return data;
        }

        public static List<SkillInfo> FindWSkillInfoList() {
            return skillInfoList;
        }
		public static SkillBaseInfo FindSkillBaseInfoFromSkillId(int skillid)
		{
			SkillBaseInfo data=null;
			data=skillBaseInfoList.Find(x=>x.SkillId==skillid);
			if (data == null) {
				Debug.Log("Error : Not Found In SkillBaseInfo. SKillID :" + skillid);
			}
			return data;
		}
        public static SkillBaseInfo FindSkillBaseInfoFromId(int id) {
            SkillBaseInfo data = null;
            data = skillBaseInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In SkillBaseInfo. ID :" + id);
            }
            return data;
        }
		public static void InitSkillBaseList()
		{
			int currentGroup = 1;
			int currentId = 0;
			foreach (SkillModel model in skillModelList) {
				string [] skill_level=model.Skill.Split(';');
				for(int i=0;i<skill_level.Length;i++)
				{
					//int skillId=int.Parse(skill_level[i])/100;
					//int level=int.Parse (skill_level[i])%10;
					//int skillType=(skillId-1)+level;
                    int skillId = int.Parse(skill_level[i]);
					SkillBaseInfo skillbase= skillBaseInfoList.Find(x=> x.SkillGroup==currentGroup && x.SkillId==skillId);
					if(skillbase==null)
					{
						currentId++;
						skillBaseInfoList.Add(new SkillBaseInfo(){Id=currentId,SkillGroup=currentGroup,SkillId=skillId,Priority=1} );
					}
					else
					{
						skillbase.Priority++;
					}
				}

				currentGroup++;
			}
		}
		public static List<SkillBaseInfo> FindSkillBaseInfoListFromGroup(int skillGroup)
		{
			List<SkillBaseInfo> list= skillBaseInfoList.FindAll ( x=> x.SkillGroup==skillGroup);
			if (list == null || list.Count == 0)
				return null;
			else {
				return list;
			}
		}
        public static List<SkillBaseInfo> FindSkillBaseInfoList() {
            return skillBaseInfoList;
        }
        public static NPCInfo FindNPCInfoFromId(int id) {
            NPCInfo data = null;
            data = npcInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In NPCInfo. ID :" + id);
            }
            return data;
        }

        public static List<NPCInfo> FindNPCInfoList() {
            return npcInfoList;
        }
        public static MapInfo FindMapInfoFromId(int id) {
            MapInfo data = null;
            data = mapInfoList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("Error : Not Found In MapInfo. ID :" + id);
            }
            return data;
        }

        public static List<MapInfo> FindMapInfoList() {
            return mapInfoList;
        }

        public static TaskInfo FindTaskInfoFromId(int id)
        {
            TaskInfo data = null;
            data = taskInfoList.Find(x => x.Id == id);
            if (data == null)
            {
                Debugger.Log("Error : Not Found In MapInfo. ID :" + id);
            }
            return data;
        }

        public static List<TaskInfo> FindTaskInfoList()
        {
            return taskInfoList;
        }
        public static List<PropInfo> GetAllProps()
		{
			if (propInfoList.Count == 0) {
				Debugger.Log("found no props info");
			}
			return propInfoList;
		}
		public static PropInfo FindPropInfoFromId(int id)
		{
			PropInfo data = null;
			data = propInfoList.Find (x=> x.Id==id);
			if (data == null) {
				Debugger.Log("failed to find the prop with id="+id.ToString());
			}
			return data;
		}
        public static GoodInfo FindGoodInfoFromId(string id)
        {
            GoodInfo data = null;
            data = goodList.Find(x=>x.Id==id);
            if (data == null)
            {
                Debugger.Log("failed to find the good with id="+id);
            }
            return data;
        }
        public static List<GoodInfo> GetGoodListByCategory(string category)
        {
            List<GoodInfo> data = new List<GoodInfo>();
            foreach (GoodInfo g in goodList)
            {
                if (g.Category == category)
                    data.Add(g);
            }
            return data;
        }
        //public static DefaultPlayerInfo FindDefaultPlayerInfoFromId(int id) {
        //    DefaultPlayerInfo data = null;
        //    data = defaultPlayerInfoList.Find(x => x.Id == id);
        //    if (data == null) {
        //        Debugger.Log("Error : Not Found In MapInfo. ID :" + id);
        //    }
        //    return data;
        //}

        public static List<DefaultPlayerInfo> FindDefaultPlayerInfoList() {
            return defaultPlayerInfoList;
        }

        public static DailyReward FindDailyRewardInfo() {
            return dailyRewardList[0];
        }
        public static List<DailyTask> FindDailyTaskInfo() {
            return dailyTaskList;
        }
        public static DailyTask FindDailyTaskInfoFromId(int id) {
            DailyTask data = null;
            data = dailyTaskList.Find(x => x.Id == id);
            if (data == null) {
                Debugger.Log("failed to find the DailyTaskInfo with id=" + id);
            }
            return data;
        }

        public static List<PopupInfo> FindPopupInfoList()
        {
            return popupInfolList;
        }

        public static PopupInfo FindPopupInfoFrromId(string id)
        {
            PopupInfo data = null;
            data = popupInfolList.Find(x => x.Id == id);
            if (data == null)
            {
                Debugger.Log("Error : Not Found In PopupInfo . ID :" + id);
            }
            return data;
        }

        public static CarConfig FindCarConfigFromId(int carId)
        {
            return FileManager.mUserInfo.CarConfigDic[carId];
        }
    }

}

/// <summary>
/// 注释，各个数据对应的文件
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DataPathAttribute : Attribute {
    public string fiePath { get; set; }
    public DataPathAttribute(string _fiePath) {
        fiePath = _fiePath;
    }
}
