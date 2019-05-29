using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;

using System.Net;
using System.Collections.Specialized;
using System.Text;
using System;
using System.Linq;

using System.Runtime.Serialization.Json;
using System.IO;

using System.Collections.Specialized;
using System.Net.Sockets;


/* [DELETE]
 * public class WodenObject {
    public GameObject go;
    public string wodenType;
    public Boolean isSelected; // Is this object selected for interaction?
    int id;
    
    public WodenObject(GameObject go, int id, string wodenType) {
        this.go = go;
        this.id = id;
        this.wodenType = wodenType;
        this.isSelected = false;
    }
}

public class Roassal2Object {
    public GameObject go;
    public string roassalType;
    public Boolean isSelected;
    int id;

    public Roassal2Object(GameObject go, int id, string roassalType, Vector3 position, Vector3 scale) {
        this.go = go;
        this.id = id;
        this.roassalType = roassalType;
        this.isSelected = false;
        this.go.transform.position = position;
        this.go.transform.localScale = scale;
    }
}*/



public class playground : MonoBehaviour {
    // attrs
    public GameObject world;
    //public Hashtable world_edges;

    public InputField inputField;
    public Text inputText;
    public Text inputColoredText; // not used yet
    public Text alertText;
    private Boolean isHide;

    public float deltaTime;
    private float fps;
    
    private Boolean[] parsedCodeBool;
    private string[] parsedCodeText; 

    private string textFieldString = "text field";
    private string textAreaString  = "text area";

    public string textArea;

    public string IP = "http://127.0.0.1";
    public string port = "1702";
    private ArrayList st_variables = new ArrayList();
    private ArrayList st_classes = new ArrayList();

    private string responseString;
    private HttpWebResponse response;

    private string engine = "None"; // values: "WODEN", "ROASSAL2"

    private JSONRootElement view;

    private Boolean DEBUG = true;

    List<GameObject> gameObjects;
    float[] scaling = { 0.05f, 0.05f, 0.05f }; //{ 0.2f, 0.2f, 0.2f };
    float[] positioning = { 0.05f, -0.05f, 0.05f }; //{ 1.0f, 1.0f, 1.0f };//{ 0.2f, -0.2f, 0.2f };
    float[] shifting = { 0.00f, 0.00f, 1.00f };
    private static readonly float r = 1.0f;

    InteractiveGameObject world_interaction;
    
    float scale_const = (float)25.0f; // divisor

    private short[] interactions = {InteractiveGameObject.MOVE, InteractiveGameObject.POPUP}; // Later, this will be read from JSON

    // Start is called before the first frame update
    void Start() {
        gameObjects = new List<GameObject>();
        fps = 0.0f;

        GameObject.Find("Canvas/PopupPanel").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        GameObject.Find("Canvas/DebugPanel").GetComponent<Image>().color = new Color(0, 0, 0, 0);

        world_interaction = world.AddComponent<InteractiveGameObject>(); // test
        world_interaction.interactions = this.interactions;

        this.transform.position = new Vector3(-2.0f, -1.0f, -1.0f);
        
        alertText.text = "(Use CTRL + D to execute)";

        this.isHide = false;
        
        // start focusing on input field
        inputField.Select();
        inputField.ActivateInputField();

        //if inputText.isEnabled then
        inputText.color = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    }

