using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Pvr_UnitySDKAPI;
using System;
using UnityEngine.EventSystems;

public class DemoManager : MonoBehaviour
{
    [SerializeField]
    private Button TestBtn;
    [SerializeField]
    private Text KeyEvent;
    [SerializeField]
    private Slider SliderH;
    [SerializeField]
    private Slider SliderV;

    [SerializeField]
    private Material normalMaterial;
    [SerializeField]
    private Material overMaterial;
    [SerializeField]
    private Material clickMaterial;
    // bubble
    public bool HaveBubble; // 是否生成泡泡
    [SerializeField]
    private Material bubbleMaterial;
    public GameObject Bubbles;
    private float times;
    private Vector3 v3ControllPos;

    public Transform direction;
    private Ray ray;
    private bool noGOClick;
    private bool noBTClick;
    private int MaxBubbleCnt = 20;
    // Use this for initialization
    void Start()
    {
        ray = new Ray();
        ray.origin = transform.position;
        //Debug.Log("transform.position = " + transform.position.x + " " + transform.position.y + " " + transform.position.z + "\n");
    }

    // Update is called once per frame
    void Update()
    {
        KeyEventTest();
        SlideTest();
        ray.origin = transform.position;
        //Debug.Log("Update_transform.position = " + transform.position.x + " " + transform.position.y + " " + transform.position.z + "\n");
        ray.direction = direction.position - transform.position;

        RayCastOfGameObject();

        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.HOME))
            Application.Quit();

        if (HaveBubble)
        {
            times -= Time.deltaTime;  //减时间
            if (times < 0 && MyBubble.BubbleCnt < MaxBubbleCnt)  //倒计时
            {
                ////产生物体
                ////GameObject obj = (GameObject)Instantiate(Bubbles);
                //float nx = UnityEngine.Random.Range(-3, 3);
                //float ny = UnityEngine.Random.Range(0, 3);
                //float nz = UnityEngine.Random.Range(-3, 3);

                ////随机位置
                ////obj.transform.position = new Vector3(ni, nt, 5);

                ////GameObject.Instantiate(Bubbles, new Vector3(ni, nt, 5), Quaternion.identity);
                //GameObject.Instantiate(Bubbles, new Vector3(nx, ny, nz), Quaternion.Euler(1, 2, 3));

                GameObject.Instantiate(Bubbles);
                //重新设置时间为0-xx之间的一个随机数   随机时间
                times = UnityEngine.Random.Range(0, 0.7f);
            }
        }
    }

    private void KeyEventTest() // 坑爹，应该是KeyEventText的，搞得那么费解
    {
        if (Controller.UPvr_GetKey(0, Pvr_KeyCode.APP))
        {
            KeyEvent.text = "AppKey";
        }
        else if (Controller.UPvr_GetKey(0, Pvr_KeyCode.TOUCHPAD))
        {
            KeyEvent.text = "TouchPadKey";
        }
        //else if (Controller.UPvr_GetKey(0, Pvr_KeyCode.HOME))
        //{
        //    KeyEvent.text = "HomeKey";
        //}
        else if (Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEDOWN))
        {
            KeyEvent.text = "VolumeDownKey";
        }
        else if (Controller.UPvr_GetKey(0, Pvr_KeyCode.VOLUMEUP))
        {
            KeyEvent.text = "VolumeUpKey";
        }
        else
        {
            KeyEvent.text = "KeyEvent";
        }
    }

    private void SlideTest()
    {
        if (Controller.UPvr_GetSwipeDirection(0) == SwipeDirection.SwipeDown || Input.GetKeyDown(KeyCode.S))
        {
            if (SliderV.value > 0f)
                SliderV.value -= 0.1f;
        }
        if (Controller.UPvr_GetSwipeDirection(0) == SwipeDirection.SwipeUp || Input.GetKeyDown(KeyCode.W))
        {
            if (SliderV.value < 1.0f)
                SliderV.value += 0.1f;
        }
        if (Controller.UPvr_GetSwipeDirection(0) == SwipeDirection.SwipeLeft || Input.GetKeyDown(KeyCode.A))
        {
            if (SliderH.value > 0f)
                SliderH.value -= 0.1f;
        }
        if (Controller.UPvr_GetSwipeDirection(0) == SwipeDirection.SwipeRight || Input.GetKeyDown(KeyCode.D))
        {
            if (SliderH.value < 1.0f)
                SliderH.value += 0.1f;
        }
    }

    private void RayCastOfGameObject()
    {

        RaycastHit hitinfo;
        if (Physics.Raycast(ray, out hitinfo))
        {
            Debug.DrawLine(ray.origin, hitinfo.point);
            GameObject gameobj = hitinfo.collider.gameObject;

            if (gameobj.tag == "Cube")
            {
                if (!noGOClick)
                {
                    gameobj.GetComponent<MeshRenderer>().material = overMaterial;
                }

                if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TOUCHPAD) || Input.GetMouseButtonDown(0))
                {
                    gameobj.GetComponent<MeshRenderer>().material = clickMaterial;
                    noGOClick = true;
                }
            }

            // bytechen: 尝试对球形进行操作
            if (HaveBubble && gameobj.tag == "Bubble")
            {
                bool bDabao = false;
                if (!noGOClick)
                {
                    //gameobj.GetComponent<MeshRenderer>().material = overMaterial; // 如果保留这行，则手柄指中目标时，目标的纹理会显示为overMaterial

                    // 如果手柄在泡泡内，相当于用手柄去打击，这时候即使没有点击按键也会爆炸
                    Vector3 vec3PosDiff = transform.position - gameobj.transform.position;

                    //bool bHasHand = false;
                    //if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected)
                    //{
                    //    Debug.Log("controller_is 0");
                    //    bHasHand = true;
                    //    v3ControllPos = Controller.UPvr_GetControllerPOS(0);
                    //}
                    //if (Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
                    //{
                    //    Debug.Log("controller_is 1");
                    //    bHasHand = true;
                    //    v3ControllPos = Controller.UPvr_GetControllerPOS(1);
                    //}
                    //Debug.Assert(bHasHand == true);
                    //Debug.Log("v3ControllPos = " + v3ControllPos.x + " " + v3ControllPos.y + " " + v3ControllPos.z + "\n");

                    float fDistance = vec3PosDiff.x * vec3PosDiff.x + vec3PosDiff.y * vec3PosDiff.y + vec3PosDiff.z * vec3PosDiff.z;
                    //float fColliderRadius = gameobj.GetComponent<Collider>().bounds.size.x;
                    //fColliderRadius = 0.5f;
                    //if (fDistance <= 0.3)   //localScale = 1 时
                    //if (fDistance <= 0.075)   //localScale = 0.5 时,大概是平方的关系
                    if (fDistance <= MyBubble.staticBubbleSize * MyBubble.staticBubbleSize * 0.3)   //
                    {
                        Destroy(gameobj);
                        bDabao = true;
                    }
                }

                if (!bDabao && Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TOUCHPAD) || Input.GetMouseButtonDown(0))
                {
                    //gameobj.GetComponent<MeshRenderer>().material = clickMaterial;
                    //Destroy(gameobj.GetComponent<MeshRenderer>());
                    Destroy(gameobj);
                    noGOClick = true;
                }
            }

            if (gameobj.tag == "Button")
            {
                if (!noBTClick)
                {
                    gameobj.GetComponent<Button>().Select();
                }
            }
        }
        else
        {
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("Cube").Length; i++)
            {
                GameObject obj = GameObject.FindGameObjectsWithTag("Cube")[i];
                obj.GetComponent<MeshRenderer>().material = normalMaterial;
            }

            // bubble
            if (HaveBubble)
            {
                for (int i = 0; i < GameObject.FindGameObjectsWithTag("Bubble").Length; i++)
                {
                    GameObject obj = GameObject.FindGameObjectsWithTag("Bubble")[i];
                    obj.GetComponent<MeshRenderer>().material = bubbleMaterial;
                }
            }

            noGOClick = false;

            if (EventSystem.current.currentSelectedGameObject != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
            noBTClick = false;
        }
    }
}

