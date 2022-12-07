using System;
using System.Collections.Generic;
using UnityEngine;
using SR.Core;
using SR.XML;
using System.Collections;
using System.IO;
using Ionic.Zip;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Umeng;

public class SRDataLoader : MonoBehaviour
{
#if UNITY_ANDROID || UNITY_WP8
    public static bool ABTesting_Flag = false;
#else
    public static bool ABTesting_Flag = true;
#endif
    protected static SRDataLoader instance = null;

    #region Singleton instance
    public static SRDataLoader Instance
    {
        get
        {
			if(instance == null)
			{
				instance = GameObject.Find("DataLoader").GetComponent<SRDataLoader>();
			}
            var dataLoader = instance; // Manager<StrawberryDataLoader>.Get();
            if (null == dataLoader)
			{
				return null;

			}
               

			//SR.Miniclip.UtilsBindings.ConsoleLog("***** AAAA " + Time.realtimeSinceStartup);
			if (!dataLoader.dataLoaded && !dataLoader.dataParsing)
            {
                if (dataLoader.downloadingZip)
                {
                    LogTool.Log("**** PELLE: STOP DOWNLOADING ZIP INCOMING REQUEST!!!! Duration: " + (Time.realtimeSinceStartup - instance.startTime));
                    dataLoader.stopDownloading = true;
                }

                dataLoader.TryLoadingXMLsFromZip();
            }
            return dataLoader;
        }
        set 
        {
            instance = value;
        }
    }
    #endregion

    #region iOS calls
#if UNITY_IPHONE && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern string _getLocale();
#endif
    #endregion

    #region Consts
    private static string OBFS(string str)
    {
        int length = str.Length;
        var array = new char[length];

        for (int i = 0; i < array.Length; i++)
        {
            char c = str[i];

            var b = (byte)(c ^ length - i);
            var b2 = (byte)((c >> 8) ^ i);
            array[i] = (char)(b2 << 8 | b);
        }

        return new string(array);
    }

	//加密算法
	public static string GetSHA512Password(string password)  
	{  
		byte[] bytes = Encoding.UTF7.GetBytes(password);  
		byte[] result;  
		SHA512 shaM = new SHA512Managed();  
		result = shaM.ComputeHash(bytes);  
		StringBuilder sb = new StringBuilder();  
		foreach (byte num in result)  
		{  
			sb.AppendFormat("{0:x2}", num);  
		}  
		return sb.ToString();  
	} 

    private const string configurationZipFile = "bie_v1.zip";
   // private /*const*/ string configurationZipPwd = OBFS("~Ũɦ;о԰ٷݫ࠵ळੰୠ౲");//"sdmt78pm07sbs";
	private /*const*/ string configurationZipPwd = GetSHA512Password("ٷ※▊ぷ┩▓ㄘЖ╔╕Ψ≮≯ゆǘйξζ");

    private const string versionFile = "versions.txt";
    private const string configurationFile = "config.xml";
    private const string configurationFileWeb = "configWeb.xml";
    private const string localizationFile = "translations.xml";
	private const string versionNumber = "versionNumber.txt";
	private static List<string> lstr = new List<string>();
    #endregion

    #region Public structs
    public struct CarsUpgradesMultipliers
    {
        public float acceleration_mult;
        public float maxSpeed_mult;
        public float resistance_mult;
        public float turbo_speed_mult;
    }
    #endregion

    #region Public members
    public TextAsset configuration;
    public TextAsset configurationWeb;
    public TextAsset localization;
    public TextAsset version;
    public string configurationZipURL = "http://www.silentbaystudios.com/test/";
    #endregion

    #region Protected Members
    protected int lastVersionNumber = 0;
    protected bool dataLoaded = false;
    public bool downloadingVersionFile = false;
    protected bool downloadingZip = false;
    protected bool stopDownloading = false;
    protected bool dataParsing = false;
    protected string versionString = "";

    /*
    protected Dictionary<string, IShopItem> items = new Dictionary<string, IShopItem>();
    protected Dictionary<string, Pack> packs = new Dictionary<string, Pack>();
    protected FruitType[] fruitsOrder;
    */

    protected bool enableChartboost;
    protected string localeToUse = "zh";
    protected int chartboostLimitPerDay;
    //protected int boostVideoAdLimitPerDay;

    //protected int androidVideoCoinsReward;
    protected int androidFlurryAdsPriority;
    protected int androidAdcolonyPriority;

    protected Dictionary<SRGameplay.CarId, PlayerExternalData> carsData = new Dictionary<SRGameplay.CarId, PlayerExternalData>();
    protected List<int[]> shopCoinsData = new List<int[]>();
    protected List<int[]> shopDiamondsData = new List<int[]>();
    protected List<int[]> shopFuelData = new List<int[]>();
    protected Dictionary<SRBooster.BoosterType, float> boostsData = new Dictionary<SRBooster.BoosterType, float>();
    protected Dictionary<UISaveMePopup.SaveMeButtonType, float[]> saveMeData = new Dictionary<UISaveMePopup.SaveMeButtonType, float[]>();
    protected Dictionary<string, List<WheelItem>> wheelData = new Dictionary<string, List<WheelItem>>();
    protected Dictionary<int, SRDailyBonusManager.DailyBonusData> dailyBonusDatas = new Dictionary<int, SRDailyBonusManager.DailyBonusData>();
    protected Dictionary<UICurrencyPopup.ShopPopupTypes, UICurrencyPopup.ShopOffers> offersData = new Dictionary<UICurrencyPopup.ShopPopupTypes, UICurrencyPopup.ShopOffers>();
	protected List<UIGiftItem.GiftInfo> giftInfos = new List<UIGiftItem.GiftInfo>();

    protected List<float> wheelDataPercentages;
    protected List<int> wheelDataQuantities;
    protected List<int> specialCarsLocks = new List<int>();
    protected List<int> specialCarsXPLevels = new List<int>();
    protected List<int> specialCarsPrices = new List<int>();
    protected List<int> alternativeSpecialCarsPrices = new List<int>();
    protected List<float> specialCarsUpgrades = new List<float>();

    protected int initDiamonds;
    protected int initCoins;
    protected float turboDuration;
    protected Dictionary<string, float> slipstreamData = new Dictionary<string, float>();
    protected List<float[]> slipstreamAdvancedData = new List<float[]>();
    protected float slipstreamEffectDuration;
    protected float saveMePopupDuration;
    protected float[] saveMePopupCost;
    protected float fbAdviseDurationIngame;
    protected float[] slideshowData = { 0, 3.0f, 1.5f };
    protected float[] turboCameraData = { 0, 3.0f, 1.5f };
    protected float[] finalCameraData = { 0, 3.0f, 1.5f };
    protected float percentageForceSpecialVehicle = 70.0f;
    protected float protectFromCollisionTime = 1.0f;
    protected int fuelVisibleInBar = 6;
    protected int resetGiftAfterDays = 1;
    protected int maxGifts = 5;
    protected int resetInvitesAfterDays = 1;
    protected int maxInvites = 5;
    protected int diamondsReward = 1;
    protected int timeToWaitToShowTrucksAdvise = 3;
    protected int timeToWaitToShowReachRank = 3;
    protected float fuelSingleRefillTime;
    protected float fuelFreezeDuration;
    protected float fuelFreezeNotificationAdvance;
    protected float slipstreamBase;
    protected float slipstreamStep;
    protected float slipstreamExponent;
    protected int crashNormal;
    protected int crashSpecialVehicle;
    protected float helicopterCoins;
    protected float helicopterCoinsDeltaTime;
    protected int extraSpinQuantity;
    protected float nearMissPercentage = 0.0f;
    protected int extraSpinCost;
    protected int missionsXPIncrement;
    protected int missionsCostIncrement;
    //protected Dictionary<OnTheRunMissions.MissionDifficulty, float> missionRewardByDifficulty;
    protected int weeklyChallengeReward;
    protected int firstFuelGiftEnabled;
    protected int firstFuelGift;
    protected float[] tiersXPThreshold;
    protected float[] tiersBuyFor;
    protected float[] tiersPrices;
    protected PriceData.CurrencyType[] tiersCurrencies;
    protected bool[] tiersComingSoon;
    protected int avgCoinsPerRun;
    protected int avgXPPerRun;
    protected bool fpsIsEnabled = false;
    protected int numScoresOffgameRanks = 50;
    protected int numScoresIngameRanks = 100;
    protected string facebookPageLink;
    protected float[] facebookLoginReward;
    protected int facebookInitialCounter = 1;
    protected int facebookDeltaCounter = 1;
    protected int facebookMaxTimesShown = 3;
    protected int facebookLoginRewardAfter = 2;
    protected int facebookDiamondsForInvite = 1;
    protected PriceData.CurrencyType[] facebookPageCurrency;

    protected int inappRemoveAdsAdviseFreq = 0;
    protected int inappRemoveAdsAdviseCounter = 0;

    protected int popupStoreFrequency = -1;
    protected int popupStoreMaxForSession = -1;

    protected List<int> dailyBonusMisteryItemDays = new List<int>();
    protected List<SRDailyBonusManager.DailyBonusData> dailyBonusMisteryRewards = new List<SRDailyBonusManager.DailyBonusData>();
    protected int recoverStreakCost;

    protected Dictionary<string, Dictionary<string, string>> strings = new Dictionary<string, Dictionary<string, string>>();

    protected Dictionary<string, float> inappDollarPricesByProductId = new Dictionary<string, float>();
	protected Dictionary<string , string> inappChargeCodeByProductId = new Dictionary<string , string>();
    protected Dictionary<string, float> facebookLoggedPopupData = new Dictionary<string, float>();

    protected Dictionary<string, CarsUpgradesMultipliers> carsUpgradesMultipliers = new Dictionary<string, CarsUpgradesMultipliers>();
    protected Dictionary<string, float> carsUpgradesDiscount = new Dictionary<string, float>();
    protected Dictionary<string, PlayerKinematics.PhysicParameters> playerPhysicParameters = new Dictionary<string, PlayerKinematics.PhysicParameters>();
    protected Dictionary<string, OpponentKinematics.PhysicParameters> opponentPhysicParameters = new Dictionary<string, OpponentKinematics.PhysicParameters>();

    protected bool[] showShopPerc = { true, true, true };
    protected int checkpointDistance = -1;
    protected float[] checkpointTimes;
    protected Dictionary<string, float> checkpointTimeData = new Dictionary<string, float>();

    protected Dictionary<string, float> specialCarsData = new Dictionary<string, float>();

    protected Dictionary<string, Dictionary<string, float>[]> trafficData = new Dictionary<string, Dictionary<string, float>[]>();
    protected Dictionary<string, Dictionary<string, float>> powerUpsData = new Dictionary<string, Dictionary<string, float>>();

    protected Dictionary<string, int> roadWorksData = new Dictionary<string, int>();
    protected Dictionary<string, int> centralMudData = new Dictionary<string, int>();

    protected int enemiesMinDistanceFromCheckpoint = -1;
    protected float policeSpawnPercentage = 100.0f;
    protected Dictionary<string, float[]> policeSpawnData = new Dictionary<string, float[]>();
    protected Dictionary<string, float[]> helicopterSpawnData = new Dictionary<string, float[]>();

    protected float speedReductionPerHit = 0.5f;
    protected int maxPlayerLevel = -1;
    protected int tiersCount = -1;

    protected Dictionary<string, int> levelUpRewardDictionary;
    protected Dictionary<string, float> missionsModificationsDictionary;
    protected Dictionary<string, float> missionsRewardDictionary;
    protected Dictionary<string, int> skipMissionPopupDictionary;
    protected Dictionary<string, float> consumableBonusesDictionary;
    protected List<SRConsumableBonusManager.ConsumableBonus> levelBonusesList;
    protected Dictionary<int, SRConsumableBonusManager.ConsumableBonus> levelBonusesAlternativedictionary;
	protected VIPRewardInfo vipReward;
	//add by hmh.achievement
	protected float slipStreamComboCountBase=1;
	protected float slipStreamComboCountStep = 1;
	protected float slipStreamComboRewardBase = 1;
	protected float slipStreamComboRewardStep = 1;
	
