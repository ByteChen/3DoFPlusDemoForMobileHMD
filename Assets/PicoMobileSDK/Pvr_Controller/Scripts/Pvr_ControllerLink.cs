///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights Reserved.
// File: Pvr_Controller
// Author: Yangel.Yan
// Date:  2017/01/11
// Discription: The demo of using controller
///////////////////////////////////////////////////////////////////////////////
#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#elif UNITY_IPHONE
#define IOS_DEVICE
#elif UNITY_STANDALONE_WIN
#define WIN_DEVICE
#endif
#endif

using UnityEngine;
using UnityEngine.UI;
using Pvr_UnitySDKAPI;
using System;
using System.Runtime.Remoting.Messaging;

public class Pvr_ControllerLink
{

#if ANDROID_DEVICE
    public AndroidJavaClass javaHummingbirdClass;
    public AndroidJavaClass javaPico2ReceiverClass;
    public AndroidJavaClass javaserviceClass;
    public AndroidJavaClass javavractivityclass;
    public AndroidJavaClass javaCVClass;
    public AndroidJavaObject activity;
#endif
    public string gameobjname = "";
    public bool picoDevice = false;
    public string hummingBirdMac;
    public int hummingBirdRSSI;
    public string lark2key;
    public bool hbserviceBindState = false;
    public bool cvserviceBindState = false;
    public bool controller0Connected = false;
    public bool controller1Connected = false;
    public ControllerHand Controller0;
    public ControllerHand Controller1;
    public int platFormType = -1; //0 phone，1 Pico Neo，2 Pico Goblin 3 Pico CV
    public int trackingmode = -1;
    public int systemProp = -1;   //0默认cv6dof 1hb3dof 2优先cv6dof 3优先cv3dof
    public int enablehand6dofbyhead = -1;
    public bool switchHomeKey = true;
    public Pvr_ControllerLink(string name)
    {
        gameobjname = name;
        hummingBirdMac = "";
        hummingBirdRSSI = 0;
        Debug.Log(gameobjname);
        StartHummingBirdService();
        Controller0 = new ControllerHand();
        Controller1 = new ControllerHand();
    }

    private void StartHummingBirdService()
    {
#if ANDROID_DEVICE
        try
        {
            UnityEngine.AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            javaHummingbirdClass = new AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.HbClientActivity");
            javaCVClass = new AndroidJavaClass("com.picovr.picovrlib.cvcontrollerclient.ControllerClient");
            javavractivityclass = new UnityEngine.AndroidJavaClass("com.psmart.vrlib.VrActivity");
            javaserviceClass = new AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.UnityClient");
            Pvr_UnitySDKAPI.System.Pvr_SetInitActivity(activity.GetRawObject(), javaHummingbirdClass.GetRawClass());
            int enumindex = (int)GlobalIntConfigs.PLATFORM_TYPE;
            Render.UPvr_GetIntConfig(enumindex, ref platFormType);
            PLOG.I("platform" + platFormType);
            enumindex = (int)GlobalIntConfigs.TRACKING_MODE;
            Render.UPvr_GetIntConfig(enumindex, ref trackingmode);
            PLOG.I("trackingmode" + trackingmode);
            systemProp = GetSysproc();
            PLOG.I("systemProp" + systemProp);
            enumindex = (int) GlobalIntConfigs.ENBLE_HAND6DOF_BY_HEAD;
            Render.UPvr_GetIntConfig(enumindex, ref enablehand6dofbyhead);
            PLOG.I("enablehand6dofbyhead" + enablehand6dofbyhead);
            if (trackingmode == 0 || trackingmode == 1 || (trackingmode == 3 && systemProp == 1))
            {
                picoDevice = platFormType != 0;
                javaPico2ReceiverClass = new UnityEngine.AndroidJavaClass("com.picovr.picovrlib.hummingbirdclient.HbClientReceiver");
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver", activity, gameobjname);
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "setPlatformType", platFormType);
            }
            else
            {
                picoDevice = true;
                SetGameObjectToJar(gameobjname);
            }
            if (IsServiceExisted())
            {
                BindService();
            }
        }
        catch (AndroidJavaException e)
        {
            Debug.LogError("ConnectToAndriod------------------------catch" + e.Message);
        }
#endif
    }

    public bool IsServiceExisted()
    {
        
        bool service = false;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref service, javaserviceClass, "isServiceExisted", activity,trackingmode);
