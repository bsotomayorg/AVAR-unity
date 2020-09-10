using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class TextHighlighter{

    private HashSet<string> hset;
    private HashSet<string> hset_var;
    //private Regex regex_class = new Regex("\\b[A-Z][a-zA-Z]*");
    private Regex regex_class = new Regex("\\b[A-Z][a-zA-Z] *");
    private Regex regex_var = new Regex("([^:=] *):=");
    //private Regex regex_method = new Regex("([^:] *):");

    private char[] separators = {'(',')'};

    private string highlighted_text = "";
    private string[] tokens;
    private Boolean[] isHighlight;

    // debug
    private string hset_asString ="";
    private string hset_var_asString = "";

    // bsotomayor's attempt 06/18


    public TextHighlighter() {
        hset = new HashSet<String>();
        this.hset_var = new HashSet<String>();
    }

    public string getHighlightedText(string script) {
        this.highlighted_text = script;
        this.hset_asString = "";
        this.hset_var_asString = "";
        script = script.Replace("\n", " ");
        
        foreach (Match m in this.regex_class.Matches(script))
        {
            hset.Add(m.ToString());
        }

        script = Regex.Replace(script, @"[^0-9a-zA-Z \n]+", ""); // it gets each token and remove special syms 
        this.tokens = script.Split(' ');
        string token;

        foreach (Match m in this.regex_var.Matches(script))
        {
            token = m.ToString();
            hset_var.Add(m.ToString());
            Debug.Log("VAR: "+token);
        }

        foreach (string t in hset) {
            this.highlighted_text = Regex.Replace(this.highlighted_text, @"\b" + t + @"\b", "<color=blue>" + t + "</color>");
        }

        //refresh hashmap
        var temp = "";
        List<string> remove_list = new List<string>();
        if (hset.Count > 0) foreach (string t in hset) {
            if (t.Length > 0 &&  Array.IndexOf(tokens, t) <= 0){
                Debug.Log("Array.IndexOf(tokens, t = "+t+") = " + Array.IndexOf(tokens, t));
                    remove_list.Add(t);
                    Debug.Log("\tRemoving: '"+t+"'");
                    temp += ("'"+t + "' ");
                }
        }

        foreach(string r in remove_list) { hset.Remove(r); }

        var tokens_str = "";
        foreach (string t in tokens) { tokens_str += "'" + t + "' ";  }
        foreach (string t in this.hset) { this.hset_asString += "'" + t + "' "; }

        Debug.Log("tokens = [ " + tokens_str + "]\n"+
            "Class: ("+this.hset.Count+")"+ this.hset_asString + "\n"+
            "Var:   ("+this.hset_var.Count+")" + this.hset_var_asString);

        return this.highlighted_text;

    }

    public string justAnotherHLTAttempt(string script) {
        string str_out = "";
        str_out += "script before: " + script + "\n";
        script = Regex.Replace(script, "<color=blue> | </color>", "");
        str_out += "script cleared: " + script + "\n";
        this.highlighted_text = script;

        // for each class key workd add it to the 'hset'
        foreach (Match m in this.regex_class.Matches(script)) {
            hset.Add(m.ToString());
        }

        //colorize each word from the 'hset'
        var str_classes = "";
        foreach (string t in hset) {
            //Debug.Log("t = " + t);
            str_classes += t + " ";
            this.highlighted_text = Regex.Replace(this.highlighted_text, @"\b" + t + @"\b", "<color=blue>" + t + "</color>");// "<color=blue>" + t + "</color>");
        }

        Debug.Log(str_out+"\nstr_classes: "+str_classes);

        return this.highlighted_text;
        
    }

    public string getHighlightedText2(string script)
    {
        Debug.Log("script before: " + script);
        script = Regex.Replace(script, "<color=blue>", "");
        script = Regex.Replace(script, "</color>", ""); // @"(?s)(<color=blue>(.*)</color>)", "");
        Debug.Log("script after : " + script);
        this.highlighted_text = script;
        this.hset_asString = "";
        this.hset_var_asString = "";
        script = script.Replace("\n", " ");

        foreach (Match m in this.regex_class.Matches(script))
        {
            hset.Add(m.ToString());
        }

        script = Regex.Replace(script, @"[^0-9a-zA-Z \n]+", ""); // it gets each token and remove special syms 
        this.tokens = script.Split(' ');
        string token;

        foreach (Match m in this.regex_var.Matches(script))
        {
            token = m.ToString();
            hset_var.Add(m.ToString());
            Debug.Log("VAR: " + token);
        }

        foreach (string t in hset)
        {
            //Debug.Log("t = " + t);
            this.highlighted_text = Regex.Replace(this.highlighted_text, @"\b" + t + @"\b", "<color=blue>"+t+"</color>");// "<color=blue>" + t + "</color>");
        }


        //refresh hashmap
        var temp = "";
        List<string> remove_list = new List<string>();
        if (hset.Count > 0) foreach (string t in hset)
            {
                if (t.Length > 0 && Array.IndexOf(tokens, t) <= 0)
                {
                    //Debug.Log("Array.IndexOf(tokens, t = " + t + ") = " + Array.IndexOf(tokens, t));
                    remove_list.Add(t);
                    //Debug.Log("\tRemoving: '" + t + "'");
                    temp += ("'" + t + "' ");
                }
            }

        foreach (string r in remove_list) { hset.Remove(r); }

        var tokens_str = "";
        foreach (string t in tokens) { tokens_str += "'" + t + "' "; }
        foreach (string t in this.hset) { this.hset_asString += "'" + t + "' "; }

        Debug.Log("tokens = [ " + tokens_str + "]\n" +
            "Class: (" + this.hset.Count + ")" + this.hset_asString + "\n" +
            "Var:   (" + this.hset_var.Count + ")" + this.hset_var_asString+"\n"+"'"+this.highlighted_text+"'");

        return this.highlighted_text;

    }

    public string getTokensAsStrings()
    {
        return this.hset_asString;
    }

}
