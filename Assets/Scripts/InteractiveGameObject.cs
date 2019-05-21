using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.WindowsMixedReality;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using UnityEngine.UI;

public class InteractiveGameObject : MonoBehaviour/*, IMixedRealityInputHandler*///ObjectCommands : Singleton<ObjectCommands>
{
    public bool isActive = false;

    //private GameObject go_popup;
    //public Text text_popop;

    // blink variables
    Color originalColor;
    short count;
    public string popup_msg = "";
    Vector3 originalPosition;
    float originalDistance;
    Vector3 cameraOriginalPosition;

    public float deltaTime;
    float fps;

    void Awake() {
        /*originalColor = this.GetComponent<Renderer>().material.color;
        count = 0;*/
        //text_popop = new Text();
        //go_popup = new GameObject();
        //text_popop = go_popup.AddComponent<Text>();
        //text_popop.transform.SetParent(this.transform);
        originalPosition = this.transform.position;
        originalDistance = Vector3.Distance(Camera.main.transform.position, this.transform.position);
        cameraOriginalPosition = Camera.main.transform.position;
    }

    // values which indicates which is the interaction with the object once the user taps and gazes an object.
    //public const short mode = 0; 
    public string [] interactions;
    /*public int selected_mode = 0;
    public const int MAKE_RIGID_BODY = 0;
    public const int ROTATE = 1;*/

    //Dictionary<int, Action> functions = new Dictionary<int, Action> { rotate(), makeRigidBody() };

    /*Dictionary<string, Action> functions = new Dictionary<string, Action>
    {
        { "rotate", () => rotate() },
        { "makeRigidBody"  , () => makeRigidBody() }
    };*/

    void Update() {
        // interaction when the object is just gazed
        if(Array.IndexOf(this.interactions,"Popup") >= 0)
        {
            this.showPopup();
        }

        if (isActive) { // interactinos when the the object is selected

            showPopup();
            //showPopup();
            //changePosition();
            foreach (string interaction in this.interactions) { 
                switch (interaction) {
                    case "RigidBody":
                        makeRigidBody();
                        break;
                    case "Rotate":
                        rotate();
                        break;
                    case "Move":
                        moveElement();
                        break;
                    case "Blink":
                        blink();
                        break;
                    case "Popup":
                        this.showPopup();
                        break;
                    default:
                        //createPopUp("No interaction defined");
                        break;
                }
            }
        }
        //changePosition();
    }
    void OnAirTapped() {
        var selected = GameObject.Find("GestureManager").GetComponent<GazeGestureManager>().selected_object;
        if ( (this.name == "World" && selected == 0) || (this.name != "World" && selected == 1)) {
            if (!isActive) this.originalPosition = this.transform.position;

            isActive = !isActive;

            Debug.Log("ObjCommand[" + this.name + "].OnAirTapped, isActive = " + isActive + ")");
        }
    }

    void testTap()
    {
        this.showPopup("TAP!");
    }
    void testHold()
    {
        this.showPopup("Hold...");
    }

    private void rotate() {
        if (this.name == "World")
        {
            // nothing
        } else { // woden gameobjects
            this.transform.Rotate(0, 1, 0);
        }
    }
    private void makeRigidBody() {
        if (!this.GetComponent<Rigidbody>()) {
            var rigidbody = this.gameObject.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    private void showPopup() {
        //var popup_text = GameObject.Find("Canvas/Popup").GetComponent<Text>().text;
        //popup_text = "(" + this.name + " mode: " + this.mode + ")";
        this.showPopup( this.popup_msg );
    }

    private void setInactive() {
        if (this.name != "World" && this.isActive) {
            this.isActive = false;
            Debug.Log("ObjCommand[" + this.name + "].SetInactive, isActive = " + this.isActive + ")");
        }
    }
    private void setInactiveWorld() {
        if (this.name == "World" && this.isActive) {
            this.isActive = false;
            Debug.Log("ObjCommand[" + this.name + "].SetInactiveWorld, isActive = " + this.isActive + ")");
        }
    }

    private void moveElement()
    {
        if (this.name != "World"){ // moving a woden object
            //var distance = Camera.main.transform.position + originalPosition;
            //this.transform.position = distance;
            //this.transform.position = Camera.main.transform.forward + Camera.main.transform.position;
            
            var distance = Vector3.Distance(this.originalPosition, Camera.main.transform.position);
            this.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;

            //this.transform.position = Camera.main.transform.forward + originalPosition;// + new Vector3(0,0,-0.05f);
            //this.showPopup("[mv](WodenObj)" + this.transform.forward+" FPS: ");// + " (d="+distance+")";
        } else { // moving the world
                 //this.transform.position = Camera.main.transform.forward + Camera.main.transform.position + this.originalPosition;
                 //this.transform.position = this.originalDistance + Camera.main.transform.position;

            // it works partially (it doesn't take the rotation movement) 
            //this.transform.position = Camera.main.transform.position - this.cameraOriginalPosition;// + Camera.main.transform.forward);
            
            // attempt 1
            //this.transform.position = Vector3.Scale(Camera.main.transform.position - this.cameraOriginalPosition, Camera.main.transform.forward*this.originalDistance);// + Camera.main.transform.forward);
            // attempt2:
            //this.transform.position = Camera.main.transform.forward * (this.originalDistance + 1.0f);
            // attempt #3:
            //this.transform.position = Camera.main.transform.forward * Vector3.Distance(this.originalPosition, new Vector3(0,0,0));

            // attempt #4:
            var distance = Vector3.Distance(this.originalPosition, Camera.main.transform.position);

            this.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;

            // let's compute FPS
            this.deltaTime += (Time.deltaTime - this.deltaTime) * 0.1f;
            this.fps = 1.0f / deltaTime;

            this.showPopup("[mv](world) to" + this.transform.position + "FPS: " + String.Format("{0:0.##}", this.fps)); // + "|"+ Camera.main.transform.forward.normalized + "|"+distance);
            
        }
        // first: selection

    }

    //function to blink the text 
    public void blink() {
        //blink it forever. You can set a terminating condition depending upon your requirement. Here you can just set the isBlinking flag to false whenever you want the blinking to be stopped.
        Color originalColor = Color.yellow; // this.GetComponent<Renderer>().material.color;
        Color changedColor = Color.cyan;// originalColor;
        //float wait_time = 0.5f;
        //changedColor.a = 1.0f;
        if (isActive) {
            //this.GetComponent<Renderer>().material.color = changedColor;

            if (this.count > 10) {
                this.GetComponent<Renderer>().material.color = Color.blue;
            } else {
                this.GetComponent<Renderer>().material.color = Color.white;
                if (this.count == 20) this.count = 0;
            }
            this.count++;
        }
        //this.GetComponent<Renderer>().material.color = originalColor;
    }

    /*public void test() {
        if (isActive)
        {

        }
        this.GetComponent<Renderer>().material.shader = Color.yellow;
    }*/
    private void showPopup(string msg)
    {
        //if (GameObject.Find("GestureManager").GetComponent<GazeGestureManager>().hitInfo..collider.gameObject == null)
        if (GameObject.Find("GestureManager").GetComponent<GazeGestureManager>().FocusedObject != null) {
            GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text = msg;
            GameObject.Find("Canvas/PopupPanel").GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        } else if(GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text!=""){
            GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text = "";
            GameObject.Find("Canvas/PopupPanel").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
    }

}
