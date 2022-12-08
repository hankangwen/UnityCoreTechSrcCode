using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class BIEFramework_Manager_PanelManagerWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("CreatePanel", CreatePanel),
			new LuaMethod("New", _CreateBIEFramework_Manager_PanelManager),
			new LuaMethod("GetClassType", GetClassType),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "BIEFramework.Manager.PanelManager", typeof(BIEFramework.Manager.PanelManager), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateBIEFramework_Manager_PanelManager(IntPtr L)
	{
		LuaDLL.luaL_error(L, "BIEFramework.Manager.PanelManager class does not have a constructor function");
		return 0;
	}

	static Type classType = typeof(BIEFramework.Manager.PanelManager);

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, classType);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int CreatePanel(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		BIEFramework.Manager.PanelManager obj = (BIEFramework.Manager.PanelManager)LuaScriptMgr.GetUnityObjectSelf(L, 1, "BIEFramework.Manager.PanelManager");
		string arg0 = LuaScriptMgr.GetLuaString(L, 2);
		LuaFunction arg1 = LuaScriptMgr.GetLuaFunction(L, 3);
		obj.CreatePanel(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int Lua_Eq(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 2);
		Object arg0 = LuaScriptMgr.GetLuaObject(L, 1) as Object;
		Object arg1 = LuaScriptMgr.GetLuaObject(L, 2) as Object;
		bool o = arg0 == arg1;
		LuaScriptMgr.Push(L, o);
		return 1;
	}
}

