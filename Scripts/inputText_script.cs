using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Http;

using System.Net;
using System.Collections.Specialized;
using System.Text;
using System;

using System.Runtime.Serialization.Json;
using System.IO;

using System.Collections.Specialized;
using System.Net.Sockets;

public class inputText_script : MonoBehaviour {
    // attrs
    public InputField inputField;
    public Text inputText;
    public Text inputColoredText; // not used yet
    public Text alertText;
    private Boolean isHide;

    /* Friday May 5. Testing */
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

    private RootElement view;

    List<GameObject> gameObjects;
    float[] scaling = { 0.05f, 0.05f, 0.05f }; //{ 0.2f, 0.2f, 0.2f };
    float[] positioning = { 0.1f, -0.1f, 0.1f }; //{ 1.0f, 1.0f, 1.0f };//{ 0.2f, -0.2f, 0.2f };
    float[] shifting = { 0.00f, 0.00f, 1.00f };
    private static readonly float r = 1.0f;

    /*void OnGUI(){

        textFieldString = GUI.TextField(new Rect(500, 25, 100, 30), textFieldString, 25);
        textAreaString = GUI.TextArea(new Rect(700, 25, 100, 100), textAreaString, 250);
    }*/

    // Start is called before the first frame update
    void Start() {
        
        gameObjects = new List<GameObject>();
        this.transform.position = new Vector3(-2.0f, -1.0f, -1.0f);
        

        alertText.text = "(Use CTRL + D to execute)";

        this.isHide = false;
        
        // start focusing on input field
        inputField.Select();
        inputField.ActivateInputField();

        //if inputText.isEnabled then
        inputText.color = new Color(0.0f, 0.0f, 0.0f, 1.0f); // BORRAR Monday May 6th 
    }

    // Update is called once per frame
    void Update() {
        manageInput(inputField);

        // debug
        Vector3 canvasPosition = GameObject.FindObjectOfType<Canvas>().transform.position;

        /*double temp = Math.Sqrt(canvasPosition.x * canvasPosition.x + canvasPosition.y * canvasPosition.y + canvasPosition.z * canvasPosition.z);
        canvasPosition.x = canvasPosition.x / ((float)temp);
        canvasPosition.y = canvasPosition.y / ((float)temp);
        canvasPosition.z = canvasPosition.z / ((float)temp);
        */

        Vector3 canvasRotation = GameObject.FindObjectOfType<Canvas>().transform.eulerAngles;
        // tetha  = alpha
        // PI     = 180
        // tetha = PI * alpha / 180
        /*changeAlertMessage("(" +
            canvasPosition.x.ToString("F2") + ", " +
            canvasPosition.y.ToString("F2") + ", " +
            canvasPosition.z.ToString("F2") + ") | (" +
            Math.Cos(Math.PI * canvasRotation.x / 180.0f).ToString("F2") + ", " +
            Math.Cos(Math.PI * canvasRotation.y / 180.0f).ToString("F2") + ", " +
            Math.Cos(Math.PI * canvasRotation.z / 180.0f).ToString("F2") + ")", Color.yellow);
        this.alertText.fontSize = 14;*/

        this.shifting = new float[] {/*
                (float)(r * Math.Sin(Math.PI * canvasRotation.y / 180.0f) * Math.Cos( (Math.PI * canvasRotation.z / 180.0f) )),
                (float)(r * Math.Cos(Math.PI * canvasRotation.x / 180.0f) ) - 1.0f,
                (float)(r * Math.Sin(Math.PI * canvasRotation.y / 180.0f) * Math.Sin( (Math.PI * canvasRotation.z / 180.0f) )) + 1.0f*/

                r * ( canvasPosition.x + (float)( Math.Sin(Math.PI * canvasRotation.y / 180.0f) * Math.Cos( (Math.PI * canvasRotation.z / 180.0f) ))),
                r * ( canvasPosition.y + (float)( Math.Sin(1 - (Math.PI * canvasRotation.x / 180.0f)) * Math.Cos( 1 - (Math.PI * canvasRotation.z / 180.0f) )) - 0.5f),
                //(float)(r * Math.Sin(Math.PI * canvasRotation.y / 180.0f) * Math.Sin( (Math.PI * canvasRotation.x / 180.0f) )) + 1.0f
                2.0f * ( canvasPosition.z + (float)( Math.Cos(Math.PI * canvasRotation.z / 180.0f) ) -1.0f )// - r

            }; // 1.0f};
    }

