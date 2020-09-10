using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class InteractiveGameObject : NavigationReactor
{
    public bool isActive = false;

    // blink variables
    Color originalColor;
    short count;
    public string popup_msg = "";
    Vector3 originalPosition;
    Vector3 cameraOriginalPosition;
    System.DateTime timeStart;
    System.DateTime timeEnd;
    void Awake()
    {
       
    }

    // values which indicates which is the interaction with the object once the user taps and gazes an object.
    public const short RIGID_BODY = 0;
    public const short ROTATE = 1;
    public const short MOVE = 2;
    public const short POPUP = 3;

    public short[] interactions;
    private GameObject m_world;
    private Vector3 relativePos;
    private float focusDistance;


    public void f_Init(bool isWorld, short[] interact, string msg)
    {
        originalPosition = this.transform.position;
        cameraOriginalPosition = Camera.main.transform.position;
        if (isWorld)
        {
            // let's add a Canvas for visualize Labels
            //var c = gameObject.AddComponent<Canvas>();
            //c.renderMode = RenderMode.WorldSpace;
            //c.pixelPerfect = true;
            //// for rendering, let's add a Canvas Scaler which will avoid blurred text
            //gameObject.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 25;
            interactions = interact;
        }
        else
        {
            interactions = interact;
            popup_msg = msg;
        }
    }
    void Update()
    {
        // interaction when the object is just gazed
        //if (Array.IndexOf(this.interactions, POPUP) >= 0)
        //{
        //    this.showPopup();
        //}

        if (isActive)
        { // interactinos when the the object is selected     
            foreach (short interaction in this.interactions)
            {
                switch (interaction)
                {
                    case RIGID_BODY:
                        makeRigidBody();
                        break;
                    case ROTATE:
                        //rotate();
                        break;
                    case MOVE:
                        moveElement();
                        break;
                    case POPUP:
                        //this.showPopup();
                        break;
                    default:
                        //createPopUp("No interaction defined");
                        break;
                }
            }
        }
    }
    public void OnAirTapped(Vector3 focusPos)
    {
        relativePos = transform.position - focusPos;
        focusDistance = Vector3.Distance(focusPos, Camera.main.transform.position);
        isActive = true;

        this.originalPosition = this.transform.position;


    }



    private void rotate()
    {
        if (this.name != "World")
        { // woden or roassal2 gameobjects
            this.transform.Rotate(0, 1, 0);
        }
    }
    private void makeRigidBody()
    {
        if (!this.GetComponent<Rigidbody>())
        {
            var rigidbody = this.gameObject.AddComponent<Rigidbody>();
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    public void setInactive()
    {
        isActive = false;
        Debug.Log("ObjCommand[" + this.name + "].SetInactive, isActive = " + this.isActive + ")");
        OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.MoveObject, "from " + originalPosition + " to " + transform.position);

    }
    private void setInactiveWorld()
    {

        isActive = false;
        Debug.Log("ObjCommand[" + this.name + "].SetInactiveWorld, isActive = " + this.isActive + ")");
        OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.MoveObject, "from " + originalPosition + " to " + transform.position);

    }

    private void moveElement()
    {
        transform.position = relativePos + Camera.main.transform.position + Camera.main.transform.forward * focusDistance;

    }

    
    public override void NavigationUpdate(Vector3 deltaPos, GameObject origin)
    {
        gameObject.transform.RotateAround(origin.transform.position,Vector3.up, -10 * deltaPos.x);
    }

    public override void NavigationStart(Vector3 startPos, GameObject origin)
    {
        timeStart = DateTime.Now;
    }
    public override void NavigationEnd(Vector3 endPos, GameObject origin)
    {
        timeEnd = DateTime.Now;
        OperationLog.Instance.AddToOperationLog(OperationLog.OperationEvent.RotateObject, "from " + timeStart.Hour.ToString("d2") + ":" + timeStart.Minute.ToString("d2") + ":" + timeStart.Second.ToString("d2")
             + " to " + timeEnd.Hour.ToString("d2") + ":" + timeEnd.Minute.ToString("d2") + ":" + timeEnd.Second.ToString("d2"));
    }
}