	protected float killCarComboCountBase = 1;
	protected float killCarComboCountStep = 1;
	protected float killCarComboRewardBase = 1;
	protected float killCarComboRewardStep = 1;
	
	protected float maxDistanceCountBase = 1;
	protected float maxDistanceCoundStep = 1;
	protected float maxDistanceRewardBase = 1;
	protected float maxDistanceRewardStep = 1;
	
	protected float coinCollectionCountBase = 1;
	protected float coinCollectionCountStep=1;
	protected float coinCollectionRewardBase=1;
	protected float coinCollectionRewardStep = 1;
    /*
    protected Dictionary<string, Recipe> recipes = new Dictionary<string, Recipe>();
    protected Dictionary<StrawberryBonus.Type, Boost> boosts = new Dictionary<StrawberryBonus.Type, Boost>();
    */

    public float startTime;
	#endregion 

    #region Public Mission Data
    public int[] metersToReachFirstTimeData = { 2000, 0, -1 };
    public int[] destroyTrafficFirstTimeData = { 10, 0, -1 };
    public int[] reachComboFirstTimeData = { 1, 0, -1 };
    public int[] jumpQuantityFirstTimeData = { 2, 0, -1 };
    public int[] collectBigCoinsFirstTimeData = { 5, 0, -1 };

    public int[] metersToReachData = { 3000, 500, 25000 };
    public int[] jumpQuantityData = { 4, 1, 25 };
    public int[] onAirData = { 4, 1, 12 };
    public int[] passCheckpointData = { 4, 1, 15 };
    public int[] collectCoinsData = { 50, 10, 400 };
    public int[] collectBigCoinsData = { 12, 2, 100 };
    public int[] fastLaneMetersData = { 2000, 250, 12000 };
    public int[] centralLaneMetersData = { 2000, 250, 18000 };
    public int[] leftLaneMetersData = { 2000, 250, 18000 };
    public int[] rightLaneMetersData = { 2000, 250, 18000 };
    public int[] avoidBarriersData = { 3000, 250, 25000 };
    public int[] destroyTrafficData = { 30, 3, 250 };
    public int[] destroyTrafficInTurboData = { 15, 3, 200 };
    public int[] destroyTrafficInTurboWrongLaneData = { 8, 2, 150 };
    public int[] destroyPoliceQuantityData = { 1, 1, 6 };
    public int[] runWithBigfootForData = { 600, 200, 9000 };
    public int[] runWithTankForData = { 600, 200, 9000 };
    public int[] runWithFiretruckForData = { 600, 200, 9000 };
    public int[] runWithUFOForData = { 600, 200, 9000 };
    public int[] runWithPlaneForData = { 600, 200, 9000 };
    public int[] destroyCarsDataWSpecial = { 9, 3, 180 };
    public int[] destroyCarsWithSpecialInWrongLaneData = { 4, 2, 120 };
    public int[] reachComboData = { 1, 0, -1 };
    #endregion

    #region Public Properties

    public bool DataIsLoaded { get { return dataLoaded; } }

    #endregion

