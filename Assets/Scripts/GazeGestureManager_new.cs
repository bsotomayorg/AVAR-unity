using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;

public class GazeGestureManager_new : MonoBehaviour
{
    public static GazeGestureManager Instance { get; private set; }
    
    // Represents the hologram that is currently being gazed at.
    public GameObject FocusedObject { get; private set; }

    GestureRecognizer recognizer;

    // Start is called before the first frame update
    void Awake()
    {
        Debug.Log("GazeGestureManager Awake!");
        // Set up a GestureRecognizer to detect Select gestures.
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap | GestureSettings.Hold );

        recognizer.Tapped += (args) =>
        {
            // Send an OnSelect message to the focused object and its ancestors.
            if (FocusedObject != null) {
                if (GameObject.Find("World").GetComponent<InteractiveGameObject>().isActive)
                { // if world is active, next tap is for diselect "world"
                    Debug.Log("GazeGestureManager => Sending msg(world): diselect");
                    FocusedObject = GameObject.Find("World").gameObject;
                    FocusedObject.SendMessageUpwards("OnAirTapped", SendMessageOptions.DontRequireReceiver);
                }
                else
                { // if world is not selected, then it is possible to select a interactive game object
                    Debug.Log("GazeGestureManager => Sending message");
                    FocusedObject.SendMessageUpwards("OnAirTapped", SendMessageOptions.DontRequireReceiver);
                }
            } else {
                FocusedObject = GameObject.Find("World").gameObject;
                Debug.Log("GazeGestureManager => Sending msg(world): select");
                FocusedObject.SendMessageUpwards("OnAirTapped", SendMessageOptions.DontRequireReceiver);
            }
        };

        /*recognizer.NavigationStartedEvent += NavigationRecognizer_NavigationStartedEvent;
        recognizer.NavigationUpdatedEvent += NavigationRecognizer_NavigationUpdatedEvent;
        recognizer.NavigationCompletedEvent += NavigationRecognizer_NavigationCompletedEvent;
        recognizer.NavigationCanceledEvent += NavigationRecognizer_NavigationCanceledEvent;*/

    }

    /*private void NavigationRecognizer_NavigationCanceledEvent(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay)
    {
        GameObject.Find("Canvas/Popup").GetComponent<Text>().text = "Canceled";
    }

    private void NavigationRecognizer_NavigationCompletedEvent(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay)
    {
        GameObject.Find("Canvas/Popup").GetComponent<Text>().text = "Completed";
    }

    private void NavigationRecognizer_NavigationUpdatedEvent(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay)
    {
        GameObject.Find("Canvas/Popup").GetComponent<Text>().text = "Updated";
    }

    private void NavigationRecognizer_NavigationStartedEvent(InteractionSourceKind source, Vector3 normalizedOffset, Ray headRay)
    {
        GameObject.Find("Canvas/Popup").GetComponent<Text>().text = "Started";
    }*/

    // Update is called once per frame
    void Update()
    {
        // Figure out which hologram is focused this frame.
        GameObject oldFocusObject = FocusedObject;

        // Do a raycast into the world based on the user's
        // head position and orientation.
        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;
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
            //Debug.Log("FocusedObject.name: NULL");
        }

        // If the focused object changed this frame,
        // start detecting fresh gestures again.
        if (FocusedObject != oldFocusObject) {
            recognizer.CancelGestures();
            recognizer.StartCapturingGestures();
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

    // Navigation https://forums.hololens.com/discussion/2507/add-navigation-gesture
    private void PerformRotation() {
        /*if (GestureManager.Instance.IsNavigating)
        {
            // Calculate rotationFactor based on GestureManager's NavigationPosition.X and multiply by RotationSensitivity.
            // This will help control the amount of rotation.
            rotationFactor = GestureManager.Instance.NavigationPosition.x * RotationSensitivity;

            // transform.Rotate along the Y axis using rotationFactor.
            transform.Rotate(new Vector3(0, -1 * rotationFactor, 0));

        }*/
    }



}
