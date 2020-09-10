using System;
using System.Collections.Generic;

[Serializable]
public class JSONElement {
    public JSONShape shape;
    public List<float> position;
    public List<float> color = new List<float>();
    public string id;
    public string type;
    public List<string> interactions;

    public string from_id;
    public string to_id;
    public string model;
}