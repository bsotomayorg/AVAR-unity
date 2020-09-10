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

using System.Net.Sockets;
using System.Text.RegularExpressions;

public class playground : MonoBehaviour
{
    #region public variables
    // attrs
    public GameObject world;


    public textsInWindow debugPanel;
    public textsInWindow PopupPanel;
    /// <summary>
    /// to change your ip setting
    /// </summary>
    public EditIpSetting m_IpSetting;
    /// <summary>
    /// codeEditor
    /// </summary>
    public CodeEditor m_codeEditor;
    /// <summary>
    /// whether the CodeEditor is visible.
    /// </summary>
    public WorldCursor m_cursor;
    public bool isCodeEditorOn { get; private set; }
    #endregion

    private Color NormalColor = new Color(0.2f,0.2f,0.2f,1);
    private Color ErrorColor = Color.red;


    #region private variables
    /// <summary>
    /// the parent of the generated virtual objects.
    /// </summary>
    private short views_count = 0; // number of views 

    // debug
    private bool DEBUG = true;
    private double fps;
    private double dt = 0.0;
    private short frameCount = 0;
    private float updateRate = 1.0f;

    /// <summary>
    /// is this component already gotten initialized?
    /// </summary>
    private bool isInited = false;

    #endregion




    //private Vector2 consolePanel_dim;
    //private Vector2 consoleText_dim;

    // highlighted text
    private Boolean[] parsedCodeBool;
    private string[] parsedCodeText;
    TextHighlighter th;
    HashSet<string> hset; // test bsotomayor 0618
    private ArrayList st_variables = new ArrayList();
    private ArrayList st_classes = new ArrayList();





    private string responseString;
    private HttpWebResponse response;
    private string engine = "None"; // values: "WODEN", "ROASSAL2"
    private JSONRootElement view;

    float[] scaling = { 0.05f, 0.05f, 0.05f }; //{ 0.2f, 0.2f, 0.2f };
    float[] positioning = { 0.05f, -0.05f, 0.05f }; //{ 1.0f, 1.0f, 1.0f };//{ 0.2f, -0.2f, 0.2f };
    float[] shifting = { 0.00f, 0.00f, 1.00f };
    private static readonly float r = 1.0f;

    //InteractiveGameObject world_interaction; // not yet

    float scale_const = (float)25.0f; // divisor

    private short[] interactions = { InteractiveGameObject.MOVE, InteractiveGameObject.POPUP }; // Later, this will be read from JSON




    #region properties
    private static playground instance;

    public Material planeMaterail;
    private bool isIpSettingOn ;
    private float timeCounter;
    private string savedCodes;

