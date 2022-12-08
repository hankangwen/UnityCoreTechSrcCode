using System;
using LuaInterface;

public class BIEFramework_LuaHelperWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("GetType", GetType),
			new LuaMethod("GetPanelManager", GetPanelManager),
			new LuaMethod("GetResManager", GetResManager),
			new LuaMethod("GetMusicManager", GetMusicManager),
			new LuaMethod("OnCallLuaFunc", OnCallLuaFunc),
			new LuaMethod("OnJsonCallFunc", OnJsonCallFunc),
			new LuaMethod("New", _CreateBIEFramework_LuaHelper),
			new LuaMethod("GetClassType", GetClassType),
		};

		LuaScriptMgr.RegisterLib(L, "BIEFramework.LuaHelper", regs);
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateBIEFramework_LuaHelper(IntPtr L)
	{
		LuaDLL.luaL_error(L, "BIEFramework.LuaHelper class does not have a constructor function");
		return 0;
	}

	static Type classType = typeof(BIEFramework.LuaHelper);

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, classType);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetType(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		string arg0 = LuaScriptMgr.GetLuaString(L, 1);
		Type o = BIEFramework.LuaHelper.GetType(arg0);
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetPanelManager(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 0);
		BIEFramework.Manager.PanelManager o = BIEFramework.LuaHelper.GetPanelManager();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetResManager(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 0);
		BIEFramework.Manager.ResourceManager o = BIEFramework.LuaHelper.GetResManager();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetMusicManager(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 0);
		BIEFramework.Manager.MusicManager o = BIEFramework.LuaHelper.GetMusicManager();
		LuaScriptMgr.Push(L, o);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnCallLuaFunc(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		LuaStringBuffer arg0 = LuaScriptMgr.GetStringBuffer(L, 1);
		LuaFunction arg1 = LuaScriptMgr.GetLuaFunction(L, 2);
		BIEFramework.LuaHelper.OnCallLuaFunc(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int OnJsonCallFunc(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		string arg0 = LuaScriptMgr.GetLuaString(L, 1);
		LuaFunction arg1 = LuaScriptMgr.GetLuaFunction(L, 2);
		BIEFramework.LuaHelper.OnJsonCallFunc(arg0,arg1);
		return 0;
	}
}

