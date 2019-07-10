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

    // console
    public GameObject consolePanel;
    public Text consoleText;
    private Vector2 consolePanel_dim;
    private Vector2 consoleText_dim;

    // highlighted text
    private Boolean[] parsedCodeBool;
    private string[] parsedCodeText;
    TextHighlighter th;
    HashSet<string> hset; // test bsotomayor 0618
    private ArrayList st_variables = new ArrayList();
    private ArrayList st_classes = new ArrayList();

    // text handlying
    private string textFieldString = "";
    private string textAreaString  = "";

    public string textArea;

    // connection attrs
    public string IP = "http://127.0.0.1";
    public string port = "1702";
    

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

    private short[] interactions = { InteractiveGameObject.MOVE, InteractiveGameObject.POPUP}; // Later, this will be read from JSON

    // Start is called before the first frame update
    void Start() {
        gameObjects = new List<GameObject>();
        fps = 0.0f;

        GameObject.Find("Canvas/PopupPanel").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        GameObject.Find("Canvas/DebugPanel").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text = "";
        //world_interaction.interactions = this.interactions; // when a world (or view) should has an interaction
        
        consoleText.text = "";
        consoleText_dim = consoleText.transform.localScale;
        consolePanel_dim = consolePanel.transform.localScale;
        consoleText.transform.localScale = Vector2.zero;
        consolePanel.transform.localScale = Vector2.zero;

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
        if (Input.anyKeyDown && !this.isHide) {
            // get highlitedtext
            //this.inputColoredText.text = th.justAnotherHLTAttempt(inputField.text);
            this.inputField.text = th.justAnotherHLTAttempt(inputField.text);
            /*this.inputColoredText.text = th.getHighlightedText(inputField.text);
            var outstr = "";
            outstr += "caretPlainText:" + this.inputField.caretPosition + "\n";
            outstr += "caretColorized:" + this.input.caretPosition + "\n";
            Debug.Log();*/
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

            byte[] data = data = System.Text.Encoding.ASCII.GetBytes(script);

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
            
            // let's read the response stream. It is stored in 'responseString' variable
            responseString = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();
            Debug.LogWarning("responseString: " + responseString);

            generate_geometries = !responseString.Contains("Message"); // true if the response contains elements

            if (responseString.Contains("Message")){ // Systax exception or error
                Debug.Log("ERROR: "+responseString+" ");
                Pharo_SyntaxError(responseString);
            } else { // script executed successfully !
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
                        
                        // adding other properties
                        
                        // adding interaction
                        var interaction = obj.go.AddComponent<InteractiveGameObject>();
                        interaction.interactions = this.interactions;

                        //if (interaction is popup):
                        interaction.popup_msg = obj.type;

                        element_count += 1;
                        
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
                } 
            }

            if (this.view.value != null) { // if text representention of Roassal vars is not null
                consoleText.text = ">> Result:\n"+this.view.value;
                consoleText.transform.localScale = this.consoleText_dim;
                consolePanel.transform.localScale = this.consolePanel_dim;
            } else {
                consoleText.transform.localScale = Vector3.zero;
                consolePanel.transform.localScale = Vector3.zero;
            }
            Debug.Log("consoleText.text="+consoleText.text);

            //this.world.transform.position = world_centroid_position; [Not yet]

            changeAlertMessage("View loaded correctly", new Color(220, 20, 20));

            Vector3 v_shifting = new Vector3(shifting[0], shifting[1], shifting[2]);

            var distance = 1.0f;
            current_world.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;
            if (this.engine == "WODEN") {
                current_world.transform.RotateAroundLocal(new Vector3(1, 0, 0), (float)Math.PI);
            } else {
                //current_world.transform.position += v_shifting;
            }
        }
        
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
        
        this.debugText.GetComponent<Text>().text = msg;
    }

    public void Pharo_SyntaxError(string msg) {
        throw new System.ArgumentException(msg);
    }
}
