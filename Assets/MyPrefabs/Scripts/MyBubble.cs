using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyBubble : MonoBehaviour {
    public static int BubbleCnt;                 //泡泡计数
    public bool isRotate = true;                 //是否开启旋转
    public int RotateSpeed = 50;                 //旋转的速度
    public bool isMove = true;                   //是否开启移动
    public float MaxMoveSpeed = 2;               //移动的最大速度
    private float MoveSpeed;                     //移动速度
    public float MaxLifeTime = 15;
    private float LifeTime;
    private int directions;
    private float ZoomWidth = 7f;               //泡泡活动在以原点为中心的正方形区域内
    public float BubbleSize;
    public static float staticBubbleSize;       // 只是为了方便其他脚本访问BubbleSize

    public GameObject ParticleSystem;
   
    // Use this for initialization
    void Start () {
        BubbleCnt++;
        staticBubbleSize = BubbleSize;
        LifeTime = Random.Range(1, MaxLifeTime);
        MoveSpeed = Random.Range(0f, MaxMoveSpeed);
        // 初始化位置和大小
        float fBound = 5;
        float nx = UnityEngine.Random.Range(-1* fBound, fBound);
        float ny = UnityEngine.Random.Range(-0.5f, 2);
        float nz = UnityEngine.Random.Range(-1 * fBound, fBound);
        transform.position = new Vector3(nx, ny, nz);
        //大小
        transform.localScale = new Vector3(BubbleSize, BubbleSize, BubbleSize);
        directions = Random.Range(1, 5);
    }

    // Update is called once per frame
    void Update () {
        if (isRotate)
        {
            RotateAxisOfSelf(SelfAxis.Y, RotateSpeed);
        }
        if (isMove) {
            Move();
        }
        LifeTime -= Time.deltaTime;
        if (LifeTime < 0) {
            Destroy(this.gameObject);
        }
    }

    //void FixedUpdate()
    void Move()
    {
        if (directions == 1)
        {
            transform.position += new Vector3(MoveSpeed * Time.deltaTime, 0, 0);
            //Quaternion lookRot = Quaternion.LookRotation(new Vector3(1f, 0, 0));
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, MoveSpeed * Time.deltaTime);
        }
        if (directions == 2)
        {
            transform.position -= new Vector3(MoveSpeed * Time.deltaTime, 0, 0);
            //Quaternion lookRot = Quaternion.LookRotation(new Vector3(-1f, 0, 0));
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, MoveSpeed * Time.deltaTime);
        }
        if (directions == 3)
        {
            transform.position += new Vector3(0, 0, MoveSpeed * Time.deltaTime);
            //Quaternion lookRot = Quaternion.LookRotation(new Vector3(0, 0, 1f));
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, MoveSpeed * Time.deltaTime);
        }
        if (directions == 4)
        {
            transform.position -= new Vector3(0, 0, MoveSpeed * Time.deltaTime);
            //Quaternion lookRot = Quaternion.LookRotation(new Vector3(0, 0, -1f));
            //transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, MoveSpeed * Time.deltaTime);
        }
        if (transform.position.x > ZoomWidth)
        {
            transform.position = new Vector3(ZoomWidth, transform.position.y, transform.position.z);
            if (directions == 1 || directions == 3)
            {
                directions++;
            }
            else
                directions--;
        }
        if (transform.position.x < -1*ZoomWidth)
        {
            transform.position = new Vector3(-1* ZoomWidth, transform.position.y, transform.position.z);
            if (directions == 1 || directions == 3)
            {
                directions++;
            }
            else
                directions--;
        }
        if (transform.position.z > ZoomWidth)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, ZoomWidth);
            if (directions == 1 || directions == 3)
            {
                directions++;
            }
            else
                directions--;
        }
        if (transform.position.z < -1* ZoomWidth)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, ZoomWidth);
            if (directions == 1 || directions == 3)
            {
                directions++;
            }
            else
                directions--;
        }
        //directions = Random.Range(1, 5);
    }

    void OnDestroy() {
        BubbleCnt--;
        GameObject part = GameObject.Instantiate(ParticleSystem, transform.position, Quaternion.Euler(1, 0, 0));
        Destroy(part, 1f);
    }

    /// 让物体绕自身的轴旋转
    private void RotateAxisOfSelf(SelfAxis selfAxis, int speed = 50)
    {
        switch (selfAxis)
        {
            case SelfAxis.X:
                this.transform.Rotate(new Vector3(1 * Time.deltaTime * speed, 0, 0));
                break;
            case SelfAxis.Y:
                this.transform.Rotate(new Vector3(0, 1 * Time.deltaTime * speed, 0));
                break;
            case SelfAxis.Z:
                this.transform.Rotate(new Vector3(0, 0, 1 * Time.deltaTime * speed));
                break;
            default:
                this.transform.Rotate(new Vector3(1 * Time.deltaTime * speed, 0, 0));
                break;
        }
    }

    //枚举轴
    enum SelfAxis
    {
        X,
        Y,
        Z,
    }

    void OnCollisionEnter(Collision collision)
    {
        int v;
        do {
            v = Random.Range(1, 4);
        } while (v == directions);
        directions = v;
    }
}
