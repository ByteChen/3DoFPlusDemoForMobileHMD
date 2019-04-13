using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;

public class Pvr_ControllerVisual : MonoBehaviour
{

    private Renderer controllerRenderer;
    private float tipsAlpha = 0;
    public bool isController0;
    public GameObject touchpoint;
    public Transform tips;
    public Transform power;
    public Material m_idle;
    public Material m_app;
    public Material m_home;
    public Material m_touchpad;
    public Material m_volUp;
    public Material m_volDn;
    public Material m_trigger;

    public Sprite power1;
    public Sprite power2;
    public Sprite power3;
    public Sprite power4;
    public Sprite power5;

    public enum TipBtn
    {
        app,
        touchpad,
        home,
        volup,
        voldown,
    }
    
    void Awake()
    {
        controllerRenderer = GetComponent<Renderer>();
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    ChangeKeyEffects(isController0 ? 0 : 1);
	}

    public void ChangeTipsText(TipBtn tip,string key)
    {
        if (tips != null)
        {
            switch (tip)
            {
                case TipBtn.app:
                    {
                        tips.Find("apptip/btn/Text").GetComponent<Text>().text = key;
                    }
                    break;
                case TipBtn.touchpad:
                    {
                        tips.Find("touchtip/btn/Text").GetComponent<Text>().text = key;
                    }
                    break;
                case TipBtn.home:
                    {
                        tips.Find("hometip/btn/Text").GetComponent<Text>().text = key;
                    }
                    break;
                case TipBtn.volup:
                    {
                        tips.Find("volup/btn/Text").GetComponent<Text>().text = key;
                    }
                    break;
                case TipBtn.voldown:
                    {
                        tips.Find("voldown/btn/Text").GetComponent<Text>().text = key;
                    }
                    break;
            }
        }
    }
    private void ChangeKeyEffects(int hand)
    {
        if (Controller.UPvr_IsTouching(hand))
        {
            touchpoint.SetActive(true);
            if (m_trigger != null)
            {
                touchpoint.transform.localPosition =
                    new Vector3(0.0132f - Controller.UPvr_GetTouchPadPosition(hand).y * 0.0001f,
                        0.01153f + Controller.UPvr_GetTouchPadPosition(hand).x * 0.0001f, 0.0128f);
            }
            else
            {
                touchpoint.transform.localPosition = new Vector3(1.4f - Controller.UPvr_GetTouchPadPosition(hand).y * 0.01098f, 0.9f, -0.8f - Controller.UPvr_GetTouchPadPosition(hand).x * 0.01098f);
            }
        }
        else
        {
            touchpoint.SetActive(false);
        }
        if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.TOUCHPAD))
        {
            controllerRenderer.material = m_touchpad;
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.APP))
        {
            controllerRenderer.material = m_app;
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.HOME))
        {
            controllerRenderer.material = m_home;
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.VOLUMEUP))
        {
            controllerRenderer.material = m_volUp;
        }
        else if (Controller.UPvr_GetKey(hand, Pvr_KeyCode.VOLUMEDOWN))
        {
            controllerRenderer.material = m_volDn;
        }
        else if (Controller.Upvr_GetControllerTriggerValue(hand) > 0)
        {
            controllerRenderer.material = m_trigger;
        }
        else
        {
            controllerRenderer.material = m_idle;
        }
        
	    tipsAlpha = (330 -  transform.parent.parent.localRotation.eulerAngles.x) / 45.0f;
        if (transform.parent.parent.localRotation.eulerAngles.x >= 270 &&
	        transform.parent.parent.localRotation.eulerAngles.x <= 330)
        {
            tipsAlpha = Mathf.Max(0.0f, tipsAlpha);
            tipsAlpha = tipsAlpha > 1.0f ? 1.0f : tipsAlpha;
        }
	    else
        {
            tipsAlpha = 0.0f;
        }
        if (tips != null)
            tips.GetComponent<CanvasGroup>().alpha = tipsAlpha;

        if (power != null)
        {
            if (Pvr_ControllerManager.Instance.ShowPowerToast)
            {
                if (Controller.UPvr_GetControllerPower(hand) == 1)
                {
                    power.gameObject.SetActive(true);
                }
                else
                {
                    power.gameObject.SetActive(Vector3.Distance(transform.parent.parent.localPosition, Pvr_UnitySDKManager.SDK.HeadPose.Position) <= 0.35f);
                }
                switch (Controller.UPvr_GetControllerPower(hand))
                {
                    case 1:
                        power.Find("Image").GetComponent<Image>().sprite = power1;
                        power.Find("Image").GetComponent<Image>().color = Color.red;
                        break;
                    case 2:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power1;
                        break;
                    case 3:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power1;
                        break;
                    case 4:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power2;
                        break;
                    case 5:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power2;
                        break;
                    case 6:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power3;
                        break;
                    case 7:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power3;
                        break;
                    case 8:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power4;
                        break;
                    case 9:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power4;
                        break;
                    case 10:
                        power.Find("Image").GetComponent<Image>().color = Color.white;
                        power.Find("Image").GetComponent<Image>().sprite = power5;
                        break;
                    default:
                        power.Find("Image").GetComponent<Image>().color = Color.clear;
                        break;
                }
            }
            else
            {
                power.gameObject.SetActive(false);
            }
            
        }

        
    }

}