    public static playground Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<playground>();
            return instance;
        }
    }
    #endregion

    // Start is called before the first frame update
    /// <summary>
    /// initialization function
    /// </summary>
    public void f_Init()
    {

        debugPanel.f_Init();
        PopupPanel.f_Init();

        m_codeEditor.f_Init();
        m_IpSetting.f_Init();
        PopupPanel.gameObject.SetActive(false);
        isIpSettingOn = true;
        m_IpSetting.IPsetting(isIpSettingOn);

        fps = 0.0f;

        //world_interaction.interactions = this.interactions; // when a world (or view) should has an interaction


        this.transform.position = new Vector3(-2.0f, -1.0f, -1.0f);
        changeAlertMessage("(Use CTRL + D to execute)",NormalColor);
        //alertText.text = "(Use CTRL + D to execute)";

        th = new TextHighlighter();

        isCodeEditorOn = true;
        isInited = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInited)
            return;
        manageInput();
        if (this.DEBUG) showDebug();


        // update edges
        foreach (GameObject e in GameObject.FindGameObjectsWithTag("Edge"))
        {
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
        if (timeCounter < 1)
            timeCounter += Time.deltaTime;
        else
        {
            timeCounter = 0;
            if (GazeGestureManager.Instance.FocusedObject == null)
            {
                OperationLog.Instance.AddToPosLog(Camera.main.transform.position, Camera.main.transform.eulerAngles, "there is nothing in the direction of ", Camera.main.transform.forward);
            }
            else
            {
                GameObject temp = GazeGestureManager.Instance.FocusedObject;
                while (true)
                {
                    if (temp.transform.parent != world.transform)
                    {
                        temp = temp.transform.parent.gameObject;
                    }
                    else
                    {
                        break;
                    }
                }
                OperationLog.Instance.AddToPosLog(Camera.main.transform.position, Camera.main.transform.eulerAngles, " looking at " + GazeGestureManager.Instance.FocusedObject.name + " in " + temp.name + " on ", GazeGestureManager.Instance.FocusedObject.transform.position);
            }
        }
    }


    void manageInput()
    {

        //string temp_script_versions = "";
        if (Input.anyKeyDown && this.isCodeEditorOn)
        {
            // get highlitedtext
            //this.inputColoredText.text = th.justAnotherHLTAttempt(inputField.text);
            //this.inputField.text = th.justAnotherHLTAttempt(inputField.text);
            /*this.inputColoredText.text = th.getHighlightedText(inputField.text);
            var outstr = "";
            outstr += "caretPlainText:" + this.inputField.caretPosition + "\n";
            outstr += "caretColorized:" + this.input.caretPosition + "\n";
            Debug.Log();*/
        }

        if (isCodeEditorOn&&!isIpSettingOn)
        {
            //execute the codes
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
            { // Do it!
                try
                {
                    // clean previous objects (if they exist)
                    var objects = GameObject.FindGameObjectsWithTag("WodenObj");
                    var rt2_objects = GameObject.FindGameObjectsWithTag("Roassal2Obj");
                    var edges = GameObject.FindGameObjectsWithTag("Edge");

                    if (Input.GetKey(KeyCode.LeftShift))
                    { // if LeftShift is also pressed, then all the views are removed
                        Debug.Log("LeftShift Pressed!");
                        foreach (GameObject o in objects)
                            Destroy(o.gameObject);

                        foreach (GameObject o in rt2_objects)
                            Destroy(o.gameObject);

                        foreach (GameObject o in edges)
                            Destroy(o.gameObject);

                        for (int i = 0; i < this.views_count; i++)
                        {
                            Destroy(GameObject.Find("World/World" + i));
                            Debug.LogWarning("World/World" + i + " deleted!!");
                        }
                        this.views_count = 0;
                    }

                }
                finally
                { // send script to backend and deploy new geometries
                    GameObject new_view = new GameObject("World" + this.views_count);
                    string tempText = string.Empty;
                    new_view.AddComponent<InteractiveGameObject>().f_Init(true, this.interactions, tempText);
                    new_view.transform.parent = this.world.transform;
                    sendMsg(m_codeEditor.CodeInputPanel.M_Input.text, new_view);

                    this.views_count += 1;

                    tempText = m_codeEditor.CodeInputPanel.M_Input.text + "\n" + m_codeEditor.ConsolePanel.M_Input.text+"\n"+ new_view.transform.childCount + " objects\n";
                    OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.ExecuteScript,tempText);

                }
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Tab))
            {
                m_codeEditor.ChangeActiveIndex(false);
            }
            else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Tab))
            {
                m_codeEditor.ChangeActiveIndex(true);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            {
                savedCodes = m_codeEditor.CodeInputPanel.M_Input.text;
                changeAlertMessage("(Ctrl+S)", NormalColor);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                m_codeEditor.CodeInputPanel.M_Input.text = savedCodes;
            }
        }

        //show/hide inputfield
        if (Input.GetKeyUp(KeyCode.LeftAlt)&&!isIpSettingOn)
        {
            SetCodeEditor();
        }
        //if(Input.GetKeyDown(KeyCode.F1))
        //IP address setting
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.N)||Input.GetKeyDown(KeyCode.F1))
        {
            isIpSettingOn = !isIpSettingOn;
            m_IpSetting.IPsetting(isIpSettingOn);
            //m_IpSetting.gameObject.SetActive(isIpSettingOn);
        }
    }

    private void changeAlertMessage(string msg, Color color)
    {
        m_codeEditor.ConsolePanel.M_Input.text = msg;
        m_codeEditor.ConsolePanel.M_Input.textComponent.color = color;
        //alertText.text = msg;
        //alertText.color = color;
    }

    public void sendMsg(string script, GameObject current_world)
    {
        var generate_geometries = false;
        try
        {
            // SEND POST
            var request = (HttpWebRequest)WebRequest.Create("http://"+m_IpSetting.input_IPAddress.text + ":" + m_IpSetting.input_port.text + "/");
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
            changeAlertMessage("Code sent correctly", NormalColor);

            response = (HttpWebResponse)request.GetResponse();

            // let's read the response stream. It is stored in 'responseString' variable
            responseString = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();
            Debug.LogWarning("responseString: " + responseString);

            // ### [BEGIN] bsotomayor edit 081219 ### 
            generate_geometries = responseString.Contains("elements"); // true if the response contains elements

            // script executed successfully !
            this.view = JsonUtility.FromJson<JSONRootElement>(responseString);
            if (this.view.elements != null)
            {
                Debug.LogWarning("JSON contains " + this.view.elements.Length + " lines");
            }
            else
            {
                Debug.Log("ERROR loc = " + this.view.errormsg.location + " msg:" + this.view.errormsg.message);
                var msg = "";
                Color c;
                if (this.view.value != "" && this.view.errormsg.message == null)
                { // Textual Value 
                    msg = this.view.value;
                    Debug.Log("this.view.value = " + this.view.value);
                    c = new Color(0, 0, 0);
                }
                else
                {
                    if (this.view.errormsg.location == null)
                    { // Error
                        msg = this.view.errormsg.message;
                        Debug.Log("this.view.errormsg.location = 0");
                        c = new Color(220, 0, 0);
                    }
                    else
                    { // SyntaxError
                        msg = this.view.errormsg.message + "\nLocation: " + this.view.errormsg.location;
                        Debug.Log("this.view.errormsg.location = " + this.view.errormsg.location);
                        c = new Color(220, 0, 0);

                        // lets change the caretPosition by the Error location.
                        m_codeEditor.CodeInputPanel.M_Input.caretPosition = (Int16.Parse(this.view.errormsg.location)) - 1;

                    }
                }
                Debug.Log("msg = '" + msg + "' color '" + c.ToString() + "'");
                changeAlertMessage(msg, c);
            }
            // ### [END] bsotomayor edit 081219 ### 
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

            changeAlertMessage(msg, ErrorColor);
            Debug.Log(msg + "\n" + e.StackTrace.ToString());

        };

        // GENERATE GEOMETRIES
        if (generate_geometries)
        {
            AVAR_Element obj;
            AVAR_Edge edge;
            int index_edges = 0;

            float min_z = 1000.0f;
            int element_count = 0;

            current_world.transform.position = new Vector3(0, 0, 0);

            //Scale? (if it is possible to do on Pharo, then that's better!) ####
            float maxX = 1.0f;
            float maxZ = 1.0f;

            if (this.view.RTelements != null) for (int i = 0; i < this.view.RTelements.Length; i++)
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

            if (this.view.elements != null)
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
                            this.view.elements[i],
                            positioning,
                            scaling,
                            scale_const, //not necessary
                            col,
                            current_world);

                        // adding other properties
                        // adding interaction
                        // [BEGIN] bsotomayor 081919 (popup interaction for 3D viz)
                        obj.go.AddComponent<InteractiveGameObject>().f_Init(false,this.interactions, this.view.elements[i].model);
                        // [END] bsotomayor 081919 (popup interaction for 3D viz)


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
            if (this.view.RTelements != null) for (int i = 0; i < this.view.RTelements.Length; i++)
                {

                    if (this.view.RTelements[i].type == "RTelement")
                    {

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

                        // Add popup text for Roassal2 elements
                        // [BEGIN] bsotomayor 081919 (for popup interaction in 2D viz)
                        obj.go.AddComponent<InteractiveGameObject>().f_Init(false, this.interactions, this.view.RTelements[i].model);
                        // [END] bsotomayor 081919 (for popup interaction in 2D viz)

                    }

                    else if (this.view.RTelements[i].type == "RTedge")
                    {
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
            changeAlertMessage("View loaded correctly", NormalColor);


            if (this.view.value != null)
            { // if text representention of Roassal vars is not null
              //consolePanel.gameObject.SetActive(true);
              //consolePanel.m_text.text= ">> Result:\n" + this.view.value;
              ////consoleText.transform.localScale = this.consoleText_dim;
              ////consolePanel.transform.localScale = this.consolePanel_dim;
                changeAlertMessage(" " + view.value.Replace('\r', '\n'), NormalColor);//   '/r' in pharo , '\n' in C#
            }
            else
            {
                //consolePanel.gameObject.SetActive(false);
                //consoleText.transform.localScale = Vector3.zero;
                //consolePanel.transform.localScale = Vector3.zero;
            }
            //Debug.Log("consoleText.text=" + consolePanel.m_text.text);

            //this.world.transform.position = world_centroid_position; [Not yet]

            //changeAlertMessage("View loaded correctly", new Color(220, 20, 20));

            Vector3 v_shifting = new Vector3(shifting[0], shifting[1], shifting[2]);

            var distance = 1.5f;
            current_world.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;
            if (this.view.elements != null)
            {
                current_world.transform.eulerAngles = 180 * Vector3.right;
            }
            else
            {
                current_world.transform.LookAt(2 * current_world.transform.position - Camera.main.transform.position);
            }


            //SetCodeEditor();

        }
        if (current_world.transform.childCount > 0)
        {
            SetCodeEditor();
        }
    }

    private void showDebug()
    {
        this.frameCount++;
        this.dt += Time.deltaTime;
        if (this.dt > 1.0 / this.updateRate)
        {
            this.fps = frameCount / this.dt;
            frameCount = 0;
            this.dt -= 1.0 / this.updateRate;
        }
        var msg = "FPS: " + String.Format("{0:0.##}", this.fps) +
            "\nUpdate rate = " + String.Format("{0:0.##}", this.updateRate) + "s";


        debugPanel.gameObject.SetActive(!isCodeEditorOn);
        debugPanel.m_text.text = msg;
    }

    public void Pharo_SyntaxError(string msg)
    {
        throw new System.ArgumentException(msg);
    }

    private void SetCodeEditor()
    {
        isCodeEditorOn = !isCodeEditorOn;
        m_codeEditor.gameObject.SetActive(isCodeEditorOn);
        m_cursor.gameObject.SetActive(!isCodeEditorOn);

        if (!isCodeEditorOn)
        {
            for (int i = 0; i < world.transform.childCount; i++)
            {
                world.transform.GetChild(i).transform.localScale = Vector3.one;
            }
            
        }
        else
        {
            for (int i = 0; i < world.transform.childCount; i++)
            {
                world.transform.GetChild(i).transform.localScale = Vector3.zero;
            }
            m_codeEditor.changeActivePanel(0);
        }



    }

    private int gameObjectCounter(JSONRootElement v)
    {
        if (v == null) return 0;
        int c = 0;
        if (v.elements != null) foreach (JSONElement e in v.elements) if (e.type == "RTelement" || e.type == "element" || e.type == "RTedge" || e.type == "edge") c++;
        if (v.RTelements != null) foreach (JSONElement e in v.RTelements) if (e.type == "RTelement" || e.type == "element" || e.type == "RTedge" || e.type == "edge") c++;
        return c;
    }
}
