using System;
using System.Collections.Generic;

[Serializable]
public class JSONElement{
    public List<float> position;
    public int id;
    public string type;
    public JSONShape shape;

    public int id_from;
    public int id_to;
    public List<float> color = new List<float>();
}