using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class DropdownExample : NavigationReactorUI
{
    float startBarValue;
    ScrollRect m_template;
    ScrollRect m_DropDownList;
    public ItemReact m_itemPrefab;
    bool isItemTooMuch = false;

    public Dropdown M_Dp { get; private set; }
    public Image M_background
    {
        get { return m_background; }
    }

    List<Dropdown.OptionData> exampleNames;
    bool isInited = false;
    [SerializeField]
    private Image m_background;
    private bool isOpen = false;


    // Start is called before the first frame update
    void Start()
    {

    }

    public void f_Init()
    {
        M_Dp = GetComponentInChildren<Dropdown>();

        m_template = M_Dp.template.GetComponent<ScrollRect>();

        exampleNames = new List<Dropdown.OptionData>();
        M_Dp.ClearOptions();
        exampleNames.Add(new Dropdown.OptionData("Examples:"));
        foreach (var a in ExamplesCodeBlockData.Instance.M_CodeBlocks)
        {
            exampleNames.Add(new Dropdown.OptionData(a.Key));
        }
        M_Dp.AddOptions(exampleNames);
        //M_Dp.onValueChanged.AddListener(f_dp_callCodeBlock);
        M_SelectHandler = M_Dp;
        exampleNames.Clear();
        isInited = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isOn">whether the dropdown list is on</param>
    public override void changeState(bool isOn)
    {
        if (!isInited)
            return;
        isOpen = isOn;
        if (isOn)
        {
            M_Dp.Select();
            M_Dp.Show();
            isItemTooMuch = SetItem();

            if (isItemTooMuch)
            {
                m_DropDownList = M_Dp.transform.Find("Dropdown List").GetComponent<ScrollRect>();

            }
        }
        else
        {
            M_Dp.Hide();
            isItemTooMuch = false;
        }
    }

    private bool SetItem()
    {
        return M_Dp.options.Count > (int)(m_template.GetComponent<RectTransform>().rect.height / m_itemPrefab.GetComponent<RectTransform>().rect.height);
    }

    public void f_dp_callCodeBlock(int a)
    {
        if (!isInited)
            return;

        if (ExamplesCodeBlockData.Instance.M_CodeBlocks.ContainsKey(M_Dp.options[a].text))
        {
            playground.Instance.m_codeEditor.CodeInputPanel.M_Input.text = ExamplesCodeBlockData.Instance.M_CodeBlocks[M_Dp.options[a].text].Replace('\r', ' ');
            OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.SelectExample, M_Dp.options[a].text);

        }
        else
        {
            OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.SelectExample, "Choose nothing.");

        }
        playground.Instance.m_codeEditor.changeActivePanel(0);
        //playground.Instance.m_codeEditor.changeActivePanel(0);
        M_Dp.value = 0;
        //else
        //    playground.Instance.scrollInput.m_input.text = "";
    }



    // Update is called once per frame
    void Update()
    {
        if (!isInited)
            return;
        if (isOpen)
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
                f_dp_callCodeBlock(M_Dp.value);

    }
    public void getCurSelectNum(float a)
    {
        if (!isInited || !isItemTooMuch)
            return;
        m_DropDownList.verticalScrollbar.value = 1 - a / (M_Dp.options.Count - 1);
    }

    public override void NavigationStart(Vector3 startPos, GameObject origin)
    {
        //if (!isInited || !isItemTooMuch)
        //    return;
        //startBarValue = m_template.verticalScrollbar.value;
    }
    public override void NavigationUpdate(Vector3 deltaPos, GameObject origin)
    {
        //if (!isInited || !isItemTooMuch)
        //    return;
        //m_template.verticalScrollbar.value = startBarValue + 1-deltaPos.y * 3;
    }

    public override void NavigationEnd(Vector3 endPos, GameObject origin)
    {
        //if (!isInited || !isItemTooMuch)
        //    return;
        //int a = Mathf.CeilToInt(1 - m_template.verticalScrollbar.value) * (M_Dp.options.Count - 1);

        //foreach (var b in m_DropDownList.transform.GetComponentsInChildren<ItemReact>())
        //    if (b.m_id == a)
        //    {
        //        b.m_tg.Select();
        //        break;
        //    }

    }
}
