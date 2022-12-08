
CtrlName = {
	Prompt = "PromptCtrl",
	Message = "MessageCtrl"
}

--协议类型--
ProtocalType = {
	BINARY = 0,
	PB_LUA = 1,
	PBC = 2,
	SPROTO = 3,
}
--当前使用协议类型-
TestProtoType = ProtocalType.PB_LUA;

Util = BIEFramework.Util;
AppConst = BIEFramework.AppConst;
LuaHelper = BIEFramework.LuaHelper;
ByteBuffer = BIEFramework.ByteBuffer;

ResManager = LuaHelper.GetResManager();
PanelManager = LuaHelper.GetPanelManager();
MusicManager = LuaHelper.GetMusicManager();