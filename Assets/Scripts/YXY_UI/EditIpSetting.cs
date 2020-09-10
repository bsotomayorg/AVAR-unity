using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditIpSetting : MonoBehaviour
{

    /// <summary>
    /// ipv4 address of the pharo server,default value is "http://127.0.0.1"
    /// </summary>
    public InputField input_IPAddress;
    /// <summary>
    /// port of the pharo server,default value is "1702"
    /// </summary>
    public InputField input_port;


    public InputField input_User;
    public Button Btn_Submit;

    public Text text_tips;
    private string oriIpAddress;

    private int focusIndex;
    private string oriPort;
    /// <summary>
    /// is this after initialzed ?
    /// </summary>
    private bool isInited = false;
    private string formatedText;

    public void f_Init()
    {        
        //gameObject.SetActive(false);
        Btn_Submit.onClick.AddListener(SubmitIpSetting);
        changeFocus(0);
        isInited = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInited)
            return;
        if (gameObject.activeSelf)
        {

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ChangeActiveIndex();
                //if (input_IPAddress.isFocused)
                //{
                //    input_port.Select();
                //    input_port.ActivateInputField();
                //}
                //else if(input_port.isFocused)
                //{
                //    input_IPAddress.Select();
                //    input_IPAddress.ActivateInputField();
                //}
            }
        }
    }

    private void ChangeActiveIndex()
    {
        if (!isInited)
            return;
        if (focusIndex >= 3)
            focusIndex = 0;
        else
            focusIndex = focusIndex + 1;

        changeFocus(focusIndex);
    }

    private void changeFocus(int focusIndex)
    {
        switch (focusIndex)
        {
            case 0:
                input_User.Select();
                input_User.ActivateInputField();
                focusIndex = 0;
                break;
            case 1:
                input_IPAddress.Select();
                input_IPAddress.ActivateInputField();                
                focusIndex = 1;//necessary when select an example in dropdown list.
                break;
            case 2:
                input_port.Select();
                input_port.ActivateInputField();
                focusIndex = 2;
                break;
            case 3:
                Btn_Submit.Select();
                focusIndex = 3;
                break;
            default:
                break;
        }
    }

    public void IPsetting(bool isOn)
    {
        if (!isInited)
            return;
        gameObject.SetActive(isOn);
        if (isOn)
        {            
            this.transform.SetAsLastSibling();
            //input_IPAddress.Select();
            //input_IPAddress.ActivateInputField();
            changeFocus(0);
            oriIpAddress = input_IPAddress.text;
            oriPort = input_port.text;
            changeTips("(use \"Tab\" to switch input field)", Color.white);
        }
        else if(playground.Instance.isCodeEditorOn)
        {

            playground.Instance.m_codeEditor.changeActivePanel(0);
        }
    }

    private void SubmitIpSetting()
    {
        bool submitSuccessfully = true;
        formatedText = string.Empty;
        formatedText += " Name: " + input_User.text + "\n";
        formatedText += " IP Address: " + input_IPAddress.text + "\n";
        formatedText += " Port: " + input_port.text + "\n";
        if (input_User.text.Length == 0)
        {
            changeTips("User name is empty!", Color.red);
            submitSuccessfully = false;
        }
        if (input_User.text.Contains("/") || input_User.text.Contains("\\") || input_User.text.Contains(":"))
        {
            input_User.text = string.Empty;
            changeTips("/ , \\ and : are not allowed", Color.red);
            submitSuccessfully = false;
        }
        if (input_IPAddress.text.Length == 0)
        {
            input_IPAddress.text = oriIpAddress;
            changeTips("Wrong IP address!", Color.red);
            submitSuccessfully = false;
        }
        if (input_port.text.Length == 0)
        {
            input_port.text = oriPort;
            changeTips("Wrong Port!", Color.red);
            submitSuccessfully = false;
        }
        if (submitSuccessfully)
        {
            changeTips("Submit successfully!", Color.white);
            OperationLog.Instance.submitConfiguration(input_User.text);
        }
        OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.LaunchConfiguration, formatedText);
    }

    IEnumerator pingIp()
    {
        Ping ping = new Ping(input_IPAddress.text);
        float timeCounter = 0;
        while (!ping.isDone)
        {
            yield return new WaitForSeconds(0.1f);
            if (timeCounter >=10)
            {
                changeTips("Invalid Ip address!", Color.red);
                formatedText += "Result: Invalid Ip address!";
                OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.LaunchConfiguration, formatedText);

                input_IPAddress.text = oriIpAddress;
                changeFocus(0);
                break;
            }
            else
            {
                changeTips("ping Ip address...", new Color(0, 1, 1));
            }
            timeCounter++;
        }
        if (ping.isDone)
        {
            changeTips("Submit successfully", Color.white);
            formatedText += "Result: Submit successfully";
            OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.LaunchConfiguration, formatedText);

            yield return null;
        }
    }

    private void changeTips(string content, Color color)
    {
        text_tips.text = content;
        text_tips.color = color;
    }
}