    #region Coroutines
    IEnumerator DownloadVersionFile()
    {
        //SR.Miniclip.UtilsBindings.ConsoleLog("***** START DOWNLOAD VERSION FILE " + Time.realtimeSinceStartup);
        Asserts.Assert(!downloadingVersionFile);
        downloadingVersionFile = true;

        WWW versionLoader = new WWW(configurationZipURL + versionFile + "?nocache=" + Environment.TickCount);

        while (!versionLoader.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (versionLoader.isDone && string.IsNullOrEmpty(versionLoader.error))
        {
            versionString = versionLoader.text;
            LogTool.Log("Download version file OK!. Duration: " +  (Time.realtimeSinceStartup - startTime));
        }
        else
            versionString = version.text;

        versionLoader.Dispose();
        //SR.Miniclip.UtilsBindings.ConsoleLog("***** END DOWNLOAD VERSION FILE " + Time.realtimeSinceStartup);

        LogTool.Log("VERSION NUMBER: " + versionString);
        //if (int.Parse(versionString) > lastVersionNumber)
        //{
        //    this.StartCoroutine(this.DownloadZip());
        //}
        //else
            downloadingVersionFile = false;

        PlayerPrefs.SetInt("last_vn", lastVersionNumber);
    }

    IEnumerator DownloadZip()
    {
        //SR.Miniclip.UtilsBindings.ConsoleLog("***** START DOWNLOAD ZIP " + Time.realtimeSinceStartup);

        Asserts.Assert(!downloadingZip);
        downloadingZip = true;

        WWW zipLoader = new WWW(configurationZipURL + configurationZipFile + "?nocache=" + Environment.TickCount);
		LogTool.Log("configurationZipURL---------------------- : "+configurationZipURL+configurationZipFile);
        while (!zipLoader.isDone)
        {
            if (stopDownloading)
            {
                downloadingZip = false;
                stopDownloading = false;

                LogTool.Log("Download configuration STOPPED!");
            }

            yield return new WaitForEndOfFrame();
        }

        if (zipLoader.isDone && string.IsNullOrEmpty(zipLoader.error))
        {
            LogTool.Log("**** PELLE: DOWNLOADING ZIP COMPLETED! Duration: " + (Time.realtimeSinceStartup - startTime));
            using (FileStream fs = new FileStream(Path.Combine(Application.persistentDataPath, configurationZipFile), FileMode.Create))
            {
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(zipLoader.bytes, 0, zipLoader.bytes.Length);
                fs.Flush();
            }

            zipLoader.Dispose();

            if (!downloadingZip)
            {
                LogTool.Log("Download configuration OK!");
                yield break;
            }
            else
                LogTool.Log("Download configuration OK, configurations will be loaded from new zip!");
        }
        else
        {
            LogTool.Log("Download configuration error:\n" + zipLoader.error);

            zipLoader.Dispose();
            downloadingZip = false;
            stopDownloading = false;

            yield break;
        }

        if (!dataLoaded && !stopDownloading)
            this.TryLoadingXMLsFromZip();

        downloadingZip = false;
        stopDownloading = false;
		//downloadingVersionFile = false;
		//SR.Miniclip.UtilsBindings.ConsoleLog("***** END DOWNLOAD ZIP " + Time.realtimeSinceStartup);

    }
    #endregion

    #region Protected methods
#if UNITY_EDITOR
    protected void SaveConfigXMLToZip()
    {
        using (ZipFile zipFile = new ZipFile(Encoding.UTF8))
        {
            zipFile.Password = configurationZipPwd;
            zipFile.AddEntry(configurationFile, configuration.bytes);
            zipFile.AddEntry(localizationFile, localization.bytes);
            string zipPath = Path.Combine(Application.persistentDataPath, configurationZipFile);
            LogTool.Log("Saving configuration in \"" + zipPath + "\"");
            zipFile.Save(zipPath);
        }
    }
#endif
	//读取配置versionnumber.txt 配置文件
	public static void ReadTxt()
	{
		StreamReader sr = new StreamReader(versionFile,Encoding.Default);
		string line;
		while ((line = sr.ReadLine()) != null) 
		{
			string[] str = line.Split(new string[] {"="}, StringSplitOptions.None);
			lstr.Add(str[1]);
		}		
	}
	public List<string> GetVersionDetails()
	{
		List<string> ls = new List<string> ();
		
		StreamReader sr = new StreamReader (new MemoryStream (version.bytes), Encoding.Default);
		string line;
		while ((line = sr.ReadLine()) != null) {
			string[] str = line.Split (new string[] { "=" }, StringSplitOptions.None);
			ls.Add (str [1]);
		}
		return ls;
	}
	//获取版本信息
	public static List<string> GetVersion()
	{
		return lstr;
	}
    protected void TryLoadingXMLsFromZip()
    {
        string zipPath = Path.Combine(Application.persistentDataPath, configurationZipFile);
        if (!File.Exists(zipPath))
        {
            LogTool.Log("Configuration not found!");
            this.ParseConfigXML(configuration.text, false);
            this.ParseLocalizationXML(localization.text, false);
            return;
        }

        using (ZipFile zipFile = new ZipFile(zipPath, Encoding.UTF8))
        {
            zipFile.Password = configurationZipPwd;

            ZipEntry xmlConfEntry   = zipFile[configurationFile],
                     xmlLocaleEntry = zipFile[localizationFile];
            if (null == xmlConfEntry || null == xmlLocaleEntry)
            {
                LogTool.Log("Downloaded configuration INVALID!");
                this.ParseConfigXML(configuration.text, false);
                this.ParseLocalizationXML(localization.text, false);
                return;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                xmlConfEntry.Extract(ms);

                string xmlText = Encoding.UTF8.GetString(ms.GetBuffer(), 0, ms.GetBuffer().Length);
                this.ParseConfigXML(xmlText, true);

                ms.Seek(0, SeekOrigin.Begin);
                xmlLocaleEntry.Extract(ms);

                xmlText = Encoding.UTF8.GetString(ms.GetBuffer(), 0, ms.GetBuffer().Length);
                this.ParseLocalizationXML(xmlText, true);
            }
        }

        //PushNotificationsManager.Instance.Init();
    }

    protected void ParseCarData(XMLNode node)
    {
        Asserts.Assert(node.tagName.Equals("car"));

        SRGameplay.CarId carId = (SRGameplay.CarId)System.Enum.Parse( typeof( SRGameplay.CarId ), node.GetAttributeAsString("id") );

        PlayerExternalData tempData = new PlayerExternalData();
        tempData.minAcceleration = int.Parse(node.GetAttributeAsString("min_acceleration"), CultureInfo.InvariantCulture);
        tempData.maxAcceleration = int.Parse(node.GetAttributeAsString("max_acceleration"), CultureInfo.InvariantCulture);
        tempData.minMaxSpeed = int.Parse(node.GetAttributeAsString("min_speed"), CultureInfo.InvariantCulture);
        tempData.maxMaxSpeed = int.Parse(node.GetAttributeAsString("max_speed"), CultureInfo.InvariantCulture);
        tempData.minResistance = int.Parse(node.GetAttributeAsString("min_resistance"), CultureInfo.InvariantCulture);
        tempData.maxResistance = int.Parse(node.GetAttributeAsString("max_resistance"), CultureInfo.InvariantCulture);
        tempData.minTurboSpeed = int.Parse(node.GetAttributeAsString("min_turbospeed"), CultureInfo.InvariantCulture);
        tempData.maxTurboSpeed = int.Parse(node.GetAttributeAsString("max_turbospeed"), CultureInfo.InvariantCulture);
        tempData.buyCost = int.Parse(node.GetAttributeAsString("buyCost"), CultureInfo.InvariantCulture);
        tempData.upgradeCurrency = PriceData.CurrencyType.FirstCurrency;
        tempData.buyCurrency = node.GetAttributeAsString("currencyBuyType")=="coins" ? PriceData.CurrencyType.FirstCurrency:PriceData.CurrencyType.SecondCurrency;
        tempData.locked = tempData.buyCost > 0;
        tempData.lockedByDaily = node.GetAttributeAsInt("lockedByDaily", 0); //tempData.buyCost < 0 ? 1 : 0; 
        tempData.unlockAtLevel = node.GetAttributeAsInt("unlock_xp_level", -1);

        tempData.alternativeCost = int.Parse(node.GetAttributeAsString("alternativeCost", "-1"), CultureInfo.InvariantCulture);
        tempData.alternativeBuyCurrency = node.GetAttributeAsString("alternativeCurrencyType", "coins") == "coins" ? PriceData.CurrencyType.FirstCurrency : PriceData.CurrencyType.SecondCurrency;

        carsData.Add(carId, tempData);
        //LogTool.Log("** LOADED DATA FORM CAR: " + carId + " " + tempData.buyCost);
    }

    protected void ParseShopPrices(XMLNode node, string shopType, bool percVisible)
    {
        int quantity = int.Parse(node.GetAttributeAsString("quantity"), CultureInfo.InvariantCulture),
            price = int.Parse(node.GetAttributeAsString("price"), CultureInfo.InvariantCulture);

        //LogTool.Log("** --->: " + shopType+" "+quantity + " " + price);

        int[] tempData = {quantity, price};
        switch (shopType)
        {
            case "coins":
                showShopPerc[2] = percVisible;
                shopCoinsData.Add(tempData);
                break;
            case "diamonds":
                showShopPerc[1] = percVisible;
                shopDiamondsData.Add(tempData);
                break;
            case "fuel":
                showShopPerc[0] = percVisible;
                shopFuelData.Add(tempData);
                break;
        }
    }


    protected void ParseBoostsPrices(XMLNode node)
    {

        SRBooster.BoosterType type = (SRBooster.BoosterType)System.Enum.Parse(typeof(SRBooster.BoosterType), node.tagName);
        float price = -1.0f;
        if (node.HasAttribute("value"))
            price = node.GetAttributeAsFloat("value");
        else
            price = node.GetAttributeAsFloat("multiplier");

        boostsData.Add(type, price);
    }

	protected void ParseGiftData (XMLNode node) {
		UIGiftItem.GiftInfo gi = new UIGiftItem.GiftInfo();
		gi.name = node.GetAttributeAsString("name");
		gi.showName = node.GetAttributeAsString("showName");
		gi.des = node.GetAttributeAsString("des");
		gi.rewardList = new List<UIGiftItem.GiftRewardItem>();
		string[] types = node.GetAttributeAsString("type").Split(',');
		string[] values = node.GetAttributeAsString("value").Split(',');
		for ( int i = 0 ; i < types.Length ; i++ ) {
			UIGiftItem.GiftRewardItem gri = new UIGiftItem.GiftRewardItem();
			gri.type = (UIGiftItem.GiftRewardType)Enum.Parse(typeof( UIGiftItem.GiftRewardType ), types[i],true);
			gri.value = values[i];
			gi.rewardList.Add(gri);
		}
		giftInfos.Add(gi);
	}
    protected void ParseSpecialCarsData(XMLNode node)
    {
        //int locked = int.Parse(node.GetAttributeAsString("price"), CultureInfo.InvariantCulture) > 0 ? 1 : 0;
        int locked = int.Parse(node.GetAttributeAsString("unlock_xp_level"), CultureInfo.InvariantCulture) > 0 ? 1 : 0;
        specialCarsLocks.Add(locked);
        specialCarsXPLevels.Add(int.Parse(node.GetAttributeAsString("unlock_xp_level"), CultureInfo.InvariantCulture));
        specialCarsPrices.Add(int.Parse(node.GetAttributeAsString("price"), CultureInfo.InvariantCulture));
        alternativeSpecialCarsPrices.Add(int.Parse(node.GetAttributeAsString("alternativeCost", "-1"), CultureInfo.InvariantCulture));
        specialCarsUpgrades.Add( node.GetAttributeAsFloat("multiplier") );
    }
    
    protected void ParseSaveMeData(XMLNode node)
    {
        UISaveMePopup.SaveMeButtonType type = (UISaveMePopup.SaveMeButtonType)System.Enum.Parse(typeof(UISaveMePopup.SaveMeButtonType), node.tagName);
#if UNITY_WEBPLAYER
        float[] data = { node.GetAttributeAsFloat("multiplier"), node.GetAttributeAsFloat("upgrade"), node.GetAttributeAsFloat("seconds") };  //float[] data = { node.GetAttributeAsFloat("cost"), node.GetAttributeAsFloat("upgrade"), node.GetAttributeAsFloat("seconds") };  //dani float[] data = { node.GetAttributeAsFloat("first_use"), node.GetAttributeAsFloat("upgrade"), node.GetAttributeAsFloat("seconds") };
#else
        float[] data = { node.GetAttributeAsFloat("multiplier"), node.GetAttributeAsFloat("upgrade"), node.GetAttributeAsFloat("seconds") };
#endif

        saveMeData.Add(type, data);
    }

    protected void ParseWheelData(XMLNode node, string wheelItemType, WheelItem.Level level)
    {
        WheelItem item = new WheelItem((UIWheelItem.WheelItem)System.Enum.Parse(typeof(UIWheelItem.WheelItem), node.GetAttributeAsString("type")), int.Parse(node.GetAttributeAsString("quantity"), CultureInfo.InvariantCulture), level);
        wheelData[wheelItemType].Add(item);
    }

    protected void ParseDailyBonusData(XMLNode node)
    {

		List<SRDailyBonusManager.DailyBonus> bonusTypes = new List<SRDailyBonusManager.DailyBonus>();
		List<int> quantitys = new List<int>();
		string [] type2  = node.GetAttributeAsString("type").Split(';');

//		string [] type2 = type.Split(';');
		String [] qy =  node.GetAttributeAsString("quantity").Split(';');
		SRDailyBonusManager.isIndex7day = EncryptedPlayerPrefs.GetInt("isIndex7day",0);
		if(SRDailyBonusManager.isIndex7day>0){//表示头七天已经领取过  读取副表 删除车辆
			foreach (XMLNode reward in node.children) {
				type2  = reward.GetAttributeAsString("type").Split(';');
				qy =  reward.GetAttributeAsString("quantity").Split(';');
			}
		}

//		LogTool.Log("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa :"+node.tagName+":"+node.children.Count);


//		LogTool.Log("aaaaaaaaaaa ------------------------------------------  : "+node.GetAttributeAsString("type")+":bb:"+node.GetAttributeAsString("quantity"));
		for(int i=0;i<type2.Length;i++){
			bonusTypes.Add((SRDailyBonusManager.DailyBonus)System.Enum.Parse(typeof(SRDailyBonusManager.DailyBonus),type2[i]));
			quantitys.Add(int.Parse(qy[i]));
		}


//        SRDailyBonusManager.DailyBonus type = (SRDailyBonusManager.DailyBonus)System.Enum.Parse(typeof(SRDailyBonusManager.DailyBonus), node.GetAttributeAsString("type"));
//        int quantity =  node.GetAttributeAsInt("quantity");
//        List<SRDailyBonusManager.DailyBonusData> misteryRewards = null;
//		misteryRewards = new List<SRDailyBonusManager.DailyBonusData>();
//		string bonusId = node.GetAttributeAsInt("bonusId");
//		string type = node.GetAttributeAsString("type");
//        if (type == SRDailyBonusManager.DailyBonus.Mistery) //新版本去除随机相
//        {
//            misteryRewards = new List<SRDailyBonusManager.DailyBonusData>();
//            foreach (XMLNode reward in node.children)
//            {
//                SRDailyBonusManager.DailyBonus subType = (SRDailyBonusManager.DailyBonus)System.Enum.Parse(typeof(SRDailyBonusManager.DailyBonus), reward.GetAttributeAsString("type"));
//                int subQuantity = reward.GetAttributeAsInt("quantity");
//                SRDailyBonusManager.DailyBonusData rewardItem = new SRDailyBonusManager.DailyBonusData(subType, subQuantity);
//                misteryRewards.Add(rewardItem);
//            }
//        }

//        SRDailyBonusManager.DailyBonusData item = new SRDailyBonusManager.DailyBonusData(type, quantity, misteryRewards);
		SRDailyBonusManager.DailyBonusData item = new SRDailyBonusManager.DailyBonusData(bonusTypes, quantitys, null);
//		LogTool.Log("aaaaaaaaaaaaaa ------------------------------:"+node.GetAttributeAsInt("bonusId") + " bbbbbb : "+node.GetAttributeAsString("type"));
        dailyBonusDatas.Add(node.GetAttributeAsInt("bonusId"), item);
    }

    protected void ParseInappPriceData(XMLNode node)
    {
        string productId = node.GetAttributeAsString("product_id");
		string chargeCode = node.GetAttributeAsString(PaymentTool.DianXin,string.Empty);
        float priceInDollars = node.GetAttributeAsFloat("price");

        inappDollarPricesByProductId.Add(productId, priceInDollars);
		inappChargeCodeByProductId.Add(productId , chargeCode);
    }

    protected void ParseOffersData(XMLNode offerType)
    {
        UICurrencyPopup.ShopOffers currOffer = new UICurrencyPopup.ShopOffers();
        currOffer.type = (UICurrencyPopup.ShopPopupTypes)System.Enum.Parse(typeof(UICurrencyPopup.ShopPopupTypes), offerType.tagName);
        foreach (XMLNode node in offerType.children)
        {
            if (node.tagName == "popular")
                currOffer.specialOffers.Add(int.Parse(node.GetAttributeAsString("id"), CultureInfo.InvariantCulture));
            else if (node.tagName == "best")
                currOffer.bestOffers.Add(int.Parse(node.GetAttributeAsString("id"), CultureInfo.InvariantCulture));
        }
        offersData.Add(currOffer.type, currOffer);
    }

    protected void ParsePhysicParameterData(XMLNode node, string type)
    {
        float sideAcceleration,
              limitMaxSpeed;

        if (type.Equals("opponent"))
        {
            float acceleration = node.GetAttributeAsFloat("acceleration");
            float maxSpeed = node.GetAttributeAsFloat("max_speed");
            float maxSpeedWrongDirection = node.GetAttributeAsFloat("max_speed_wrong_direction");
            sideAcceleration = node.GetAttributeAsFloat("side_acceleration");

            OpponentKinematics.PhysicParameters data = new OpponentKinematics.PhysicParameters(acceleration, maxSpeed, maxSpeedWrongDirection, sideAcceleration);
            opponentPhysicParameters.Add(type, data);
        }
        else
        {
            float[] acceleration = ParseArrayData(node, "acceleration");
            float[] maxSpeed = ParseArrayData(node, "max_speed");
            float[] resistance = ParseArrayData(node, "resistance");
            float[] turboSpeed = ParseArrayData(node, "turbo_speed");
            sideAcceleration = node.GetAttributeAsFloat("side_acceleration");
            limitMaxSpeed = node.GetAttributeAsFloat("limitMaxSpeed");

            if (node.HasAttribute("speedReductionPerHit"))
                speedReductionPerHit = node.GetAttributeAsFloat("speedReductionPerHit");

            PlayerKinematics.PhysicParameters data = new PlayerKinematics.PhysicParameters(acceleration, maxSpeed, resistance, turboSpeed, sideAcceleration, limitMaxSpeed);
            playerPhysicParameters.Add(type, data);
        }
    }
    
    //car mission data
    protected void ParseCarMissionData(XMLNode node, string type)
    {
        if (type.Equals("mission_data"))
        {
            metersToReachFirstTimeData = ParseArrayIntData(node, "metersToReachFirstTimeData");
            destroyTrafficFirstTimeData = ParseArrayIntData(node, "destroyTrafficFirstTimeData");
            reachComboFirstTimeData = ParseArrayIntData(node, "reachComboFirstTimeData");
            jumpQuantityFirstTimeData = ParseArrayIntData(node, "jumpQuantityFirstTimeData");
            collectBigCoinsFirstTimeData = ParseArrayIntData(node, "collectBigCoinsFirstTimeData");
            metersToReachData = ParseArrayIntData(node, "metersToReachData");
            jumpQuantityData = ParseArrayIntData(node, "jumpQuantityData");
            onAirData = ParseArrayIntData(node, "onAirData");
            passCheckpointData = ParseArrayIntData(node, "passCheckpointData");
            collectCoinsData = ParseArrayIntData(node, "collectCoinsData");
            collectBigCoinsData = ParseArrayIntData(node, "collectBigCoinsData");
            fastLaneMetersData = ParseArrayIntData(node, "fastLaneMetersData");
            centralLaneMetersData = ParseArrayIntData(node, "centralLaneMetersData");
            leftLaneMetersData = ParseArrayIntData(node, "leftLaneMetersData");
            rightLaneMetersData = ParseArrayIntData(node, "rightLaneMetersData");
            avoidBarriersData = ParseArrayIntData(node, "avoidBarriersData");
            destroyTrafficData = ParseArrayIntData(node, "destroyTrafficData");
            destroyTrafficInTurboData = ParseArrayIntData(node, "destroyTrafficInTurboData");
            destroyTrafficInTurboWrongLaneData = ParseArrayIntData(node, "destroyTrafficInTurboWrongLaneData");
            destroyPoliceQuantityData = ParseArrayIntData(node, "destroyPoliceQuantityData");
            runWithBigfootForData = ParseArrayIntData(node, "runWithBigfootForData");
            runWithTankForData = ParseArrayIntData(node, "runWithTankForData");
            runWithFiretruckForData = ParseArrayIntData(node, "runWithFiretruckForData");
            runWithUFOForData = ParseArrayIntData(node, "runWithUFOForData");
            runWithPlaneForData = ParseArrayIntData(node, "runWithPlaneForData");
            destroyCarsDataWSpecial = ParseArrayIntData(node, "destroyCarsDataWSpecial");
            destroyCarsWithSpecialInWrongLaneData = ParseArrayIntData(node, "destroyCarsWithSpecialInWrongLaneData");
            reachComboData = ParseArrayIntData(node, "reachComboData");

        }
    }

    protected void ParseCarsUpgradesMultipliersData(XMLNode node)
    {
        CarsUpgradesMultipliers data = new CarsUpgradesMultipliers();
        data.acceleration_mult = node.GetAttributeAsFloat("acceleration");
        data.maxSpeed_mult = node.GetAttributeAsFloat("max_speed");
        data.resistance_mult = node.GetAttributeAsFloat("resistance");
        data.turbo_speed_mult = node.GetAttributeAsFloat("turbo_speed");
        carsUpgradesMultipliers.Add(node.GetAttributeAsString("id"), data);

        if (node.HasAttribute("discount"))
            carsUpgradesDiscount.Add(node.GetAttributeAsString("id"), node.GetAttributeAsFloat("discount"));
    }

    protected float[] ParseArrayData(XMLNode node, string type)
    {
        List<float> tempList = new List<float>();
        string data = node.GetAttributeAsString(type);
        string[] array = data.Split(',');
        for (int i = 0; i < array.Length; ++i){
            tempList.Add(float.Parse(array[i], CultureInfo.InvariantCulture));
		}
        return tempList.ToArray();
    }

    //int
    protected int[] ParseArrayIntData(XMLNode node, string type)
    {
        List<int> tempList = new List<int>();
        string data = node.GetAttributeAsString(type);
        string[] array = data.Split(',');
        for (int i = 0; i < array.Length; ++i)
            tempList.Add(int.Parse(array[i], CultureInfo.InvariantCulture));

        return tempList.ToArray();
    }

    protected bool[] ParseBoolArrayData(XMLNode node, string type)
    {
        List<bool> tempList = new List<bool>();
        string data = node.GetAttributeAsString(type);
        char[] charSeparators = new char[] { ',' };
        string[] array = data.Split(charSeparators);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = array[i].Trim();
            tempList.Add(array[i] == "1");
        }

        return tempList.ToArray();
    }

