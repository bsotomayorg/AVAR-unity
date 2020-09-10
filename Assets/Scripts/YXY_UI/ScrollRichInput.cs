using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ScrollRichInput : NavigationReactorUI
{
    float startBarValue;
    public Image M_backGround { get; private set; }
    public TMP_InputField M_Input;
    float basicHeight = 0;
    float CurrentTotalLineCount = 0;
    float oriHeightNormal = 0;
    float oriHeightConsole = 0;
    float oriTotalWidth = 0;
    bool isEntented = false;
    float currentOriHeight = 0;
    bool isInited = false;
    /// <summary>
    /// initialization function
    /// </summary>
    /// <param name="totalHeight">the total height of code editor</param>
    /// <param name="normalHeightParam">how many percent of size for this window to take up when in normal mode</param>
    /// <param name="consoleOnHeightParam">how many percent of size for this window to take up when when console on</param>
    public void f_Init(float totalHeight, float normalHeightParam, float consoleOnHeightParam)
    {
        M_backGround = GetComponent<Image>();
        M_backGround.enabled = false;
        //M_Input = GetComponentInChildren<TMP_InputField>();
        oriHeightNormal = totalHeight * normalHeightParam;
        oriHeightConsole = totalHeight * consoleOnHeightParam;
        currentOriHeight = oriHeightNormal;
        oriTotalWidth= GetComponent<RectTransform>().rect.width;
        GetComponent<RectTransform>().sizeDelta = new Vector2(oriTotalWidth, currentOriHeight);
        M_Input.selectionColor = new Color(0.429f, 0.674f, 1.0f, 0.753f);
        basicHeight = M_Input.textComponent.preferredHeight + M_Input.textComponent.lineSpacing;
        //m_input.onValueChanged.AddListener(hehe);
        M_SelectHandler = M_Input;
        isInited = true;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="isOn">Whether ConsolePanel is on</param>
    public override void changeState(bool isOn)
    {
        currentOriHeight = isOn ? oriHeightConsole : oriHeightNormal;
        GetComponent<RectTransform>().sizeDelta = new Vector2(oriTotalWidth, currentOriHeight);
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

    }

    void hehe(string a)
    {
        Debug.Log(M_Input.stringPosition);      
    }
    

    public override void NavigationUpdate(Vector3 deltaPos, GameObject origin)
    {
        M_Input.verticalScrollbar.value = startBarValue - deltaPos.y*3;
    }
    public override void NavigationStart(Vector3 startPos, GameObject origin)
    {
        startBarValue = M_Input.verticalScrollbar.value;
    }
    public override void NavigationEnd(Vector3 endPos, GameObject origin)
    {
        M_Input.caretPosition = (int)(M_Input.verticalScrollbar.value * M_Input.text.Length);
        //m_input.MoveToStartOfLine(true, false);
    }


}
