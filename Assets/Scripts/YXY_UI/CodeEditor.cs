using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CodeEditor : MonoBehaviour
{

    public ScrollRichInput CodeInputPanel;
    public ScrollRichInput ConsolePanel;
    public DropdownExample DropDownPanel;
    public NavigationReactorUI CurFocusedPanel { get; private set; }
    private int curPanelIndex = 0;
    private bool isInited=false;

    private enum curFocusedPanel
    {

        None = -1,
        CodeInputPanel = 0,
        DropDownPanel = 1,
        ConsolePanel = 2,

    }  
    




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void f_Init()
    {
        float totalHeight = GetComponent<RectTransform>().rect.height - DropDownPanel.GetComponent<RectTransform>().rect.height;
        DropDownPanel.f_Init();
        CodeInputPanel.f_Init(totalHeight,7/8f,1/3f);
        ConsolePanel.f_Init(totalHeight,1/8f,2/3f);
        curPanelIndex = 0;
        changeActivePanel(curPanelIndex);
        isInited = true;
    }

    public void ChangeActiveIndex(bool isClockingOrder)
    {
        if (!isInited)
            return;
        int lastNum = curPanelIndex;
        if (isClockingOrder)
        {
            if (curPanelIndex >= 2)
                curPanelIndex = 0;
            else
                curPanelIndex = curPanelIndex + 1;
        }
        else
        {
            if (curPanelIndex <= 0)
                curPanelIndex = 2;
            else
                curPanelIndex = curPanelIndex - 1;
        }

        changeActivePanel(curPanelIndex);

        OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.ChangeFocus, "from " + Enum.GetName(typeof(curFocusedPanel), lastNum) + " to " + Enum.GetName(typeof(curFocusedPanel), curPanelIndex));

        }
    public void changeActivePanel(int curIndex)
    {
        switch (curIndex)
        {
            case 0:
                CodeInputPanel.M_backGround.enabled = true;
                DropDownPanel.M_background.enabled = false;
                ConsolePanel.M_backGround.enabled = false;
                DropDownPanel.changeState(false);
                CodeInputPanel.changeState(false);
                ConsolePanel.changeState(false);
                CurFocusedPanel = CodeInputPanel;
                curPanelIndex = 0;//necessary when select an example in dropdown list.
                break;
            case 1:
                CodeInputPanel.M_backGround.enabled = false;
                DropDownPanel.M_background.enabled = true;
                ConsolePanel.M_backGround.enabled = false;
                DropDownPanel.changeState(true);
                CurFocusedPanel = DropDownPanel;
                curPanelIndex = 1;
                break;
            case 2:
                CodeInputPanel.M_backGround.enabled = false;
                DropDownPanel.M_background.enabled = false;
                ConsolePanel.M_backGround.enabled = true;
                DropDownPanel.changeState(false);
                CodeInputPanel.changeState(true);
                ConsolePanel.changeState(true);
                CurFocusedPanel = ConsolePanel;
                curPanelIndex = 2;
                break;
            default:
                break;
        }


        //bool isConsoleOn = false ;
        //curPanel = isConsoleOn ? ConsolePanel : CodeInputPanel;
        //CodeInputPanel.changeSize(isConsoleOn);
        //ConsolePanel.changeSize(isConsoleOn);
        CurFocusedPanel.M_SelectHandler.Select();
    }


    
}
