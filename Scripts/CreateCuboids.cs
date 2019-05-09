using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateCuboids : MonoBehaviour {
	public TextAsset data;
	List<GameObject> cubes;
	public float repeatRate = 0.5f;
	public int timeoutCounter = 10;

    //public InputField playground; // bsotomayor test (April, 15th)

    void Start () {
		cubes = new List<GameObject> ();
		InvokeRepeating ("initCreateCubes", 1f, repeatRate);

        /*var input = gameObject.GetComponent<InputField>();
        var se = new InputField.SubmitEvent();
        se.AddListener(SubmitName);
        input.onEndEdit = se;*/
    }

    private void SubmitName(string arg0){
        Debug.Log(arg0);
    }

	private void initCreateCubes(){
        /* Not used as of now (Thu Apr 25th)
		destroyAll ();
		cubes = new List<GameObject> ();
		Vector3 scale = new Vector3 (0.03f, 0.03f, 0.03f);
		Vector3 position = new Vector3 (0.2f, -0.2f, 0.2f);
		Vector3 shift = new Vector3 (0.0f, 0.0f,1.0f);
		createCubes (scale, position, shift);*/
	}


	void createCubes(Vector3 scaling, Vector3 positioning, Vector3 shifting){
		string[] lines = data.text.Split('\r');
		float maxX = 0f;
        float maxY = 0f;
        float maxZ = 0f;
        for (int i = 0; i < lines.Length; i++) {
			if (lines[i].Length > 0) {
                //split line
                string[] attributes = lines[i].Split('\t'); //,
                GameObject obj;

                switch (attributes[0]) {
                    case "RWCube":
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        break;
                    case "RWUVSphere":
                        obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        break;
                    case "RWCylinder":
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        break;
                    default:
                        // by default a cube is deployed
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        break;
                }

                // get position of object
                Vector3 scale = new Vector3(
                    (float.Parse(attributes[5], System.Globalization.CultureInfo.InvariantCulture)) * scaling.x,
                    (float.Parse(attributes[4], System.Globalization.CultureInfo.InvariantCulture)) * scaling.y,
                    (float.Parse(attributes[6], System.Globalization.CultureInfo.InvariantCulture)) * scaling.z);
                    obj.transform.localScale = Vector3.Scale(transform.localScale, scale);

                if (maxX < (float.Parse(attributes[5], System.Globalization.CultureInfo.InvariantCulture))) {
                    maxX = (float.Parse(attributes[5], System.Globalization.CultureInfo.InvariantCulture));
                    };
                if (maxY < (float.Parse(attributes[4], System.Globalization.CultureInfo.InvariantCulture))) {
                    maxY = (float.Parse(attributes[4], System.Globalization.CultureInfo.InvariantCulture));
                    };
                if (maxZ < (float.Parse(attributes[6], System.Globalization.CultureInfo.InvariantCulture))) {
                    maxZ = (float.Parse(attributes[6], System.Globalization.CultureInfo.InvariantCulture));
                    };

                // Set position
                Vector3 position = new Vector3(
                    (float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture)) * positioning.x + shifting.x,
                    (float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture)) * positioning.y + shifting.y, 
                    (float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture)) * positioning.z + shifting.z);
                obj.transform.position = position;
                // set properties:
                // color:
                Color color = new Color(
                    float.Parse(attributes[7], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(attributes[8], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(attributes[9], System.Globalization.CultureInfo.InvariantCulture));
                obj.GetComponent<Renderer>().material.color = color;
                obj.transform.parent = this.transform;
                // behaviour ?? (ask to Leonel!)
                obj.AddComponent<TapToSelectCube>();
                cubes.Add(obj);
            }
			
		};
		GameObject bases = GameObject.CreatePrimitive (PrimitiveType.Cube);
		bases.transform.localScale = Vector3.Scale (transform.localScale, new Vector3(maxX/2, maxY/2, 0.001f));
		bases.transform.position = new Vector3 (0f,0f, 1.15f);
		Color color2 = new Color (0f,0f,0f);

        GameObject inputBox = GameObject.CreatePrimitive(PrimitiveType.Plane);
        
        bases.GetComponent<Renderer>().material.color = color2;
		bases.transform.parent = this.transform.parent;
		bases.name = "";
		cubes.Add (bases);

        // test April 15th
        //playground.text = "Test text :)"; // <--- ACA VOY :)
        //playground.transform.Translate(1, 1, 1);
        
	
	}
	// Update is called once per frame
	void Update () {
		
	}

	void destroyAll(){
		//Debug.Log ("Before: "+cubes.Count);
		foreach (GameObject o in cubes) {
			Destroy(o);
		}
		//Debug.Log ("After: "+cubes.Count);

	}
}