using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

public class GazeGestureManager : MonoBehaviour
{
    public static GazeGestureManager Instance { get; private set; }
    
    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }
    private GameObject SelectedObject;

    GestureRecognizer recognizer;
    GestureRecognizer recognizerTestTap;
    GestureRecognizer recognizerTestHold;
    public RaycastHit hitInfo;

    public Vector3 handPosition_start = new Vector3(0, 0, 0);
    public Vector3 handPosition_current  = new Vector3(0, 0, 0);

    GameObject go_start; 
    GameObject go_end;

    LineRenderer line;

    public short selected_object = -1; // -1: null, 0: world, 1: woden or roassal2 object

    // Start is called before the first frame update
    void Awake() {
        go_start = new GameObject();
        go_end   = new GameObject();

        line = go_start.AddComponent<LineRenderer>();

        line.startColor = Color.green;
        line.endColor = Color.blue;

        line.startWidth = 0;
        line.endWidth = 0;

        line.enabled = false;

        Debug.Log("GazeGestureManager Awake!");
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();

        recognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.Hold);//| GestureSettings.NavigationX | GestureSettings.NavigationY);

        recognizer.Tapped += (args) =>
        {
            Debug.Log("before: this.selected_object = " + this.selected_object);

            if (this.selected_object == -1) { // if previously was nothing selected
                if(FocusedObject != null) { // an object was selected
                    SelectedObject = hitInfo.collider.gameObject;
                    if (true){//Input.GetKey(KeyCode.LeftControl)){ // then, we want to select a complete view
                        Debug.LogWarning("Seleccionado el padre '" + SelectedObject.transform.parent.gameObject.name + "'") ;
                        SelectedObject = SelectedObject.transform.parent.gameObject; // World/view selection
                        this.selected_object = 0; //?

                        // let's create a transparent plane in order to keep gazing this object and move the whole view
                        if (GameObject.Find("World/" + SelectedObject.name + "/temp") == null)
                        {
                            Debug.Log("null!!"); createTempMovingPlane();
                        }
                        else
                        {
                            Debug.Log("Temp exists already;)");
                        }

                        Debug.Log("GazeGesturerManager -> Focus: World ("+SelectedObject.name+")");
                    } else if(SelectedObject.tag == "Roassal2Obj"){ // otherwise, we want to select a single object
                        this.selected_object = 1;
                        Debug.Log("GazeGesturerManager -> Focus: WodenObj (" + SelectedObject.name + ")");

                    }
                    SelectedObject.SendMessageUpwards("OnAirTapped", SendMessageOptions.DontRequireReceiver);
                }

            } else if (this.selected_object == 0) { // if previously was selected the world
                // assign position to the selected object
                if (SelectedObject == null) { Debug.LogWarning("SelectedObj = 'null'"); }
                else { Debug.LogWarning("SelectedObj = " + SelectedObject.name); }
                SelectedObject.SendMessageUpwards("setInactiveWorld", SendMessageOptions.DontRequireReceiver);
                Debug.Log("Setting inactive: "+SelectedObject.name+"(obj.name)");
                this.selected_object = -1;

            } else if (this.selected_object == 1) { // if previously was selected a woden object
                // assign position to the selected object
                SelectedObject.SendMessageUpwards("setInactive", SendMessageOptions.DontRequireReceiver);
                Debug.Log("Setting inactive: " + SelectedObject.name + "(obj.name)");
                this.selected_object = -1;
            }
            Debug.Log("after : this.selected_object = " + this.selected_object);
            Debug.Log("--");
        };

        recognizer.NavigationStarted += (args) =>  {
            this.handPosition_start = this.handPosition_current;
            Debug.Log("NavigationStarted: " + this.handPosition_start);
            line.enabled = true;
        };
        recognizer.NavigationUpdated += (args) => {
            InteractionManager.InteractionSourceUpdatedLegacy += GetPosition;
            float [] rotation_velocity = { 0.5f, 1.0f, 0.5f };// 0.2f;

            Vector3 dif = (this.handPosition_current - this.handPosition_start);

            GameObject.Find("Canvas/PopupPanel/Popup").GetComponent<Text>().text = "[MODE: Rotating]";

            Debug.Log("NavigationUpdated: "+this.handPosition_current+"-"+this.handPosition_start+"="+ dif);

            line.startColor = Color.green;
            line.endColor = Color.blue;
            line.GetComponent<Renderer>().material.color = new Color(255,255,255,0.2f);

            line.startWidth = 0.0025f;
            line.endWidth = 0.02f;

            // Temp
            Vector3 shift = Camera.main.transform.position + Camera.main.transform.forward ;
            line.SetPosition(0, shift + (new Vector3(0,0.05f,0.0f)+this.handPosition_start));
            line.SetPosition(1, shift + (new Vector3(0,0.05f,0.0f)+this.handPosition_current));

            GameObject.Find("World").gameObject.transform.RotateAroundLocal(new Vector3(0, 1, 0), -dif.x * rotation_velocity[0]);
            //GameObject.Find("World").gameObject.transform.RotateAroundLocal(new Vector3(1, 0, 0), -dif.z * rotation_velocity[1]);
            //GameObject.Find("World").gameObject.transform.RotateAroundLocal(new Vector3(0, 0, 1), dif.y * rotation_velocity[2]);
        };
        recognizer.NavigationCompleted += (args) =>  {
            line.enabled = false;
            Debug.Log("NavigationCompleted");
        };
        recognizer.NavigationCanceled += (args) => {
            line.enabled = false;
            Debug.Log("NavigationCanceled");
        };

    }

    private void GetPosition(InteractionSourceState state)  {
        Vector3 pos = new Vector3(0,0,0);
        if (state.properties.sourcePose.TryGetPosition(out pos)) {
            this.handPosition_current = pos;
        }

    }

    // Update is called once per frame
    void Update() {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;
        
        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo)) {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
            //Debug.Log("FocusedObject.name: " + FocusedObject.name + " position " + FocusedObject.transform.position);
        }
        else {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
            //GameObject.Find("World").GetComponent<InteractiveGameObject>().popup_msg = "";
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject) {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
        }
    }

    // Gesture recognizer (https://abhijitjana.net/2016/05/29/understanding-the-gesture-and-adding-air-tap-gesture-into-your-unity-3d-holographic-app/)
    private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay) {
        if (FocusedObject != null) {
            Debug.Log("TAP!");
            FocusedObject.SendMessage("OnAirTapped", SendMessageOptions.RequireReceiver);
        }
    }

    private void createTempMovingPlane() {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Plane);
        temp.name = "temp";
        temp.tag = "Roassal2Obj";
        Vector3 pos = new Vector3(
            SelectedObject.transform.position.x,
            SelectedObject.transform.position.y,
            (SelectedObject.transform.position.z - 0.003f)
        );
        //temp.transform.localRotation = new Quaternion(-(float)Math.PI/4, 0, 0, 0);
        temp.transform.RotateAroundLocal(new Vector3(1, 0, 0), -(float)Math.PI / 2.0f);
        temp.transform.position = pos; // SelectedObject.transform.position; //Vector3.zero;
        temp.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        var color = Color.white;
        color.a = 0f;
        temp.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
        temp.GetComponent<Renderer>().material.color = color;
        temp.transform.SetParent(SelectedObject.transform);
        }
    
}
