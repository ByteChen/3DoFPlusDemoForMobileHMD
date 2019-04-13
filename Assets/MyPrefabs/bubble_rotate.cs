using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bubble_rotate : MonoBehaviour {

    public bool isOpen = true;                 //是否开始旋转
    public int speed = 50;                   //旋转的速度

    public float MoveSpeed = 1;
    public int channge = 1;

    // Use this for initialization
    void Start()
    {

    }

#if false
    void FixedUpdate() {
        if (channge == 1)
        {
            transform.position += new Vector3(MoveSpeed * Time.deltaTime, 0, 0);
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(1f, 0, 0));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, MoveSpeed * Time.deltaTime);
        }
        if (channge == 2)
        {
            transform.position -= new Vector3(MoveSpeed * Time.deltaTime, 0, 0);
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(-1f, 0, 0));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, MoveSpeed * Time.deltaTime);
        }
        if (channge == 3)
        {
            transform.position += new Vector3(0, 0, MoveSpeed * Time.deltaTime);
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(0, 0, 1f));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, MoveSpeed * Time.deltaTime);
        }
        if (channge == 4)
        {
            transform.position -= new Vector3(0, 0, MoveSpeed * Time.deltaTime);
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(0, 0, -1f));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, MoveSpeed * Time.deltaTime);
        }
        if (transform.position.x > 20f)
        {
            transform.position = new Vector3(20f, transform.position.y, transform.position.z);
            if (channge == 1 || channge == 3)
            {
                channge++;
            }
            else
                channge--;
        }
        if (transform.position.x < -20f)
        {
            transform.position = new Vector3(-20f, transform.position.y, transform.position.z);
            if (channge == 1 || channge == 3)
            {
                channge++;
            }
            else
                channge--;
        }
        if (transform.position.z > 20f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 20f);
            if (channge == 1 || channge == 3)
            {
                channge++;
            }
            else
                channge--;
        }
        if (transform.position.z < -20f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 20f);
            if (channge == 1 || channge == 3)
            {
                channge++;
            }
            else
                channge--;
        }
        channge = Random.Range(1, 5);
    }
#endif

    // Update is called once per frame
    void Update()
    {
        if (isOpen)
        {
            RotateAxisOfSelf(SelfAxis.Y, speed);
        }


#if false
        Vector3 velocity = Vector3.zero;//速度
        float moveSpeed = 5;
        Vector3 target = Vector3.zero; 
        if (Random.value < 0.01f)
            target = transform.position + Quaternion.Euler(0, Random.value * 360, 0) * Vector3.right * 10;//随机指定目标
        Vector3 direct = target - transform.position;
        direct.y = 0;//防止y方向移动
        if (direct.sqrMagnitude > 1)
        {
            transform.rotation = Quaternion.LookRotation(direct);//改变朝向
            velocity = direct.normalized * moveSpeed / 3;
        }
        velocity -= GetComponent<Rigidbody>().velocity;
        velocity.y = 0;

        //速度过大时减速
        if (velocity.sqrMagnitude > moveSpeed * moveSpeed)
            velocity = velocity.normalized * moveSpeed;

        GetComponent<Rigidbody>().AddForce(velocity, ForceMode.VelocityChange);
        velocity = Vector3.zero;
#endif
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

    //void OnCollisionEnter(Collision collision)
    //{
    //    channge = UnityEngine.Random.Range(1, 4);
    //}

    //枚举轴
    enum SelfAxis
    {
        X,
        Y,
        Z,
    }
}
