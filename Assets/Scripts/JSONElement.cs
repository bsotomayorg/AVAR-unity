using System;
using System.Collections.Generic;

[Serializable]
public class JSONElement {
    public JSONShape shape;
    public List<float> position;
    public List<float> color = new List<float>();
    public int id;
    public string type;

    public int from_id;
    public int to_id;
}