using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ItemReact : MonoBehaviour,ISelectHandler
{
    [SerializeField]
    private DropdownExample m_fatherCtrl;
    public Toggle m_tg { get; private set; }
    public int m_id { get; private set; }
    public void OnSelect(BaseEventData eventData)
    {
        m_fatherCtrl.getCurSelectNum(m_id);

    }


    // Start is called before the first frame update
    void Start()
    {
        m_tg = GetComponent<Toggle>();
        //Debug.Log(name);
        m_id = int.Parse(name.Substring(4).Split(':')[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
