///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights Reserved.
// File: Pvr_ControllerManager
// Author: Yangel.Yan
// Date:  2017/01/11
// Discription: Be Sure Your controller demo has this script
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
using System;
using System.Collections;
using LitJson;
using Pvr_UnitySDKAPI;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Pvr_ControllerManager : MonoBehaviour
{

    /************************************    Properties  *************************************/
    private static Pvr_ControllerManager instance = null;

    public static Pvr_ControllerManager Instance
    {
        get
        {

            if (instance == null)
            {
                instance = UnityEngine.Object.FindObjectOfType<Pvr_ControllerManager>();
            }
            if (instance == null)
            {
                var go = new GameObject("GameObject");
                instance = go.AddComponent<Pvr_ControllerManager>();
                go.transform.localPosition = Vector3.zero;
            }
            return instance;
        }
    }
    #region Properties

    public static bool longPressclock;//长按键锁，保证长按抬起后不响应短按up
    public static Pvr_ControllerLink controllerlink;
    private float cTime = 1.0f;
    public int Swipewidth = 100;  //滑动值，0-255，滑动超过此值，则判定为成功
    private float longpresstime = 0.5f; //长按键0.5s响应长按
    private bool stopConnect; //解绑手柄锁
    private SystemLanguage localanguage;
    public Text toast;
    private bool controllerServicestate;
    private float disConnectTime;
    private int triggernum;
    public  bool ShowPowerToast;

    #endregion

    //HB Controller 连接状态
    public delegate void ControllerStatusChange(string isconnect);
    public static event ControllerStatusChange ControllerStatusChangeEvent;
    //CV ControllerThread启动成功
    public delegate void ControllerThreadStartedCallback();
    public static event ControllerThreadStartedCallback ControllerThreadStartedCallbackEvent;
    //CV service Bind成功的回调
    public delegate void SetControllerServiceBindState();
    public static event SetControllerServiceBindState SetControllerServiceBindStateEvent;
    //主控手改变
    public delegate void ChangeMainControllerCallBack(string index);
    public static event ChangeMainControllerCallBack ChangeMainControllerCallBackEvent;
    //CV Controller 连接状态改变拓展版（增加能力显示）
    //推荐使用这个，这个携带能力，下边的基础版回调不带能力
    public delegate void SetControllerAbility(string data);
    public static event SetControllerAbility SetControllerAbilityEvent;
    //CV Controller 连接状态改变
    public delegate void SetControllerStateChanged(string data);
    public static event SetControllerStateChanged SetControllerStateChangedEvent;
    //HB Mac 地址
    public delegate void SetHbControllerMac(string mac);
    public static event SetHbControllerMac SetHbControllerMacEvent;
    //获取版本号
    public delegate void ControllerDeviceVersionCallback(string data);
    public static event ControllerDeviceVersionCallback ControllerDeviceVersionCallbackEvent;
    //获取手柄SN号
    public delegate void ControllerSnCodeCallback(string data);
    public static event ControllerSnCodeCallback ControllerSnCodeCallbackEvent;
    //手柄解绑
    public delegate void ControllerUnbindCallback(string status);
    public static event ControllerUnbindCallback ControllerUnbindCallbackEvent;
    //station工作状态
    public delegate void ControllerStationStatusCallback(string status);
    public static event ControllerStationStatusCallback ControllerStationStatusCallbackEvent;
    //station忙碌的状态
    public delegate void ControllerStationBusyCallback(string status);
    public static event ControllerStationBusyCallback ControllerStationBusyCallbackEvent;
    //OTA升级错误
    public delegate void ControllerOtaStartCodeCallback(string data);
    public static event ControllerOtaStartCodeCallback ControllerOtaStartCodeCallbackEvent;
    //手柄版本号和SN号回调
    public delegate void ControllerDeviceVersionAndSNCallback(string data);
    public static event ControllerDeviceVersionAndSNCallback ControllerDeviceVersionAndSNCallbackEvent;
    //手柄唯一识别码回调
    public delegate void ControllerUniqueIDCallback(string data);
    public static event ControllerUniqueIDCallback ControllerUniqueIDCallbackEvent;
    //解绑手柄后的回调
    public delegate void ControllerCombinedKeyUnbindCallback(string data);
    public static event ControllerCombinedKeyUnbindCallback ControllerCombinedKeyUnbindCallbackEvent;
    /*************************************  Unity API ****************************************/
    #region Unity API
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (instance != this)
        {
            Debug.LogError("instance object should be a singleton.");
            return;
        }
        if (controllerlink == null)
        {
            controllerlink = new Pvr_ControllerLink(this.gameObject.name);
        }
    }
    // Use this for initialization
    void Start()
    {
        localanguage = Application.systemLanguage;

        if (controllerlink.trackingmode != 2 && controllerlink.trackingmode != 3)
        {
            Invoke("CheckControllerService", 10.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_ANDROID
        //区分平台
        if (controllerlink.cvserviceBindState)
        {
            if (controllerlink.controller0Connected)
            {
                //手柄0姿态数据
                var pose0 = controllerlink.GetCvControllerPoseData(0);
                controllerlink.Controller0.Rotation = new Quaternion(pose0[0], pose0[1], pose0[2], pose0[3]);
                controllerlink.Controller0.Position = new Vector3(pose0[4] / 1000.0f, pose0[5] / 1000.0f, -pose0[6] / 1000.0f);
                //手柄0键值数据
                var key0 = controllerlink.GetCvControllerKeyData(0);
                controllerlink.Controller0.TouchPadPosition = new Vector2(key0[0], key0[1]);
                //滑动手势判断
                SetSwipeData(controllerlink.Controller0);
                //touchpad点击
                SetTouchPadClick(controllerlink.Controller0);
                //HomeKey
                TransformData(controllerlink.Controller0.HomeKey, key0[2]);
                //AppKey
                TransformData(controllerlink.Controller0.AppKey, key0[3]);
                //TouchClickKey
                TransformData(controllerlink.Controller0.TouchKey, key0[4]);
                //VolumeuUpKey
                TransformData(controllerlink.Controller0.VolumeUpKey, key0[5]);
                //VolumeDownKey
                TransformData(controllerlink.Controller0.VolumeDownKey, key0[6]);
                //Trigger num
                controllerlink.Controller0.TriggerNum = key0[7];
                //Trigger Click
                SetTriggerClick(controllerlink.Controller0);
                //Battery
                controllerlink.Controller0.Battery = key0[8];
            }
            if (controllerlink.controller1Connected)
            {
                //手柄1姿态数据
                var pose1 = controllerlink.GetCvControllerPoseData(1);
                controllerlink.Controller1.Rotation = new Quaternion(pose1[0], pose1[1], pose1[2], pose1[3]);
                controllerlink.Controller1.Position = new Vector3(pose1[4] / 1000.0f, pose1[5] / 1000.0f, -pose1[6] / 1000.0f);
                //手柄0键值数据
                var key1 = controllerlink.GetCvControllerKeyData(1);
                controllerlink.Controller1.TouchPadPosition = new Vector2(key1[0], key1[1]);
                //滑动手势判断
                SetSwipeData(controllerlink.Controller1);
                //touchpad点击
                SetTouchPadClick(controllerlink.Controller1);
                //HomeKey
                TransformData(controllerlink.Controller1.HomeKey, key1[2]);
                //AppKey
                TransformData(controllerlink.Controller1.AppKey, key1[3]);
                //TouchClickKey
                TransformData(controllerlink.Controller1.TouchKey, key1[4]);
                //VolumeuUpKey
                TransformData(controllerlink.Controller1.VolumeUpKey, key1[5]);
                //VolumeDownKey
                TransformData(controllerlink.Controller1.VolumeDownKey, key1[6]);
                //Trigger num
                controllerlink.Controller1.TriggerNum = key1[7];
                //Trigger Click
                SetTriggerClick(controllerlink.Controller1);
                //Battery
                controllerlink.Controller1.Battery = key1[8];
            }
            
        }
        //Goblin controller
        if (controllerlink.hbserviceBindState && controllerlink.controller0Connected)
        {
            //手柄0姿态数据
            var pose0 = controllerlink.GetHBControllerPoseData();
            var jpose = JsonMapper.ToObject(pose0);
            controllerlink.Controller0.Rotation = new Quaternion(Convert.ToSingle(jpose[1].ToString()), Convert.ToSingle(jpose[2].ToString()), Convert.ToSingle(jpose[3].ToString()), Convert.ToSingle(jpose[0].ToString()));
            //手柄0键值数据
            var key0 = controllerlink.GetHBControllerKeyData();
            var jkey = JsonMapper.ToObject(key0);
            controllerlink.Controller0.TouchPadPosition = new Vector2(Convert.ToInt16(jkey[0].ToString()), Convert.ToInt16(jkey[1].ToString()));
            //滑动手势判断
            SetSwipeData(controllerlink.Controller0);
            //touchpad点击
            SetTouchPadClick(controllerlink.Controller0);
            //HomeKey
            TransformData(controllerlink.Controller0.HomeKey, Convert.ToInt16(jkey[2].ToString()));
            //AppKey
            TransformData(controllerlink.Controller0.AppKey, Convert.ToInt16(jkey[3].ToString()));
            //TouchClickKey
            TransformData(controllerlink.Controller0.TouchKey, Convert.ToInt16(jkey[4].ToString()));
            //VolumeuUpKey
            TransformData(controllerlink.Controller0.VolumeUpKey, Convert.ToInt16(jkey[5].ToString()));
            //VolumeDownKey
            TransformData(controllerlink.Controller0.VolumeDownKey, Convert.ToInt16(jkey[6].ToString()));
            //Battery
            controllerlink.Controller0.Battery = Convert.ToInt16(jkey[7].ToString());
            
        }
        //系统按键相关
        SetSystemKey();
#endif
        #region IOSData
#if IOS_DEVICE
        Controller.PVR_GetLark2SensorMessage(ref lark2x, ref lark2y, ref lark2z, ref lark2w);
        Controller.ControllerQua = new Quaternion(lark2x, lark2y, lark2z, lark2w);

        Controller.PVR_GetLark2KeyValueMessage(ref lark2touchx, ref lark2touchy, ref lark2home, ref lark2app, ref lark2click, ref lark2volup, ref lark2voldown, ref lark2power);
        if (lark2touchx > 0 || lark2touchy > 0)
        {
            if (lark2touchx == 0)
            {
                TouchPadPosition.x = 1;
            }
            if (lark2touchy == 0)
            {
                TouchPadPosition.y = 1;
            }
            TouchPadPosition.x = lark2touchx;
            TouchPadPosition.y = lark2touchy;
        }
        else
        {
            touchNum++;
            if (touchNum >= 1)
            {
                TouchPadPosition.x = 0;
                TouchPadPosition.y = 0;
                touchNum = 0;
            }
        }
        Controller.BatteryLevel = lark2power;
        #region base api 
        //键值状态
        //Home Key
        if (lark2home == 1)
        {
            if (!HomeKey.state)
            {
                HomeKey.pressedDown = true;
                longPressclock = false;
            }
            else
            {
                HomeKey.pressedDown = false;
            }
            HomeKey.state = true;
        }
        else
        {
            if (HomeKey.state)
            {
                HomeKey.pressedUp = true;
            }
            else
            {
                HomeKey.pressedUp = false;
            }
            HomeKey.state = false;
            HomeKey.pressedDown = false;
        }
        //APP Key
        if (lark2app == 1)
        {
            if (!APPKey.state)
            {
                APPKey.pressedDown = true;
                longPressclock = false;
            }
            else
            {
                APPKey.pressedDown = false;
            }
            APPKey.state = true;
        }
        else
        {
            if (APPKey.state)
            {
                APPKey.pressedUp = true;
            }
            else
            {
                APPKey.pressedUp = false;
            }
            APPKey.state = false;
            APPKey.pressedDown = false;
        }
        //Touchpad Key
        if (lark2click == 1)
        {
            if (!TouchPadKey.state)
            {
                TouchPadKey.pressedDown = true;
                longPressclock = false;
            }
            else
            {
                TouchPadKey.pressedDown = false;
            }
            TouchPadKey.state = true;
        }
        else
        {
            if (TouchPadKey.state)
            {
                TouchPadKey.pressedUp = true;
            }
            else
            {
                TouchPadKey.pressedUp = false;
            }
            TouchPadKey.state = false;
            TouchPadKey.pressedDown = false;
        }
        //VolumeUP Key
        if (lark2volup == 1)
        {
            if (!VolumeUpKey.state)
            {
                VolumeUpKey.pressedDown = true;
                longPressclock = false;
            }
            else
            {
                VolumeUpKey.pressedDown = false;
            }
            VolumeUpKey.state = true;
        }
        else
        {
            if (VolumeUpKey.state)
            {
                VolumeUpKey.pressedUp = true;
            }
            else
            {
                VolumeUpKey.pressedUp = false;
            }
            VolumeUpKey.state = false;
            VolumeUpKey.pressedDown = false;
        }
        //VolumeDown Key
        if (lark2voldown == 1)
        {
            if (!VolumeDownKey.state)
            {
                VolumeDownKey.pressedDown = true;
                longPressclock = false;
            }
            else
            {
                VolumeDownKey.pressedDown = false;
            }
            VolumeDownKey.state = true;
        }
        else
        {
            if (VolumeDownKey.state)
            {
                VolumeDownKey.pressedUp = true;
            }
            else
            {
                VolumeDownKey.pressedUp = false;
            }
            VolumeDownKey.state = false;
            VolumeDownKey.pressedDown = false;
        }


        #endregion

        #region extended api
        //打开扩展API后，提供长按和滑动功能
        if (ExtendedAPI)
        {
            //slip
            if (TouchPadPosition.x > 0 || TouchPadPosition.y > 0)
            {
                if (!touchClock)
                {
                    touchXBegin = TouchPadPosition.x;
                    touchYBegin = TouchPadPosition.y;
                    touchClock = true;
                }
                touchXEnd = TouchPadPosition.x;
                touchYEnd = TouchPadPosition.y;
            }
            else
            {
                if (touchXEnd > touchXBegin)
                {
                    if (touchYEnd > touchYBegin)
                    {
                        if (touchXEnd - touchXBegin > slipNum && ((touchXEnd - touchXBegin) > (touchYEnd - touchYBegin)))
                        {
                            //slide up
                            TouchPadKey.slideup = true;
                        }
                        if (touchYEnd - touchYBegin > slipNum && ((touchYEnd - touchYBegin) > (touchXEnd - touchXBegin)))
                        {
                            //slide right
                            TouchPadKey.slideright = true;
                        }
                    }
                    else if (touchYEnd < touchYBegin)
                    {
                        if (touchXEnd - touchXBegin > slipNum && ((touchXEnd - touchXBegin) > (touchYBegin - touchYEnd)))
                        {
                            //slide up
                            TouchPadKey.slideup = true;
                        }
                        if (touchYBegin - touchYEnd > slipNum && ((touchYBegin - touchYEnd) > (touchXEnd - touchXBegin)))
                        {
                            //slide left
                            TouchPadKey.slideleft = true;
                        }
                    }

                }
                else if (touchXEnd < touchXBegin)
                {
                    if (touchYEnd > touchYBegin)
                    {
                        if (touchXBegin - touchXEnd > slipNum && ((touchXBegin - touchXEnd) > (touchYEnd - touchYBegin)))
                        {
                            //slide down
                            TouchPadKey.slidedown = true;
                        }
                        if (touchYEnd - touchYBegin > slipNum && ((touchYEnd - touchYBegin) > (touchXBegin - touchXEnd)))
                        {
                            //slide right
                            TouchPadKey.slideright = true;
                        }
                    }
                    else if (touchYEnd < touchYBegin)
                    {
                        if (touchXBegin - touchXEnd > slipNum && ((touchXBegin - touchXEnd) > (touchYBegin - touchYEnd)))
                        {
                            //slide down 
                            TouchPadKey.slidedown = true;
                        }
                        if (touchYBegin - touchYEnd > slipNum && ((touchYBegin - touchYEnd) > (touchXBegin - touchXEnd)))
                        {
                            //slide left
                            TouchPadKey.slideleft = true;
                        }
                    }
                }
                else
                {
                    TouchPadKey.slideright = false;
                    TouchPadKey.slideleft = false;
                    TouchPadKey.slidedown = false;
                    TouchPadKey.slideup = false;
                }
                touchXBegin = 0;
                touchXEnd = 0;
                touchYBegin = 0;
                touchYEnd = 0;
                touchClock = false;
            }

            //longpress
            if (HomeKey.state)
            {
                HomeKey.timecount += Time.deltaTime;
                if (HomeKey.timecount >= longPressTime && !HomeKey.longPressedClock)
                {
                    HomeKey.longPressed = true;
                    HomeKey.longPressedClock = true;
                    longPressclock = true;
                }
                else
                {
                    HomeKey.longPressed = false;
                }
            }
            else
            {
                HomeKey.longPressedClock = false;
                HomeKey.timecount = 0;
                HomeKey.longPressed = false;
            }
            if (APPKey.state)
            {
                APPKey.timecount += Time.deltaTime;
                if (APPKey.timecount >= longPressTime && !APPKey.longPressedClock)
                {
                    APPKey.longPressed = true;
                    APPKey.longPressedClock = true;
                    longPressclock = true;
                }
                else
                {
                    APPKey.longPressed = false;
                }
            }
            else
            {
                APPKey.longPressedClock = false;
                APPKey.timecount = 0;
                APPKey.longPressed = false;
            }
            if (TouchPadKey.state)
            {
                TouchPadKey.timecount += Time.deltaTime;
                if (TouchPadKey.timecount >= longPressTime && !TouchPadKey.longPressedClock)
                {
                    TouchPadKey.longPressed = true;
                    TouchPadKey.longPressedClock = true;
                    longPressclock = true;
                }
                else
                {
                    TouchPadKey.longPressed = false;
                }
            }
            else
            {
                TouchPadKey.longPressedClock = false;
                TouchPadKey.timecount = 0;
                TouchPadKey.longPressed = false;
            }
            if (VolumeUpKey.state)
            {
                VolumeUpKey.timecount += Time.deltaTime;
                if (VolumeUpKey.timecount >= longPressTime && !VolumeUpKey.longPressedClock)
                {
                    VolumeUpKey.longPressed = true;
                    VolumeUpKey.longPressedClock = true;
                    longPressclock = true;
                }
                else
                {
                    VolumeUpKey.longPressed = false;
                }
            }
            else
            {
                VolumeUpKey.longPressedClock = false;
                VolumeUpKey.timecount = 0;
                VolumeUpKey.longPressed = false;
            }
            if (VolumeDownKey.state)
            {
                VolumeDownKey.timecount += Time.deltaTime;
                if (VolumeDownKey.timecount >= longPressTime && !VolumeDownKey.longPressedClock)
                {
                    VolumeDownKey.longPressed = true;
                    VolumeDownKey.longPressedClock = true;
                    longPressclock = true;
                }
                else
                {
                    VolumeDownKey.longPressed = false;
                }
            }
            else
            {
                VolumeDownKey.longPressedClock = false;
                VolumeDownKey.timecount = 0;
                VolumeDownKey.longPressed = false;
            }

        }
        if (Controller.UPvr_GetKeyLongPressed(0, Pvr_KeyCode.HOME))
        {
            Pvr_UnitySDKManager.pvr_UnitySDKSensor.ResetUnitySDKSensor();
            ResetController(0);
        }
        #endregion
#endif
        #endregion
    }
    void OnApplicationQuit()
    {
        var headdof = Pvr_UnitySDKManager.SDK.HeadDofNum == HeadDofNum.SixDof ? 1 : 0;
        var handdof = Pvr_UnitySDKManager.SDK.HandDofNum == HandDofNum.SixDof ? 1 : 0;
        //仅在CV设备&当前启动CV服务时
        if (controllerlink.cvserviceBindState)
        {
            controllerlink.StopControllerThread(headdof, handdof);
        }
            
    }
    private void OnApplicationPause(bool pause)
    {
        var headdof = Pvr_UnitySDKManager.SDK.HeadDofNum == HeadDofNum.SixDof ? 1 : 0;
        var handdof = Pvr_UnitySDKManager.SDK.HandDofNum == HandDofNum.SixDof ? 1 : 0;
        if (pause)
        {
            //仅在CV设备&当前启动CV服务时
            if (controllerlink.cvserviceBindState)
            {
                controllerlink.SetGameObjectToJar("");
                controllerlink.StopControllerThread(headdof, handdof);
            }
            else if(controllerlink.hbserviceBindState)
            {
                controllerlink.StopLark2Receiver();
            }
                
        }
        else
        {
            if (controllerlink.cvserviceBindState)
            {
                controllerlink.SetGameObjectToJar(controllerlink.gameobjname);
                controllerlink.StartControllerThread(headdof, handdof);
            }
            else if (controllerlink.hbserviceBindState)
            {
                controllerlink.StartLark2Receiver();
                controllerlink.controller0Connected = GetControllerConnectionState(0) == 2;
                if (!controllerlink.controller0Connected)
                {
                    controllerlink.Controller0 = new ControllerHand();
                }
            }

        }
    }
    #endregion

    /************************************ Public Interfaces  *********************************/
    #region Public Interfaces


    public void StopLark2Service()
    {
        if (controllerlink != null)
        {
            controllerlink.StopLark2Service();
        }
    }

    public Vector3 GetAngularVelocity(int num)
    {
        if (controllerlink != null)
        {
            return controllerlink.GetAngularVelocity(num);
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    public Vector3 GetAcceleration(int num)
    {
        if (controllerlink != null)
        {
            return controllerlink.GetAcceleration(num);
        }
        return new Vector3(0.0f, 0.0f, 0.0f);
    }

    public void BindService()
    {
        if (controllerlink != null)
        {
            controllerlink.BindService();
        }
    }
    public void StartScan()
    {
#if ANDROID_DEVICE
        if (controllerlink != null)
        {
            controllerlink.StartScan();
        }
#elif IOS_DEVICE
        TouchPad.ScanIOSBLEDevice(2);
#endif
    }
    public void StopScan()
    {
        if (controllerlink != null)
        {
            controllerlink.StopScan();
        }
    }
    /// <summary>
    /// Reset手柄，参数为0,1。假如一个手柄或者使用HB手柄，则传0
    /// </summary>
    /// <param name="num"></param>
    public void ResetController(int num)
    {
        if (controllerlink != null)
        {
            controllerlink.ResetController(num);
        }
    }
    public static int GetControllerConnectionState(int num)
    {
        var sta = controllerlink.GetControllerConnectionState(num);
        return sta;
    }
    public void ConnectBLE()
    {
#if ANDROID_DEVICE
        if (controllerlink != null)
        {
            controllerlink.ConnectBLE();
        }
#elif IOS_DEVICE
        TouchPad.ConnectIOSBLEDevice(controllerlink.hummingBirdMac);
#endif

    }
    public void DisConnectBLE()
    {
        if (controllerlink != null)
        {
            controllerlink.DisConnectBLE();
        }
    }

    public void SetBinPath(string path, bool isAsset)
    {
        if (controllerlink != null)
        {
            controllerlink.setBinPath(path, isAsset);
        }
    }
    public void StartUpgrade()
    {
        if (controllerlink != null)
        {
            controllerlink.StartUpgrade();
        }
    }
    public static string GetBLEImageType()
    {
        var type = controllerlink.GetBLEImageType();
        return type;
    }
    public static long GetBLEVersion()
    {
        var version = controllerlink.GetBLEVersion();
        return version;
    }
    public static string GetFileImageType()
    {
        var type = controllerlink.GetFileImageType();
        return type;
    }
    public static long GetFileVersion()
    {
        var version = controllerlink.GetFileVersion();
        return version;
    }
    public static void AutoConnectHbController(int scans)
    {
        if (controllerlink != null)
        {
            controllerlink.AutoConnectHbController(scans);
        }
    }
    public static string GetConnectedDeviceMac()
    {
        string mac = "";
        if (controllerlink != null)
        {
            mac = controllerlink.GetConnectedDeviceMac();
        }
        return mac;
    }
    //--------------
    public void setHbControllerMac(string mac)
    {
        PLOG.I("HBMacRSSI" + mac);
        controllerlink.hummingBirdMac = mac.Substring(0, 17);
        controllerlink.hummingBirdRSSI = Convert.ToInt16(mac.Remove(0, 18));
        if (SetHbControllerMacEvent != null)
            SetHbControllerMacEvent(mac.Substring(0, 17));
    }
    public int GetControllerRSSI()
    {
        return controllerlink.hummingBirdRSSI;
    }

    public void setHbServiceBindState(string state)
    {
        PLOG.I("HBBindCallBack" + state);
        controllerServicestate = true;
        //state：0-已解绑，1-已绑定，2-超时
        if (Convert.ToInt16(state) == 0)
        {
            Invoke("BindService", 0.5f);
            controllerlink.hbserviceBindState = false;
        }
        else if (Convert.ToInt16(state) == 1)
        {
            controllerlink.hbserviceBindState = true;
            controllerlink.controller0Connected = GetControllerConnectionState(0) == 2;
        }
    }
    public void setControllerServiceBindState(string state)
    {
        PLOG.I("CVBindCallBack" + state);
        //state:0 unbind,1:bind
        if (Convert.ToInt16(state) == 0)
        {
            Invoke("BindService", 0.5f);
            controllerlink.cvserviceBindState = false;
        }
        else if (Convert.ToInt16(state) == 1)
        {
            controllerlink.cvserviceBindState = true;
            if (SetControllerServiceBindStateEvent != null)
                SetControllerServiceBindStateEvent();
            var headdof = Pvr_UnitySDKManager.SDK.HeadDofNum == HeadDofNum.SixDof ? 1 : 0;
            var handdof = Pvr_UnitySDKManager.SDK.HandDofNum == HandDofNum.SixDof ? 1 : 0;
            controllerlink.StartControllerThread(headdof, handdof);
        }
    }
    public void setHbControllerConnectState(string isconnect)
    {
        PLOG.I("HBControllerConnect" + isconnect);
        if (ControllerStatusChangeEvent != null)
            ControllerStatusChangeEvent(isconnect);
        controllerlink.controller0Connected = Convert.ToInt16(isconnect) == 1;
        if (!controllerlink.controller0Connected)
            controllerlink.Controller0 = new ControllerHand();
        //state：0-断开，1-已连接，2-未知
        stopConnect = false;
    }
    //neo controller状态改变回调
    public void setControllerStateChanged(string state)
    {
        //state的格式为0,0 第一个数代表手柄，第二个数代表状态
        PLOG.I("CVControllerStateChanged" + state);
        if (SetControllerStateChangedEvent != null)
            SetControllerStateChangedEvent(state);
        int controller = Convert.ToInt16(state.Substring(0, 1));

        if (controller == 0)
        {
            controllerlink.controller0Connected = Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1)));
            if (!controllerlink.controller0Connected)
                controllerlink.Controller0 = new ControllerHand();
        }
        else
        {
            controllerlink.controller1Connected = Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1)));
            if (!controllerlink.controller1Connected)
                controllerlink.Controller1 = new ControllerHand();
        }
        if (Convert.ToBoolean(Convert.ToInt16(state.Substring(2, 1))))
        {
            controllerlink.ResetController(controller);
        }

    }
    //neo controller状态改变回调（包含手柄能力3,6）
    public void setControllerAbility(string data)
    {
        //data 格式为 ID,ability,state
        //ID 0/1 分别代表两个手柄
        //ability 1/2 1:3dof手柄 2:6dof手柄
        //state 0/1 0：断开 1：连接
        //此回调为setControllerStateChanged的扩展版，在此回调基础上增加了当前手柄的能力
        PLOG.I("setControllerAbility" + data);
        if (SetControllerAbilityEvent != null)
            SetControllerAbilityEvent(data);
    }
    //startThread成功的回调
    public void controllerThreadStartedCallback()
    {
        PLOG.I("ThreadStartSuccess");
        if (ControllerThreadStartedCallbackEvent != null)
            ControllerThreadStartedCallbackEvent();

        GetCVControllerState();
    }
    ///以下是关于CV手柄的相关回调
    //版本号的回调
    public void controllerDeviceVersionCallback(string data)
    {
        PLOG.I("VersionCallBack" + data);
        //data格式 device,deviceVersion
        //device : 0-station 1-手柄0  2-手柄1 deviceVersion:版本号
        if (ControllerDeviceVersionCallbackEvent != null)
            ControllerDeviceVersionCallbackEvent(data);
    }
    //手柄SN号回调
    public void controllerSnCodeCallback(string data)
    {
        PLOG.I("SNCodeCallBack" + data);
        //data格式：controllerSerialNum,controllerSn
        //controllerSerialNum : 0-手柄0  1-手柄1 controllerSn:Sn号 手柄的唯一标识
        if (ControllerSnCodeCallbackEvent != null)
            ControllerSnCodeCallbackEvent(data);
    }
    //手柄解绑回调
    public void controllerUnbindCallback(string status)
    {
        PLOG.I("ControllerUnBindCallBack" + status);
        // status: 0-失败  1-成功 
        if (ControllerUnbindCallbackEvent != null)
            ControllerUnbindCallbackEvent(status);
    }
    //station工作状态回调
    public void controllerStationStatusCallback(string status)
    {
        PLOG.I("StationStatusCallBack" + status);
        //STATION_STATUS{NORMAL = 0, QUERYING = 1, PAIRING = 2, OTA = 3, RESTARTING = 4, CTRLR_UNBINDING = 5, CTRLR_SHUTTING_DOWN = 6};
        if (ControllerStationStatusCallbackEvent != null)
            ControllerStationStatusCallbackEvent(status);
    }
    //station忙碌状态回调
    public void controllerStationBusyCallback(string status)
    {
        PLOG.I("StationBusyCallBack" + status);
        //当前station处于繁忙状态无法响应本次控制接口功能    status：标识当前正忙于什么工作状态
        //STATION_STATUS{NORMAL = 0, QUERYING, PAIRING, OTA, RESTARTING, CTRLR_UNBINDING, CTRLR_SHUTTING_DOWN};
        if (ControllerStationBusyCallbackEvent != null)
            ControllerStationBusyCallbackEvent(status);
    }
    //OTA升级失败回调
    public void controllerOTAStartCodeCallback(string data)
    {
        PLOG.I("OTAUpdateCallBack" + data);
        //data格式:deviceType,statusCode
        // deviceType:0-station 1-controller statusCode: 0-升级发起成功  1-升级文件未找到  2-升级文件打开失败
        if (ControllerOtaStartCodeCallbackEvent != null)
            ControllerOtaStartCodeCallbackEvent(data);
    }
    //手柄版本号和SN号回调 
    public void controllerDeviceVersionAndSNCallback(string data)
    {
        PLOG.I("DeviceVersionAndSNCallback" + data);
        //data格式 controllerSerialNum,deviceVersion
        //controllerSerialNum : 0-手柄0  1-手柄1 deviceVersion:版本号和SN号
        if (ControllerDeviceVersionAndSNCallbackEvent != null)
            ControllerDeviceVersionAndSNCallbackEvent(data);
    }
    //手柄唯一识别码回调
    public void controllerUniqueIDCallback(string data)
    {
        PLOG.I("controllerUniqueIDCallback" + data);
        //data格式 controller0ID，controller1ID
        //controller0ID ：手柄0的ID；controller1ID：手柄1的ID （如果当前手柄未连接时ID返回为0）
        if (ControllerUniqueIDCallbackEvent != null)
            ControllerUniqueIDCallbackEvent(data);
    }
    //解绑CV手柄后的回调
    public void controllerCombinedKeyUnbindCallback(string controllerSerialNum)
    {
        //controllerSerialNum 0：手柄0 1：手柄1
        if (ControllerCombinedKeyUnbindCallbackEvent != null)
            ControllerCombinedKeyUnbindCallbackEvent(controllerSerialNum);
    }
    public void setupdateFailed()
    {
        //回调方法
    }

    public void setupdateSuccess()
    {
        //回调方法
    }

    public void setupdateProgress(string progress)
    {
        //回调方法
        //升级进度 0-100 
    }

    public void setHbAutoConnectState(string state)
    {
        PLOG.I("HBAutoConnectState" + state);
        //UNKNOW = -1; //默认值
        //NO_DEVICE = 0;//没有扫描到HB手柄
        //ONLY_ONE = 1;//只扫描到一只HB手柄
        //MORE_THAN_ONE = 2;// 扫描到多只HB手柄
        //LAST_CONNECTED = 3;//扫描到上一次连接过的HB手柄
        //FACTORY_DEFAULT = 4;//扫描到工厂绑定的HB手柄（暂未启用）
        controllerServicestate = true;
        if (Convert.ToInt16(state) == 0)
        {
            if (GetControllerConnectionState(0) == 0)
            {
                ShowToast(2);
            }
        }
        if (Convert.ToInt16(state) == 2)
        {
            ShowToast(3);
        }
    }

    public void callbackControllerServiceState(string state)
    {
        PLOG.I("HBServiceState" + state);
        //state = 0,非手机平台，服务没有启动
        //state = 1,手机平台，服务没有启动，但是系统会主动启动服务
        //state = 2,手机平台，服务apk没有安装，需要安装
        controllerServicestate = true;
        if (Convert.ToInt16(state) == 0)
        {
            ShowToast(0);
        }
        if (Convert.ToInt16(state) == 1)
        {
            BindService();
        }
        if (Convert.ToInt16(state) == 2)
        {
            ShowToast(1);
        }
    }
    //主控手改变回调
    public void changeMainControllerCallback(string index)
    {
        PLOG.I("MainControllerCallBack" + index);
        //index = 0/1
        if (ChangeMainControllerCallBackEvent != null)
            ChangeMainControllerCallBackEvent(index);
    }

    private void ShowToast(int type)
    {
        switch (type)
        {
            case 0: //非手机平台，手柄服务没有启动
                if (toast != null)
                {
                    if (localanguage == SystemLanguage.Chinese || localanguage == SystemLanguage.ChineseSimplified)
                    {
                        toast.text = "手柄服务未启动，请先启动手柄服务";
                    }
                    else
                    {
                        toast.text = "No handle service found, please turnon the handle service first";
                    }
                    Invoke("HideToast", 5.0f);
                }
                break;
            case 1: //手机平台，服务apk没安装，提示安装
                if (toast != null)
                {
                    if (localanguage == SystemLanguage.Chinese || localanguage == SystemLanguage.ChineseSimplified)
                    {
                        toast.text = "未发现手柄服务，请使用PicoVR下载并安装";
                    }
                    else
                    {
                        toast.text = "No handle service found, please use PicoVR to download and install";
                    }
                    Invoke("HideToast", 5.0f);
                }
                break;
            case 2: //没有扫描到手柄
                if (toast != null)
                {
                    if (localanguage == SystemLanguage.Chinese || localanguage == SystemLanguage.ChineseSimplified)
                    {
                        toast.text = "没有扫描到手柄，请确保手机蓝牙开启，并短按手柄Home键";
                    }
                    else
                    {
                        toast.text = "Can not find any handle, please turn on bluetooth and press handle home key";
                    }
                    AutoConnectHbController(6000);
                    Invoke("HideToast", 5.0f);
                }

                break;
            case 3: //扫描到多个手柄
                if (toast != null)
                {
                    if (localanguage == SystemLanguage.Chinese || localanguage == SystemLanguage.ChineseSimplified)
                    {
                        toast.text = "扫描到多个手柄，请保持周围只有一个开启状态的手柄";
                    }
                    else
                    {
                        toast.text = "Find more than one handle, turn off the unused handle";
                    }
                    AutoConnectHbController(6000);
                    Invoke("HideToast", 5.0f);
                }
                break;
            case 4: //服务没有启动
                if (toast != null)
                {
                    if (localanguage == SystemLanguage.Chinese || localanguage == SystemLanguage.ChineseSimplified)
                    {
                        toast.text = "手柄服务启动异常，请检查后台权限及安全设置";
                    }
                    else
                    {
                        toast.text = "The handle service starts abnormally. Please check the background permissions and security settings";
                    }
                }
                break;
            default:
                return;
        }
    }
    private void HideToast()
    {
        if (toast != null)
        {
            toast.text = "";
        }
    }

    private void CheckControllerService()
    {
        if (!controllerServicestate)
        {
            ShowToast(4);
        }
    }

    private void GetCVControllerState()
    {
        var state0 = GetControllerConnectionState(0);
        var state1 = GetControllerConnectionState(1);
        PLOG.I("CVconnect" + state0 + state1);
        if (state0 == -1 && state1 == -1)
        {
            Invoke("GetCVControllerState", 0.02f);
        }
        if (state0 != -1 && state1 != -1)
        {
            controllerlink.controller0Connected = state0 == 1;
            controllerlink.controller1Connected = state1 == 1;
        }
    }

    private void SetSystemKey()
    {
        if (controllerlink.switchHomeKey)
        {
            if (Pvr_UnitySDKManager.SDK.HeadDofNum == HeadDofNum.ThreeDof || Pvr_UnitySDKManager.SDK.SixDofRecenter)
            {
                if (Controller.UPvr_GetKeyLongPressed(0, Pvr_KeyCode.HOME))
                {
                    Pvr_UnitySDKManager.pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 0);
                    if (Pvr_UnitySDKManager.SDK.HeadDofNum == HeadDofNum.SixDof && Pvr_UnitySDKManager.SDK.SixDofRecenter)
                    {
                        controllerlink.ResetHeadSensorForController();
                    }
                    ResetController(0);
                }
                if (Controller.UPvr_GetKeyLongPressed(1, Pvr_KeyCode.HOME))
                {
                    Pvr_UnitySDKManager.pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 0);
                    if (Pvr_UnitySDKManager.SDK.HeadDofNum == HeadDofNum.SixDof && Pvr_UnitySDKManager.SDK.SixDofRecenter)
                    {
                        controllerlink.ResetHeadSensorForController();
                    }
                    ResetController(1);
                }
            }
            else
            {
                if (Controller.UPvr_GetKeyLongPressed(0, Pvr_KeyCode.HOME))
                {
                    ResetController(0);
                }
                if (Controller.UPvr_GetKeyLongPressed(1, Pvr_KeyCode.HOME))
                {
                    ResetController(1);
                }
            }
        }
        
        //pico device支持的系统按键
        if (controllerlink.picoDevice)
        {
            if (controllerlink.switchHomeKey)
            {
                if (!longPressclock && (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.HOME) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.HOME)) && !stopConnect)
                {
                    controllerlink.RebackToLauncher();
                }
            }
            if (!longPressclock && (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.VOLUMEUP) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.VOLUMEUP)))
            {
                controllerlink.TurnUpVolume();
            }
            if (!longPressclock && (Controller.UPvr_GetKeyUp(0, Pvr_KeyCode.VOLUMEDOWN) || Controller.UPvr_GetKeyUp(1, Pvr_KeyCode.VOLUMEDOWN)))
            {
                controllerlink.TurnDownVolume();
            }
            if (!Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEUP) && !Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) && !Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEUP) && !Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEDOWN))
            {
                cTime = 1.0f;
            }
            if (Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEUP) || Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEUP))
            {
                cTime -= Time.deltaTime;
                if (cTime <= 0)
                {
                    cTime = 0.2f;
                    controllerlink.TurnUpVolume();
                }
            }
            if (Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) || Controller.UPvr_GetKey(1, Pvr_KeyCode.VOLUMEDOWN))
            {
                cTime -= Time.deltaTime;
                if (cTime <= 0)
                {
                    cTime = 0.2f;
                    controllerlink.TurnDownVolume();
                }
            }
        }
        if (controllerlink.trackingmode == 1 || controllerlink.trackingmode == 0)
        {
            //phone和pico device都支持解绑
            if (Controller.UPvr_GetKey(0, Pvr_KeyCode.HOME) && Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN) && !stopConnect)
            {
                disConnectTime += Time.deltaTime;
                if (disConnectTime > 1.0)
                {
                    DisConnectBLE();
                    controllerlink.hummingBirdMac = "";
                    stopConnect = true;
                    disConnectTime = 0;
                }
            }
        }
    }

    private void SetSwipeData(ControllerHand hand)
    {
        if (hand.TouchPadPosition != Vector2.zero)
        {
            if (!hand.touchClock)
            {
                hand.touchDownPosition = hand.TouchPadPosition;
                hand.touchClock = true;
            }
            hand.touchUpPosition = hand.TouchPadPosition;
        }
        else
        {

            hand.swipeData = hand.touchUpPosition - hand.touchDownPosition;
            hand.isVertical = Mathf.Abs(hand.swipeData.x) > Swipewidth;
            hand.isHorizontal = Mathf.Abs(hand.swipeData.y) > Swipewidth;

            if (hand.swipeData.x > 0f && hand.isVertical)
            {
                hand.SwipeDirection = SwipeDirection.SwipeUp;
            }
            else if (hand.swipeData.x < 0f && hand.isVertical)
            {
                hand.SwipeDirection = SwipeDirection.SwipeDown;
            }
            else if (hand.swipeData.y > 0f && hand.isHorizontal)
            {
                hand.SwipeDirection = SwipeDirection.SwipeRight;
            }
            else if (hand.swipeData.y < 0f && hand.isHorizontal)
            {
                hand.SwipeDirection = SwipeDirection.SwipeLeft;
            }
            else
            {
                hand.SwipeDirection = SwipeDirection.No;
            }
            hand.touchDownPosition = Vector2.zero;
            hand.touchUpPosition = Vector2.zero;
            hand.touchClock = false;
        }
    }

    private void SetTriggerClick(ControllerHand hand)
    {
        if (hand.TriggerNum >= 255)
        {
            if (!hand.triggerClock)
            {
                hand.triggerClick = true;
                hand.triggerClock = true;
            }
            else
            {
                hand.triggerClick = false;
            }
        }
        else
        {
            hand.triggerClick = false;
            hand.triggerClock = false;
        }
    }
    private void SetTouchPadClick(ControllerHand hand)
    {
        if (hand.TouchKey.State)
        {
            if (hand.TouchPadPosition.x <= 255 && hand.TouchPadPosition.x >= 127f + 63.5f * Mathf.Sin(45) &&
                hand.TouchPadPosition.y <= 127f + 63.5f * Mathf.Sin(45) &&
                hand.TouchPadPosition.y >= 127f - 63.5f * Mathf.Sin(45))
            {
                hand.TouchPadClick = TouchPadClick.ClickUp;
            }
            else if (hand.TouchPadPosition.x > 0 && hand.TouchPadPosition.x <= 127f - 63.5f * Mathf.Sin(45) &&
                     hand.TouchPadPosition.y <= 127f + 63.5f * Mathf.Sin(45) &&
                     hand.TouchPadPosition.y >= 127f - 63.5f * Mathf.Sin(45))
            {
                hand.TouchPadClick = TouchPadClick.ClickDown;
            }
            else if (hand.TouchPadPosition.y > 0 && hand.TouchPadPosition.y <= 127f - 63.5f * Mathf.Sin(45) &&
                     hand.TouchPadPosition.x <= 127f + 63.5f * Mathf.Sin(45) &&
                     hand.TouchPadPosition.x >= 127f - 63.5f * Mathf.Sin(45))
            {
                hand.TouchPadClick = TouchPadClick.ClickLeft;
            }
            else if (hand.TouchPadPosition.y <= 255 && hand.TouchPadPosition.y >= 127f + 63.5f * Mathf.Sin(45) &&
                     hand.TouchPadPosition.x <= 127f + 63.5f * Mathf.Sin(45) &&
                     hand.TouchPadPosition.x >= 127f - 63.5f * Mathf.Sin(45))
            {
                hand.TouchPadClick = TouchPadClick.ClickRight;
            }
            else
            {
                hand.TouchPadClick = TouchPadClick.No;
            }
        }
        else
        {
            hand.TouchPadClick = TouchPadClick.No;
        }
    }

    private void TransformData(PvrControllerKey key, int keystate)
    {
        if (keystate == 1)
        {
            key.PressedUp = false;
            if (!key.State)
            {
                key.PressedDown = true;
                longPressclock = false;
            }
            else
            {
                key.PressedDown = false;
            }
            key.State = true;
        }
        else
        {
            key.PressedUp = key.State;
            key.State = false;
            key.PressedDown = false;
        }
        if (key.State)
        {
            key.TimeCount += Time.deltaTime;
            if (key.TimeCount >= longpresstime &&
                !key.LongPressedClock)
            {
                key.LongPressed = true;
                key.LongPressedClock = true;
                longPressclock = true;
            }
            else
            {
                key.LongPressed = false;
            }
        }
        else
        {
            key.LongPressedClock = false;
            key.TimeCount = 0;
            key.LongPressed = false;
        }
    }

    #endregion

}
