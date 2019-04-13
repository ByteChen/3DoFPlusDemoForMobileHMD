using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;		// required for DllImport
using System.IO;                            // required for File

public class SphereAPP : MonoBehaviour
{
    // com.compilelife.mediacodecexample
    //public String packageName = "com/mmc/Exp2/MainActivity";
    
    private enum MediaSurfaceEventType
    {
        MS_EVENT_INIT = 0,
        MS_EVENT_SHUTDOWN = 1,
        MS_EVENT_UPDATE = 2
    };

    // LogWriter:write log to file
    private class LogWriter
    {
        public string sFilePath = "/storage/emulated/0/Download";
        public string sFileName = "PositionTrack.log";
        public FileStream fs;
        public StreamWriter sw;
        //public string logStr;

        // 初始化打印LOG的帮助类
        public LogWriter()
        {
            // 创建Writer,当前路径都是绝对的
            CreateStreamWriter();
        }

        public void CreateStreamWriter()
        {
            if (sw != null && fs != null)
            {
                Destory();
            }

            sFileName = sFilePath + "/" + sFileName; //文件的绝对路径
            if (!Directory.Exists(sFilePath))   //验证路径是否存在
            {
                Directory.CreateDirectory(sFilePath);   //不存在则创建
            }
            if (File.Exists(sFileName)) //验证文件是否存在，无则创建
            {
                File.Delete(sFileName);
            }
            fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
            sw = new StreamWriter(fs);
        }

        public void WriteLogStr(string logStr)
        {
            if (sw != null)
            {
                sw.WriteLine("jianzhao " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "   ---   " + logStr);
                sw.Flush();
            }
        }

        public void Destory()
        {
            // Destory the log writer
            if (sw != null)
            {
                sw.Close();
            }
            if (fs != null)
            {
                fs.Close();
            }
        }
    };
    // end LogWriter

    // 测试时间工具类
    System.Diagnostics.Stopwatch stopwatch;

    // logWriter
    private LogWriter logwriter;
    private string logStr;

    // 分辨率等信息
    private int width = 3840;
    private int height = 1920;

    private GameObject SphereObj;
    private Material SphereMaterial;
    private Texture SphereTexture;

    private IntPtr SphereTextureNativePtr = IntPtr.Zero;
    private Texture2D texture;
    private IntPtr androidSurface = IntPtr.Zero;
    static private AndroidJavaObject javaPlayer = null;

    // 统计信息
    int framecount = 0;

    // 6DOF information
    private bool trackPosition = false;
    private Vector3 curPosition;

    //************************************************************ intral implement ************************************************************//
    // 通过底层代码获得 AndroidSurface
    private void getAndroidSurface()
    {
        // 初始化 OVR_Media_Surface
        OVR_Media_Surface_Init();
        Debug.Log("jianzhao OVR_Media_Surface_Init");
        OVR_InitMediaSurface();
        Debug.Log("jianzhao OVR_InitMediaSurface");

        // 获得球和指针
        SphereObj = null;
        SphereMaterial = null;
        SphereTexture = null;

        SphereObj = GameObject.Find("Sphere");
        if (null != SphereObj)
        {
            Debug.Log("find Sphere");
        }
        SphereObj.transform.position = new Vector3(0, 0, 0);
        //SphereMaterial.color = Color.black;
        SphereMaterial = SphereObj.GetComponent<Renderer>().material;

        width = 3840;
        height = 1920;
        texture = new Texture2D(width, height, TextureFormat.RGB24, false, true); // 格式RGB24 或者 RGBA32
        //SphereTexture = Resources.Load("testIMG") as Texture; // 测试全景使用
        //SphereMaterial.SetTexture("_MainTex", Resources.Load<Texture2D>("testIMG"));
        SphereMaterial.SetTexture("_MainTex", texture);

        SphereTextureNativePtr = SphereMaterial.GetTexture("_MainTex").GetNativeTexturePtr();
        Debug.Log("jianzhao Get SphereTextureNativePtr");

        androidSurface = OVR_Media_Surface(SphereTextureNativePtr, width, height);
        Debug.Log("jianzhao OVR_Media_Surface");

        if (androidSurface != null)
        {
            Debug.Log("jianzhao get androidSurface");
        }

        OVR_Media_Surface_SetEventBase((int)(MediaSurfaceEventType.MS_EVENT_INIT));
    }

    // 将AndroidSurface通过JNI传递过Android程序
    private void passSurfaceToAndroid()
    {
        //AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.example.lib4unity.PlayerActivity");
        //AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("m_instance");
        AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");

        // vrdemo/medialab/huawei/com/demoplayer/Vr2Player 这里是jar包的格式
        // com.compilelife.mediacodecexample
        javaPlayer = new AndroidJavaObject("com/mmc/Exp4/MainActivity"); 
        if (javaPlayer == null)
        {
            Debug.Log("jianzhao javaPlayer is null!!");
        }
        
        javaPlayer.Call<int>("init", androidJavaObject);
        IntPtr methodId = AndroidJNI.GetMethodID(javaPlayer.GetRawClass(), "setSurface", "(Landroid/view/Surface;)V");
        //IntPtr setSurfaceMethodId = AndroidJNI.GetMethodID(javaPlayer.GetRawClass(), "setSurface", "(Landroid/graphics/SurfaceTexture;)V");
        jvalue[] parms = new jvalue[1];
        parms[0] = new jvalue();
        parms[0].l = androidSurface;
        //AndroidJNI.CallObjectMethod(javaPlayer.GetRawObject(), setSurfaceMethodId, parms);
        AndroidJNI.CallVoidMethod(javaPlayer.GetRawObject(), methodId, parms);
        Debug.Log("jianzhao unity setSurfaceMethodId done!");
    }