    protected PriceData.CurrencyType[] ParseCurrencyArrayData(XMLNode node, string type)
    {
        List<PriceData.CurrencyType> tempList = new List<PriceData.CurrencyType>();
        string data = node.GetAttributeAsString(type);
        char[] charSeparators = new char[] { ',' };
        string[] array = data.Split(charSeparators);
        for (int i = 0; i < array.Length; ++i)
        {
            array[i] = array[i].Trim();
            tempList.Add(array[i] == "Coin" ? PriceData.CurrencyType.FirstCurrency : PriceData.CurrencyType.SecondCurrency);
        }

        return tempList.ToArray();
    }

    protected void ParseVersionNumberFile(string dataText)
    {
        /*StreamReader inp_stm = new StreamReader(dataText);
        while (!inp_stm.EndOfStream)
        {
            string inp_ln = inp_stm.ReadLine();
            // Do Something with the input. 
            LogTool.Log("VERSION NUMBER: " + inp_ln);
        }
        inp_stm.Close(); */
    }

    protected void ParseConfigXML(string dataText, bool checkVersion)
    {
        Asserts.Assert(!dataLoaded && !dataParsing);
        dataParsing = true;

        XMLReader xmlReader = new XMLReader();
        XMLNode root = xmlReader.read(dataText).children[0] as XMLNode;

        if (checkVersion)
        {
            int version = root.GetAttributeAsInt("version", 0);

            XMLNode otherRoot = xmlReader.read(configuration.text).children[0] as XMLNode;
            int otherVersion = otherRoot.GetAttributeAsInt("version", 0);

            if (otherVersion >= version)
            {
                if (otherVersion > version)
                    omniata.Log.MyWarning("Configuration out of date!");
                root = otherRoot;
#if SBS_DEBUG
                LogTool.Log("Switching to builtin configuration");
#endif
            }
        }

        foreach (XMLNode record in root.children)
        {
            //load stats upgrades....
            if (record.tagName.Equals("cars"))
            {
                foreach (XMLNode node in record.children)
                    ParseCarData(node);
            }

            //load shop prices....
            if (record.tagName.Equals("shop"))
            {
                foreach (XMLNode shopType in record.children)
                {
                    bool percVisible = shopType.GetAttributeAsInt("show_perc") == 1;
                    foreach (XMLNode node in shopType.children)
                        ParseShopPrices(node, shopType.tagName, percVisible);
                }
            }

			//load boost prices....ParseGiftData
            if (record.tagName.Equals("boosts"))
            {
                foreach (XMLNode node in record.children)
                    ParseBoostsPrices(node);
            }
			//load Gift....
			if ( record.tagName.Equals("Gifts") ) {
				foreach ( XMLNode node in record.children )
					ParseGiftData(node);
			}
            //special cars data....
            if (record.tagName.Equals("special_cars"))
            {
                foreach (XMLNode node in record.children)
                {
                    if(node.tagName.Equals("General"))
                    {
                        specialCarsData = new Dictionary<string, float>();
                        specialCarsData.Add("duration", node.GetAttributeAsFloat("duration"));
                        specialCarsData.Add("delta_time", node.GetAttributeAsFloat("delta_time"));
                        specialCarsData.Add("boost_duration", node.GetAttributeAsFloat("boost_duration"));
                        if (node.HasAttribute("force_spawn_counter"))
                            specialCarsData.Add("force_spawn_counter", node.GetAttributeAsFloat("force_spawn_counter"));
                        else
                            specialCarsData.Add("force_spawn_counter", 3.0f);
                    }
                    else
                        ParseSpecialCarsData(node);
                }
            }
         
            //save me data....
            if (record.tagName.Equals("save_me"))
            {
                foreach (XMLNode node in record.children)
                {
                    if(node.tagName.Equals("Duration"))
                        saveMePopupDuration = node.GetAttributeAsFloat("seconds");
                    else if(node.tagName.Equals("Cost"))
                        saveMePopupCost = ParseArrayData(node, "diamonds");
                    else
                        ParseSaveMeData(node);
                }
            }

            //wheel data....
            if (record.tagName.Equals("wheel"))
            {
                wheelData.Add("cheap", new List<WheelItem>());
                wheelData.Add("medium", new List<WheelItem>());
                wheelData.Add("rich", new List<WheelItem>());
                wheelDataPercentages = new List<float>();
                wheelDataPercentages.Add(-1);
                wheelDataPercentages.Add(-1);
                wheelDataPercentages.Add(-1);
                wheelDataQuantities = new List<int>();
                wheelDataQuantities.Add(-1);
                wheelDataQuantities.Add(-1);
                wheelDataQuantities.Add(-1);

                foreach (XMLNode shopType in record.children)
                {
                    if (shopType.tagName.Equals("extra_spin"))
                    {
                        extraSpinQuantity = shopType.GetAttributeAsInt("quantity");
                        extraSpinCost = shopType.GetAttributeAsInt("cost");
                    }
                    else if (shopType.tagName.Equals("near_win"))
                    {
                        nearMissPercentage = shopType.GetAttributeAsFloat("perc");
                    }
                    else
                    {
                        WheelItem.Level level = (WheelItem.Level)System.Enum.Parse(typeof(WheelItem.Level), shopType.tagName);
                        wheelDataPercentages[(int)level] = shopType.GetAttributeAsFloat("perc");
                        wheelDataQuantities[(int)level] = shopType.GetAttributeAsInt("quantity");

                        foreach (XMLNode node in shopType.children)
                            ParseWheelData(node, shopType.tagName, level);
                    }
                }
            }

            //daily bonus data....
            if (record.tagName.Equals("daily_bonus"))
            {
                foreach (XMLNode dailyBonus in record.children)
                {
                    if (dailyBonus.tagName.Equals("rewards"))
                    {
                        foreach (XMLNode node in dailyBonus.children)
                        {
                            SRDailyBonusManager.DailyBonus type = (SRDailyBonusManager.DailyBonus)System.Enum.Parse(typeof(SRDailyBonusManager.DailyBonus), node.GetAttributeAsString("type"));
                            int quantity = node.GetAttributeAsInt("quantity");
                            dailyBonusMisteryRewards.Add(new SRDailyBonusManager.DailyBonusData(type, quantity));
                        }
                    }
                    /*else if (dailyBonus.tagName.Equals("mistery_item"))
                    {
                        string indexes = dailyBonus.GetAttributeAsString("days");
                        string[] days = indexes.Split(',');
                        for (int i = 0; i < days.Length; ++i)
                            dailyBonusMisteryItemDays.Add(int.Parse(days[i]));
                    }*/
                    else if (dailyBonus.tagName.Equals("recover_streak"))
                    {
                        recoverStreakCost = dailyBonus.GetAttributeAsInt("cost");
                    }
                    else
                    {
                        ParseDailyBonusData(dailyBonus);
                    }
                }
            }

            // inapp prices...
            if (record.tagName.Equals("inapps"))
            {
                foreach (XMLNode node in record.children)
                    ParseInappPriceData(node);
            }

            // special offers...
            if (record.tagName.Equals("specials"))
            {
                foreach (XMLNode offerType in record.children)
                {
                    ParseOffersData(offerType);
                }
            }
            
            // advertising...
            if (record.tagName.Equals("advertising"))
            {
                foreach (XMLNode node in record.children)
                {
                    if (node.tagName.Equals("chartboost_is_enabled"))
                        enableChartboost = node.GetAttributeAsString("value", "false").Equals("true") ? true : false;
                    else if (node.tagName.Equals("chartboost_limit_per_day"))
                        chartboostLimitPerDay = node.GetAttributeAsInt("value");
                    /*else if (node.tagName.Equals("boost_video_limit_per_day"))
                        boostVideoAdLimitPerDay = node.GetAttributeAsInt("value");*/
                }
            }

            // interstitials...
            if (record.tagName.Equals("interstitials"))
            {
                bool inter_enabled = record.GetAttributeAsBool("interstitials_are_enabled", false);
                int inter_sessionsToSkip = record.GetAttributeAsInt("num_sessions_to_skip", 2);
                int inter_maxPerSession = record.GetAttributeAsInt("max_per_session", 5);
                int inter_maxPerDay = record.GetAttributeAsInt("max_per_day", 5);
               // OnTheRunInterstitialsManager.Instance.SetConfigParameters(inter_enabled, inter_sessionsToSkip, inter_maxPerSession, inter_maxPerDay);

                foreach (XMLNode node in record.children)
                {
                    string triggerPointId = node.GetAttributeAsString("id", string.Empty);
                    bool triggerPointIsEnabled = node.GetAttributeAsBool("enabled", false);

                   // OnTheRunInterstitialsManager.Instance.SetTriggerPointEnabled(triggerPointId, triggerPointIsEnabled);
                }

              //  OnTheRunInterstitialsManager.Instance.OnConfigParametersParsed();
            }

            // video ads
            if (record.tagName.Equals("video_ads"))
            {
                bool videoads_enabled = record.GetAttributeAsBool("video_ads_are_enabled", false);
                int videoads_coinsReward = record.GetAttributeAsInt("coins_reward_for_video", 100);
                int videoads_maxVideosPerDay = record.GetAttributeAsInt("max_videos_per_day", 5);
                int videoads_maxBoosterVideosPerDay = record.GetAttributeAsInt("max_booster_videos_per_day", 3);
                int videoads_maxFreeFuelVideosPerDay = record.GetAttributeAsInt("max_freefuel_videos_per_day", 3);
                int videoads_maxFreeSaveMeVideosPerDay = record.GetAttributeAsInt("max_freesaveme_videos_per_day", 3);

                SRCoinsService.Instance.SetConfigParameters(videoads_enabled, videoads_coinsReward, videoads_maxVideosPerDay, videoads_maxBoosterVideosPerDay, videoads_maxFreeFuelVideosPerDay, videoads_maxFreeSaveMeVideosPerDay);
            }

            // android video ads...
            if (record.tagName.Equals("android_video_ads"))
            {
                foreach (XMLNode node in record.children)
                {
                    /*if (node.tagName.Equals("coins_reward"))
                        androidVideoCoinsReward = node.GetAttributeAsInt("value", 50);*/
                    if (node.tagName.Equals("flurry_ads_priority"))
                        androidFlurryAdsPriority = node.GetAttributeAsInt("value", 2);
                    if (node.tagName.Equals("adcolony_priority"))
                        androidAdcolonyPriority = node.GetAttributeAsInt("value", 1);
                }
            }
            
            //enemies data....
            if (record.tagName.Equals("police_and_helicopter"))
            {
                foreach (XMLNode node in record.children)
                {
                    if (node.tagName.Equals("general"))
                    {
                        enemiesMinDistanceFromCheckpoint = node.GetAttributeAsInt("min_distance_from_checkpoint");
                    }
                    else if (node.tagName.Equals("police"))
                    {
                        policeSpawnData = new Dictionary<string, float[]>();
                        if (node.HasAttribute("percentage"))
                            policeSpawnPercentage = node.GetAttributeAsFloat("percentage");
                        foreach (XMLNode element in node.children)
                        {
                            float[] data = { element.GetAttributeAsFloat("min_spawn_distance"), element.GetAttributeAsFloat("max_spawn_distance"), element.GetAttributeAsFloat("percentage") };
                            policeSpawnData.Add(element.tagName, data);
                        }
                    }
                    else if (node.tagName.Equals("helicopter"))
                    {
                        helicopterSpawnData = new Dictionary<string, float[]>();
                        foreach (XMLNode element in node.children)
                        {
                            float[] data = { element.GetAttributeAsFloat("min_spawn_distance"), element.GetAttributeAsFloat("max_spawn_distance") };
                            helicopterSpawnData.Add(element.tagName, data);
                        }
                    }
                }
            }

            //obstacles data....
            if (record.tagName.Equals("obstacles"))
            {
                foreach (XMLNode node in record.children)
                {
                    if (node.tagName.Equals("road_works"))
                    {
                        roadWorksData = new Dictionary<string, int>();
                        roadWorksData.Add("min_duration", node.GetAttributeAsInt("min_duration"));
                        roadWorksData.Add("max_duration", node.GetAttributeAsInt("max_duration"));
                        roadWorksData.Add("min_distance", node.GetAttributeAsInt("min_distance"));
                        roadWorksData.Add("max_distance", node.GetAttributeAsInt("max_distance"));
                    }
                    else if (node.tagName.Equals("central_mud"))
                    {
                        centralMudData = new Dictionary<string, int>();
                        centralMudData.Add("min_duration", node.GetAttributeAsInt("min_duration"));
                        centralMudData.Add("max_duration", node.GetAttributeAsInt("max_duration"));
                        centralMudData.Add("min_hole_duration", node.GetAttributeAsInt("min_hole_duration"));
                        centralMudData.Add("max_hole_duration", node.GetAttributeAsInt("max_hole_duration"));
                    }
                }
            }

            //traffic data....
            if (record.tagName.Equals("traffic"))
            {
                trafficData = new Dictionary<string, Dictionary<string, float>[]>();
                foreach (XMLNode node in record.children)
                {
                    ParseTrafficData(node);
                }
            }
            
            //power ups data....
            if (record.tagName.Equals("power_ups"))
            {
                powerUpsData = new Dictionary<string, Dictionary<string, float>>();
                foreach (XMLNode node in record.children)
                {
                    ParsePowerUpsData(node);
                }
            }
            
            //social...
            if (record.tagName.Equals("social"))
            {
                foreach (XMLNode node in record.children)
                {
                    if (node.tagName.Equals("gifts"))
                    {
                        resetGiftAfterDays = node.GetAttributeAsInt("reset_after_days");
                        maxGifts = node.GetAttributeAsInt("max_gifts");
                    }
                    else if (node.tagName.Equals("invites"))
                    {
                        resetInvitesAfterDays = node.GetAttributeAsInt("reset_after_days");
                        maxInvites = node.GetAttributeAsInt("max_invites");
                        diamondsReward = node.GetAttributeAsInt("reward_diamonds");
                    }
                    else if (node.tagName.Equals("fbadvise"))
                    {
                        fbAdviseDurationIngame = node.GetAttributeAsFloat("ingame_advise_duration");
                    }
                }
            }
            
            //rank bar...
            if (record.tagName.Equals("rank_bar"))
            {
                foreach (XMLNode node in record.children)
                {
                    if (node.tagName.Equals("level_up_reward"))
                    {
                        levelUpRewardDictionary = new Dictionary<string, int>();
                        levelUpRewardDictionary.Add("first_time", node.GetAttributeAsInt("first_time"));
                        levelUpRewardDictionary.Add("increase", node.GetAttributeAsInt("increase"));
                        levelUpRewardDictionary.Add("increase_every_levels", node.GetAttributeAsInt("increase_every_levels"));
                    }
                    else if (node.tagName.Equals("consumable_bonuses"))
                    {
                        consumableBonusesDictionary = new Dictionary<string, float>();
                        consumableBonusesDictionary.Add("level_interval", node.GetAttributeAsFloat("level_interval"));
                        consumableBonusesDictionary.Add("gems_quantitiy_init", node.GetAttributeAsFloat("gems_quantitiy_init"));
                        consumableBonusesDictionary.Add("gems_quantitiy_mult", node.GetAttributeAsFloat("gems_quantitiy_mult"));
                        consumableBonusesDictionary.Add("coins_quantitiy_init", node.GetAttributeAsFloat("coins_quantitiy_init"));
                        consumableBonusesDictionary.Add("coins_quantitiy_mult", node.GetAttributeAsFloat("coins_quantitiy_mult"));
                        consumableBonusesDictionary.Add("fuel_quantitiy_init", node.GetAttributeAsFloat("fuel_quantitiy_init"));
                        consumableBonusesDictionary.Add("fuel_quantitiy_mult", node.GetAttributeAsFloat("fuel_quantitiy_mult"));
                        consumableBonusesDictionary.Add("spins_quantitiy_init", node.GetAttributeAsFloat("spins_quantitiy_init"));
                        consumableBonusesDictionary.Add("spins_quantitiy_mult", node.GetAttributeAsFloat("spins_quantitiy_mult"));
                    }
                }
            }
            
            //rank bar...
            if (record.tagName.Equals("level_bonuses"))
            {
                levelBonusesList = new List<SRConsumableBonusManager.ConsumableBonus>();

                foreach (XMLNode node in record.children)
                {
                    SRConsumableBonusManager.ConsumableBonus tmpBonus = new SRConsumableBonusManager.ConsumableBonus();
                    tmpBonus.type = (SRConsumableBonusManager.ConsumableType)System.Enum.Parse(typeof(SRConsumableBonusManager.ConsumableType), node.GetAttributeAsString("type"));
                    tmpBonus.level = node.GetAttributeAsInt("level");
                    tmpBonus.quantity = node.GetAttributeAsInt("quantity");
                    levelBonusesList.Add(tmpBonus);
                }
            }

            if (record.tagName.Equals("level_bonuses_alternative"))
            {
                levelBonusesAlternativedictionary = new Dictionary<int, SRConsumableBonusManager.ConsumableBonus>();

                foreach (XMLNode node in record.children)
                {
                    SRConsumableBonusManager.ConsumableBonus tmpBonus = new SRConsumableBonusManager.ConsumableBonus();
                    tmpBonus.type = (SRConsumableBonusManager.ConsumableType)System.Enum.Parse(typeof(SRConsumableBonusManager.ConsumableType), node.GetAttributeAsString("type"));
                    tmpBonus.level = node.GetAttributeAsInt("level");
                    tmpBonus.quantity = node.GetAttributeAsInt("quantity");
                    levelBonusesAlternativedictionary.Add(tmpBonus.level, tmpBonus);
                }
            }

            //general...
            if (record.tagName.Equals("general"))
            {
                foreach (XMLNode node in record.children)
                {
                    if (node.tagName.Equals("tiers_count"))
                    {
                        tiersCount = node.GetAttributeAsInt("value", 4);
                    }
                    else if (node.tagName.Equals("max_player_level"))
                    {
                        maxPlayerLevel = node.GetAttributeAsInt("value", -1);
                    }
                    else if (node.tagName.Equals("mission_related_modifications"))
                    {
                        missionsModificationsDictionary = new Dictionary<string, float>();
                        missionsModificationsDictionary.Add("helicopter_distance_multiplier", node.GetAttributeAsFloat("helicopter_distance_multiplier"));
                        missionsModificationsDictionary.Add("police_distance_multiplier", node.GetAttributeAsFloat("police_distance_multiplier"));
                    }
                    else if (node.tagName.Equals("mission_reward"))
                    {
                        missionsRewardDictionary = new Dictionary<string, float>();
                        missionsRewardDictionary.Add("base_value", node.GetAttributeAsFloat("base_value"));
                        missionsRewardDictionary.Add("increase_per_level", node.GetAttributeAsFloat("increase_per_level"));
                        missionsRewardDictionary.Add("meters_multiplier", node.GetAttributeAsFloat("meters_multiplier"));
                        missionsRewardDictionary.Add("diamonds", node.GetAttributeAsFloat("diamonds"));
                        missionsRewardDictionary.Add("diamond_perc", node.GetAttributeAsFloat("diamond_perc"));
                    }
                    else if (node.tagName.Equals("skip_mission_popup"))
                    {
                        skipMissionPopupDictionary = new Dictionary<string, int>();
                        skipMissionPopupDictionary.Add("first_time", node.GetAttributeAsInt("first_time"));
                        skipMissionPopupDictionary.Add("shown_every", node.GetAttributeAsInt("shown_every"));
                        skipMissionPopupDictionary.Add("diamonds_to_skip_multiplier", node.GetAttributeAsInt("diamonds_to_skip_multiplier"));
                    }
                    else if (node.tagName.Equals("percentage_force_special_vehicle"))
                    {
                        percentageForceSpecialVehicle = node.GetAttributeAsFloat("value");
                    }
                    else if (node.tagName.Equals("initial_currencies"))
                    {
                        initDiamonds = node.GetAttributeAsInt("diamonds");
                        initCoins = node.GetAttributeAsInt("coins");
                    }
                    else if (node.tagName.Equals("special_cameras"))
                    {
                        foreach (XMLNode cameraNode in node.children)
                        {
                            if (cameraNode.tagName.Equals("slideshow"))
                            {
                                slideshowData[0] = cameraNode.GetAttributeAsFloat("active");
                                slideshowData[1] = cameraNode.GetAttributeAsFloat("enter_speed");
                                slideshowData[2] = cameraNode.GetAttributeAsFloat("exit_speed");
                            }
                            else if (cameraNode.tagName.Equals("turbo"))
                            {
                                turboCameraData[0] = cameraNode.GetAttributeAsFloat("active");
                                turboCameraData[1] = cameraNode.GetAttributeAsFloat("enter_speed");
                                turboCameraData[2] = cameraNode.GetAttributeAsFloat("exit_speed");
                            }
                            else if (cameraNode.tagName.Equals("final"))
                            {
                                finalCameraData[0] = cameraNode.GetAttributeAsFloat("active");
                                finalCameraData[1] = cameraNode.GetAttributeAsFloat("enter_speed");
                                finalCameraData[2] = cameraNode.GetAttributeAsFloat("exit_speed");
                            }
                        }
                    }
                    else if (node.tagName.Equals("collision_protection_time"))
                    {
                        protectFromCollisionTime = node.GetAttributeAsFloat("value");
                    }
                    else if (node.tagName.Equals("visible_fuel"))
                    {
                        fuelVisibleInBar = node.GetAttributeAsInt("value");
                    }
                    else if (node.tagName.Equals("time_to_wait_to_trucks_advise"))
                    {
                        timeToWaitToShowTrucksAdvise = node.GetAttributeAsInt("value");
                    }
                    else if (node.tagName.Equals("time_to_wait_to_reach_rank"))
                    {
                        timeToWaitToShowReachRank = node.GetAttributeAsInt("value");
                    }
                    else if (node.tagName.Equals("fuel_freeze"))
                    {
                        fuelFreezeDuration = node.GetAttributeAsFloat("duration_hours");
                        fuelFreezeNotificationAdvance = node.GetAttributeAsFloat("notification_advance_seconds");
                    }
                    else if (node.tagName.Equals("fuel_refill"))
                    {
                        fuelSingleRefillTime = node.GetAttributeAsFloat("duration_minutes");
                    }
                    else if (node.tagName.Equals("slipstreamReward"))
                    {
                        slipstreamBase = node.GetAttributeAsFloat("base");
                        slipstreamStep = node.GetAttributeAsFloat("step");
                        slipstreamExponent = node.GetAttributeAsFloat("exponent");
                    }
                    else if (node.tagName.Equals("opponent_crash"))
                    {
                        crashNormal = node.GetAttributeAsInt("normal");
                        crashSpecialVehicle = node.GetAttributeAsInt("special");
                    }
                    else if (node.tagName.Equals("helicopter_cone"))
                    {
                        helicopterCoins = node.GetAttributeAsFloat("coins_for_delta");
                        helicopterCoinsDeltaTime = node.GetAttributeAsFloat("delta_time");
                    }
                    else if (node.tagName.Equals("missions_xp"))
                    {
                        missionsXPIncrement = node.GetAttributeAsInt("increment");
                        /*missionRewardByDifficulty = new Dictionary<OnTheRunMissions.MissionDifficulty, float>();
                        missionRewardByDifficulty.Add(OnTheRunMissions.MissionDifficulty.Easy, node.GetAttributeAsFloat("easy"));
                        missionRewardByDifficulty.Add(OnTheRunMissions.MissionDifficulty.Medium, node.GetAttributeAsFloat("medium"));
                        missionRewardByDifficulty.Add(OnTheRunMissions.MissionDifficulty.Hard, node.GetAttributeAsFloat("hard"));*/
                    }
                    else if (node.tagName.Equals("missions_cost"))
                    {
                        missionsCostIncrement = node.GetAttributeAsInt("increment");
                    }
                    else if (node.tagName.Equals("turbo_duration"))
                    {
                        turboDuration = node.GetAttributeAsFloat("seconds");
                    }
                    else if (node.tagName.Equals("slipstream"))
                    {
                        slipstreamData.Add("duration", node.GetAttributeAsFloat("duration"));
                        slipstreamData.Add("distance", node.GetAttributeAsFloat("distance"));
                        if (node.HasAttribute("wait_for_activation"))
                            slipstreamData.Add("wait_for_activation", node.GetAttributeAsFloat("wait_for_activation"));
                        
                        if (node.HasAttribute("acceleration"))
                            slipstreamData.Add("acceleration", node.GetAttributeAsFloat("acceleration"));
                        if (node.HasAttribute("deltaSpeed"))
                            slipstreamData.Add("deltaSpeed", node.GetAttributeAsFloat("deltaSpeed"));

                        if (node.HasAttribute("acceleration_list"))
                        {
                            slipstreamAdvancedData = new List<float[]>();
                            slipstreamAdvancedData.Add(ParseArrayData(node, "acceleration_list"));
                            slipstreamAdvancedData.Add(ParseArrayData(node, "deltaSpeed_list"));
                        }
                    }
                    else if (node.tagName.Equals("slipstream_from_truck_duration"))
                    {
                        slipstreamEffectDuration = node.GetAttributeAsFloat("seconds");
                    }
                    else if (node.tagName.Equals("weekly_challemge_reward"))
                    {
                        weeklyChallengeReward = node.GetAttributeAsInt("quantity");
                    }
                    else if (node.tagName.Equals("first_fuel_gift"))
                    {
                        firstFuelGift = node.GetAttributeAsInt("quantity");
                        firstFuelGiftEnabled = node.GetAttributeAsInt("enabled");
                    }
                    else if (node.tagName.Equals("tiers_xp_threshold"))
                    {
                        tiersXPThreshold = ParseArrayData(node, "value");
                        tiersBuyFor = ParseArrayData(node, "buy_for");
                    }
                    else if (node.tagName.Equals("tiers_price"))
                    {
                        tiersPrices = ParseArrayData(node, "value");
                        tiersCurrencies = ParseCurrencyArrayData(node, "currency");
                    }
                    else if (node.tagName.Equals("tiers_coming_soon"))
                    {
                        tiersComingSoon = ParseBoolArrayData(node, "value");
                    }
                    else if (node.tagName.Equals("avg_coins_per_run"))
                    {
                        avgCoinsPerRun = node.GetAttributeAsInt("value");
                    }
                    else if (node.tagName.Equals("avg_xp_per_run"))
                    {
                        avgXPPerRun = node.GetAttributeAsInt("value");
                    }
                    else if (node.tagName.Equals("show_fps"))
                    {
                        fpsIsEnabled = node.GetAttributeAsString("value", "false").Equals("true") ? true : false;
                    }
                    else if (node.tagName.Equals("num_scores_offgame_ranks"))
                    {
                        numScoresOffgameRanks = node.GetAttributeAsInt("value", 50);
                    }
                    else if (node.tagName.Equals("num_scores_ingame_ranks"))
                    {
                        numScoresIngameRanks = node.GetAttributeAsInt("value", 100);
                    }
					else if (node.tagName.Equals("facebook_page_link"))
					{
						facebookPageLink = node.GetAttributeAsString("value", "https://www.facebook.com/pages/On-The-Run/251185165078080");
                    }
                    else if (node.tagName.Equals("facebook_login"))
                    {
                        facebookLoginReward = ParseArrayData(node, "value"); //node.GetAttributeAsInt("value");
                        facebookLoginRewardAfter = node.GetAttributeAsInt("with_reward_appear_after");
                        facebookPageCurrency = ParseCurrencyArrayData(node, "type"); //node.GetAttributeAsString("type") == "Coin" ? PriceData.CurrencyType.FirstCurrency : PriceData.CurrencyType.SecondCurrency;
                        facebookInitialCounter = node.GetAttributeAsInt("initial_counter");
                        facebookDeltaCounter = node.GetAttributeAsInt("delta_counter");
                        facebookMaxTimesShown = node.GetAttributeAsInt("max_times_shown");
                        facebookDiamondsForInvite = node.GetAttributeAsInt("diamonds_for_invite");
                    }
                    else if (node.tagName.Equals("facebook_logged_popup"))
                    {
                        facebookLoggedPopupData = new Dictionary<string, float>();
                        facebookLoggedPopupData.Add("days", node.GetAttributeAsFloat("days"));
                        facebookLoggedPopupData.Add("minutes", node.GetAttributeAsFloat("minutes"));
                        facebookLoggedPopupData.Add("friends", node.GetAttributeAsFloat("friends"));
                    }
                    else if (node.tagName.Equals("inapp_remove_ads_advisefreq"))
                    {
                        inappRemoveAdsAdviseFreq = node.GetAttributeAsInt("value", 0);
                    }
                    else if (node.tagName.Equals("popup_store"))
                    {
                        popupStoreFrequency = node.GetAttributeAsInt("frequency");
                        popupStoreMaxForSession = node.GetAttributeAsInt("max_for_session");
                    }
                    else if (node.tagName.Equals("checkpoints"))
                    {
                        checkpointDistance = node.GetAttributeAsInt("distance");
                        checkpointTimes = ParseArrayData(node, "time_list");
                        checkpointTimeData = new Dictionary<string, float>();
                        checkpointTimeData.Add("init_time", node.GetAttributeAsFloat("init_time"));
                        checkpointTimeData.Add("decrease_time", node.GetAttributeAsFloat("decrease_time"));
                        checkpointTimeData.Add("min_time", node.GetAttributeAsFloat("min_time"));
                    }
					else if (node.tagName.Equals("slipStreamComboReward"))
					{ 
						slipStreamComboCountBase=node.GetAttributeAsFloat("countbase");
						slipStreamComboCountStep=node.GetAttributeAsFloat("countstep");
						slipStreamComboRewardBase=node.GetAttributeAsFloat("rewardbase");
						slipStreamComboRewardStep=node.GetAttributeAsFloat("rewardstep");
					}
					else if (node.tagName.Equals("killCarComboReward"))
					{ 
						killCarComboCountBase=node.GetAttributeAsFloat("countbase");
						killCarComboCountStep=node.GetAttributeAsFloat("countstep");
						killCarComboRewardBase=node.GetAttributeAsFloat("rewardbase");
						killCarComboRewardStep=node.GetAttributeAsFloat("rewardstep");
					}
					else if (node.tagName.Equals("maxDistanceReward"))
					{ 
						maxDistanceCoundStep=node.GetAttributeAsFloat("countbase");
						maxDistanceCountBase=node.GetAttributeAsFloat("countstep");
						maxDistanceRewardBase=node.GetAttributeAsFloat("rewardbase");
						maxDistanceRewardStep=node.GetAttributeAsFloat("rewardstep");
					}
					else if (node.tagName.Equals("coinCollectionReward"))
					{ 
						coinCollectionCountBase=node.GetAttributeAsFloat("countbase");
						coinCollectionCountStep=node.GetAttributeAsFloat("countstep");
						coinCollectionRewardBase=node.GetAttributeAsFloat("rewardbase");
						coinCollectionRewardStep=node.GetAttributeAsFloat("rewardstep");
					}
                }
            }
            
            //physic parameters...
            if (record.tagName.Equals("car_physics"))
            {
                foreach (XMLNode node in record.children)
                    ParsePhysicParameterData(node, node.tagName);
            }
            
            //car mission
            if (record.tagName.Equals("car_mission_data"))
            {
                foreach (XMLNode node in record.children)
                    ParseCarMissionData(node, node.tagName);
            }

            //cars upgrades multipliers...
            if (record.tagName.Equals("cars_update"))
            {
                foreach (XMLNode node in record.children)
                    ParseCarsUpgradesMultipliersData(node);
            }
			//cars upgrades multipliers...
			if ( record.tagName.Equals("VIPRewardInfo") ) {
				vipReward = new VIPRewardInfo();
				vipReward.Coins = record.GetAttributeAsInt("Coins");
				vipReward.Diamonds = record.GetAttributeAsInt("Diamonds");
				vipReward.Fuel = record.GetAttributeAsInt("Fuel");
				vipReward.ExtraSpin = record.GetAttributeAsInt("ExtraSpin");
				vipReward.Checkpoints_FirstTimeAdd = record.GetAttributeAsInt("Checkpoints_FirstTimeAdd");
				vipReward.ExpAdd = record.GetAttributeAsInt("ExpAdd");
				vipReward.Turbo_AddDuration = record.GetAttributeAsInt("Turbo_AddDuration");
			}
        }

        dataParsing = false;
        dataLoaded = true;
    }

