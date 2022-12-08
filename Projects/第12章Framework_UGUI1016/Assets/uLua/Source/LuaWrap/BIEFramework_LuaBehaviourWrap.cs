using System;
using UnityEngine;
using LuaInterface;
using Object = UnityEngine.Object;

public class BIEFramework_LuaBehaviourWrap
{
	public static void Register(IntPtr L)
	{
		LuaMethod[] regs = new LuaMethod[]
		{
			new LuaMethod("AddClick", AddClick),
			new LuaMethod("ClearClick", ClearClick),
			new LuaMethod("New", _CreateBIEFramework_LuaBehaviour),
			new LuaMethod("GetClassType", GetClassType),
			new LuaMethod("__eq", Lua_Eq),
		};

		LuaField[] fields = new LuaField[]
		{
		};

		LuaScriptMgr.RegisterLib(L, "BIEFramework.LuaBehaviour", typeof(BIEFramework.LuaBehaviour), regs, fields, typeof(MonoBehaviour));
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int _CreateBIEFramework_LuaBehaviour(IntPtr L)
	{
		LuaDLL.luaL_error(L, "BIEFramework.LuaBehaviour class does not have a constructor function");
		return 0;
	}

	static Type classType = typeof(BIEFramework.LuaBehaviour);

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int GetClassType(IntPtr L)
	{
		LuaScriptMgr.Push(L, classType);
		return 1;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int AddClick(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 3);
		BIEFramework.LuaBehaviour obj = (BIEFramework.LuaBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "BIEFramework.LuaBehaviour");
		GameObject arg0 = (GameObject)LuaScriptMgr.GetUnityObject(L, 2, typeof(GameObject));
		LuaFunction arg1 = LuaScriptMgr.GetLuaFunction(L, 3);
		obj.AddClick(arg0,arg1);
		return 0;
	}

	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	static int ClearClick(IntPtr L)
	{
		LuaScriptMgr.CheckArgsCount(L, 1);
		BIEFramework.LuaBehaviour obj = (BIEFramework.LuaBehaviour)LuaScriptMgr.GetUnityObjectSelf(L, 1, "BIEFramework.LuaBehaviour");
		obj.ClearClick();
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

