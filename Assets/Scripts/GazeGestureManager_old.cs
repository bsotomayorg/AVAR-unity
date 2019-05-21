using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.UI;
//using HoloToolkit.Unity.InputModule;
using System.IO;
using System.Text;
using System;


public class GazeGestureManager_old : MonoBehaviour
{
	public static GazeGestureManager_old Instance { get; private set; }

	// Represents the hologram that is currently being gazed at.
	public GameObject FocusedObject { get; private set; }

    // test Tuesday May 14  bsotomayor
    public int tap_count = 0;
    //public GameObject go_popup;
    public Text text_popup;
    // end test

	GestureRecognizer recognizer;
	Vector3 lastPosition;
	string fileName;

	// Use this for initialization
	void Awake()
	{
		Instance = this;
		lastPosition = Camera.main.transform.position;
		fileName = "recordedPositions_"+DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second+".txt";

		// Set up a GestureRecognizer to detect Select gestures.
		recognizer = new GestureRecognizer();
		recognizer.Tapped += (args) =>
		{
			// Send an OnSelect message to the focused object and its ancestors.
			if (FocusedObject != null)
			{
				FocusedObject.SendMessageUpwards("OnSelect", SendMessageOptions.DontRequireReceiver);
			}
		};
		recognizer.StartCapturingGestures();

        //go_popup = new GameObject("Popup!");
        //go_popup.transform.SetParent(this.transform);
        //Text textPopup = go_popup.AddComponent<Text>();
        //textPopup.text = "Tadah!!!! (count="+this.tap_count+")";
        text_popup.text = "Tadah!!!! (count=" + this.tap_count + ")";
        //go_popup.transform.position = new Vector3(1.0f, 0.0f, 1.0f);
    }

	// Update is called once per frame
	void Update()
	{
		// Figure out which hologram is focused this frame.
		GameObject oldFocusObject = FocusedObject;
		//Invoke ("recordPosition", 1.0f);
		// Do a raycast into the world based on the user's
		// head position and orientation.
		var headPosition = Camera.main.transform.position;
		var gazeDirection = Camera.main.transform.forward;

		RaycastHit hitInfo;
		Text countingTextList = GameObject.FindGameObjectWithTag("TextPanel").GetComponent<Text>();
		if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
		{
			// If the raycast hit a hologram, use that as the focused object.
			FocusedObject = hitInfo.collider.gameObject;
			countingTextList.text = FocusedObject.name;
            //bsotomayor edit:
            //countingTextList.text = GameObject.Find("World/"+((string) FocusedObject.name));

            //			countingTextList.text = getName(FocusedObject.name);

            countingTextList.transform.position = new Vector3 (FocusedObject.transform.position.x+0.1f, FocusedObject.transform.position.y, FocusedObject.transform.position.z-0.1f); //doubleHeight(FocusedObject.transform.position, FocusedObject.transform.localScale);
			//countingTextList.transform.position = new Vector3(countingTextList.transform.position.x, countingTextList.transform.position.y - (Camera.main.transform.forward.y*1.5f), countingTextList.transform.position.z);
			//countingTextList.transform.rotation = Quaternion.Euler(0,0,0);
			//Canvas canvas = GameObject.FindGameObjectWithTag("TextCanvas").GetComponent<Canvas>();
			//countingTextList.transform.position = inBetween(FocusedObject.transform.position, Camera.main.transform.position);


			//System.Random rnd = new System.Random ();
			//canvas.planeDistance = rnd.Next (1, 100) / 100f;// transform.position = FocusedObject.transform.position
		}
		//else
		//{
			// If the raycast did not hit a hologram, clear the focused object.
			////FocusedObject = null;
			//countingTextList.transform.position = new Vector3(countingTextList.transform.position.x, countingTextList.transform.position.y - (Camera.main.transform.forward.y*0.01f), countingTextList.transform.position.z);
		//}
		countingTextList.transform.rotation = Quaternion.Euler(0,0,0);

		// If the focused object changed this frame,
		// start detecting fresh gestures again.
		if (FocusedObject != oldFocusObject)
		{
			recognizer.CancelGestures();
			recognizer.StartCapturingGestures();
		}

        // TEST 

        text_popup.text = "Tadah!!!! (count=" + this.tap_count + ")";
    }

	Vector3 doubleHeight(Vector3 pos, Vector3 scale){
		return(new Vector3(pos.x, pos.y + (scale.y)/1.7f, pos.z));
	}

	Vector3 inBetween(Vector3 obj, Vector3 me){
		float factor = 0.5f;
		return(new Vector3 ((obj.x - me.x) * factor, (obj.y - me.y) * factor, (obj.z - me.z) * factor));
	}

    /*private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        if (focusedObject != null)
        {
            focusedObject.SendMessage("OnAirTapped", SendMessageOptions.RequireReceiver);
        }
    }*/

    string getName(string aFullName){
		if (validFullName (aFullName)) {
			string[] words = aFullName.Split ('/');
			string last = words [words.Length - 1];
			string[] tokens = last.Split ('.');
			return tokens [0];
		} else {
			return "";
		}
	}

	string getSource(string aFullName){
		string ret = "";
		try{
			var fileStream = new FileStream(@"c:\moose\" + aFullName, FileMode.Open, FileAccess.Read);
			using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
			{
				//int i = 0;
				string line;
				bool isComment = false;
				while ((line = streamReader.ReadLine()) != null /*&& i < 100 */&& ret.Length < 10000)
				{
					// i++;
					bool skip = false;
					if (line.IndexOf("/*") > -1)
					{
						isComment = true;
					}
					if (line.IndexOf("import") > -1 || line.IndexOf("//") > -1 || line.IndexOf("package") > -1 || line.Length == 0)
					{
						skip = true;
					}
					if (!isComment && !skip)
					{
						ret += line + "\n";
					}
					if (line.IndexOf("*/") > -1)
					{
						isComment = false;
					}
				}

			}
		}
		catch(Exception e){
			ret = "ERROR: " +e.Message;//+ @"c:\moose\" + aFullName;
		}
		return ret;
	}

	bool validFullName(string aName){
		return (aName.IndexOf ('/') >= 0 && aName.IndexOf ('.') > 0);
	}



	void recordPosition (){
		Vector3 newPos = Camera.main.transform.position;
        Vector3 diff = newPos - lastPosition;
        float speed = (diff.x * diff.x + diff.y * diff.y + diff.z * diff.z);
        string line = newPos.x + "," + newPos.y + "," + newPos.z + "," + speed;
        if (File.Exists(@fileName)) {
            StreamWriter file = new StreamWriter(@fileName,true);
            file.WriteLine(line);
            file.Flush();
            file.Close();
        }
        else {
            File.WriteAllText(@fileName, line+"\n");
        }
        lastPosition = newPos;
	}

    void OnAirTapped() {
        this.tap_count++;
    }
}