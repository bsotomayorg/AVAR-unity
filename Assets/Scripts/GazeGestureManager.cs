using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

public class GazeGestureManager : MonoBehaviour
{
    public static GazeGestureManager Instance { get; private set; }

    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }
    private GameObject SelectedObject;
    private GameObject originObject;
    GestureRecognizer recognizer;
    GestureRecognizer recognizerTestTap;
    GestureRecognizer recognizerTestHold;
    public RaycastHit hitInfo;

    public Vector3 handPosition_start = Vector3.zero;
    public Vector3 handPosition_current = Vector3.zero;

    GameObject go_start;
    GameObject go_end;

    LineRenderer line;

    NavigationReactor m_reactor;
    

    bool isInited = false;
    private InteractiveGameObject objectHolded;

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        go_start = new GameObject();
        go_end = new GameObject();

        line = go_start.AddComponent<LineRenderer>();

        line.startColor = Color.green;
        line.endColor = Color.blue;

        line.startWidth = 0;
        line.endWidth = 0;

        line.enabled = false;

        Debug.Log("GazeGestureManager Awake!");
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.NavigationX | GestureSettings.NavigationY);
        recognizer.Tapped += (args) =>
        {
            if (objectHolded != null)
            {
                objectHolded.setInactive();
                objectHolded = null;
            }
            else
            {
                if (FocusedObject != null)
                { // an object was selected
                    originObject = FocusedObject;
                    SelectedObject = originObject;
                    while (true)
                    {
                        if (SelectedObject.transform.parent != playground.Instance.world.transform)
                        {
                            SelectedObject = SelectedObject.transform.parent.gameObject;
                        }
                        else
                        {
                            break;
                        }
                    }
                    objectHolded = SelectedObject.GetComponent<InteractiveGameObject>();
                    if (objectHolded != null)
                    {
                        objectHolded.OnAirTapped(hitInfo.point);
                    }
                }
            }
        };
        recognizer.NavigationStarted += (args) =>
        {
            
            this.handPosition_start = this.handPosition_current;
            //Debug.Log("NavigationStarted: " + this.handPosition_start);
            if (playground.Instance.isCodeEditorOn)
            {
                m_reactor = playground.Instance.m_codeEditor.CurFocusedPanel;
            }
            else if (FocusedObject == null)
            {
                m_reactor = null;
                return;
            }
            else
            {
                originObject = FocusedObject;
                SelectedObject = FocusedObject;
                while (true)
                {
                    if (SelectedObject.transform.parent != playground.Instance.world.transform)
                    {
                        SelectedObject = SelectedObject.transform.parent.gameObject;
                    }
                    else
                    {
                        m_reactor = SelectedObject.GetComponent<NavigationReactor>();
                        break;
                    }
                }
                line.enabled = true;
            }
            m_reactor.NavigationStart(handPosition_start, originObject);
        };
        recognizer.NavigationUpdated += (args) =>
        {
            if (m_reactor == null)
                return;
            InteractionManager.InteractionSourceUpdatedLegacy += GetPosition;
            float[] rotation_velocity = { 0.5f, 1.0f, 0.5f };// 0.2f;

            Vector3 dif = (this.handPosition_current - this.handPosition_start);



            //Debug.Log("NavigationUpdated: " + this.handPosition_current + "-" + this.handPosition_start + "=" + dif);

            if (!playground.Instance.isCodeEditorOn)
            {
                playground.Instance.PopupPanel.m_text.text = "[MODE: Rotating]";
                line.startColor = Color.green;
                line.endColor = Color.blue;
                line.GetComponent<Renderer>().material.color = new Color(255, 255, 255, 0.2f);

                line.startWidth = 0.0025f;
                line.endWidth = 0.02f;

                // Temp
                Vector3 shift = Camera.main.transform.position + Camera.main.transform.forward;
                line.SetPosition(0, shift + (new Vector3(0, 0.05f, 0.0f) + this.handPosition_start));
                line.SetPosition(1, shift + (new Vector3(0, 0.05f, 0.0f) + this.handPosition_current));
             
            }
            m_reactor.NavigationUpdate(dif, originObject);
        };
        recognizer.NavigationCompleted += (args) =>
        {
            if (m_reactor == null)
                return;
            m_reactor.NavigationEnd(handPosition_current,originObject);
            //if (playground.Instance.isCodeEditorOn)
            //{
            //    playground.Instance.m_codeEditor.CurFocusedPanel.NavigationEnd(handPosition_current);
            //}
            m_reactor = null;
            line.enabled = false;
        };
        recognizer.NavigationCanceled += (args) =>
        {
            m_reactor = null;
            line.enabled = false;
            //Debug.Log("NavigationCanceled");
        };               
        recognizer.StartCapturingGestures();
        playground.Instance.f_Init();
        isInited = true;
    }

    private void GetPosition(InteractionSourceState state)
    {
        Vector3 pos = new Vector3(0, 0, 0);
        if (state.properties.sourcePose.TryGetPosition(out pos))
        {
            this.handPosition_current = pos;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!isInited)
            return;
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            // If the raycast hit a hologram, use that as the focused object.
            FocusedObject = hitInfo.collider.gameObject;
            //Debug.Log("FocusedObject.name: " + FocusedObject.name + " position " + FocusedObject.transform.position);
        }
        else
        {
            // If the raycast did not hit a hologram, clear the focused object.
            FocusedObject = null;
            //GameObject.Find("World").GetComponent<InteractiveGameObject>().popup_msg = "";
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject)
        {
            showPopup();
            //    recognizer.CancelGestures();
            //    recognizer.StartCapturingGestures();
        }


    }

    // Gesture recognizer (https://abhijitjana.net/2016/05/29/understanding-the-gesture-and-adding-air-tap-gesture-into-your-unity-3d-holographic-app/)
    private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        if (FocusedObject != null)
        {
            Debug.Log("TAP!");
            FocusedObject.SendMessage("OnAirTapped", SendMessageOptions.RequireReceiver);
        }
    }
    private void showPopup()
    {
        if (FocusedObject == null)
        {
            playground.Instance.PopupPanel.m_text.text = "";
            playground.Instance.PopupPanel.gameObject.SetActive(false);
        }
        else
        {
            if (FocusedObject.GetComponent<InteractiveGameObject>() == null)
            {
                playground.Instance.PopupPanel.m_text.text = "";
                playground.Instance.PopupPanel.gameObject.SetActive(false);
            }
            else
            {
                var msg = FocusedObject.GetComponent<InteractiveGameObject>().popup_msg;
                playground.Instance.PopupPanel.m_text.text = msg;
                playground.Instance.PopupPanel.gameObject.SetActive(msg != "");
            }
        }
    }
    private void createTempMovingPlane()
    {
        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Plane);
        temp.name = "temp";
        temp.tag = "Roassal2Obj";
        Vector3 pos = new Vector3(
            SelectedObject.transform.position.x,
            SelectedObject.transform.position.y,
            (SelectedObject.transform.position.z - 0.003f)
        );
        //temp.transform.localRotation = new Quaternion(-(float)Math.PI/4, 0, 0, 0);
        temp.transform.Rotate(new Vector3(1, 0, 0), -(float)Math.PI / 2.0f);
        temp.transform.position = pos; // SelectedObject.transform.position; //Vector3.zero;
        temp.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        temp.GetComponent<Renderer>().material = playground.Instance.planeMaterail;
        //var color = Color.white;
        //color.a = 0f;
        //temp.GetComponent<Renderer>().material.shader = Shader.Find("Transparent/Diffuse");
        //temp.GetComponent<Renderer>().material.color = color;
        temp.transform.SetParent(SelectedObject.transform);
    }

}
