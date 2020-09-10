using System;

[Serializable]
public class JSONRootElement {
    public string key;
    public JSONElement[] elements;
    public JSONElement[] RTelements;
    public string value;
    public JSONError errormsg;
}