    protected void ParsePowerUpsData(XMLNode node)
    {
        Dictionary<string, float> singlePowerUpValues = new Dictionary<string, float>();
        singlePowerUpValues.Add("start_min_spawn_distance", node.GetAttributeAsFloat("start_min_spawn_distance"));
        singlePowerUpValues.Add("start_max_spawn_distance", node.GetAttributeAsFloat("start_max_spawn_distance"));
        singlePowerUpValues.Add("end_min_spawn_distance", node.GetAttributeAsFloat("end_min_spawn_distance"));
        singlePowerUpValues.Add("end_max_spawn_distance", node.GetAttributeAsFloat("end_max_spawn_distance"));

        powerUpsData.Add(node.tagName, singlePowerUpValues);
    }

    protected void ParseTrafficData(XMLNode node)
    {
        Dictionary<string, float> trafficInitValues = new Dictionary<string, float>();
        Dictionary<string, float> trafficLimitValues = new Dictionary<string, float>();
        Dictionary<string, float> trafficIncrementsValues = new Dictionary<string, float>();
        string dataName = "data";

        if(node.HasAttribute("range"))
            dataName += node.GetAttributeAsString("range");
        
        foreach (XMLNode element in node.children)
        {
            if (element.tagName.Equals("initial_values"))
            {
                trafficInitValues.Add("start_min_spawn_distance", element.GetAttributeAsFloat("start_min_spawn_distance"));
                trafficInitValues.Add("start_max_spawn_distance", element.GetAttributeAsFloat("start_max_spawn_distance"));
                trafficInitValues.Add("end_min_spawn_distance", element.GetAttributeAsFloat("end_min_spawn_distance"));
                trafficInitValues.Add("end_max_spawn_distance", element.GetAttributeAsFloat("end_max_spawn_distance"));
            }
            else if (element.tagName.Equals("limit_values"))
            {
                trafficLimitValues.Add("max_start_min_spawn_distance", element.GetAttributeAsFloat("max_start_min_spawn_distance"));
                trafficLimitValues.Add("max_start_max_spawn_distance", element.GetAttributeAsFloat("max_start_max_spawn_distance"));
                trafficLimitValues.Add("max_end_min_spawn_distance", element.GetAttributeAsFloat("max_end_min_spawn_distance"));
                trafficLimitValues.Add("max_end_max_spawn_distance", element.GetAttributeAsFloat("max_end_max_spawn_distance"));
            }
            else if (element.tagName.Equals("increments"))
            {
                trafficIncrementsValues.Add("delta_start_min_spawn_distance", element.GetAttributeAsFloat("delta_start_min_spawn_distance"));
                trafficIncrementsValues.Add("delta_start_max_spawn_distance", element.GetAttributeAsFloat("delta_start_max_spawn_distance"));
                trafficIncrementsValues.Add("delta_end_min_spawn_distance", element.GetAttributeAsFloat("delta_end_min_spawn_distance"));
                trafficIncrementsValues.Add("delta_end_max_spawn_distance", element.GetAttributeAsFloat("delta_end_max_spawn_distance"));
            }
        }

        Dictionary<string, float>[] dataList = {trafficInitValues, trafficLimitValues, trafficIncrementsValues};
        trafficData.Add(dataName, dataList);
    }

