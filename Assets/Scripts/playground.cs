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
using System.Text.RegularExpressions;

public class playground : MonoBehaviour {
    // attrs
    public GameObject world;
    //public Hashtable world_edges;
    private short views_count = 0; // number of views 

    public InputField inputField;
    public Text inputText;
    public Text inputColoredText; // not used yet
    public Text alertText;
    private Boolean isHide;
    public GameObject debugPanel;
    public Text debugText;

    // debug
    private Boolean DEBUG = true;
    private double fps;
    private double dt = 0.0;
    private short frameCount = 0;
    private float updateRate = 1.0f;

    // highlighted text
    private Boolean[] parsedCodeBool;
    private string[] parsedCodeText;
    TextHighlighter th;

    // text handlying
    private string textFieldString = "";
    private string textAreaString  = "";

    // CTRL+Z implementation
    /*private string [] script_versions;
    private const short undo_lenght = 10;
    private short pointer = 0; // point to the current script version
    */

    public string textArea;

    // connection attrs
    public string IP = "http://127.0.0.1";
    public string port = "1702";
    private ArrayList st_variables = new ArrayList();
    private ArrayList st_classes = new ArrayList();

    private string responseString;
    private HttpWebResponse response;

    private string engine = "None"; // values: "WODEN", "ROASSAL2"

    private JSONRootElement view;
    
    List<GameObject> gameObjects;
    float[] scaling = { 0.05f, 0.05f, 0.05f }; //{ 0.2f, 0.2f, 0.2f };
    float[] positioning = { 0.05f, -0.05f, 0.05f }; //{ 1.0f, 1.0f, 1.0f };//{ 0.2f, -0.2f, 0.2f };
    float[] shifting = { 0.00f, 0.00f, 1.00f };
    private static readonly float r = 1.0f;

    //InteractiveGameObject world_interaction; // not yet
    
    float scale_const = (float)25.0f; // divisor

    private short[] interactions = {InteractiveGameObject.MOVE, InteractiveGameObject.POPUP}; // Later, this will be read from JSON