#endif
        PLOG.I("serviceExisted ?" + service);
        return service;

    }
    public void SetGameObjectToJar(string name)
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setGameObjectCallback", name);
#endif
    }

    public void BindService()
    {
        PLOG.I("Start Bind");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaserviceClass, "bindService", activity,trackingmode);
#endif
    }
    public void UnBindService()
    {
        PLOG.I("Start UnBind");
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaserviceClass, "unbindService", activity,trackingmode);
#endif
    }

    public void StopLark2Receiver()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopReceiver",activity);
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopOnBootReceiver",activity);
#endif
    }
    public void StartLark2Receiver()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver",activity, gameobjname);
#endif
    }
    public void StopLark2Service()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "stopReceiver", activity); 
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "unbindHbService", activity);
#endif
    }
    public void StartLark2Service()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaPico2ReceiverClass, "startReceiver",activity, gameobjname);
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "bindHbService", activity);
#endif
    }
    public int getHandness()
    {
        int handness = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref handness, javavractivityclass, "getPvrHandness", activity);
#endif
        PLOG.I("HandNess =" + handness);
        return handness;
    }
    public void StartScan()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "scanHbDevice", true);
#endif
    }
    public void StopScan()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "scanHbDevice", false);
#endif
    }

    public int GetSysproc()
    {
        int prop = -1;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref prop, javaserviceClass, "getSysproc");
#endif
        return prop;
    }

    public void ResetController(int num)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "resetControllerSensorState",num);
        }
        else
        {
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "resetHbSensorState");
		}
#elif IOS_DEVICE
        Controller.Pvr_ResetSensor(3);

#endif
        PLOG.I("ResetController" + num);
    }

    public void ConnectBLE()
    {
        if (hummingBirdMac != "")
        {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "connectHbController", hummingBirdMac);
#endif
        }
        PLOG.I("ConnectHBController" + hummingBirdMac);
    }

    public void DisConnectBLE()
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "disconnectHbController");
#endif
    }
    public bool StartUpgrade()
    {
        bool start = false;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<bool>(ref start, javaHummingbirdClass, "startUpgrade");
#endif
        return start;
    }
    public void setBinPath(string path, bool isasset)
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod( javaHummingbirdClass, "setBinPath",path,isasset);
#endif
    }

    public string GetBLEImageType()
    {
        string type = "";
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type, javaHummingbirdClass, "getBLEImageType");
#endif
        return type;
    }

    public long GetBLEVersion()
    {
        long version = 0L;
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<long>(ref version, javaHummingbirdClass, "getBLEVersion");
#endif
        return version;
    }

    public string GetFileImageType()
    {
        string type = "";
#if ANDROID_DEVICE
      Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref type, javaHummingbirdClass, "getFileImageType");
#endif
        return type;
    }

    public long GetFileVersion()
    {
        long version = 0L;
#if ANDROID_DEVICE
      Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<long>(ref version, javaHummingbirdClass, "getFileVersion");
#endif
        return version;
    }

    public int GetControllerConnectionState(int num)
    {
        //hb 0未连接 1连接中 2连接成功
        //cv 0未连接 1连接成功
        int state = -1;
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref state, javaCVClass, "getControllerConnectionState",num);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref state, javaHummingbirdClass, "getHbConnectionState");
        }
#endif
        PLOG.D("GetControllerState:" + num  + "state:"+ state);
        return state;
    }

    public void RebackToLauncher()
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startLauncher");
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "startLauncher");
        }
#endif
    }

    public void TurnUpVolume()
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "turnUpVolume", activity);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "turnUpVolume", activity);
        }
#endif
        PLOG.I("TurnUpVolume");
    }

    public void TurnDownVolume()
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "turnDownVolume", activity);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "turnDownVolume", activity);
        }
#endif
        PLOG.I("TurnDownVolume");
    }
    
    /// <summary>
    /// 获取goblin手柄的原始姿态数据
    /// </summary>
    /// <param name="hand">0,1分别代表两个手柄</param>
    /// <returns></returns>
    public string GetHBControllerPoseData()
    {
        string data = "";
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref data, javaHummingbirdClass, "getHBSensorState");
#endif
        PLOG.D("HBControllerData" + data);
        return data;
    }

    public float[] GetCvControllerPoseData(int hand)
    {
        var data = new float[7] { 0, 0, 0, 0, 0, 0, 0 };
#if ANDROID_DEVICE
        if (enablehand6dofbyhead == 1)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaCVClass, "getControllerSensorState", hand,Pvr_UnitySDKManager.SDK.headData);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaCVClass, "getControllerSensorState", hand);
        }

