using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
public class ExamplesCodeBlockData
{
    private static ExamplesCodeBlockData instance;
    private Dictionary<string, string> m_codeBlocks;
    string originalString;
    private string[] exampleCodeBlocks;



    public static ExamplesCodeBlockData Instance
    {
        get
        {
            if (instance == null)
                instance = new ExamplesCodeBlockData();
            return instance;
        }
    }

    public Dictionary<string, string> M_CodeBlocks
    {

        get
        {
            if (m_codeBlocks == null)
                m_codeBlocks = new Dictionary<string, string>();
            return m_codeBlocks;
        }
    }



    public ExamplesCodeBlockData()
    {
        if (m_codeBlocks == null)
        {
            m_codeBlocks = new Dictionary<string, string>();
        }
        else
        {
            m_codeBlocks.Clear();
        }

        TextAsset sourceDocument = Resources.Load("PharoExamples") as TextAsset;
        //Debug.Log(sourceDocument.GetType());
        originalString = sourceDocument.text;
        exampleCodeBlocks = Regex.Split(originalString, "Exp:");
        for (int i = 1; i < exampleCodeBlocks.Length; i++)
        {
            int keyPos = exampleCodeBlocks[i].IndexOf("\n");
            string exampleName = exampleCodeBlocks[i].Substring(0, keyPos);//不允许用Split，不然信息中的其余冒号也会被抹杀
            string exampleCode = exampleCodeBlocks[i].Substring(keyPos + 1);
            m_codeBlocks.Add(exampleName, exampleCode.Trim());
        }


    }
}
