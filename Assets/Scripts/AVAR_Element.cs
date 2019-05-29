using System;
using UnityEngine;

public class AVAR_Element
{
    public GameObject go;
    public string type;
    public Boolean isSelected; // Is this object selected for interaction?
    int id;
    public string engine; 
    
    public AVAR_Element(int id, string type, Vector3 position, Vector3 scale, Color col)
    {
        this.id = id;
        this.type = type;
        if (type.Substring(0,2) == "RT") {
            this.engine = "ROASSAL2";
        }
        else
        {
            this.engine = "WODEN";
        }

        switch (type)
        {
            case "cube":
                this.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "UVSphere":
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
            default:
                this.go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                position = Vector3.zero;
                scale = Vector3.zero;
                break;
        }
        this.go.transform.position = position;
        this.go.transform.localScale = scale;
        this.go.GetComponent<Renderer>().material.color = col;

        if (type.Substring(0,2) == "RT") {
            this.go.tag = "WodenObj";
        } else  {
            this.go.tag = "Roassal2Obj";
        }
        this.go.name = id+"";
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