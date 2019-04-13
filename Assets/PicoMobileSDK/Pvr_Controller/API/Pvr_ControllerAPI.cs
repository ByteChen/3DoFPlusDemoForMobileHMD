///////////////////////////////////////////////////////////////////////////////
// Copyright 2015-2017  Pico Technology Co., Ltd. All Rights Reserved.
// File: Controller
// Author: Yangel.Yan
// Date:  2017/01/11
// Discription: The Controller API 
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
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace Pvr_UnitySDKAPI
{

    public class PvrControllerKey
    {
        public bool State;
        public bool PressedDown;
        public bool PressedUp;
        public bool LongPressed;
        public float TimeCount;
        public bool LongPressedClock;
        public PvrControllerKey()
        {
            State = false;
            PressedDown = false;
            PressedUp = false;
            LongPressed = false;
            TimeCount = 0;
            LongPressedClock = false;
        }
    }

    public class ControllerHand
    {
        public PvrControllerKey AppKey;
        public PvrControllerKey TouchKey;
        public PvrControllerKey HomeKey;
        public PvrControllerKey VolumeDownKey;
        public PvrControllerKey VolumeUpKey;
        public Vector2 TouchPadPosition;
        public int TriggerNum;
        public Quaternion Rotation;
        public Vector3 Position;
        public int Battery;
        public Vector2 touchDownPosition; 
        public Vector2 touchUpPosition;
        public Vector2 swipeData;
        public bool isVertical;
        public bool isHorizontal;
        public bool touchClock;
        public bool triggerClick;
        public bool triggerClock;
        public ControllerState ConnectState;
        public SwipeDirection SwipeDirection;
        public TouchPadClick TouchPadClick;

        public ControllerHand()
        {
            AppKey = new PvrControllerKey();
            TouchKey = new PvrControllerKey();
            HomeKey = new PvrControllerKey();
            VolumeDownKey = new PvrControllerKey();
            VolumeUpKey = new PvrControllerKey();
            TouchPadPosition = new Vector2();
            Rotation = new Quaternion();
            Position = new Vector3();
            touchDownPosition = new Vector2();
            touchUpPosition = new Vector2();
            swipeData = new Vector2();
            isVertical = false;
            isHorizontal = false;
            touchClock = false;
            triggerClick = false;
            triggerClock = false;
            Battery = 0;
            ConnectState = ControllerState.Error;
            SwipeDirection = SwipeDirection.No;
            TouchPadClick = TouchPadClick.No;
        }
    }

    public enum ControllerState
    {
        Error = -1,
        DisConnected = 0,
        Connecting = 1,
        Connected = 2,
    }
    /// <summary>
    /// 手柄按键值
    /// </summary>
    public enum Pvr_KeyCode
    {
        APP = 1,
        TOUCHPAD = 2,
        HOME = 3,
        VOLUMEUP = 4,
        VOLUMEDOWN = 5,
    }
    /// <summary>
    /// 手柄Touchpad滑动方向
    /// </summary>
    public enum SwipeDirection
    {
        No =0 ,
        SwipeUp = 1,
        SwipeDown = 2,
        SwipeRight = 3,
        SwipeLeft = 4,
    }

    /// <summary>
    /// 手柄Touchpad点击方向
    /// </summary>
    public enum TouchPadClick
    {
        No = 0,
        ClickUp = 1,
        ClickDown = 2,
        ClickRight = 3,
        ClickLeft = 4,
    }

    public struct Controller
    {
        /**************************** Private Static Funcations *******************************************/
#region Private Static Funcation

#if ANDROID_DEVICE
        public const string LibFileName = "Pvr_UnitySDK";
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_CalcArmModelParameters(float[] headOrientation,float[] controllerOrientation,float[] gyro);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetPointerPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetElbowPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetWristPose( float[] rotation,  float[] position);
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_GetShoulderPose( float[] rotation,  float[] position);
           [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_SetArmModelParameters(int hand, int gazeType, float elbowHeight, float elbowDepth, float pointerTiltAngle);
#endif
#endregion


        /**************************** Public Static Funcations *******************************************/
#region Public Static Funcation  

        
        public static Vector2 UPvr_GetTouchPadPosition(int hand)
        {
            switch (hand)
            {
                case 0:
                {
                    var postion = Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition;
                    return postion;
                }
                case 1:
                {
                    var postion = Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition;
                    return postion;
                }
            }
            return new Vector2(0, 0);
        }

        public static ControllerState UPvr_GetControllerState(int hand)
        {
            switch (hand)
            {
                case 0:
                    if (Pvr_ControllerManager.controllerlink.cvserviceBindState)
                    {
                        Pvr_ControllerManager.controllerlink.Controller0.ConnectState = Pvr_ControllerManager.GetControllerConnectionState(0) == 1 ? ControllerState.Connected : ControllerState.DisConnected;
                    }
                    else
                    {
                        switch (Pvr_ControllerManager.GetControllerConnectionState(0))
                        {
                            case 2:
                                Pvr_ControllerManager.controllerlink.Controller0.ConnectState = ControllerState.Connected;
                                break;
                            case 1:
                                Pvr_ControllerManager.controllerlink.Controller0.ConnectState = ControllerState.Connecting;
                                break;
                            default:
                                Pvr_ControllerManager.controllerlink.Controller0.ConnectState = ControllerState.DisConnected;
                                break;
                        }
                    }
                    return Pvr_ControllerManager.controllerlink.Controller0.ConnectState;
                case 1:
                    if (Pvr_ControllerManager.controllerlink.cvserviceBindState)
                    {
                        Pvr_ControllerManager.controllerlink.Controller1.ConnectState = Pvr_ControllerManager.GetControllerConnectionState(1) == 1 ? ControllerState.Connected : ControllerState.DisConnected;
                    }
                    return Pvr_ControllerManager.controllerlink.Controller1.ConnectState;
                 
            }
            return ControllerState.Error;
        }
        /// <summary>
        /// 获取手柄rotation数据
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <returns></returns>
        public static Quaternion UPvr_GetControllerQUA(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Rotation;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Rotation;
            }
            return new Quaternion(0, 0, 0, 1);
        }
        /// <summary>
        /// 获取手柄position数据
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <returns></returns>
        public static Vector3 UPvr_GetControllerPOS(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Position;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Position;
            }
            return new Vector3(0, 0, 0);
        }
        /// <summary>
        /// 获取手柄trigger键的值（仅适用于CV Controller）
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static int Upvr_GetControllerTriggerValue(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.TriggerNum;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.TriggerNum;
            }
            return 0;
        }

        /// <summary>
        /// 手柄trigger点击功能（仅适用于CV Controller）
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static bool Upvr_ControllerTriggerClick(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.triggerClick;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.triggerClick;
            }
            return false;
        }
        /// <summary>
        /// 获取手柄的电量，cv电量为1-10，goblin电量为1-4
        /// </summary>
        public static int UPvr_GetControllerPower(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.Battery;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.Battery;
            }
            return 0;
        }
        /// <summary>
        /// 获取触摸板的滑动方向
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static SwipeDirection UPvr_GetSwipeDirection(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.SwipeDirection;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.SwipeDirection;
            }
            return SwipeDirection.No;
        }
        /// <summary>
        /// 获取触摸板的点击方向
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public static TouchPadClick UPvr_GetTouchPadClick(int hand)
        {
            switch (hand)
            {
                case 0:
                    return Pvr_ControllerManager.controllerlink.Controller0.TouchPadClick;
                case 1:
                    return Pvr_ControllerManager.controllerlink.Controller1.TouchPadClick;
            }
            return TouchPadClick.No;
        }

        /// <summary>
        /// 获取按键状态，持续性状态，按下为true，抬起为false
        /// </summary>
        /// <param name="hand">0,1，假如只有一个手柄则传0</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool UPvr_GetKey(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.AppKey.State;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.HomeKey.State;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.TouchKey.State;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUpKey.State;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDownKey.State;
                    default:
                        return false;
                }
            }
            if (hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.AppKey.State;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.HomeKey.State;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.TouchKey.State;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUpKey.State;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDownKey.State;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取按键的按下状态，瞬时性状态，只在按下的时候响应一次
        /// </summary>
        /// <param name="hand">0,1</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool UPvr_GetKeyDown(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.AppKey.PressedDown;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.HomeKey.PressedDown;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.TouchKey.PressedDown;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUpKey.PressedDown;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDownKey.PressedDown;
                    default:
                        return false;
                }
            }
            if(hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.AppKey.PressedDown;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.HomeKey.PressedDown;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.TouchKey.PressedDown;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUpKey.PressedDown;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDownKey.PressedDown;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取按键的按下状态，瞬时性状态，只在按下的时候响应一次
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool UPvr_GetKeyUp(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.AppKey.PressedUp;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.HomeKey.PressedUp;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.TouchKey.PressedUp;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUpKey.PressedUp;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDownKey.PressedUp;
                    default:
                        return false;
                }
            }
            if (hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.AppKey.PressedUp;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.HomeKey.PressedUp;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.TouchKey.PressedUp;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUpKey.PressedUp;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDownKey.PressedUp;
                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取Key的状态，仅当长按0.5s时为true，一次性事件
        /// </summary>
        public static bool UPvr_GetKeyLongPressed(int hand, Pvr_KeyCode key)
        {
            if (hand == 0)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller0.AppKey.LongPressed;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller0.HomeKey.LongPressed;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller0.TouchKey.LongPressed;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeUpKey.LongPressed;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller0.VolumeDownKey.LongPressed;
                    default:
                        return false;
                }
            }
            if (hand == 1)
            {
                switch (key)
                {
                    case Pvr_KeyCode.APP:
                        return Pvr_ControllerManager.controllerlink.Controller1.AppKey.LongPressed;
                    case Pvr_KeyCode.HOME:
                        return Pvr_ControllerManager.controllerlink.Controller1.HomeKey.LongPressed;
                    case Pvr_KeyCode.TOUCHPAD:
                        return Pvr_ControllerManager.controllerlink.Controller1.TouchKey.LongPressed;
                    case Pvr_KeyCode.VOLUMEUP:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeUpKey.LongPressed;
                    case Pvr_KeyCode.VOLUMEDOWN:
                        return Pvr_ControllerManager.controllerlink.Controller1.VolumeDownKey.LongPressed;
                    default:
                        return false;
                }
            }
            return false;
        }

        public static bool UPvr_IsTouching(int hand)
        {
            const float tolerance = 0;
            switch (hand)
            {
                case 0:
                {
                    return Math.Abs(Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.x) > tolerance ||
                           Math.Abs(Pvr_ControllerManager.controllerlink.Controller0.TouchPadPosition.y) > tolerance;
                }
                case 1:
                {
                    return Math.Abs(Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.x) > tolerance ||
                           Math.Abs(Pvr_ControllerManager.controllerlink.Controller1.TouchPadPosition.y) > tolerance;
                }
            }
            return false;
        }
        /// <summary>
        /// 获取当前主控手为哪个 0/1
        /// </summary>
        /// <returns></returns>
        public static int UPvr_GetMainHandNess()
        {
            return Pvr_ControllerManager.controllerlink.GetMainControllerIndex();
        }
        /// <summary>
        /// 设置当前手柄为主控手
        /// </summary>
        public static void UPvr_SetMainHandNess(int hand)
        {
            Pvr_ControllerManager.controllerlink.SetMainController(hand);
        }
        /// <summary>
        /// 获取当前手柄的能力（3dof/6dof）
        /// </summary>
        /// <param name="hand">0/1</param>
        /// <returns>-1:错误码 0：默认值（可以认为是6dof）  1：3dof手柄 2:6dof手柄</returns>
        public static int UPvr_GetControllerAbility(int hand)
        {
            return Pvr_ControllerManager.controllerlink.GetControllerAbility(hand);
        }
        //获取版本号 deviceType：0-station 1-手柄0  2-手柄1
        public void UPvr_GetDeviceVersion(int deviceType)
        {
            Pvr_ControllerManager.controllerlink.GetDeviceVersion(deviceType);
        }
        //获取手柄Sn号  controllerSerialNum : 0-手柄0  1-手柄1
        public void UPvr_GetControllerSnCode(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.GetControllerSnCode(controllerSerialNum);
        }
        //解绑手柄 controllerSerialNum : 0-手柄0  1-手柄1
        public void UPvr_SetControllerUnbind(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.SetControllerUnbind(controllerSerialNum);
        }
        //重启station
        public void UPvr_SetStationRestart()
        {
            Pvr_ControllerManager.controllerlink.SetStationRestart();
        }
        //发起station OTA升级
        public void UPvr_StartStationOtaUpdate()
        {
            Pvr_ControllerManager.controllerlink.StartStationOtaUpdate();
        }
        //发起手柄ota升级 mode：1-RF 升级通讯模块 2-升级STM32模块 ； controllerSerialNum : 0-手柄0  1-手柄1
        public void UPvr_StartControllerOtaUpdate(int mode, int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.StartControllerOtaUpdate(mode, controllerSerialNum);
        }
        // 进入配对模式 controllerSerialNum：0-手柄0  1-手柄1
        public void UPvr_EnterPairMode(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.EnterPairMode(controllerSerialNum);
        }
        //手柄关机controllerSerialNum：0-手柄0  1-手柄1
        public void UPvr_SetControllerShutdown(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.SetControllerShutdown(controllerSerialNum);
        }
        // 获取当前station的配对状态 返回值0-未配对状态 1-正在配对状态
        public int UPvr_GetStationPairState()
        {
            return Pvr_ControllerManager.controllerlink.GetStationPairState();
        }
        //获取station ota升级进度
        public int UPvr_GetStationOtaUpdateProgress()
        {
            return Pvr_ControllerManager.controllerlink.GetStationOtaUpdateProgress();
        }
        //获取Controller ota升级进度
        //正常0-100
        //异常 101 : 没有收到成功升级的标识 102：手柄没有进入升级状态 103：升级中断异常
        public int UPvr_GetControllerOtaUpdateProgress()
        {
            return Pvr_ControllerManager.controllerlink.GetControllerOtaUpdateProgress();
        }
        //同时获取手柄的版本号和SN号  controllerSerialNum：0-手柄0  1-手柄1
        public void UPvr_GetControllerVersionAndSN(int controllerSerialNum)
        {
            Pvr_ControllerManager.controllerlink.GetControllerVersionAndSN(controllerSerialNum);
        }
        //获取手柄的唯一识别码
        public void UPvr_GetControllerUniqueID()
        {
            Pvr_ControllerManager.controllerlink.GetControllerUniqueID();
        }
        //将station从当前的配对模式中中断出来
        public void UPvr_InterruptStationPairMode()
        {
            Pvr_ControllerManager.controllerlink.InterruptStationPairMode();
        }

        // <summary>
        // 获取手柄的陀螺仪数据
        // </summary>
        public static Vector3 Upvr_GetAngularVelocity(int num)
        {
            Vector3 Aglr = new Vector3(0.0f, 0.0f, 0.0f);
#if ANDROID_DEVICE
            Aglr = Pvr_ControllerManager.Instance.GetAngularVelocity(num);
#elif IOS_DEVICE
            float[] Angulae = new float[3] { 0, 0, 0 };
            getHbAngularVelocity(Angulae);
            Aglr = new Vector3(Angulae[0], Angulae[1], Angulae[2]);
#endif
            return Aglr;
        }       

        public static Vector3 Upvr_GetAcceleration(int num)
        {
            Vector3 Acc = new Vector3(0.0f, 0.0f, 0.0f);
#if ANDROID_DEVICE
            Acc = Pvr_ControllerManager.Instance.GetAcceleration(num);
#elif IOS_DEVICE
            float[] Accel = new float[3] { 0, 0, 0 };
            getHbAcceleration(Accel);
            Acc = new Vector3(Accel[0], Accel[1], Accel[2]);
#endif
            return Acc;
        }
        public static void UPvr_SetArmModelParameters(int hand, int gazeType, float elbowHeight, float elbowDepth, float pointerTiltAngle)
        {
#if ANDROID_DEVICE || IOS_DEVICE
            Pvr_SetArmModelParameters( hand,  gazeType,  elbowHeight,  elbowDepth,  pointerTiltAngle);
#endif
        }

        public static void UPvr_CalcArmModelParameters(float[] headOrientation, float[] controllerOrientation, float[] controllerPrimary)
        {
#if ANDROID_DEVICE || IOS_DEVICE
            Pvr_CalcArmModelParameters( headOrientation,  controllerOrientation, controllerPrimary);
#endif
        }
        public static void UPvr_GetPointerPose( float[] rotation,  float[] position)
        {
#if ANDROID_DEVICE || IOS_DEVICE
            Pvr_GetPointerPose(  rotation,  position);
#endif
        }
        public static void UPvr_GetElbowPose( float[] rotation,  float[] position)
        {
#if ANDROID_DEVICE || IOS_DEVICE
            Pvr_GetElbowPose(  rotation,   position);
#endif
        }
        public static void UPvr_GetWristPose( float[] rotation,  float[] position)
        {
#if ANDROID_DEVICE || IOS_DEVICE
            Pvr_GetWristPose(  rotation,  position);
#endif
        }
        public static void UPvr_GetShoulderPose( float[] rotation,  float[] position)
        {
#if ANDROID_DEVICE || IOS_DEVICE
            Pvr_GetShoulderPose(  rotation,   position);
#endif
        }