#endif
        Quaternion pose = new Quaternion(data[0], data[1], data[2], data[3]);
        Vector3 pos = new Vector3(data[4], data[5], data[6]);
        PLOG.D("CVControllerData " + hand + "Rotation:" + data[0] + data[1] + data[2] + data[3] + "Position:" +
               data[4] + data[5] + data[6] + "eulerAngles:" + pose.eulerAngles);

        if (float.IsNaN(pose.x) || float.IsNaN(pose.y) || float.IsNaN(pose.z) || float.IsNaN(pose.w))
        {
            pose = Quaternion.identity;
        }
        if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
        {
            pos = Vector3.zero;
        }
        return new float[7] { pose.x, pose.y, pose.z, pose.w, pos.x, pos.y, pos.z };
    }
    /// <summary>
    /// 获取手柄的键值
    /// </summary>
    /// <param name="hand">0,1 分别代表两个手柄</param>
    /// <returns></returns>
    public string GetHBControllerKeyData()
    {
        string data = "";
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref data, javaHummingbirdClass, "getHBKeyEvent");
#endif
        PLOG.D("HBControllerKey" + data);
        return data;
    }

    public int[] GetCvControllerKeyData(int hand)
    {
        var data = new int[9] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, javaCVClass, "getControllerKeyEvent", hand);
#endif
        PLOG.D("CVControllerKey" + data[0] + data[1] + data[2] + data[3] + data[4] + data[5] + data[6] + data[7] +
               data[8]);
        return data;
    }
    /// <summary>
    /// 自动连接Pico goblin手柄
    /// </summary>
    /// <param name="scanTimeMs">扫描时间，单位毫秒</param>
    public void AutoConnectHbController(int scanTimeMs)
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaHummingbirdClass, "autoConnectHbController",scanTimeMs,gameobjname);
#endif
    }

    /// <summary>
    /// 0 3dof 1 6dof
    /// </summary>
    /// <param name="headSensorState"></param>
    /// <param name="handSensorState"></param>
    public void StartControllerThread(int headSensorState, int handSensorState)
    {
        PLOG.I("StartControllerThread" + headSensorState + handSensorState);
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startControllerThread",headSensorState,handSensorState);
#endif
    }
    public void StopControllerThread(int headSensorState, int handSensorState)
    {
        PLOG.I("StopControllerThread" + headSensorState + handSensorState);
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "stopControllerThread",headSensorState,handSensorState);
#endif
    }

    /// <summary>
    /// 获取手柄陀螺仪数据
    /// </summary>
    /// <param name="num">参数为0,1，分别代表两个手柄</param>
    /// <returns></returns>
    public Vector3 GetAngularVelocity(int num)
    {
        var angulae = new float[3] { 0, 0, 0 };
        try
        {
#if ANDROID_DEVICE

            if (cvserviceBindState)
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref angulae, javaCVClass, "getControllerAngularVelocity", num);
            }
            else
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref angulae, javaHummingbirdClass, "getHbAngularVelocity");
            }
#endif
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        PLOG.D("PvrLog Gyro:" + angulae[0] + angulae[1] + angulae[2]);
        if (!float.IsNaN(angulae[0]) && !float.IsNaN(angulae[1]) && !float.IsNaN(angulae[2]))
        {
            return new Vector3(angulae[0], angulae[1], angulae[2]);
        }
        return new Vector3(0, 0, 0);
    }

    /// <summary>
    /// 获取手柄加速度数据
    /// </summary>
    /// <returns></returns>
    public Vector3 GetAcceleration(int num )
    {
        var accel = new float[3] { 0, 0, 0 };
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref accel, javaCVClass, "getControllerAcceleration", num);
        }
        else
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref accel, javaHummingbirdClass, "getHbAcceleration");
        }

