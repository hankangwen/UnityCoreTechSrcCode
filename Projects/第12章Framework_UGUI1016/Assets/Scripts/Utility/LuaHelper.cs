using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using LuaInterface;
using System;
using Junfine.Debuger;
using BIEFramework.Manager;

//  LuaHelper.cs
//  Author: Jxw
//  2015-10-16

namespace BIEFramework {
    public static class LuaHelper {

        /// <summary>
        /// getType
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public static System.Type GetType(string classname) {
            Assembly assb = Assembly.GetExecutingAssembly();  //.GetExecutingAssembly();
            System.Type t = null;
            t = assb.GetType(classname); ;
            if (t == null) {
                t = assb.GetType(classname);
            }
            return t;
        }

        /// <summary>
        /// 面板管理器
        /// </summary>
        public static PanelManager GetPanelManager() {
            return Facade.Instance.GetManager<PanelManager>(ManagerName.Panel);
        }

        /// <summary>
        /// 资源管理器
        /// </summary>
        public static ResourceManager GetResManager() {
            return Facade.Instance.GetManager<ResourceManager>(ManagerName.Resource);
        }

        /// <summary>
        /// 音乐管理器
        /// </summary>
        public static MusicManager GetMusicManager() {
            return Facade.Instance.GetManager<MusicManager>(ManagerName.Music);
        }

        /// <summary>
        /// pbc/pblua函数回调
        /// </summary>
        /// <param name="func"></param>
        public static void OnCallLuaFunc(LuaStringBuffer data, LuaFunction func) {
            byte[] buffer = data.buffer;
            if (func != null) {
                LuaScriptMgr mgr = Facade.Instance.GetManager<LuaScriptMgr>(ManagerName.Lua);
                int oldTop = func.BeginPCall();
                LuaDLL.lua_pushlstring(mgr.lua.L, buffer, buffer.Length);
                if (func.PCall(oldTop, 1)) func.EndPCall(oldTop);
            }
            Debug.LogWarning("OnCallLuaFunc buffer:>>" + buffer + " lenght:>>" + buffer.Length);
        }

        /// <summary>
        /// cjson函数回调
        /// </summary>
        /// <param name="data"></param>
        /// <param name="func"></param>
        public static void OnJsonCallFunc(string data, LuaFunction func) {
            Debug.LogWarning("OnJsonCallback data:>>" + data + " lenght:>>" + data.Length);
            if (func != null) func.Call(data);
        }
    }
}