    void manageInput(InputField input) {

        /*if (Input.anyKeyDown)
        {
            string [] texts = this.getHighlightedText(input.text);
            input.text = texts[0]; // plane
            inputColoredText.text = texts[1]; // plane
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
        
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D) ) {
            try {   // clean previos objects (if they exist)
                var objects = GameObject.FindGameObjectsWithTag("WodenObj");
                foreach (GameObject o in objects) {
                    Destroy(o.gameObject);
                }
            }
            finally { // send script to backend and deploy new geometries
                sendMsg(input.text);
                Debug.Log("Highlighted text: " + getHighlighted(input.text));
            }
        }
    }

    // TESTING WED 24TH
    // https://stackoverflow.com/questions/40155890/parse-nested-json-in-unity
    [System.Serializable]
    public class RootElement {
        public string key;
        public RWElement[] elements;
    }
    [System.Serializable]
    public class RWElement
    {
        public List<float> position;// = new List<float>();
        public string type;// = "";
        public Shape shape;// = new Shape();
        public List<float> from = new List<float>();
        public List<float> to = new List<float>();
        public List<float> color = new List<float>();
    }
    /*[System.Serializable]
    public class RWEdge
    {
        public string type;
        public List<float> from_position;
        public List<float> to_position;
        public List<float> color;
    }*/
    [System.Serializable]
    public class Shape {
        public string shapeDescription;
        public List<float> extent;
        public List<float> color;
        /*public Shape()
        {
            this.shapeDescription = "";
            this.extent = new List<float>();
            this.color = new List<float>();
        }*/
    }

    private void changeAlertMessage(string msg, Color color)
    {
        alertText.text = msg;
        alertText.color = color;
    }

    public void sendMsg(string script) {
        //string content = inputField.text;
        try {
            // SEND POST
            var request = (HttpWebRequest)WebRequest.Create(IP + ":" + port + "/");
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
            Debug.LogWarning("Json Content:");

            this.view = JsonUtility.FromJson<RootElement>(responseString);

            // Get camera position (shifting)
            /*this.shifting = new float[] {
                GameObject.FindObjectOfType<Camera>().transform.position.x,
                GameObject.FindObjectOfType<Camera>().transform.position.y,
                1.0f+GameObject.FindObjectOfType<Camera>().transform.position.z };
            */

            // GENERATE GEOMETRIES
            Debug.Log("NUMBER OF GEOMETRIES!!! " + this.view.elements.Length);
            for (int i = 0; i < this.view.elements.Length; i++)
            {
                Debug.Log("type :" + this.view.elements[i].type);// + " | pos:" + this.view.elements[i].position);
            }

            GameObject obj;
            int index_edges = 0;
            // elements
            for (int i = 0; i < this.view.elements.Length; i++)
            {
                if (this.view.elements[i].type == "camera")
                {
                    Debug.Log("type :" + this.view.elements[i].type + " | pos:" + this.view.elements[i].position);
                }
                else if (this.view.elements[i].type == "element")
                {
                    float maxY = 0f;
                    float maxZ = 0f;

                    switch (this.view.elements[i].shape.shapeDescription)
                    {
                        case "cube":
                            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            break;
                        case "UVSphere":
                            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            break;
                        case "cylinder":
                            obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                            break;
                        default:
                            // by default a cube is deployed
                            obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            break;

                    }

                    

                    // set position, scale, and shifting
                    obj.transform.position = new Vector3(
                        this.view.elements[i].position[0] * positioning[0] + shifting[0],
                        this.view.elements[i].position[1] * positioning[1] + shifting[1],
                        this.view.elements[i].position[2] * positioning[2] + shifting[2]
                        );
                    obj.transform.localScale = Vector3.Scale(transform.localScale, new Vector3(
                        this.view.elements[i].shape.extent[0] * scaling[0],
                        this.view.elements[i].shape.extent[1] * scaling[1],
                        this.view.elements[i].shape.extent[2] * scaling[2]
                        ));
                    // adding other properties
                    Color col = new Color(
                        this.view.elements[i].shape.color[0],
                        this.view.elements[i].shape.color[1],
                        this.view.elements[i].shape.color[2]);
                    col.a = this.view.elements[i].shape.color[3];
                    obj.GetComponent<Renderer>().material.color = col;

                    // add object to the list of objects
                    obj.tag = "WodenObj";
                    gameObjects.Add(obj);

                    Debug.Log(
                        "type :" + this.view.elements[i].type +
                        " | shape.shapeDescription: " + this.view.elements[i].shape.shapeDescription +
                        " | pos: (" + this.view.elements[i].position[0] + 
                        "," + this.view.elements[i].position[1] +
                        "," + this.view.elements[i].position[2] + ")" +
                        " | real (" + obj.transform.position[0] +
                        "," + obj.transform.position[1] +
                        "," + obj.transform.position[2] + ")" +
                        " | shape.extent: (" + this.view.elements[i].shape.extent[0] +
                        "," + this.view.elements[i].shape.extent[1] +
                        "," + this.view.elements[i].shape.extent[2] + ") " +
                        " | shape.color : (" + this.view.elements[i].shape.color[0] +
                        "," + this.view.elements[i].shape.color[1] +
                        "," + this.view.elements[i].shape.color[2] +
                        ", alpha=" + this.view.elements[i].shape.color[3] + ") "
                        );
                }

                else if (this.view.elements[i].type == "edge")
                {
                    obj = new GameObject();
                    obj.tag = "WodenObj";
                    var lr = obj.AddComponent<LineRenderer>();

                    /*this.view.elements[i].color[0] *= 255.0f;
                    this.view.elements[i].color[1] *= 255.0f;
                    this.view.elements[i].color[2] *= 255.0f;*/

                    /*obj = new GameObject();
                    obj.tag = "WodenObj";
                    obj.AddComponent<LineRenderer>();
                    LineRenderer lr = obj.GetComponent<LineRenderer>();*/
                    //lr.tag = "WodenObj";
                    Color color = new Color(
                        this.view.elements[i].color[0],
                        this.view.elements[i].color[1],
                        this.view.elements[i].color[2]
                        );
                    Debug.Log(
                        "EdgeColor: ("+ this.view.elements[i].color[0]
                        + ", " + this.view.elements[i].color[1]
                        + ", " + this.view.elements[i].color[2] + ")"
                        );
                    /*
                    Debug.Log("origin will be: (" +
                        this.view.elements[i].from[0] + ", " +
                        this.view.elements[i].from[1] + ", " +
                        this.view.elements[i].from[2] + ") ");
                    Debug.Log("destin will be: (" +
                        this.view.elements[i].to[0] + ", " +
                        this.view.elements[i].to[1] + ", " +
                        this.view.elements[i].to[2] + ") ");
                    */
                    var origin = new Vector3(
                            this.view.elements[i].from[0],
                            this.view.elements[i].from[1],
                            this.view.elements[i].from[2] //+ 1.0f
                            );
                    var destination = new Vector3(
                             this.view.elements[i].to[0],
                             this.view.elements[i].to[1],
                             this.view.elements[i].to[2] //+ 1.0f
                             );

                    var dist = Vector3.Distance(origin, destination);
                    Vector3 pointAlongLine = Vector3.Normalize(destination - origin) + origin;

                    /*lr.SetPosition(0, origin);
                    lr.SetPosition(1, pointAlongLine);*/

                    //lr.SetColors(color, color);
                    //lr.colorGradient.(color,color);
                    lr.startColor = color; lr.endColor = color;
                    obj.GetComponent<Renderer>().material.color = color;
                    //lr.SetWidth(0.01f, 0.01f); // usar SCALE
                    lr.startWidth = 0.01f; lr.endWidth = 0.01f;
                    lr.SetPosition(0,//index_edges,
                        new Vector3(
                            origin[0] * positioning[0] + shifting[0],
                            origin[1] * positioning[1] + shifting[1],
                            origin[2] * positioning[2] + shifting[2]
                            ));
                    lr.SetPosition(1,//(index_edges+1),
                        new Vector3(
                            destination[0] * positioning[0] + shifting[0],
                            destination[1] * positioning[1] + shifting[1],
                            destination[2] * positioning[2] + shifting[2]
                            ));
                    /*lr.transform.localScale = Vector3.Scale(transform.localScale, new Vector3(
                        0.01f * scaling[0],
                        0.01f * scaling[1],
                        0.01f * scaling[2]
                        ));*/
                    index_edges += 1;

                    Debug.Log(
                       "type :" + this.view.elements[i].type +
                       //" | shape.shapeDescription: " + this.view.elements[i].shape.shapeDescription +

                       " | elements.from_position: (" + this.view.elements[i].from[0] +
                       ", " + this.view.elements[i].from[1] +
                       ", " + this.view.elements[i].from[2] + ")" +
                       " (" + lr.GetPosition(0)[0] +
                       ", " + lr.GetPosition(0)[1] +
                       ", " + lr.GetPosition(0)[2] + ") " +
                       " | elements.to_position: (" + this.view.elements[i].to[0] +
                       ", " + this.view.elements[i].to[1] +
                       ", " + this.view.elements[i].to[2] + ")" +
                       " (" + lr.GetPosition(1)[0] +
                       ", " + lr.GetPosition(1)[1] +
                       ", " + lr.GetPosition(1)[2] + ") "
                       );
                    /*
                    Can't add component 'LineRenderer' to PlaygroundManager because such a component is already added to the game object!
UnityEngine.GameObject:AddComponent()
                    (Parece que Linerenderer sólo hay uno... así que debe ser global)
                    */
                }

            }
            
            
            changeAlertMessage("View loaded correctly", new Color(220, 220, 220));
        }
        catch (WebException e)
        {
            changeAlertMessage("Server Error", new Color(220, 0, 0));
            Debug.Log("Server Error");
        };
        
        
    }
    /*public GameObject createObject(string type)
    {
        GameObject obj;
        switch (type)
        {
            case "cube":
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                break;
            case "UVSphere":
                obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                break;
            case "cylinder":
                obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                break;
            case "line":
                obj = null;
        }
    }*/

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
        // string newtext = "";
        char[] delimiterChars = { '\n' };
        string[] reservedWords = new string[] { "RWView", "RWCube", "RWUVSphere", "RWCylinder", "RWXLineLayout", "." };
        string highlighted_text = "";
        string original_text    = "";
        string [] lines = text.Split(delimiterChars);
        string [] plane_lines = new string[lines.Length];
        
        for (int index_line = 0; index_line < lines.Length; index_line++)
        {

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
}