    // Start is called before the first frame update
    void Start() {
        gameObjects = new List<GameObject>();
        fps = 0.0f;

        GameObject.Find("Canvas/PopupPanel").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        GameObject.Find("Canvas/DebugPanel").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text = "";
        //world_interaction = world.AddComponent<InteractiveGameObject>(); // test
        //world_interaction.interactions = this.interactions;
        
        //script_versions = new string[undo_lenght];

        this.transform.position = new Vector3(-2.0f, -1.0f, -1.0f);
        
        alertText.text = "(Use CTRL + D to execute)";

        th = new TextHighlighter();

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

        if(this.DEBUG) showDebug();

        // debug
        Vector3 canvasPosition = GameObject.FindObjectOfType<Canvas>().transform.position;
        Vector3 canvasRotation = GameObject.FindObjectOfType<Canvas>().transform.eulerAngles;

        // update edges
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Edge")) {
            GameObject temp_world = e.transform.parent.gameObject;
            var origin_dest = e.name.Split('-');
            string id_origin = origin_dest[0];
            string id_destination = origin_dest[1];
            GameObject o = GameObject.Find("World/" + temp_world.name + "/" + id_origin);
            GameObject d = GameObject.Find("World/" + temp_world.name + "/" + id_destination);
            Vector3 origin = o.transform.position;
            Vector3 destination = d.transform.position;

            var lr = e.GetComponent<LineRenderer>();
            lr.SetPosition(0, origin);
            lr.SetPosition(1, destination);
        }
        
    }

    void manageInput(InputField input) {

        //string temp_script_versions = "";
        if (Input.anyKey && !this.isHide) {
            // highlighted text: this.inputColoredText.text = th.getHighlightedText(inputField.text);

            // save current script
            //this.script_versions[this.script_version.] = inputField.text;
            //this.script_versions.Add(inputField.text);
            /*if (this.pointer == 0 || this.script_versions[this.pointer - 1] != inputField.text)
            {
                if (this.pointer > 0 && this.pointer < (undo_lenght - 1))
                {
                    this.script_versions[this.pointer] = inputField.text;
                    this.pointer += 1;
                    if (this.script_versions[this.pointer] != "")
                    {
                        for (int i = this.pointer; i < undo_lenght;i++)
                        {
                            this.script_versions[i] = "";
                        }
                    }
                }
                else if (this.pointer == (undo_lenght-1))
                {
                    for (int i = 1; i < (undo_lenght); i++)
                    {
                        this.script_versions[i - 1] = this.script_versions[i];
                    }
                    this.script_versions[this.pointer] = inputField.text;
                }
                else
                {
                    this.script_versions[this.pointer] = inputField.text;
                    if (this.pointer < (undo_lenght-1)) this.pointer += 1;
                }

                for (int i = 0; i < (undo_lenght); i++)
                {
                    if (this.pointer != i)
                    {
                        temp_script_versions += "Script version [" + i + "] = " + this.script_versions[i] + "\n";
                    }
                    else
                    {
                        temp_script_versions += "Script version ->[" + i + "] = " + this.script_versions[i] + "\n";
                    }
                }
                Debug.Log(temp_script_versions + "pointer=" + this.pointer);
                temp_script_versions = "";
            }*/
           
        }

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

            // Rendering
            // hide or show canvas panels of views
            if (this.isHide)
            {
                GameObject w = GameObject.Find("World");
                for (int i = 0; i < w.transform.childCount; i++)
                {
                    GameObject child = w.transform.GetChild(i).gameObject;
                    child.GetComponent<Canvas>().transform.localScale = Vector3.one;
                }
            }
            else
            {
                GameObject w = GameObject.Find("World");
                for (int i = 0; i < w.transform.childCount; i++)
                {
                    GameObject child = w.transform.GetChild(i).gameObject;
                    child.GetComponent<Canvas>().transform.localScale = Vector3.zero;

                }
            }
        }

        /*if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Z)) { // Undo
            if(this.pointer > 0) this.pointer -= 1;
            inputField.text = this.script_versions[this.pointer];
            Debug.Log("Undo");
        }
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyDown(KeyCode.Y)) { // Redo
            if(this.pointer < (undo_lenght-1)) this.pointer += 1;
            inputField.text = this.script_versions[this.pointer];
            Debug.Log("Redo");
        }
        */
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D)) { // Do it!
            try {   
                // clean previous objects (if they exist)
                var objects = GameObject.FindGameObjectsWithTag("WodenObj");
                var rt2_objects = GameObject.FindGameObjectsWithTag("Roassal2Obj");
                var edges = GameObject.FindGameObjectsWithTag("Edge");

                if (Input.GetKey(KeyCode.LeftShift)){ // if LeftShift is also pressed, then all the views are removed
                    Debug.Log("LeftShift Pressed!");
                    foreach (GameObject o in objects)
                        Destroy(o.gameObject);

                    foreach (GameObject o in rt2_objects)
                        Destroy(o.gameObject);

                    foreach (GameObject o in edges)
                        Destroy(o.gameObject);

                    for (int i = 0; i < this.views_count; i++) {
                        Destroy(GameObject.Find("World/World" + i));
                        Debug.LogWarning("World/World" + i + " deleted!!");
                    }
                    this.views_count = 0;
                }
                
            }
            finally { // send script to backend and deploy new geometries
                GameObject new_view = new GameObject("World" + this.views_count);
                var interaction = new_view.AddComponent<InteractiveGameObject>();
                
                // let's add a Canvas for visualize Labels
                var c = new_view.AddComponent<Canvas>();
                // for rendering, let's add a Canvas Scaler which will avoid blurred text
                var cs = new_view.AddComponent<CanvasScaler>();
                cs.dynamicPixelsPerUnit = 25;

                c.renderMode = RenderMode.WorldSpace;
                c.pixelPerfect = true;
                interaction.interactions = this.interactions;

                new_view.transform.parent = this.world.transform;
                sendMsg(input.text, new_view);

                this.views_count += 1;
            }
        }
    }

    private void changeAlertMessage(string msg, Color color) {
        alertText.text = msg;
        alertText.color = color;
    }

    public void sendMsg(string script, GameObject current_world) {
        var generate_geometries = false;
        try
        {
            // SEND POST
            var request = (HttpWebRequest)WebRequest.Create(IP + ":" + port + "/");
            request.Timeout = 5000;
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

            /*Debug.Log("request.GetResponse?");
            Debug.Log("Header: " + request.Headers.ToString());
            Debug.Log("GetType: " + request.GetType().ToString());
            Debug.Log("GetResponseAsync.Status: " + request.GetResponseAsync().Status.ToString());
            Debug.Log("GetResponseAsync.Result: " + request);
            Debug.Log("HaveResponse: " + request.HaveResponse);
            Debug.Log("requestToString: " + request.ToString());
            Debug.Log("ContentType: " + request.ContentType);
            Debug.Log("GetRequestStream: " + request.GetRequestStream().ToString());
            Debug.Log("Cast?");*/
            response = (HttpWebResponse)request.GetResponse();
            /*
            Debug.Log("response");

            Stream test = response.GetResponseStream();
            Debug.Log("test");
            StreamReader t2 = new System.IO.StreamReader(test);
            Debug.Log("t2");
            responseString = t2.ReadToEnd();
            Debug.Log("!");*/

            responseString = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();

            Debug.LogWarning("responseString: " + responseString);

            generate_geometries = !responseString.Contains("Message"); // true if the response contains elements

            if (responseString.Contains("Message")){ // Systax exception or error
                Debug.Log("ERROR: "+responseString+" ");
                Pharo_SyntaxError(responseString);
            } else {
                this.view = JsonUtility.FromJson<JSONRootElement>(responseString);
                if(this.view.elements != null)
                    Debug.LogWarning("JSON contains " + this.view.elements.Length + " lines");
            }
            
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
            msg += "\n" + e.Source;
            /*msg += "\n" + e.Data;
            var st = new System.Diagnostics.StackTrace(e, true);
            // Get the top stack frame
            var frame = st.GetFrame(0);
            // Get the line number from the stack frame
            var line = frame.GetFileLineNumber();
            msg += "\nline:'" + line + "'";*/

            changeAlertMessage(msg, new Color(220, 0, 0));
            Debug.Log(msg+"\n"+e.StackTrace.ToString());

        };
        
        // GENERATE GEOMETRIES
        if (generate_geometries) {
            AVAR_Element obj;
            AVAR_Edge edge;
            int index_edges = 0;

            float min_z = 1000.0f;
            int element_count = 0;

            current_world.transform.position = new Vector3(0, 0, 0);

            //Scale? (if it is possible to do on Pharo, then that's better!) ####
            float maxX = 1.0f;
            float maxZ = 1.0f;

            if (this.view.RTelements !=null) for (int i = 0; i < this.view.RTelements.Length; i++)
            {
                //Debug.Log("type: " + this.view.RTelements[i].type);
                if (this.view.RTelements[i].type == "RTelement") // No es necesario
                {
                    if (maxX < this.view.RTelements[i].position[0])
                        maxX = this.view.RTelements[i].position[0];
                    if (maxZ < this.view.RTelements[i].position[1])
                        maxZ = this.view.RTelements[i].position[1];
                }

            }
            //Debug.Log("MaxX: " + maxX + ", maxZ: " + maxZ);


            if (this.view.elements != null) for (int i = 0; i < this.view.elements.Length; i++)
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
                            this.view.elements[i],
                            positioning,
                            scaling,
                            scale_const, //not necessary
                            col,
                            current_world);
                        /*obj = new AVAR_Element(
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
                            col,
                            current_world
                            );*/

                        // adding other properties

                        //obj.go.GetComponent<Renderer>().material.color = col;

                        // add object to the list of objects
                        //obj.go.tag = "WodenObj";
                        //obj.go.transform.parent = world.transform;
                        // not necessary: obj.transformParent(current_world);
                        //obj.go.name = this.view.elements[i].id+"";

                        // adding interaction
                        var interaction = obj.go.AddComponent<InteractiveGameObject>();
                        interaction.interactions = this.interactions;

                        //if (interaction is popup):
                        interaction.popup_msg = obj.type;

                        element_count += 1;

                        //if (min_z > obj.go.transform.position.y) min_z = obj.go.transform.position.y;

                    }
                    if (this.view.elements[i].type == "edge")
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
                            col,
                            current_world
                            );

                        //edge.transformParent(current_world);
                        
                        //if (this.DEBUG)
                        edge.print();

                        index_edges += 1;
                    }
                }
            if (this.view.RTelements !=null) for (int i = 0; i < this.view.RTelements.Length; i++)
            {
                if (this.view.RTelements[i].type == "RTelement") {

                    Color col = new Color(
                        this.view.RTelements[i].color[0],
                        this.view.RTelements[i].color[1],
                        this.view.RTelements[i].color[2]
                        );

                    // Roassal2 objects
                    if (engine == "NONE") engine = "ROASSAL2";

                        // Create a Roassal2 element
                    obj = new AVAR_Element(
                        this.view.RTelements[i],
                        positioning,
                        scaling,
                        scale_const,
                        col,
                        current_world);
                    /*  scale_const,
                        this.view.RTelements[i].id,
                        this.view.RTelements[i].shape.shapeDescription,
                        new Vector3( // position
                            (this.view.RTelements[i].position[0] * positioning[0]) / scale_const, // + shifting[0],
                            (this.view.RTelements[i].position[1] * positioning[1]) / scale_const // + shifting[1],
                                                                                                 //shifting[2]
                            ),
                        new Vector3( // scale
                            this.view.RTelements[i].shape.extent[0] * scaling[0] / scale_const,
                            this.view.RTelements[i].shape.extent[1] * scaling[1] / scale_const,
                            0.00002f
                            ),
                        col,
                        current_world
                        );*/

                    //obj.transformParent(current_world);
                }

                else if (this.view.RTelements[i].type == "RTedge") {
                    Color col = new Color(
                        this.view.RTelements[i].color[0],
                        this.view.RTelements[i].color[1],
                        this.view.RTelements[i].color[2]
                        );

                    edge = new AVAR_Edge(
                        this.view.RTelements[i].to_id,
                        this.view.RTelements[i].from_id,
                        this.view.RTelements[i].type,
                        0.001f,
                        col,
                        current_world
                        );

                    edge.print();
                    edge.transformParent(current_world);

                    index_edges += 1;
                } // else if (this.view.RTelements[i].shape.type)
            }

            //this.world.transform.position = world_ceontroid_position;
            changeAlertMessage("View loaded correctly", new Color(220, 20, 20));

            Vector3 v_shifting = new Vector3(shifting[0], shifting[1], shifting[2]);
            //if(maxX !=0.0f && maxZ!=0.0f)

            var distance = 1.0f;
            current_world.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;
            if (this.engine == "WODEN") {
                current_world.transform.RotateAroundLocal(new Vector3(1, 0, 0), (float)Math.PI);
            } else {
                //nothing yet! current_world.transform.position += v_shifting;
            }
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
        this.frameCount++;
        this.dt += Time.deltaTime;
        if (this.dt > 1.0 / this.updateRate) {
            this.fps = frameCount / this.dt;
            frameCount = 0;
            this.dt -= 1.0 / this.updateRate;
        }
        var msg = "FPS: " + String.Format("{0:0.##}", this.fps)+
            "\nUpdate rate = "+ String.Format("{0:0.##}", this.updateRate)+ "s";
        //this.fps = 1.0 / Time.deltaTime;

        var debugPanel = GameObject.Find("Canvas/DebugPanel"); 

        this.debugPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        if (!this.isHide)
        {
            this.debugPanel.GetComponent<Image>().color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            this.debugText.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        } else
        {
            this.debugPanel.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            this.debugText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        
        //Debug.Log( th.getHighlightedText(inputField.text) );
        //msg += "\nL:"+th.getTokensAsStrings(); //th.getHighlightedText(inputField.text);
        
        this.debugText.GetComponent<Text>().text = msg;
    }

    public void Pharo_SyntaxError(string msg) {
        throw new System.ArgumentException(msg);
    }
}
