using System;
using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;

public class Pvr_ControllerDemo : MonoBehaviour
{
    public GameObject HeadSetController;
    public GameObject controller0;
    public GameObject controller1;

    private Ray ray;
    private GameObject currentController;

    private Transform lastHit;
    private Transform currentHit;
    // Use this for initialization
    void Start()
    {
        ray = new Ray();
        Pvr_ControllerManager.ControllerThreadStartedCallbackEvent += ThreadStartSuccess;
        Pvr_ControllerManager.SetControllerStateChangedEvent += ControllerStateListener;
        currentController = controller0;
    }

    // Update is called once per frame
    void Update()
    {
        if (HeadSetController.activeSelf)
        {
            HeadSetController.transform.parent.localRotation = Quaternion.Euler(Pvr_UnitySDKManager.SDK.HeadPose.Orientation.eulerAngles.x, Pvr_UnitySDKManager.SDK.HeadPose.Orientation.eulerAngles.y, 0);

            ray.direction = HeadSetController.transform.position - HeadSetController.transform.parent.parent.Find("Head").position;
            ray.origin = HeadSetController.transform.parent.parent.Find("Head").position;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                currentHit = hit.transform;

                if (currentHit != null && lastHit != null && currentHit != lastHit)
                {
                    if (lastHit.transform.gameObject.activeInHierarchy && lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                    {
                        lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = false;
                    }
                }
                if (currentHit != null && lastHit != null && currentHit == lastHit)
                {
                    if (!currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                    {
                        currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = true;

                    }
                }
                lastHit = hit.transform;
                Debug.DrawLine(ray.origin, hit.point, Color.red);
            }
        }
        else
        {
            if (currentController != null)
            {
                ray.direction = currentController.transform.Find("dot").position - currentController.transform.Find("start").position;
                ray.origin = currentController.transform.Find("start").position;
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    currentHit = hit.transform;

                    if (currentHit != null && lastHit != null && currentHit != lastHit)
                    {
                        if (lastHit.transform.gameObject.activeInHierarchy && lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                        {
                            lastHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = false;
                        }
                    }
                    if (currentHit != null && lastHit != null && currentHit == lastHit)
                    {
                        if (!currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled)
                        {
                            currentHit.GetComponent<Pvr_UIGraphicRaycaster>().enabled = true;

                        }
                    }
                    lastHit = hit.transform;
                    Debug.DrawLine(ray.origin, hit.point, Color.red);
                    currentController.transform.Find("dot").position = hit.point;
                }
            }
        }
    }
    void OnDestroy()
    {
        Pvr_ControllerManager.ControllerThreadStartedCallbackEvent -= ThreadStartSuccess;
        Pvr_ControllerManager.SetControllerStateChangedEvent -= ControllerStateListener;
    }
    private void ShowControllerRay()
    {
        var mainhand = Controller.UPvr_GetMainHandNess();
        if (mainhand == 0)
        {
            currentController = controller0;
            if (controller0.activeSelf)
            {
                controller0.transform.Find("dot").gameObject.SetActive(true);
                controller0.transform.Find("ray_alpha").gameObject.SetActive(true);
            }
            if (controller1.activeSelf)
            {
                controller1.transform.Find("dot").gameObject.SetActive(false);
                controller1.transform.Find("ray_alpha").gameObject.SetActive(false);
            }
        }
        else
        {
            currentController = controller1;
            if (controller0.activeSelf)
            {
                controller0.transform.Find("dot").gameObject.SetActive(false);
                controller0.transform.Find("ray_alpha").gameObject.SetActive(false);
            }
            if (controller1.activeSelf)
            {
                controller1.transform.Find("dot").gameObject.SetActive(true);
                controller1.transform.Find("ray_alpha").gameObject.SetActive(true);
            }
        }
    }

    private void ShowController0()
    {
        controller0.SetActive(Controller.UPvr_GetControllerState(0) == ControllerState.Connected);
    }

    private void ShowController1()
    {
        controller1.SetActive(Controller.UPvr_GetControllerState(1) == ControllerState.Connected);
    }

    private void ThreadStartSuccess()
    {
        if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
            Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
        {
            HeadSetController.SetActive(false);
        }
        else
        {
            HeadSetController.SetActive(true);
        }
        Invoke("ShowController0", 0.1f);
        Invoke("ShowController1", 0.1f);
        Invoke("ShowControllerRay", 0.1f);
    }

    private void ControllerStateListener(string data)
    {
        if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
            Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
        {
            HeadSetController.SetActive(false);
        }
        int controller = Convert.ToInt16(data.Substring(0, 1));
        if (controller == 0)
        {
            ShowController0();
        }
        else
        {
            ShowController1();
        }

        ShowControllerRay();

        if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
            Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
        {
            HeadSetController.SetActive(false);
        }
        else
        {
            HeadSetController.SetActive(true);
        }
    }
}