    protected void ParseLocalizationXML(string localeText, bool checkVersion)
    {
        XMLReader xmlReader = new XMLReader();
        XMLNode root = xmlReader.read(localeText).children[0] as XMLNode;

        if (checkVersion)
        {
            int version = root.GetAttributeAsInt("version", 0);

            XMLNode otherRoot = xmlReader.read(localization.text).children[0] as XMLNode;
            int otherVersion = otherRoot.GetAttributeAsInt("version", 0);

            if (otherVersion >= version)
            {
                if (otherVersion > version)
                    omniata.Log.MyWarning("Localization out of date!");
#if SBS_DEBUG
                LogTool.Log("Switching to builtin localization");
#endif
                root = otherRoot;
            }
        }

        foreach (XMLNode record in root.children)
        {
            Dictionary<string, string> items = new Dictionary<string, string>();
            foreach (XMLNode item in record.children)
                items.Add(item.tagName, item.cdata);

            //LogTool.Log("record.attributes[id] " + record.attributes["id"]);
            strings.Add(record.attributes["id"], items);
        }

#if UNITY_IPHONE && !UNITY_EDITOR
        localeToUse = _getLocale();
#else
        localeToUse = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
#endif

        if (SRCoinsService.Instance != null)
            SRCoinsService.Instance.OnTranslationsReady();
    }
    #endregion