#if IOS_DEVICE
        [DllImport("__Internal")]
        private static extern void Pvr_SetArmModelParameters(int hand, int gazeType, float elbowHeight, float elbowDepth, float pointerTiltAngle);
        [DllImport("__Internal")]
        private static extern void Pvr_CalcArmModelParameters(float[] headOrientation, float[] controllerOrientation, float[] gyro);
        [DllImport("__Internal")]
        private static extern void Pvr_GetPointerPose(float[] rotation, float[] position);
        [DllImport("__Internal")]
        private static extern void Pvr_GetElbowPose(float[] rotation, float[] position);
        [DllImport("__Internal")]
        private static extern void Pvr_GetWristPose(float[] rotation, float[] position);
        [DllImport("__Internal")]
        private static extern void Pvr_GetShoulderPose(float[] rotation, float[] position);
        [DllImport("__Internal")]
	    public static extern void PVR_GetLark2SensorMessage (ref float x, ref float y, ref float z, ref float w);
        [DllImport("__Internal")]
        public static extern void PVR_GetLark2KeyValueMessage(ref int touchpadx, ref int touchpady, ref int home, ref int app, ref int click, ref int volup, ref int voldown, ref int power);
        [DllImport("__Internal")]
        public static extern void getHbAngularVelocity(float[] gyro);
        [DllImport("__Internal")]
        public static extern void getHbAcceleration(float[] acce);
        [DllImport("__Internal")]
        public static extern int Pvr_ResetSensor(int index); //reset sensor index = 3
#endif
#endregion
    }

}
