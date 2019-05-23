using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class InteractiveGameObject : MonoBehaviour {
    public bool isActive = false;
    

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
        originalPosition = this.transform.position;
        originalDistance = Vector3.Distance(Camera.main.transform.position, this.transform.position);
        cameraOriginalPosition = Camera.main.transform.position;
    }

    // values which indicates which is the interaction with the object once the user taps and gazes an object.
    public const short RIGID_BODY = 0;
    public const short ROTATE = 1;
    public const short MOVE = 2;
    public const short POPUP = 3;

    public short [] interactions;

    void Update() {
        // interaction when the object is just gazed
        if(Array.IndexOf(this.interactions,POPUP) >= 0) {
            this.showPopup();
        }

        if (isActive) { // interactinos when the the object is selected

            showPopup();

            foreach (short interaction in this.interactions) { 
                switch (interaction) {
                    case RIGID_BODY:
                        makeRigidBody();
                        break;
                    case ROTATE:
                        rotate();
                        break;
                    case MOVE:
                        moveElement();
                        break;
                    case POPUP:
                        this.showPopup();
                        break;
                    default:
                        //createPopUp("No interaction defined");
                        break;
                }
            }
        }
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
        if (this.name == "World"){
            // nothing (yet)
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
            var distance = Vector3.Distance(this.originalPosition, Camera.main.transform.position);
            this.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;

        } else { // moving the world
            var distance = Vector3.Distance(this.originalPosition, Camera.main.transform.position);

            this.transform.position = Camera.main.transform.position + Camera.main.transform.forward * distance;

            // let's compute FPS
            this.deltaTime += (Time.deltaTime - this.deltaTime) * 0.1f;
            this.fps = 1.0f / deltaTime;

            this.showPopup("[mv](world) to" + this.transform.position + "FPS: " + String.Format("{0:0.##}", this.fps)); // + "|"+ Camera.main.transform.forward.normalized + "|"+distance);
            }

    }

    //function to blink the text 
    /*public void blink() {
        //blink it forever. You can set a terminating condition depending upon your requirement. Here you can just set the isBlinking flag to false whenever you want the blinking to be stopped.
        Color originalColor = Color.yellow; // this.GetComponent<Renderer>().material.color;
        Color changedColor = Color.cyan;// originalColor;
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
    }
    */
    
    private void showPopup(string msg) {
        if (GameObject.Find("GestureManager").GetComponent<GazeGestureManager>().FocusedObject != null) {
            GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text = msg;
            GameObject.Find("Canvas/PopupPanel").GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        } else if(GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text!=""){
            GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text = "";
            GameObject.Find("Canvas/PopupPanel").GetComponent<Image>().color = new Color(0, 0, 0, 0);
        }
    }

}