    #region Public methods
	//add by hmh.achievement
	public float[] GetSlipStreamComboParms()
	{
		float[] f = { slipStreamComboCountBase,slipStreamComboCountStep,slipStreamComboRewardBase,slipStreamComboRewardStep};
		return f;
	}
	public float[] GetKillCarComboParms()
	{
		float[] f = { killCarComboCountBase,killCarComboCountStep,killCarComboRewardBase,killCarComboRewardStep};
		return f;
	}
	public float[] GetMaxDistanceRewardParms()
	{
		float[] f = { maxDistanceCoundStep,maxDistanceCountBase,maxDistanceRewardBase,maxDistanceRewardStep};
		return f;
	}
	public float[] GetCoinCollectionRewardParms()
	{
		float[] f = { coinCollectionCountBase,coinCollectionCountStep,coinCollectionRewardBase,coinCollectionRewardStep};
		return f;
	}
    public bool ShowInappRemoveAdsFeedbackPopup()
    {

		return false;  //jxw

        if (SRInAppManager.Instance == null)
            return false;

        if (SRInAppManager.Instance.UserIsPurchaser)
            return false;

        bool showPopup = false;
        if (inappRemoveAdsAdviseFreq == 0 && inappRemoveAdsAdviseCounter == 0)
        {
            showPopup = true;
        }
        else if (inappRemoveAdsAdviseFreq > 0 && inappRemoveAdsAdviseCounter == 0)
        {
            showPopup = true;
        }
        else if (inappRemoveAdsAdviseFreq > 0 && inappRemoveAdsAdviseCounter >= inappRemoveAdsAdviseFreq)
        {
            showPopup = true;
            inappRemoveAdsAdviseCounter = 0;
        }

        inappRemoveAdsAdviseCounter++;
        LogTool.Log("showPopup " + showPopup + " inappRemoveAdsAdviseFreq " + inappRemoveAdsAdviseFreq + " inappRemoveAdsAdviseCounter " + inappRemoveAdsAdviseCounter);

        if (showPopup)
        {
            SRUITransitionManager.Instance.OpenPopup("MsgPopup");
            UIManager.Instance.FrontPopup.GetComponent<UIMsgPopup>().SetPopupText(SRDataLoader.Instance.GetLocaleString("inapp_remove_ads"));
        }

        return showPopup;
    }

    public int GetLevelUpRewardData(string data, int defaultValue)
    {
        if (levelUpRewardDictionary.ContainsKey(data))
            return levelUpRewardDictionary[data];
        else
            return
                defaultValue;
    }
    
    public int GetLevelBonusCount( )
    {
        if (levelBonusesList != null)
            return levelBonusesList.Count;
        else
            return 0;
    }
    
    public int GetTiersCount( )
    {
        return tiersCount;
    }

    public int GetMaxPlayerLevel( )
    {
        return maxPlayerLevel;
    }

	public List<UIGiftItem.GiftInfo> GetGiftInfos () {
		return giftInfos;
	}

    public SRConsumableBonusManager.ConsumableBonus GetLevelBonus(int index)
    {
        return levelBonusesList[index];
    }

    public SRConsumableBonusManager.ConsumableBonus GetLevelBonusAlternative(int level)
    {
        if (levelBonusesAlternativedictionary.ContainsKey(level))
            return levelBonusesAlternativedictionary[level];
        else
        {
            SRConsumableBonusManager.ConsumableBonus tmp = new SRConsumableBonusManager.ConsumableBonus();
            tmp.type = SRConsumableBonusManager.ConsumableType.OnlyDiamonds;
            tmp.quantity = SRConsumableBonusManager.Instance.GetDiamondsForLevelUp(level);
            tmp.level = level;
            return tmp;
        }
    }
    
    public float GetConsumableBonusesData(string data, float defaultValue)
    {
        if (consumableBonusesDictionary.ContainsKey(data))
            return consumableBonusesDictionary[data];
        else
            return
                defaultValue;
    }
    
    public float GetMissionsModificationsData(string data, float defaultValue)
    {
        if (missionsModificationsDictionary.ContainsKey(data))
            return missionsModificationsDictionary[data];
        else
            return
                defaultValue;
    }

    public float GetMissionsRewardData(string data, float defaultValue)
    {
        if (missionsRewardDictionary.ContainsKey(data))
            return missionsRewardDictionary[data];
        else
            return
                defaultValue;
    }

    public int GetSkipPopupData(string data, int defaultValue)
    {
        if (skipMissionPopupDictionary.ContainsKey(data))
            return skipMissionPopupDictionary[data];
        else
            return
                defaultValue;
    }

	public string GetFacebookPageLink()
	{
		return facebookPageLink;
	}
	
    public float[] GetFacebookReward()
	{
		return facebookLoginReward;
	}

    public int GetFacebookRewardAppearAfter()
	{
        return facebookLoginRewardAfter;
	}
    
    public int[] GetFacebookPopupData()
	{
        int[] data = {facebookInitialCounter, facebookDeltaCounter, facebookMaxTimesShown};
        return data;
	}
    
    public int GetFacebookDiamondsForInvite()
	{
        return facebookDiamondsForInvite;
	}

    public PriceData.CurrencyType[] GetFacebookRewardCurrency()
	{
		return facebookPageCurrency;
	}

    public int GetNumScoresOffgameRanks()
    {
        return numScoresOffgameRanks;
    }

    public int GetNumScoresIngameRanks()
    {
        return numScoresIngameRanks;
    }
    

    public int[] GetPopupStoreData()
    {
        int [] data = {popupStoreFrequency, popupStoreMaxForSession};
        return data;
    }

    public bool GetFpsIsEnabled()
    {
        return fpsIsEnabled;
    }

    public bool GetChartboostEnabled()
    {
        return enableChartboost;
    }

    public int GetChartboostLimitPerDay()
    {
        return chartboostLimitPerDay;
    }

    /*public int GetBoostVideoAdLimitPerDay()
    {
        return boostVideoAdLimitPerDay;
    }*/

    /*public int GetAndroidVideoCoinsReward()
    {
        return androidVideoCoinsReward;
    }*/

    public int GetAndroidFlurryAdsPriority()
    {
        return androidFlurryAdsPriority;
    }

    public int GetAndroidAdcolonyPriority()
    {
        return androidAdcolonyPriority;
    }

    public string GetLocaleStringWithLocale(string locale, string str)
    {
        Dictionary<string, string> hash;
        string strOut;

        if (!strings.TryGetValue(locale, out hash))
            hash = strings["zh"];
        
        if (hash.TryGetValue(str, out strOut))
            return strOut;

        return str; // string.Empty;
    }

    public string GetLocaleString(string str)
    {
        // ToDo: optimize getting dictionary at start
        //return this.GetLocaleStringWithLocale(CultureInfo.CurrentCulture.TwoLetterISOLanguageName, str);

        //pelle cheat language
        //localeToUse = "es";
		localeToUse = "zh";
        return this.GetLocaleStringWithLocale(localeToUse, str);
    }
    
    public PlayerExternalData GetPlayerData(SRGameplay.CarId carId)
    {
        return this.carsData[carId];
    }

    public List<int[]> GetShopData(UICurrencyPopup.ShopPopupTypes shopType)
    {
        if (shopType==UICurrencyPopup.ShopPopupTypes.Diamonds)
            return this.shopDiamondsData;
        else if (shopType == UICurrencyPopup.ShopPopupTypes.Money)
            return this.shopCoinsData;
        else
            return this.shopFuelData;
    }

    public float GetBoostPrice(SRBooster.BoosterType type)
    {
        return this.boostsData[type];
    }

    public float[] GetSlideshowData()
    {
        return slideshowData;
    }

    public float[] GetTurboCameraData()
    {
        return turboCameraData;
    }

    public float[] GetFinalCameraData()
    {
        return finalCameraData;
    }

    public float GetPercentageForceSpecialVehicle()
    {
        return percentageForceSpecialVehicle;
    }
    
    public float GetCollisionProtectionTime( )
    {
        return protectFromCollisionTime;
    }

    public int GetFuelVisibleInBar()
    {
        return fuelVisibleInBar;
    }

    public int GetDaysResetGifts()
    {
        return resetGiftAfterDays;
    }

    public int GetMaxGifts()
    {
        return maxGifts;
    }
    
    public int GetDaysResetInvites()
    {
        return resetInvitesAfterDays;
    }

    public int GetMaxInvites()
    {
        return maxInvites;
    }

    public int GetInviteDiamondsReward()
    {
        return diamondsReward;
    }
    
    public int GetTimeToWaitToShowTrucksAdvise()
    {
        return timeToWaitToShowTrucksAdvise;
    }
    
    public int GetTimeToWaitToShowReachRankAdvise()
    {
        return timeToWaitToShowReachRank;
    }
    
    public int[] GetSpecialCarsLockData( )
    {
        return this.specialCarsLocks.ToArray();
    }
    
