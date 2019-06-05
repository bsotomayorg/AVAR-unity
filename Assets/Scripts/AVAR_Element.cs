using System;
using UnityEngine;
using UnityEngine.UI;

public class AVAR_Element
{
    public GameObject go;
    public string type;
    public Boolean isSelected; // Is this object selected for interaction?
    string id;
    public string engine;
    Vector3 position;
    Vector3 scale;

    public AVAR_Element(JSONElement element, float [] positioning, float[] scaling, float scale_const, Color col, GameObject world)
    {
        //string id, string type, Vector3 position, Vector3 scale, Color col, GameObject world)
        this.id = element.id;
        this.type = element.shape.shapeDescription;
        
        if (type.Substring(0,2) == "RT") {
            this.engine = "ROASSAL2";
            this.position = new Vector3( // position
            (element.position[0] * positioning[0]) / scale_const, // + shifting[0],
            (element.position[1] * positioning[1]) / scale_const // + shifting[1],
            //shifting[2]
            );
            this.scale = new Vector3( // scale
                element.shape.extent[0] * scaling[0] / scale_const,
                element.shape.extent[1] * scaling[1] / scale_const,
                0.00002f
                );
        }
        else
        {
            this.engine = "WODEN";
            this.position = new Vector3( // position
                element.position[0] * positioning[0], //+ shifting[0],
                element.position[1] * positioning[1], // + shifting[1],
                element.position[2] * positioning[2] // + shifting[2]
             );
            this.scale = new Vector3( // scale
                element.shape.extent[0] * scaling[0],
                element.shape.extent[1] * scaling[1],
                element.shape.extent[2] * scaling[2]
                );
        }

        switch (type)
        {
            case "cube":
                this.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "uvSphere":
                this.go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "cylinder":
                this.go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            case "RTEllipse":
                this.go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                this.go.transform.RotateAroundLocal(new Vector3(1, 0, 0), (float) Math.PI / 2.0f);
                scale = new Vector3(scale.x, scale.z, scale.y);
                break;
            case "RTBox":
                this.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "RTlabel":
                if (element.shape.text != "nil") {
                    Debug.Log("A label was found!!: '" + element.shape.text + "'");
                    this.go = new GameObject();
                    GameObject child = new GameObject("\""+element.shape.text+"\"");
                    child.transform.SetParent(go.transform);
                    Text t = child.AddComponent<Text>();
                    //child.AddComponent<RectTransform>();
                    t.text = element.shape.text;
                    child.GetComponent<Text>().font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
                } else {
                    this.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    position = Vector3.zero;
                    scale = Vector3.zero;
                }
                break;
            default:
                this.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                position = Vector3.zero;
                scale = Vector3.zero;
                break;
        }
        if (this.type == "RTlabel") {
            this.go.GetComponentInChildren<RectTransform>().position = position;
            this.go.GetComponentInChildren<RectTransform>().localScale = scale;
        } else {
            this.go.GetComponent<Renderer>().material.color = col;
        }

        this.go.transform.position = position;
        this.go.transform.localScale = scale;

        if (element.interactions != null)
            Debug.Log("element.interactions (number) = " + element.interactions.Count);

        if (type.Substring(0,2) == "RT") {
            this.go.tag = "WodenObj";
        } else  {
            this.go.tag = "Roassal2Obj";
        }
        this.go.name = id+"";

        this.transformParent(world);
    }
    private void print() {
        Debug.Log(
            "engine :" + this.engine +
            " | type: " + this.type +
            " | real_pos:" + this.go.transform.position +
            " | color" + (this.go.GetComponent<Renderer>().material.color).ToString()
            );
    }

    public void transformParent(GameObject world) {
        this.go.transform.parent = world.transform;
    }
}