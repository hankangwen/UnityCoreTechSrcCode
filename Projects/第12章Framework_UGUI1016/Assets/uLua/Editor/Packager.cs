﻿using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using BIEFramework;

public class Packager {
    public static string platform = string.Empty;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();

    /// <summary>
    /// 载入素材
    /// </summary>
    static UnityEngine.Object LoadAsset(string file) {
        if (file.EndsWith(".lua")) file += ".txt";
        return AssetDatabase.LoadMainAssetAtPath("Assets/Builds/" + file);
    }

    //在iPhone平台编译iPhone资源
    [MenuItem("Game/Build iPhone Resource", false, 11)]
    public static void BuildiPhoneResource() { 
        BuildTarget target;
        target = BuildTarget.iOS;
        BuildAssetResource(target, false);
    }

    //在Android平台编译Android资源
    [MenuItem("Game/Build Android Resource", false, 12)]
    public static void BuildAndroidResource() {
        BuildAssetResource(BuildTarget.Android, true);
    }

    //在Windows平台编译Windows资源
    [MenuItem("Game/Build Windows Resource", false, 13)]
    public static void BuildWindowsResource() {
        BuildAssetResource(BuildTarget.StandaloneWindows, true);
    }

    /// <summary>
    /// 生成绑定素材
    /// </summary>
    public static void BuildAssetResource(BuildTarget target, bool isWin) {
        string dataPath = Util.DataPath;
        if (Directory.Exists(dataPath)) {
            Directory.Delete(dataPath, true);
        }
        string assetfile = string.Empty;  //素材文件名
        string resPath = AppDataPath + "/" + AppConst.AssetDirname + "/";
        if (!Directory.Exists(resPath)) Directory.CreateDirectory(resPath);
        if (AppConst.ExampleMode) {
            BuildPipeline.BuildAssetBundles(resPath, BuildAssetBundleOptions.None, target);
        }

        string luaPath = resPath + "/lua/";

        //----------复制Lua文件----------------
        if (Directory.Exists(luaPath)) {
            Directory.Delete(luaPath, true);
        }
        Directory.CreateDirectory(luaPath);

        paths.Clear(); files.Clear();
        string luaDataPath = Application.dataPath + "/lua/".ToLower();
        Recursive(luaDataPath);
        int n = 0;
        foreach (string f in files) {
            if (f.EndsWith(".meta")) continue;
            string newfile = f.Replace(luaDataPath, "");
            string newpath = luaPath + newfile;
            string path = Path.GetDirectoryName(newpath);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            if (File.Exists(newpath)) {
                File.Delete(newpath);
            }
            if (AppConst.LuaEncode) {
                UpdateProgress(n++, files.Count, newpath);
                EncodeLuaFile(f, newpath, isWin);
            } else {
                File.Copy(f, newpath, true);
            }
        }
        EditorUtility.ClearProgressBar();

        ///----------------------创建文件列表-----------------------
        /// 对文件md5加密的代码模块
        string newFilePath = resPath + "/files.txt";
        if (File.Exists(newFilePath)) File.Delete(newFilePath);

        paths.Clear(); files.Clear();
        Recursive(resPath);

        FileStream fs = new FileStream(newFilePath, FileMode.CreateNew);
        StreamWriter sw = new StreamWriter(fs);
        for (int i = 0; i < files.Count; i++) {
            string file = files[i];
            string ext = Path.GetExtension(file);
            if (file.EndsWith(".meta") || file.Contains(".DS_Store")) continue;

            string md5 = Util.md5file(file);
            string value = file.Replace(resPath, string.Empty);
            sw.WriteLine(value + "|" + md5);    //资源和md5码之间用"|"表示
        }
        sw.Close(); fs.Close();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 数据目录
    /// </summary>
    static string AppDataPath {
        get { return Application.dataPath.ToLower(); }
    }

    /// <summary>
    /// 遍历目录及其子目录
    /// </summary>
    static void Recursive(string path) {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names) {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs) {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    static void UpdateProgress(int progress, int progressMax, string desc) {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    static void EncodeLuaFile(string srcFile, string outFile, bool isWin) {
        if (!srcFile.ToLower().EndsWith(".lua")) {
            File.Copy(srcFile, outFile, true);
            return;
        }
        string luaexe = string.Empty;
        string args = string.Empty;
        string exedir = string.Empty;
        string currDir = Directory.GetCurrentDirectory();
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            luaexe = "luajit.exe";
            args = "-b " + srcFile + " " + outFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luajit/";
        } else if (Application.platform == RuntimePlatform.OSXEditor) {
            luaexe = "./luac";
            args = "-o " + outFile + " " + srcFile;
            exedir = AppDataPath.Replace("assets", "") + "LuaEncoder/luavm/";
        }
        Directory.SetCurrentDirectory(exedir);
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = luaexe;
        info.Arguments = args;
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.UseShellExecute = isWin;
        info.ErrorDialog = true;
        Util.Log(info.FileName + " " + info.Arguments);

        Process pro = Process.Start(info);
        pro.WaitForExit();
        Directory.SetCurrentDirectory(currDir);
    }

    [MenuItem("Game/Build Protobuf-lua-gen File")]
    public static void BuildProtobufFile() {
        if (!AppConst.ExampleMode) {
            Debugger.LogError("若使用编码Protobuf-lua-gen功能，需要自己配置外部环境！！");
            return;
        }
        string dir = AppDataPath + "/Lua/3rd/pblua";
        paths.Clear(); files.Clear(); Recursive(dir);

        string protoc = "d:/protobuf-2.4.1/src/protoc.exe";
        string protoc_gen_dir = "\"d:/protoc-gen-lua/plugin/protoc-gen-lua.bat\"";

        foreach (string f in files) {
            string name = Path.GetFileName(f);
            string ext = Path.GetExtension(f);
            if (!ext.Equals(".proto")) continue;

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = protoc;
            info.Arguments = " --lua_out=./ --plugin=protoc-gen-lua=" + protoc_gen_dir + " " + name;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;
            info.WorkingDirectory = dir;
            info.ErrorDialog = true;
            Util.Log(info.FileName + " " + info.Arguments);

            Process pro = Process.Start(info);
            pro.WaitForExit();
        }
        AssetDatabase.Refresh();
    }
}