    // Update is called once per frame
    void Update() {
        manageInput(inputField);

        if(!inputField.enabled) showDebug();

        // debug
        Vector3 canvasPosition = GameObject.FindObjectOfType<Canvas>().transform.position;
        Vector3 canvasRotation = GameObject.FindObjectOfType<Canvas>().transform.eulerAngles;

        // update edges
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Edge")) {
            var origin_dest = e.name.Split('-');
            string id_origin = origin_dest[0];
            string id_destination = origin_dest[1];
            GameObject o = GameObject.Find("World/" + id_origin);
            GameObject d = GameObject.Find("World/" + id_destination);
            Vector3 origin = o.transform.position;
            Vector3 destination = d.transform.position;

            var lr = e.GetComponent<LineRenderer>();
            lr.SetPosition(0, origin);
            lr.SetPosition(1, destination);
        }
    }

    void manageInput(InputField input) {


        /*if (Input.anyKey && !this.isHide) {
            var arr = getHighlightedText(input.text);
            this.inputColoredText.text = arr[0];
        }*/

        if (Input.GetKeyDown(KeyCode.LeftAlt)) {
            this.isHide = !this.isHide;

            if (this.isHide) {
                inputField.selectionColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                inputField.image.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            } else {
                inputField.selectionColor = new Color(0.429f, 0.674f, 1.0f, 0.753f);
                inputField.image.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }

            inputField.placeholder.enabled = !inputField.placeholder.enabled;
            inputField.enabled = !inputField.enabled;
            inputText.enabled = !inputText.enabled;
            alertText.enabled = !alertText.enabled;

            inputField.Select();
            inputField.ActivateInputField();
        }
        
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D) ) { // Do it!
            try {   
                // clean previous objects (if they exist)
                var objects = GameObject.FindGameObjectsWithTag("WodenObj");
                var rt2_objects = GameObject.FindGameObjectsWithTag("Roassal2Obj");
                var edges = GameObject.FindGameObjectsWithTag("Edge");

                foreach (GameObject o in objects)
                    Destroy(o.gameObject);

                foreach (GameObject o in rt2_objects)
                    Destroy(o.gameObject);

                foreach (GameObject o in edges)
                    Destroy(o.gameObject);
                
            }
            finally { // send script to backend and deploy new geometries
                sendMsg(input.text);
            }
        }
    }

    private void changeAlertMessage(string msg, Color color) {
        alertText.text = msg;
        alertText.color = color;
    }

    public void sendMsg(string script) {
        Debug.Log("SEND MSG: shifting:" + shifting[0] +"," + shifting[1] + "," + shifting[2] +".");
        try
        {
            // SEND POST
            var request = (HttpWebRequest)WebRequest.Create(IP + ":" + port + "/");
            request.Timeout = 3000;
            var data = System.Text.Encoding.ASCII.GetBytes(script + "v encodeAllElementsInSceneAsJSON");

            request.Method = "POST";
            request.ContentType = "text/xml;charset=\"utf-8\"";
            request.ContentLength = data.Length;

            // GET RESPONSE
            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }
            changeAlertMessage("Code sent correctly", new Color(220, 220, 220));

            response = (HttpWebResponse)request.GetResponse();
            responseString = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();

            Debug.LogWarning("responseString: " + responseString);

            this.view = JsonUtility.FromJson<JSONRootElement>(responseString);
            Debug.LogWarning("JSON contains " + this.view.elements.Length + " lines");
        }
        catch (Exception e)
        {
            var msg = "";
            if (e is WebException)
            {
                msg = "Server Error";
            }
            else
            {
                msg = "Error";
            }
            msg += "\n" + e.Message;
            msg += "\n" + e.TargetSite.ToString();

            changeAlertMessage(msg, new Color(220, 0, 0));
            Debug.Log(msg);

        };


        // GENERATE GEOMETRIES

        AVAR_Element obj;
        AVAR_Edge edge;
        //Roassal2Object robj; [DELETE]
        int index_edges = 0;

        float min_z = 1000.0f;
        int element_count = 0;

        this.world.transform.position = new Vector3(0, 0, 0);

        //Scale? (if it is possible to do on Pharo, then that's better!) ####
        float maxX = 1.0f;
        float maxZ = 1.0f;

        for (int i = 0; i < this.view.elements.Length; i++) {
            //test Tue, May 28th
            Debug.Log("type: " + this.view.elements[i].type);
            if (this.view.elements[i].type == "RTelement")
            {
                if (maxX < this.view.elements[i].position[0])
                    maxX = this.view.elements[i].position[0];
                if (maxZ < this.view.elements[i].position[1])
                    maxZ = this.view.elements[i].position[1];
            }
        }
        Debug.Log("MaxX: " + maxX + ", maxZ: " + maxZ);


        for (int i = 0; i < this.view.elements.Length; i++)
        {

            if (this.view.elements[i].type == "camera")
            {
                Debug.Log("type :" + this.view.elements[i].type + " | pos:" + this.view.elements[i].position);
            }
            else if (this.view.elements[i].type == "element")
            {
                if (engine == "NONE") engine = "WODEN";
                Debug.Log("A Woden 'element'");

                Color col = new Color(
                    this.view.elements[i].color[0],
                    this.view.elements[i].color[1],
                    this.view.elements[i].color[2]);
                col.a = this.view.elements[i].color[3];

                obj = new AVAR_Element(
                    this.view.elements[i].id,
                    this.view.elements[i].shape.shapeDescription,
                        new Vector3(
                        this.view.elements[i].position[0] * positioning[0], //+ shifting[0],
                        this.view.elements[i].position[1] * positioning[1], // + shifting[1],
                        this.view.elements[i].position[2] * positioning[2] // + shifting[2]
                        ),
                    Vector3.Scale(transform.localScale, new Vector3(
                        this.view.elements[i].shape.extent[0] * scaling[0],
                        this.view.elements[i].shape.extent[1] * scaling[1],
                        this.view.elements[i].shape.extent[2] * scaling[2]
                        )),
                    col
                    );
                /* [DELETE] switch (this.view.elements[i].shape.shapeDescription)
                {
                    case "cube":
                        obj = new AVARObject(
                            GameObject.CreatePrimitive(PrimitiveType.Cube),
                            this.view.elements[i].id,
                            "RWCube",
                            new Vector3(
                                this.view.elements[i].position[0] * positioning[0], // + shifting[0],
                                this.view.elements[i].position[1] * positioning[1], // + shifting[1],
                                this.view.elements[i].position[2] * positioning[2] // + shifting[2]
                            ),
                            Vector3.Scale(transform.localScale, new Vector3(
                                this.view.elements[i].shape.extent[0] * scaling[0],
                                this.view.elements[i].shape.extent[1] * scaling[1],
                                this.view.elements[i].shape.extent[2] * scaling[2]
                                )),
                        "WODEN"
                            );
                        break;
                    case "UVSphere":
                        obj = new AVARObject(
                            GameObject.CreatePrimitive(PrimitiveType.Sphere),
                            this.view.elements[i].id,
                            "RWUVSphere",
                            "WODEN"
                            );
                        break;
                    case "cylinder":
                        obj = new WodenObject(
                            GameObject.CreatePrimitive(PrimitiveType.Cylinder),
                            this.view.elements[i].id,
                            "RWCylinder");
                        break;
                    default:
                        // by default a cube is deployed
                        obj = new WodenObject(
                            GameObject.CreatePrimitive(PrimitiveType.Cube),
                            this.view.elements[i].id,
                            "Undefined");
                        break;
                }

                // set position, scale, and shifting
                obj.go.transform.position = new Vector3(
                    this.view.elements[i].position[0] * positioning[0], // + shifting[0],
                    this.view.elements[i].position[1] * positioning[1], // + shifting[1],
                    this.view.elements[i].position[2] * positioning[2] // + shifting[2]
                    );
                obj.go.transform.localScale = Vector3.Scale(transform.localScale, new Vector3(
                    this.view.elements[i].shape.extent[0] * scaling[0],
                    this.view.elements[i].shape.extent[1] * scaling[1],
                    this.view.elements[i].shape.extent[2] * scaling[2]
                    ));
                */

                // adding other properties
                    
                //obj.go.GetComponent<Renderer>().material.color = col;

                // add object to the list of objects
                //obj.go.tag = "WodenObj";
                //obj.go.transform.parent = world.transform;
                obj.transformParent(world);
                //obj.go.name = this.view.elements[i].id+"";

                // adding interaction
                var interaction = obj.go.AddComponent<InteractiveGameObject>();
                interaction.interactions = this.interactions;
                    
                //if (interaction is popup):
                interaction.popup_msg = obj.type;

                element_count += 1;

                //if (min_z > obj.go.transform.position.y) min_z = obj.go.transform.position.y;
                    
                /*Debug.Log(
                    "type :" + this.view.elements[i].type +
                    " | shape.shapeDescription: " + this.view.elements[i].shape.shapeDescription +
                    " | pos: (" + this.view.elements[i].position[0] +
                    "," + this.view.elements[i].position[1] +
                    "," + this.view.elements[i].position[2] + ")" +
                    " | real (" + obj.go.transform.position[0] +
                    "," + obj.go.transform.position[1] +
                    "," + obj.go.transform.position[2] + ")" +
                    " | shape.extent: (" + this.view.elements[i].shape.extent[0] +
                    "," + this.view.elements[i].shape.extent[1] +
                    "," + this.view.elements[i].shape.extent[2] + ") " +
                    " | shape.color : (" + this.view.elements[i].color[0] +
                    "," + this.view.elements[i].color[1] +
                    "," + this.view.elements[i].color[2] +
                    ", alpha=" + this.view.elements[i].color[3] + ") "
                    );*/
            }
            else if (this.view.elements[i].type == "edge")
            {
                Color col = new Color(
                        this.view.elements[i].color[0],
                        this.view.elements[i].color[1],
                        this.view.elements[i].color[2]
                        );
                col.a = this.view.elements[i].color[3];
                edge = new AVAR_Edge(
                    this.view.elements[i].from_id,
                    this.view.elements[i].to_id,
                    this.view.elements[i].type,
                    0.005f,
                    col
                    );

                edge.transformParent(world);

                //if (this.DEBUG)
                    edge.print();

                /*Debug.Log(
                    "EdgeColor: ("+ this.view.elements[i].color[0]
                    + ", " + this.view.elements[i].color[1]
                    + ", " + this.view.elements[i].color[2] + ")"
                    );

                string id_origin = this.view.elements[i].from_id.ToString();
                string id_destination = this.view.elements[i].to_id.ToString();
                Debug.Log("Edge which connects ["+id_origin+"] with ["+id_destination+"]");
                Vector3 origin = GameObject.Find("World/"+id_origin).transform.position;
                Vector3 destination = GameObject.Find("World/" + id_destination).transform.position;

                Debug.Log(
                    "[EDGE] type: " + this.view.elements[i].type +
                    "from: " + origin + " to: " + destination);

                var dist = Vector3.Distance(origin, destination);
                Vector3 pointAlongLine = Vector3.Normalize(destination - origin) + origin;
                    
                lr.startColor = color; lr.endColor = color;
                obj.go.GetComponent<Renderer>().material.color = color;
                lr.startWidth = 0.005f; lr.endWidth = 0.005f;
                lr.SetPosition(0, origin);
                lr.SetPosition(1, destination);*/

                index_edges += 1;
            }
            else if (this.view.elements[i].type == "RTelement")
            {
                    
                Color col = new Color(
                    this.view.elements[i].color[0],
                    this.view.elements[i].color[1],
                    this.view.elements[i].color[2]
                    );

                // Roassal2 objects
                if (engine == "NONE") engine = "ROASSAL2"; 
                    
                // Create a Roassal2 element
                obj = new AVAR_Element(
                    this.view.elements[i].id,
                    this.view.elements[i].shape.shapeDescription,
                    new Vector3( // position
                        (this.view.elements[i].position[0] * positioning[0]) / scale_const, // + shifting[0],
                        (this.view.elements[i].position[1] * positioning[1]) / scale_const // + shifting[1],
                        //shifting[2]
                        ),
                    new Vector3( // scale
                        this.view.elements[i].shape.extent[0] * scaling[0] / scale_const,
                        this.view.elements[i].shape.extent[1] * scaling[1] / scale_const,
                        0.00002f
                        ),
                    col
                    );
                /*
                Vector3 pos = new Vector3( // position
                                (this.view.elements[i].position[0] * positioning[0]) / scale_const + shifting[0],
                                (this.view.elements[i].position[1] * positioning[1]) / scale_const + shifting[1],
                                0.00002f
                                );
                Vector3 scale = new Vector3( // scale
                        this.view.elements[i].shape.extent[0] * scaling[0] / scale_const,
                        0.00002f,
                        this.view.elements[i].shape.extent[1] * scaling[1] / scale_const
                        );
                Debug.Log("[RTElement] pos" + pos);
                Debug.Log("[RTElement] scale" + scale);

                switch (this.view.elements[i].shape.shapeDescription)
                {
                    case "RTEllipse":
                            
                        edge = new Roassal2Object(
                            GameObject.CreatePrimitive(PrimitiveType.Cylinder),
                            this.view.elements[i].id,
                            "RTEllipse",
                            pos,
                            new Vector3( // scale
                                this.view.elements[i].shape.extent[0] * scaling[0] / scale_const,
                                0.00002f,
                                this.view.elements[i].shape.extent[1] * scaling[1] / scale_const
                                )
                            );
                        robj.go.transform.RotateAroundLocal(new Vector3(1, 0, 0), (float)Math.PI/2.0f);
                        break;
                    case "RTBox":
                        robj = new Roassal2Object(
                            GameObject.CreatePrimitive(PrimitiveType.Cube),
                            this.view.elements[i].id,
                            "RTBox",
                            pos,
                            new Vector3( // scale
                                this.view.elements[i].shape.extent[0] * scaling[0] / scale_const,
                                this.view.elements[i].shape.extent[1] * scaling[1] / scale_const,
                                0.00002f
                                )
                            );
                        break;
                    default:
                        // by default a cube is deployed
                        robj = new Roassal2Object(
                            GameObject.CreatePrimitive(PrimitiveType.Cube),
                            this.view.elements[i].id,
                            "Undefined",
                            new Vector3(0,0,0),
                            new Vector3(0,0,0)
                        );
                        break;
                }

                robj.go.tag = "Roassal2Obj";
                robj.go.transform.parent = world.transform;
                robj.go.name = this.view.elements[i].id + "";


                // adding other properties
                Color col = new Color(
                    this.view.elements[i].color[0],
                    this.view.elements[i].color[1],
                    this.view.elements[i].color[2]);
                col.a = this.view.elements[i].color[3];
                robj.go.GetComponent<Renderer>().material.color = col;
                */
                /*Debug.Log("RObj created: "+ robj.roassalType);
                Debug.Log(
                    "type :" + this.view.elements[i].type +
                    " | shape.shapeDescription: " + this.view.elements[i].shape.shapeDescription +
                    " | pos: (" + this.view.elements[i].position[0] +
                    "," + this.view.elements[i].position[1] + ")" +
                    " | real (" + robj.go.transform.position[0] +
                    "," + robj.go.transform.position[1] +")" +
                    " | shape.extent: (" + this.view.elements[i].shape.extent[0] +
                    "," + this.view.elements[i].shape.extent[1] + ") " +
                    " | shape.color : (" + this.view.elements[i].color[0] +
                    "," + this.view.elements[i].color[1] +
                    "," + this.view.elements[i].color[2] +
                    ", alpha=" + this.view.elements[i].color[3] + ") "
                    );
                */


                obj.transformParent(world);
            }

            else if (this.view.elements[i].type == "RTedge") {
                Color col = new Color(
                    this.view.elements[i].color[0],
                    this.view.elements[i].color[1],
                    this.view.elements[i].color[2]
                    );

                edge = new AVAR_Edge(
                    this.view.elements[i].to_id,
                    this.view.elements[i].from_id,
                    this.view.elements[i].type,
                    0.001f,
                    col
                    ); 

                edge.transformParent(world);

                /*
                obj.go.tag = "Roassal2Obj";
                obj.go.name = this.view.elements[i].id + "";

                var lr = obj.go.AddComponent<LineRenderer>();
                lr.tag = "Edge";
                lr.name = this.view.elements[i].from_id + "-" + this.view.elements[i].to_id;

                lr.transform.parent = world.transform;

                Color color = Color.white;

                Debug.Log(
                    "EdgeColor: (" + this.view.elements[i].color[0]
                    + ", " + this.view.elements[i].color[1]
                    + ", " + this.view.elements[i].color[2] + ")"
                    );

                string id_origin = this.view.elements[i].from_id.ToString();
                string id_destination = this.view.elements[i].to_id.ToString();
                Debug.Log("Edge which connects [" + id_origin + "] with [" + id_destination + "]");
                Vector3 origin = GameObject.Find("World/" + id_origin).transform.position;
                Vector3 destination = GameObject.Find("World/" + id_destination).transform.position;

                Debug.Log(
                    "[EDGE] type: " + this.view.elements[i].type +
                    "from: " + origin + " to: " + destination);

                var dist = Vector3.Distance(origin, destination);
                Vector3 pointAlongLine = Vector3.Normalize(destination - origin) + origin;

                lr.startColor = color; lr.endColor = color;
                obj.go.GetComponent<Renderer>().material.color = color;
                lr.startWidth = 0.001f; lr.endWidth = 0.001f;
                lr.SetPosition(0, origin);
                lr.SetPosition(1, destination);
                */ 

                index_edges += 1;
            }
        }
            
        //this.world.transform.position = world_ceontroid_position;
        changeAlertMessage("View loaded correctly", new Color(220, 20, 20));

        Vector3 v_shifting = new Vector3(shifting[0], shifting[1], shifting[2]);
        //if(maxX !=0.0f && maxZ!=0.0f)

        var distance = 1.0f;
        this.world.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance ;
        if (this.engine == "WODEN") {
            this.world.transform.RotateAroundLocal(new Vector3(1, 0, 0), (float)Math.PI);
        } else
        {
            this.world.transform.position = v_shifting;
        }
        
        
    }

    // function which returns a colored text (highlighted)
    public string getHighlighted(string text) {
        // string newtext = "";
        string[] lines = text.Split('\n');
        string[] new_line = new string[lines.Length];
        string new_text = "";

        //string[] separators = new string[] { ",", ".", "!", "\'", " ", "\'s" };
        string[] reservedWords = new string[] {
            "RWView", "RWCube", "RWUVSphere", "RWCylinder", "RWXLineLayout", "." };

        char[] delimiterChars = { ' ', '\n', '(', ')' }; //{ ' ', ',', '.', ':', '\t','(',')' };

        for (int index_line = 0; index_line < lines.Length; index_line++) {
            string line = lines[index_line];
            string[] letters = line.Split(' ');

            // highlighting
            for (int i = 1; i < letters.Length; i++) {
                if (letters[i] == ":=") { // look for variables:
                    this.st_variables.Add(letters[i - 1]); // Let's add previous token as variable!
                }
                else if (char.IsUpper(letters[i][0])) { // looking for classes:
                    this.st_classes.Add(letters[i]);
                }
            }
            new_line[index_line] = string.Join(" ", letters);
        }

        text = string.Join("\n", new_line);

        foreach (string word in reservedWords) {
            text = text.Replace(word, (string)("<color=blue>" + word + "</color>"));
        }
        Debug.LogWarning("[IN ] List of variables: \n");
        foreach (string word in st_variables) {
            text = text.Replace(word + " ", (string)("<color=green>" + word + " </color>"));
            Debug.LogWarning("\t[VARS] " + word);
        }
        Debug.LogWarning("[IN ] List of classes: \n");
        foreach (string word in st_classes) {
            //text = text.Replace(word + " ", (string)("<color=blue>" + word + " </color>"));
            text = text.Replace(word, (string)("<color=blue>" + word + "</color>"));
            Debug.LogWarning("\t[CLASS] '" + word + "'");
        }

        return text;

    }
    public string[] getHighlightedText(string text) {
         //  Notes
         //  + Add a boolean for tokens already colored. This variable should be static.
         //  + Skip "(" to evaluate if the token starts with Uppercase or not
         //  + Catch the defined variables on the playground.

        // string newtext = "";
        char[] delimiterChars = { '\n' };
        string[] reservedWords = new string[] { "RWView", "RWCube", "RWUVSphere", "RWCylinder", "RWXLineLayout", "." };
        string highlighted_text = "";
        string original_text    = "";
        string [] lines = text.Split(delimiterChars);
        string [] plane_lines = new string[lines.Length];
        
        for (int index_line = 0; index_line < lines.Length; index_line++) {

            string line = lines[index_line];
            // let's see if there's a "(" directly connected to a char. In that case, it will insert a " ".
            for (int i = 0; i < line.Length - 1; i++)
            {
                if ((line[i] == '(' || line[i] == ')') && line[i+1] != ' ')
                {
                    line = line.Insert(i + 1, " ");
                }
                
            }
            Debug.Log("PREVIOUS LINE: "+lines[index_line]);
            Debug.Log("CURRENT  LINE: " + line);

            plane_lines[index_line] = line;
            lines[index_line] = line;

            string[] tokens = line.Split(' ');


            // highlighting
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i].Length > 0)
                {
                    Debug.Log("[getHighlightedText] Token: '"+tokens[i]+"'");
                    if (char.IsUpper(tokens[i][0]))
                    {
                        Debug.Log("[getHighlightedText] <color=blue>" + tokens[i] + "</color> first char: " + tokens[i][0]);
                        //tokens[i] = "[" + tokens[i] + "]";
                        tokens[i] = "<color=blue>" + tokens[i] + "</color>";
                    } else if ((char)(tokens[i][tokens[i].Length - 1]) == ':')
                    {
                        Debug.Log("[getHighlightedText] <i>" + tokens[i] + "</i> first char: " + tokens[i][0]);
                        //tokens[i] = "[" + tokens[i] + "]";
                        tokens[i] = "<i>" + tokens[i] + "</i>";
                    }
                }

                // looking for variables
                /*if (tokens[i] == ":=")
                { // look for variables:
                    this.st_variables.Add(tokens[i - 1]); // Let's add previous token as variable!
                }
                else*/ /*if (char.IsUpper(tokens[i][0]))
                { // looking for classes:
                    this.st_classes.Add(tokens[i]);
                }*/
            }
            lines[index_line] = string.Join(" ", tokens);

            Debug.Log("plane line: " + plane_lines[index_line]);
            Debug.Log("hlghd line: " + line);

        }
        original_text = string.Join("\n", plane_lines);
        highlighted_text = string.Join("\n", lines);

        Debug.Log("O. TEXT:"+original_text);
        Debug.Log("H. TEXT:"+highlighted_text);

        this.inputText.text = original_text;
        Debug.Log("inputTextModified: '"+ original_text+"'");

        /*foreach (string word in reservedWords)
        {
            text = text.Replace(word, (string)("<color=blue>" + word + "</color>"));
        }*/
        /*Debug.LogWarning("[IN ] List of variables: \n");
        foreach (string word in st_variables)
        {
            text = text.Replace(word + " ", (string)("<color=green>" + word + " </color>"));
            Debug.LogWarning("\t[VARS] " + word);
        }
        Debug.LogWarning("[IN ] List of classes: \n");
        foreach (string word in st_classes)
        {
            //text = text.Replace(word + " ", (string)("<color=blue>" + word + " </color>"));
            text = text.Replace(word, (string)("<color=blue>" + word + "</color>"));
            Debug.LogWarning("\t[CLASS] '" + word + "'");
        }*/

       
        // temporal (Monday, May 6th)
        //this.changeAlertMessage(new_text.Replace("\n",""), Color.yellow);

        return new string [] {original_text, highlighted_text } ;

    }

    private void showDebug()
    {
        var msg = "FPS: " + String.Format("{0:0.##}", this.fps);
        GameObject.Find("Canvas/DebugPanel/Debug").GetComponent<Text>().text = msg;
        GameObject.Find("Canvas/DebugPanel").GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    }
}