    // 启动Android端程序
    private void startAndroidAPP()
    {
        IntPtr methodId = IntPtr.Zero;
        methodId = AndroidJNI.GetMethodID(javaPlayer.GetRawClass(), "UnityStart", "(I)V");
        if (methodId != IntPtr.Zero)
        {
            jvalue[] parms = new jvalue[1];
            parms[0] = new jvalue();
            int i = 203;
            parms[0].i = i;
            Debug.Log("jianzhao StartAppMethodId is not null!!");
            AndroidJNI.CallVoidMethod(javaPlayer.GetRawObject(), methodId, parms);
        }
        else
        {
            Debug.Log("jianzhao StartAppMethodId is null!!");
        }
    }

    // 传递位置信息给Android程序
    private void PassArrayToJava(Vector3 position)
    {
        float[] curPosition = { position.x, position.y, position.z};
        IntPtr jAryPtr = AndroidJNIHelper.ConvertToJNIArray(curPosition);
        jvalue[] parms = new jvalue[1];
        parms[0].l = jAryPtr;

        IntPtr methodId = AndroidJNIHelper.GetMethodID(javaPlayer.GetRawClass(), "updatePositionFromUnity");
        AndroidJNI.CallVoidMethod(javaPlayer.GetRawObject(), methodId, parms);
    }

    // 关闭Android端程序
    static private void CloseAndroidAPP()
    {
        Debug.Log("jianzhao CloseAndroidAPP!!!");
        //AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        //AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
        //javaPlayer = new AndroidJavaObject("com/compilelife/mediacodecexample/MainActivity");
        if (javaPlayer != null)
        {
            IntPtr methodId = IntPtr.Zero;
            methodId = AndroidJNI.GetMethodID(javaPlayer.GetRawClass(), "UnityClose", "(I)V");
            if (methodId != IntPtr.Zero)
            {
                jvalue[] parms = new jvalue[1];
                parms[0] = new jvalue();
                int i = 203;
                parms[0].i = i;
                AndroidJNI.CallVoidMethod(javaPlayer.GetRawObject(), methodId, parms);
            }
            else
            {
                Debug.Log("jianzhao CloseAndroidAPP::methodId is null!!!");
            }
        }
        else
        {
            Debug.Log("jianzhao CloseAndroidAPP::javaPlayer is null!!!");
        }

        Application.Quit();
    }

    static public void onDestroy()
    {
        CloseAndroidAPP();
    }

    //************************************************************ Unity Life cycle ************************************************************//
    void Awake()
    {
        Debug.Log("jianzhao SphereAPP Awake");

        // 初始化打印帮助类,打印的话只需要调用如下方法
        logwriter = new LogWriter();
        //logwriter.WriteLogStr(logStr);

        // 6DOF::Pvr_UnitySDKManager.SDK.HeadPose.Position
        curPosition.Set(0, 0, 0);

        getAndroidSurface();
        passSurfaceToAndroid();
    }

    void Start()
    {
        Debug.Log("jianzhao SphereAPP Start");
        startAndroidAPP();

        if (trackPosition)
        {
            //logStr = "curPosition: " + curPosition.x + " " + curPosition.y + " " + curPosition.z;
            //logwriter.WriteLogStr(logStr);
        }

    }

    void Update()
    {
        //Debug.Log("jianzhao SphereAPP Update");

        UnityRenderEvent((int)(MediaSurfaceEventType.MS_EVENT_UPDATE));
        framecount++;


        curPosition = Pvr_UnitySDKManager.SDK.HeadPose.Position;
        //if (trackPosition)
        //{
        //logStr = "curPosition: " + curPosition.x + " " + curPosition.y + " " + curPosition.z;
        //logwriter.WriteLogStr(logStr);
        //}

        //if (framecount % 2 == 0)
        //{
        //    PassArrayToJava(curPosition);
        //}

        // 测试求的大小有没有影响
        //curPosition.x = 0;
        //curPosition.y = 0;
        //curPosition.z = 0;
        SphereObj.transform.position = curPosition;
        PassArrayToJava(curPosition);

#if false // jianzhao test
        if (framecount == 30)
        {
            curPosition.x = 0;
            curPosition.y = 0;
            curPosition.z = 0;
            PassArrayToJava(curPosition);
        }
        else if (framecount == 90)
        {
            curPosition.x = 2;
            curPosition.y = 2;
            curPosition.z = 2;
            PassArrayToJava(curPosition);
        }
        else if (framecount == 200)
        {
            curPosition.x = 3;
            curPosition.y = 3;
            curPosition.z = 3;
            PassArrayToJava(curPosition);
        }
#endif
    }

    void Destory()
    {
        //logwriter.Destory();
    }

    //************************************************************ OVR APi START ****************************************//
    [DllImport("OculusMediaSurface")]
    private static extern void OVR_Media_Surface_Init();

    [DllImport("OculusMediaSurface")]
    private static extern void OVR_InitMediaSurface();

    // This function returns an Android Surface object that is
    // bound to a SurfaceTexture object on an independent OpenGL texture id.
    // Each frame, before the TimeWarp processing, the SurfaceTexture is checked
    // for updates, and if one is present, the contents of the SurfaceTexture
    // will be copied over to the provided surfaceTexId and mipmaps will be 
    // generated so normal Unity rendering can use it.
    [DllImport("OculusMediaSurface")]
    private static extern IntPtr OVR_Media_Surface(IntPtr surfaceTexId, int surfaceWidth, int surfaceHeight);

    [DllImport("OculusMediaSurface")]
    private static extern void OVR_Media_Surface_SetEventBase(int eventBase);

    [DllImport("OculusMediaSurface")]
    private static extern void UnityRenderEvent(int eventID);

}