#endif
        PLOG.D("PvrLog Acce:" + accel[0] + accel[1] + accel[2]);
        if (!float.IsNaN(accel[0]) && !float.IsNaN(accel[1]) && !float.IsNaN(accel[2]))
        {
            return new Vector3(accel[0], accel[1], accel[2]);
        }
        return new Vector3(0,0,0);
    }

    /// <summary>
    /// 获取当前连接的手柄的MAC地址
    /// </summary>
    /// <returns></returns>
    public string GetConnectedDeviceMac()
    {
        string mac = "";
#if ANDROID_DEVICE
        if (hbserviceBindState)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<string>(ref mac, javaHummingbirdClass, "getConnectedDeviceMac");
        }
#endif
        return mac;
    }
    /// <summary>
    /// 手柄振动接口
    /// </summary>
    /// <param name="hand">0,1分别代表两个手柄</param>
    /// <param name="strength">振动强度0-255</param>
    public void VibateController(int hand, int strength)
    {
#if ANDROID_DEVICE
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "vibrateControllerStrength", hand,strength);
#endif
    }
    
    public int GetMainControllerIndex()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index, javaCVClass, "getMainControllerIndex");
        }
#endif
        return index;
    }
    
    public void SetMainController(int index)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setMainController",index); 
        }
#endif
    }
    public void ResetHeadSensorForController()
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "resetHeadSensorForController");
        }
#endif
    }
    //获取版本号 deviceType：0-station 1-手柄0  2-手柄1
    public void GetDeviceVersion(int deviceType)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getDeviceVersion",deviceType); 
        }
#endif
    }
    //获取手柄Sn号  controllerSerialNum : 0-手柄0  1-手柄1
    public void GetControllerSnCode(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerSnCode",controllerSerialNum); 
        }
#endif
    }
    //解绑手柄 controllerSerialNum : 0-手柄0  1-手柄1
    public void SetControllerUnbind(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setControllerUnbind",controllerSerialNum); 
        }
#endif
    }
    //重启station
    public void SetStationRestart()
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setStationRestart"); 
        }
#endif
    }
    //发起station OTA升级
    public void StartStationOtaUpdate()
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startStationOtaUpdate"); 
        }
#endif
    }
    //发起手柄ota升级 mode：1-RF 升级通讯模块 2-升级STM32模块 ； controllerSerialNum : 0-手柄0  1-手柄1
    public void StartControllerOtaUpdate(int mode, int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "startControllerOtaUpdate",mode,controllerSerialNum); 
        }
#endif
    }
    // 进入配对模式 controllerSerialNum：0-手柄0  1-手柄1
    public void EnterPairMode(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "enterPairMode",controllerSerialNum); 
        }
#endif
    }
    //手柄关机controllerSerialNum：0-手柄0  1-手柄1
    public void SetControllerShutdown(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "setControllerShutdown",controllerSerialNum); 
        }
#endif
    }
    // 获取当前station的配对状态 返回值0-未配对状态 1-正在配对状态
    public int GetStationPairState()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getStationPairState"); 
        }
#endif
        PLOG.I("StationPairState" + index);
        return index;
    }
    //获取station ota升级进度
    public int GetStationOtaUpdateProgress()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getStationOtaUpdateProgress"); 
        }
#endif
        PLOG.I("StationOtaUpdateProgress" + index);
        return index;
    }
    //获取Controller ota升级进度
    public int GetControllerOtaUpdateProgress()
    {
        int index = -1;
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getControllerOtaUpdateProgress"); 
        }
#endif
        PLOG.I("ControllerOtaUpdateProgress" + index);
        return index;
    }

    //同时获取手柄的版本号和SN号  controllerSerialNum：0-手柄0  1-手柄1
    public void GetControllerVersionAndSN(int controllerSerialNum)
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerVersionAndSN",controllerSerialNum); 
        }
#endif
    }

    //获取手柄的唯一识别码
    public void  GetControllerUniqueID()
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "getControllerUniqueID"); 
        }
#endif
    }

    //将station从当前的配对模式中中断出来
    public void InterruptStationPairMode()
    {
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaCVClass, "interruptStationPairMode"); 
        }
#endif
    }
    //获取当前手柄的能力模式（3dof手柄还是6dof手柄）
    public int GetControllerAbility(int controllerSerialNum)
    {
        int index = -1;
#if ANDROID_DEVICE
        if (cvserviceBindState)
        {
           Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod<int>(ref index,javaCVClass, "getControllerAbility",controllerSerialNum);
        }
#endif
        return index;
    }

    public void SwitchHomeKey(bool state)
    {
        switchHomeKey = state;
    }


}
