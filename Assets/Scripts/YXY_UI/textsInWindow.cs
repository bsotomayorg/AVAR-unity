using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class textsInWindow : MonoBehaviour
{
    public Image m_image { private set; get; }
    public  Text m_text { private set; get; }
    

    public void f_Init()
    {

        m_image = GetComponent<Image>();
        m_text = GetComponentInChildren<Text>();

        m_text.text = "";
        m_image.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        //gameObject.SetActive(false);

    }

    // Start is called before the first frame update
    void Start()
    {    

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