    public int[] GetSpecialCarsXPLevelsData( )
    {
        return this.specialCarsXPLevels.ToArray();
    }

    public int[] GetSpecialCarsPriceData( )
    {
        return this.specialCarsPrices.ToArray();
    }

    public int[] GetAlternativeSpecialCarsPriceData( )
    {
        return this.alternativeSpecialCarsPrices.ToArray();
    }
    
    public float[] GetSpecialCarsUpgradeData()
    {
        return this.specialCarsUpgrades.ToArray();
    }
    
    public float[] GetSaveMeData(UISaveMePopup.SaveMeButtonType type)
    {
        return this.saveMeData[type];
    }
    
    public float GetSaveMeDuration( )
    {
        return saveMePopupDuration;
    }
    
    public float[] GetSaveMeCost( )
    {
        return saveMePopupCost;
    }
    
    public float GetFBAdviseDuration()
    {
        return fbAdviseDurationIngame;
    }

    public float GetTurboDuration()
    {
        return PlayerPersistentData.Instance.IsVIP ? turboDuration * (SRDataLoader.instance.GetVIPRewardInfo().Turbo_AddDuration + 100)/100 :  turboDuration;
    }

    public float GetSlipStreamFromTruckDuration()
    {
        return slipstreamEffectDuration;
    }

    public int GetWeeklyChallengeReward()
    {
        return weeklyChallengeReward;
    }

    public int GetFirstFuelGift()
    {
        return firstFuelGift;
    }

    public float[] GetTiersPrices()
    {
        return tiersPrices;
    }
    
    public float[] GetTiersPlayerLevelThreshold()
    {
        return tiersXPThreshold;
    }
    
    public float[] GetTiersBuyFor()
    {
        return tiersBuyFor;
    }

    public PriceData.CurrencyType[] GetTiersCurrencies()
    {
        return tiersCurrencies;
    }

    public bool[] GetTiersComingSoon()
    {
        return tiersComingSoon;
    }
    
    public int GetAvgCoinsForRun()
    {
        return avgCoinsPerRun;
    }

    public int GetAvgXPForRun()
    {
        return avgXPPerRun;
    }
    
    public bool GetFirstFuelGiftEnabled()
    {
        return firstFuelGiftEnabled==1;
    }
    
    public float GetSlipstreamData(string key)
    {
        return slipstreamData[key];
    }

    public bool HasSlipstreamData(string key)
    {
        return slipstreamData.ContainsKey(key);
    }

    public bool SlipstreamAdvancedDataValid()
    {
        return slipstreamAdvancedData.Count > 0;
    }

    public float[] GetSlipstreamAdvancedData(string data)
    {
        if (data == "acceleration_list")
            return slipstreamAdvancedData[0];
        else
            return slipstreamAdvancedData[1];
    } 
    
    public List<WheelItem> GetWheelData(string rewardLevel)
    {
        return this.wheelData[rewardLevel];
    }
    
    public float GetWheelDataPercentage(WheelItem.Level rewardLevel)
    {
        return wheelDataPercentages[(int)rewardLevel];
    }
    
    public int GetWheelDataQuantities(WheelItem.Level rewardLevel)
    {
        return wheelDataQuantities[(int)rewardLevel];
    }
    
    public PlayerKinematics.PhysicParameters GetPlayerPhysicData(string type)
    {
        return playerPhysicParameters[type];
    }
    
    public float GetPlayerSpeedReductionPerHit( )
    {
        return Mathf.Clamp01(1.0f - speedReductionPerHit);
    }

    public CarsUpgradesMultipliers GetCarUpgradeMultiplierData(string carId)
    {
        return carsUpgradesMultipliers[carId];
    }

    public float GetCarsUpgradesDiscount(string carId)
    {
        if (carsUpgradesDiscount.ContainsKey(carId))
            return carsUpgradesDiscount[carId];
        else
            return 0.0f;
    }
    
    public OpponentKinematics.PhysicParameters GetOpponentPhysicData(string type)
    {
        return opponentPhysicParameters[type];
    }

    public bool IsInappPriceAvailable(string productId)
    {
        return inappDollarPricesByProductId.ContainsKey(productId);
    }

    public UICurrencyPopup.ShopOffers GetOffersData(UICurrencyPopup.ShopPopupTypes type)
    {
        return offersData[type];
    }

    public int GetInitialDiamonds()
    {
        return initDiamonds;
    }

    public int GetInitialCoins()
    {
#if UNITY_WEBPLAYER
        initCoins = 1000000;
#endif

        return initCoins;
    }

    public float GetFuelFreezeTime()
    {
        return fuelFreezeDuration;
    }

    public float GetFuelFreezeNotificationTime()
    {
        return fuelFreezeNotificationAdvance;
    }

    public float GetFuelSingleRefillTime()
    {
        return fuelSingleRefillTime;
    }

    public float[] GetSlipstreamRewardData()
    {
        float[] retValue = { slipstreamBase, slipstreamStep, slipstreamExponent };
        return retValue;
    }

    public int[] GetCrashData()
    {
        int[] retValue = { crashNormal, crashSpecialVehicle };
        return retValue;
    }

    public float[] GetHelicopterData()
    {
        float[] retValue = { helicopterCoins, helicopterCoinsDeltaTime };
        return retValue;
    }

    public int GetMissionsXpData()
    {
        return missionsXPIncrement;
    }

    /*public int GetMissionsRewardData(OnTheRunMissions.MissionDifficulty difficulty)
    {
        return (int)missionRewardByDifficulty[difficulty];
    }*/
    
    public int GetMissionsCostData()
    {
        return missionsCostIncrement;
    }
    
    /*public List<int> GetDailyBonusMisteryDays()
    {
        return dailyBonusMisteryItemDays;
    }*/

    public int GetRecoverStreakCost()
    {
        return recoverStreakCost;
    }

    public List<SRDailyBonusManager.DailyBonusData> GetDailyBonusMisteryReward()
    {
        return dailyBonusMisteryRewards;
    }
    
    public int[] GetExtraSpinData()
    {
        int[] retValue = { extraSpinQuantity, extraSpinCost };
        return retValue;
    }
    
    public float GetNearMissPercentage()
    {
        return nearMissPercentage;
    }

    public float GetInappPriceInDollars(string productId)
    {
        float priceInDollars = 0.0f;
        if (IsInappPriceAvailable(productId))
            priceInDollars  = inappDollarPricesByProductId[productId];

        return priceInDollars;
    }
	public string GetInappChargeCode (string productId) {
		string chargeCode = string.Empty;
		if ( inappChargeCodeByProductId.ContainsKey(productId) )
			chargeCode = inappChargeCodeByProductId[productId];

		return string.IsNullOrEmpty(chargeCode) ? productId : chargeCode;
	}
    public SRDailyBonusManager.DailyBonusData GetBonusDataById(int bonusId)
    {
        return dailyBonusDatas[bonusId];
    }

    public int GetActiveDailyBonusesNumber()
    {
        return dailyBonusDatas.Count;
    }

    public int GetCheckpointDistance()
    {
        return checkpointDistance;
    }
    
    public float[] GetCheckpointTimes()
    {
		if ( PlayerPersistentData.Instance.IsVIP && checkpointTimes .Length > 0) {
			checkpointTimes[0] += GetVIPRewardInfo().Checkpoints_FirstTimeAdd;
		}
        return checkpointTimes;
    }
    
    public float GetCheckpointData(string data)
    {
        return checkpointTimeData[data];
    }

    public float GetFacebookLoggedPopupData(string data)
    {
        return facebookLoggedPopupData[data];
    }
    
    public float GetSpecialCarData(string data)
    {
        return specialCarsData[data];
    }

    public bool GetShowShopPerc(UICurrencyPopup.ShopPopupTypes shopType)
    {
        bool retValue = true;
        switch(shopType)
        {
            case UICurrencyPopup.ShopPopupTypes.Fuel:
                retValue = showShopPerc[0];
                break;
            case UICurrencyPopup.ShopPopupTypes.Diamonds:
                retValue = showShopPerc[1];
                break;
            case UICurrencyPopup.ShopPopupTypes.Money:
                retValue = showShopPerc[2];
                break;
        }
        return retValue;
    }

    public float GetPowerUpsData(string type, string data)
    {
        return powerUpsData[type][data];
    }

    public float GetTrafficData(string type, string data)
    {
        float retValue = -1;
        int playerLevel = PlayerPersistentData.Instance.Level;
        string trafficLevel = "data";

        if (trafficData.Count > 1 && SRDataLoader.ABTesting_Flag)
        {
            Dictionary<string, Dictionary<string, float>[]>.KeyCollection keys = trafficData.Keys;
            
            foreach (string key in keys)
            {
                if (key.Length > trafficLevel.Length)
                {
                    int levelRange = int.Parse(key.Substring(trafficLevel.Length));
                    if (playerLevel <= levelRange)
                    {
                        trafficLevel = key;
                        break;
                    }
                }
            }
        }

        switch(type)
        {
            case "init":
                retValue = trafficData[trafficLevel][0][data];// trafficInitValues[data];
                break;
            case "limit":
                retValue = trafficData[trafficLevel][1][data];// trafficLimitValues[data];
                break;
            case "increment":
                retValue = trafficData[trafficLevel][2][data];// trafficIncrementsValues[data];
                break;
        }

        return retValue;
    }
    
    public float GetRoadworksData(string data)
    {
        return roadWorksData[data];
    }

    public float GetCentralMudData(string data)
    {
        return centralMudData[data];
    }

    public int GetEnemiesMinDistanceFromCheckpoint()
    {
        return enemiesMinDistanceFromCheckpoint;
    }
    
    public float GetPoliceSpawnPercentage( )
    {
        return policeSpawnPercentage;
    }

    public float[] GetPoliceData(string data)
    {
        return policeSpawnData[data];
    }

    public float[] GetHelicopterData(string data)
    {
        return helicopterSpawnData[data];
    }
	public string getVersionAndUmengChannel()
	{
		List<string> lStr = SRDataLoader.Instance.GetVersionDetails();
		if (lStr.Count == 0){
			return "1.0.0";
		}else if (lStr.Count == 1){
			return lStr[0];
		}else{
			CannIndex.Cann = lStr[1];
			return lStr[0] + "(" + lStr[1] + ")";
		}
		
		//return "1.0.6(aiyouxi)"; //LoadXml();
	}



	public VIPRewardInfo GetVIPRewardInfo () {
		return vipReward;
	}
    #endregion

    #region Unity Callbacks
    new void Awake()
    {
		AndroidSDKTool.Instance.init();
		getVersionAndUmengChannel();
		GA.StartWithAppKeyAndChannelId("5705ccbde0f55aa53e0019f2" ,CannIndex.Cann);
//		GA.StartWithAppKeyAndChannelId("5705ccbde0f55aa53e0019f2" , AndroidSDKTool.channel.ToString());
        SRDataLoader.Instance = this;

#if UNITY_EDITOR && !UNITY_WEBPLAYER
        this.SaveConfigXMLToZip();
#endif
        dataLoaded = false;
        downloadingZip = false;
        stopDownloading = false;

        //base.Awake();

        startTime = Time.realtimeSinceStartup;
        lastVersionNumber = PlayerPrefs.GetInt("last_vn", 0);

#if UNITY_WEBPLAYER
        this.ParseConfigXML(configurationWeb.text, false);
        this.ParseLocalizationXML(localization.text, false);
#elif UNITY_EDITOR
        this.TryLoadingXMLsFromZip();

		#elif !UNITY_EDITOR
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            this.StartCoroutine(this.DownloadVersionFile());
            this.StartCoroutine(this.DownloadZip());
        }
#endif

        DontDestroyOnLoad(gameObject);
    }

    /*
    void OnApplicationPause(bool paused)
    {
        if (paused && dataLoaded)
        {
            var itemsList = items.Values;
            foreach (var item in itemsList)
                item.SaveData();
        }
    }
#if UNITY_EDITOR
    void OnApplicationQuit()
    {
        if (dataLoaded)
        {
            var itemsList = items.Values;
            foreach (var item in itemsList)
                item.SaveData();
        }
    }
#endif
     */
    #endregion
}
