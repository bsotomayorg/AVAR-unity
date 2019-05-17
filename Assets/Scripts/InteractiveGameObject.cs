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
    public string mode = "";
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

        if (isActive) {

            showPopup();
            //showPopup();
            //changePosition();
            switch (this.mode) {
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
                default:
                    //createPopUp("No interaction defined");
                    break;
            }
        }
        //changePosition();
    }
    void OnAirTapped() {
        if (!isActive) this.originalPosition = this.transform.position;
        isActive = !isActive;
        //this.GetComponent<Renderer>().material.color = originalColor;

        Debug.Log("ObjCommand["+this.name+"] > Message received! (mode = " + this.mode + ", isActive = " + isActive + ")");
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
        this.showPopup( popup_msg + " mode: "+this.mode );
    }

    private void moveElement()
    {
        if (this.name != "World"){ // moving a woden object
            //var distance = Camera.main.transform.position + originalPosition;
            //this.transform.position = distance;
            this.transform.position = Camera.main.transform.forward + Camera.main.transform.position;
            //this.transform.position = Camera.main.transform.forward + originalPosition;// + new Vector3(0,0,-0.05f);
            this.showPopup("[mv](WodenObj)" + this.transform.forward);// + " (d="+distance+")";
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
            var distance = Vector3.Distance(this.originalPosition, new Vector3(0, 0, 0));

            this.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;

            this.showPopup("[mv](world) to" + this.transform.position); // + "|"+ Camera.main.transform.forward.normalized + "|"+distance);

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
        GameObject.Find("Canvas/Popup").GetComponent<Text>().text = msg;
    }

}
