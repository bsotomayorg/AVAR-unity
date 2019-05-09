using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System;
#if WINDOWS_UWP
using Windows.Storage;
using System.Threading.Tasks;
using System;
#endif

public class TapToSelectCube : MonoBehaviour {

	int showing =0;
	public Text countingTextList;

	// Called by GazeGestureManager when the user performs a Select gesture
	void OnSelect()
	{
		countingTextList = GameObject.FindGameObjectWithTag("TextPanel").GetComponent<Text>();
		countingTextList.text = getName(this.name);
		Invoke("clear",2f);
	}

	// Update is called once per frame
	void Update()
	{
	}

	string getName(string aFullName){
		if (validFullName (aFullName)) {
			string[] words = aFullName.Split ('/');
			string last = words [words.Length - 1];
			string[] tokens = last.Split ('.');
			return tokens [0];
		} else {
			return aFullName;
		}
	}

	bool validFullName(string aName){
		return (aName.IndexOf ('/') > 0 && aName.IndexOf ('.') > 0);
	}
	void clear(){
		countingTextList = GameObject.FindGameObjectWithTag("TextPanel").GetComponent<Text>();
					countingTextList.text = "";
	}
}
