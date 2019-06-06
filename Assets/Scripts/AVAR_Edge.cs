using System;
using UnityEngine;

public class AVAR_Edge
{
    public GameObject go;
    public string type;
    public string engine;
    public LineRenderer lr;

    private Vector3 origin_pos;
    private Vector3 destination_pos;
    
    public AVAR_Edge(string id_origin, string id_destination, string type, float width, Color color, GameObject world)
    {
        this.go = new GameObject();
        Debug.LogWarning("[EDGE] "+id_origin+"-"+id_destination);

        // create the line
        this.lr = this.go.AddComponent<LineRenderer>();
        this.lr.tag = "Edge";
        this.lr.name = id_origin + "-" + id_destination;

        this.type = type;
        if (type.Substring(0, 2) == "RT") {
            this.engine = "ROASSAL2";
        } else {
            this.engine = "WODEN";
        }

        // set color
        this.go.GetComponent<Renderer>().material.color = color;
        this.lr.startColor = color; this.lr.endColor = color;
        this.lr.startWidth = width; this.lr.endWidth = width;

        // set position
        Debug.Log("LOOOKING FOR: World / " + world.name + "/" + id_origin);
        this.origin_pos = GameObject.Find("World/" + world.name + "/" + id_origin).transform.position;
        this.destination_pos = GameObject.Find("World/" + world.name + "/" + id_destination).transform.position;
        this.lr.SetPosition(0, this.origin_pos);
        this.lr.SetPosition(1, this.destination_pos);

        // set tag and name
        this.go.tag = "Edge";
        this.go.name = id_origin + "-" + id_destination;

        this.transformParent(world);
    }

    public void print()
    {
        Debug.Log(
            "engine :" + this.engine +
            " | type: " + this.type +
            " | from:" + this.origin_pos +
            " | to:" + this.destination_pos +
            " | color" + (this.lr.startColor).ToString()
            );
    }

    public void transformParent(GameObject world)
    {
        this.lr.transform.parent = world.transform;
    }
}