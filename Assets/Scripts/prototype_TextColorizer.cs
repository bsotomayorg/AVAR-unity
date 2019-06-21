using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text.RegularExpressions;

public class prototype_TextColorizer : MonoBehaviour{
    public InputField inputField;

    private List<Boolean> tagMask = new List<Boolean>();

    private int prev_caret_pos;
    private int delta_caret_pos;

    //private bool changes = false;

    void Start() {

        string script = "";//v := RWView new.\ntemp aMethod: thing.\n(1 to: 10).\nv run.";
        inputField.text = script;
        //this.highlighter(script);
        prev_caret_pos = inputField.caretPosition;
        this.delta_caret_pos = 0;

        //tagMask = new List<Boolean>();
    }

    public string highlighter(string script)
    {
        var prev_tag = Regex.Matches(script, "<color=blue>").Count;
        string prev_script = script;
        
        script = Regex.Replace(script, @"<[^>]*>", String.Empty);
        //script = Regex.Replace(script, @"<[^>]*>", String.Empty);
        var str_out = "";
        char [] seps = {' ',',', ';', '\n', '(', ')', '.' };
        string [] tokens = script.Split(seps, System.StringSplitOptions.RemoveEmptyEntries);

        //script = Regex.Replace(script, @"\b<color=blue>\b", "");

        List<string> vars = new List<string>();
        List<string> classes = new List<string>();

        decimal myDec;
        for (int i = 0; i < tokens.Length; i++)
        {
            if (Char.IsUpper(tokens[i][0])) { // class
                str_out += "'<color=blue>" + tokens[i] + "</color>'|";
                classes.Add(tokens[i]);
            } else if (tokens[i][tokens[i].Length - 1] == ':') { // method
                str_out += "'<i>" + tokens[i] + "</i>'|";
            } else if ((i + 1) < tokens.Length && tokens[i + 1] == ":=") { //var
                str_out += "'<color=green>" + tokens[i] + "</color>'|";
                vars.Add(tokens[i]);
            } else if (vars.Contains(tokens[i])) { // known vars
                str_out += "'<color=gray>" + tokens[i] + "</color>'|";
            } else if (decimal.TryParse(tokens[i], out myDec)) {
                str_out += "'<color=red>"+tokens[i]+"</color>'|";
            }
            else { // another symbol
                str_out += "'" + tokens[i] + "'|";
            }
        }

        string[] lines = script.Split(new char [] {'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
        string ret = "";
        //string plain_ret = "";

        for (int i = 0; i < lines.Length; i++) {
            string[] words = lines[i].Split(' ');
            //Debug.Log("line #" + i);
            //foreach (string w in words)
            //    Debug.Log("'"+w+"'");
            foreach (string w in words) //for (int j = 0; j < words[j].Length; j++)
            {
                if (vars.Contains(w) && w.Length > 0) {
                    ret += "<color=blue>" + w + "</color>";
                    //plain_ret += w;
                    this.delta_caret_pos += "<color=blue></color>".Length;// + w.Length;
                } else if(classes.Contains(w))
                {
                    ret += "<color=blue>" + w + "</color>";
                    //plain_ret += w;
                    this.delta_caret_pos += "<color=blue></color>".Length;// + w.Length;
                } else {
                    ret += w;
                    //plain_ret += w;
                }
                ret += " ";
                //plain_ret += " ";

            }

            ret = ret.Substring(0, ret.Length - 1);
            //plain_ret = plain_ret.Substring(0, plain_ret.Length - 1);
            //var dif = "<color=blue></color>".Length;
            Debug.Log("TEST: caretpos:" + inputField.caretPosition + "delta" + this.delta_caret_pos);
            //Debug.Log("'"+ ret.Substring(0, (inputField.caretPosition ))+ "'");
            var curr_tags = Regex.Matches(ret, "<color=blue>").Count;
            this.delta_caret_pos = curr_tags * "<color=blue></color>".Length;
            if (curr_tags < prev_tag) { // user eliminate a variable or class
                var index = 0;
                while (prev_script[index] == script[index]){
                    index += 1;
                    //delta_caret_pos -= prev_tag * "<color=blue></color>".Length;
                }
                inputField.caretPosition = index;
                //this.changes = true;
            } else if(prev_script == ret) // no changes in script content
            {
                Debug.Log("SCRIPT == RET!!");
                this.delta_caret_pos = 0;
            }
            Debug.Log("SCRIPT: '"+ prev_script + "'\nRET: '"+ ret + "'");
            
            ret += "\n";
        }

        string new_script = ret.Substring(0, ret.Length - 1);
        updateTagMask(new_script);
        str_out += "\n";
        foreach (Boolean flag in tagMask) {
            if (flag) { str_out += "1"; } else { str_out += "0"; }
        }
        str_out += "\n";
        foreach (Char c in new_script) {
            if (c == '<' || c == '>') { str_out += '|'; }
            else { str_out += c; }
        }
        Debug.Log("caret_pos:" + inputField.caretPosition + " #chars:" + ret.Length + " delta:" + this.delta_caret_pos + "\n'" + ret + "'\n\n" + str_out);

        return ret.Substring(0, ret.Length - 1);
    }

    private void updateTagMask(string t) {
        this.tagMask.Clear();
        bool isTag = false;
        for (int i = 0; i < t.Length; i++)
        {
            if (t[i] == '<') { isTag = true; }

            this.tagMask.Add(isTag);

            if (t[i] == '>' && isTag) { isTag = false; }
        }

        // update cursor (caret)
        //Debug.Log("caretpos:"+(inputField.caretPosition)+ "+" + (this.delta_caret_pos - 1)+"="+ (inputField.caretPosition + this.delta_caret_pos - 1));
        //Debug.Log("is caret="+(inputField.caretPosition+this.delta_caret_pos - 1 )+ " at tag?" + this.tagMask[inputField.caretPosition + this.delta_caret_pos - 1]);

    }

    // Update is called once per frame
    void Update() {
        Event e = Event.current;
        if (inputField.text.Length > 0 && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.LeftControl))) {
        //if (this.changes) { 
            this.prev_caret_pos = inputField.caretPosition;
            inputField.text = this.highlighter(inputField.text);
            inputField.caretPosition += (this.delta_caret_pos) + Regex.Matches(inputField.text, "\n").Count + 1;
            this.delta_caret_pos = 0;
        }
        else if (Input.GetKeyDown(KeyCode.Backspace))
        {
            inputField.text = this.highlighter(inputField.text);
            inputField.caretPosition += (this.delta_caret_pos) + Regex.Matches(inputField.text, "\n").Count + 1;
            this.delta_caret_pos = 0;
        }
        if (Input.GetKeyDown(KeyCode.Delete)) {
            // Quedé aquí. Ahora es necesario eliminar cosas.. y tener cuidado de no estar "pisando" una etiqueta
            Debug.LogWarning("DELETE PRESSED. caret pos: " + inputField.caretPosition);// + "script char in that pos:"+inputField.text[inputField.caretPosition]);
            }

        /*if (Input.anyKeyDown && inputField.text != "" && inputField.caretPosition>0)
        {
            Debug.Log("caret pos:"+inputField.caretPosition+" tagMask.count:"+this.tagMask.Count);
            Debug.Log("is caret at tag?" + this.tagMask[inputField.caretPosition - 1]);

        }//if (this.tagMask[inputField.caretPosition - 1]) { }
        */
        bool directions = (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow));
        if (directions && this.tagMask.Count>0 && inputField.caretPosition>0) {
            short caret_dir = 1;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) caret_dir = -1;
            this.updateTagMask(inputField.text);
            Debug.Log(inputField.caretPosition + " masksize:" + this.tagMask.Count);
            var c = 0;
            while (this.tagMask[(inputField.caretPosition - 1)] && (inputField.caretPosition+caret_dir) > 0){
                inputField.caretPosition += caret_dir;
                //Debug.Log(" >>"+(inputField.caretPosition + caret_dir));
                c++;
                if (c > 100) { Debug.LogWarning("BREAK LOOP"); break; }
            }
        }
    }

    /* Complicaciones que hacen de esta implementación aún más compleja:
     + Al momento de recorrer el código usando las teclas direccionales, también se recorren los caracteres ocultos 
     (en este caso, las etiquetas)*/
    
}
