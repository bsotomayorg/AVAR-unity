using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.IO;
using System;

public class ListStringStringComparer : IComparer<KeyValuePair<string, string>>
{
    public int Compare(KeyValuePair<string, string> a, KeyValuePair<string, string> b)
    {
        return b.Value.Length - a.Value.Length;
    }
}

/// <summary>
/// Implements the Data space handler of a categorical data space object
/// Splits objects into individual cubes (and minimizes the count of unity game objects while doing so)
/// </summary>
[System.Serializable]
public class DataSpaceHandler: MonoBehaviour
{
		
    public GameObject dataObject;

    //TODO replace with something more sensible
    public Text countingTextList;

    //todo performance
    public List<Vector3> dataPositions;
    public List<Vector3> dataPositionsC1;
    public List<Vector3> dataPositionsC2;
    public List<string> dataClasses;
    public List<string> dataSrc;
    public List<Vector3> dataMetrics;
    private string fileName;
    private Vector3 lastPos;

    [SerializeField]
    public TextAsset data;

    //FIXMEE performance
    //the solid material (selection)
    [SerializeField]
    public Material dataMappedMaterial;
    //the transparent material (rest) (and potential ordered wrong)
    [SerializeField]
    public Material dataMappedTransparent;


    private float minX = 0.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMinX
    {
        get
        {
            return minX;
        }

        set
        {
            minX = value;
            dataMappedMaterial.SetFloat("_SelectionMinX", minX);
        }
    }


    private float maxX = 1.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMaxX
    {
        get
        {
            return maxX;
        }

        set
        {
            maxX = value;
            dataMappedMaterial.SetFloat("_SelectionMaxX", maxX);
        }
    }


    private float minY = 0.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMinY
    {
        get
        {
            return minY;
        }

        set
        {
            minY = value;
            dataMappedMaterial.SetFloat("_SelectionMinY", minY);
        }
    }


    private float maxY = 1.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMaxY
    {
        get
        {
            return maxY;
        }

        set
        {
            maxY = value;
            dataMappedMaterial.SetFloat("_SelectionMaxY", maxY);
        }
    }


    private float minZ = 0.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMinZ
    {
        get
        {
            return minZ;
        }

        set
        {
            minZ = value;
            dataMappedMaterial.SetFloat("_SelectionMinZ", minZ);
        }
    }


    private float maxZ = 1.0f;
    //selection booleans
    [SerializeField]
    public float SelectionMaxZ
    {
        get
        {
            return maxZ;
        }

        set
        {
            maxZ = value;
            dataMappedMaterial.SetFloat("_SelectionMaxZ", maxZ);
        }
    }

    // Use this for initialization
    void Start()
    {
		Debug.Log ("DataSpaceHandler.Start()");
        //TODO rework
        //if (GameObject.FindGameObjectWithTag("SelectionText") != null)
       //// {
         //   countingTextList = GameObject.FindGameObjectWithTag("SelectionText").GetComponent<Text>();
       //// }
        //prepare data
        string[] lines = data.text.Split('\r');

        //copy material and set selection parameter
        dataMappedMaterial = new Material(dataMappedMaterial);
        dataMappedMaterial.SetFloat("_SelectionSphereRadiusSquared", 25);
        dataMappedMaterial.SetVector("_SelectionSphereCenter", new Vector3(0.5f, 0.5f, 0.5f));

        dataMappedTransparent.SetFloat("_SelectionSphereRadiusSquared", 25);
        dataMappedTransparent.SetVector("_SelectionSphereCenter", new Vector3(0.5f, 0.5f, 0.5f));

        int count = 0;

        List<GameObject> childCat1 = new List<GameObject>();

        //Debug.Log("Starting Creating datapoints");
        //initialize points
        for (int i = 0; i < lines.Length; i++)
        {
            //split line
            string[] attributes = lines[i].Split(',');

            //prepare data point game objects
			GameObject dataPoint = Instantiate(dataObject);
			//dataPoint.GetComponent<Renderer>().material = new Material(Shader.Find("Diffuse"));
            dataPoint.transform.parent = gameObject.transform;
            dataPoint.transform.localScale = Vector3.Scale(transform.localScale, new Vector3(
                (float.Parse(attributes[7], System.Globalization.CultureInfo.InvariantCulture)) * 0.1f,
                (float.Parse(attributes[9], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f,//0.5f
                (float.Parse(attributes[8], System.Globalization.CultureInfo.InvariantCulture)) * 0.1f));
            Vector3 dataPosition = new Vector3(//0.0f, 0.0f, 0.0f);
                (float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f,
                ((float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture)) * 0.5f ),//0.8f
                (float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f);
			//Vector3 dataPosition = new Vector3(//0.0f, 0.0f, 0.0f);
			//	(float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f -0.7f,
			//	((float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture)) * 0.5f ),//0.8f
			//	(float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f -10.0f);
			
            //this worksfor azureus (city-normaliyed-volume-ext)
            //dataPoint.transform.localScale = Vector3.Scale(transform.localScale, new Vector3(
            //    (float.Parse(attributes[7], System.Globalization.CultureInfo.InvariantCulture)) * 0.03f,
            //    (float.Parse(attributes[9], System.Globalization.CultureInfo.InvariantCulture)) * 0.1f,
            //    (float.Parse(attributes[8], System.Globalization.CultureInfo.InvariantCulture)) * 0.03f));
            //Vector3 dataPosition = new Vector3(//0.0f, 0.0f, 0.0f);
            //    (float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f,
            //    (float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture)) * 0.2f,
            //    (float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f);

            //this worksfor Tomcat (city-normaliyed-volume
            //dataPoint.transform.localScale = Vector3.Scale(transform.localScale, new Vector3(
            //    (float.Parse(attributes[7], System.Globalization.CultureInfo.InvariantCulture)) * 0.1f,
            //    (float.Parse(attributes[9], System.Globalization.CultureInfo.InvariantCulture)) * 0.5f,
            //    (float.Parse(attributes[8], System.Globalization.CultureInfo.InvariantCulture)) * 0.1f));
            //Vector3 dataPosition = new Vector3(//0.0f, 0.0f, 0.0f);
            //    (float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f,
            //    (float.Parse(attributes[3], System.Globalization.CultureInfo.InvariantCulture)) * 0.8f,
            //    (float.Parse(attributes[2], System.Globalization.CultureInfo.InvariantCulture)) * 1.0f);

            Vector3 metrics = new Vector3(
                (float.Parse(attributes[1], System.Globalization.CultureInfo.InvariantCulture)),
                (float.Parse(attributes[4], System.Globalization.CultureInfo.InvariantCulture)),
                (float.Parse(attributes[7], System.Globalization.CultureInfo.InvariantCulture)));

            dataPoint.transform.localPosition = dataPosition;

            //add the data position
            dataPositions.Add(dataPosition);
            dataClasses.Add(attributes[0]);
            dataSrc.Add(attributes[10]);
            dataMetrics.Add(metrics);

            //set vertex color
            Mesh mesh = dataPoint.GetComponent<MeshFilter>().mesh;
            dataPositionsC1.Add(mesh.bounds.min);
            dataPositionsC2.Add(mesh.bounds.max);
            Vector3[] vertices = mesh.vertices;
            //Vector3[] newVertices = mesh.vertices;
            // Vector3[] normals = mesh.normals;
            Color[] colors = new Color[vertices.Length];
            for (int t = 0; t < vertices.Length; t++)
            {
                colors[t] = new Color(float.Parse(attributes[4], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(attributes[5], System.Globalization.CultureInfo.InvariantCulture),
                    float.Parse(attributes[6], System.Globalization.CultureInfo.InvariantCulture));
                // newVertices[t] += normals[i] * Mathf.Sin(Time.time);
                //colors[t] = new Color(0.2f, 0.6f, 0.4f);
            }
            mesh.colors = colors;
            //mesh.vertices = newVertices;
            childCat1.Add(dataPoint);

            //count++;
           // lastPos = GameObject.FindGameObjectWithTag("MainCamera").transform.position;
           // fileName = DateTime.Now.Year + "" + DateTime.Now.Month + "" + DateTime.Now.Day + "" + DateTime.Now.Hour + "" + DateTime.Now.Minute + "" + DateTime.Now.Second;


        }
		// myBezier = new Bezier( new Vector3( -5f, 0f, 0f ), UnityEngine.Random.insideUnitSphere * 2f, UnityEngine.Random.insideUnitSphere * 2f, new Vector3( 5f, 0f, 0f ) );

        //Debug.Log("Starting Creating Cubes");
        createTiledCube(childCat1);


        //combine children

        //Debug.Log(count);
    }
		


    // Update is called once per frame
    void Update()
    {
		//Vector3 vec = myBezier.GetPointAtTime( t );
		//transform.position = vec;

		//t += 0.001f;
		//if( t > 1f )
		//	t = 0f;
		
        /*Vector3 newPos = GameObject.FindGameObjectWithTag("MainCamera").transform.position;
        Vector3 diff = newPos - lastPos;
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
        lastPos = newPos;*/
    }

    //Create a cube with all objects of that type and subdivide if needed
    private GameObject createTiledCube(List<GameObject> objects)
    {
		Debug.Log ("DataSpaceHandler.createTiledCube()");
        GameObject ret = null;

        //calculate max objects per cube (unity vertices limit)
        int vertexCount = dataObject.GetComponent<MeshFilter>().sharedMesh.vertexCount * objects.Count;
        //Debug.Log("Total Vertices:" + vertexCount);

        //Unity limitation. we need to split the object list
        if (vertexCount > System.UInt16.MaxValue)
        {
            //
            //Debug.Log("Tiling cubes. Needed subcubes:"+ System.Math.Ceiling((double)vertexCount/ System.UInt16.MaxValue));
            GameObject tiledCube = new GameObject("tiledCube");
            tiledCube.transform.parent = gameObject.transform;
            tiledCube.transform.localPosition = new Vector3(0, 0, 0);
            tiledCube.transform.localScale = new Vector3(1, 1, 1);

            int objectsPerRun = (int)System.Math.Floor(System.UInt16.MaxValue / (double)dataObject.GetComponent<MeshFilter>().sharedMesh.vertexCount);

            //iterate objects and create tiled cubes
            int index = 0;
            while (index < objects.Count)
            {
                createCubeObject(objects.GetRange(index, index + objectsPerRun >= objects.Count ? objects.Count - index : objectsPerRun), tiledCube);
                index += objectsPerRun;
            }
            ret = tiledCube;
        }
        else
        {
            ret = createCubeObject(objects, gameObject);
        }

        //destroy child objects
        foreach (GameObject o in objects)
        {
            Destroy(o);
        }

        return ret;
    }

    //create a single colored cube object out of the children
    private GameObject createCubeObject(List<GameObject> objects, GameObject parent)
    {
		Debug.Log ("DataSpaceHandler.createCubeObject()");
        //"realobject"
        GameObject cube = new GameObject("Cube");
        cube.transform.parent = parent.transform;
		/*Rigidbody rBody = cube.AddComponent<Rigidbody> ();
		rBody.isKinematic = true;
		rBody.useGravity = false;
		BoxCollider bCollider = cube.AddComponent<BoxCollider> ();
		bCollider.isTrigger = true;*/

		BoxCollider bCollider = cube.AddComponent<BoxCollider> ();

        MeshFilter filter = cube.AddComponent<MeshFilter>();
        MeshRenderer renderer = cube.AddComponent<MeshRenderer>();


        renderer.material = dataMappedMaterial;

        mergeChildren(cube, objects, filter);

        cube.transform.parent = parent.transform;
        cube.transform.localPosition = new Vector3(0, 0, 0);
        cube.transform.localScale = new Vector3(1, 1, 1);
        // cube.SetActive(true);

        //"transparent object" since unity has some draw problems
        GameObject transCube = new GameObject("TransCubeCube");
        transCube.transform.parent = parent.transform;
		/*Rigidbody rBodyTrans = transCube.AddComponent<Rigidbody> ();
		rBodyTrans.isKinematic = true;
		rBodyTrans.useGravity = false;
		BoxCollider bColliderTrans = transCube.AddComponent<BoxCollider> ();
		bColliderTrans.isTrigger = true;*/

        MeshFilter filterTrans = transCube.AddComponent<MeshFilter>();
        filterTrans.sharedMesh = filter.mesh;

        renderer = transCube.AddComponent<MeshRenderer>();
        renderer.material = dataMappedTransparent;

        transCube.transform.parent = parent.transform;
        transCube.transform.localPosition = new Vector3(0, 0, 0);
        transCube.transform.localScale = new Vector3(1, 1, 1);
        transCube.SetActive(true);

        dataMappedTransparent.SetFloat("_InverseSelection", -1.0f);
        dataMappedTransparent.SetFloat("_TargetAlpha", 1.0f);
        //dataMappedTransparent.SetFloat("_InverseSelection", -1.0f);
        //dataMappedTransparent.SetFloat("_TargetAlpha", 0.2f);
        return cube;
    }

	void onCollisionEnter(Collision col){
		Debug.Log ("Collision--" + col.gameObject.transform.position);
	}

    private void mergeChildren(GameObject parent, List<GameObject> objects, MeshFilter target)
    {
		Debug.Log ("DataSpaceHandler.mergeChildren()");
        CombineInstance[] combine = new CombineInstance[objects.Count];
        //        System.Random rnd = new System.Random();
        for (int i = 0; i < objects.Count; i++)
        {
            //make sure the points are aligned with the scatterplot
            Vector3 localPos = objects[i].transform.localPosition;
            objects[i].transform.parent = parent.transform;
            objects[i].transform.localPosition = localPos;
            combine[i].mesh = objects[i].GetComponent<MeshFilter>().sharedMesh;
            combine[i].transform = objects[i].transform.localToWorldMatrix;
            objects[i].SetActive(false);
        }

        target.mesh.CombineMeshes(combine);
    }

    /// <summary>
    /// Set the selection by specifying a bounding box. TODO change to plane / sphere coliders
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="minZ"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <param name="maxZ"></param>
    public void setSelection(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
		Debug.Log ("DataSpaceHandler.setSelection()");
        SelectionMinX = minX;
        SelectionMaxX = maxX;

        SelectionMinY = minY;
        SelectionMaxY = maxY;

        SelectionMinZ = minZ;
        SelectionMaxZ = maxZ;
    }


    /// <summary>
    /// Set the selection by specifying a bounding box. TODO change to plane / sphere coliders
    /// </summary>
    /// <param name="minX"></param>
    /// <param name="minY"></param>
    /// <param name="minZ"></param>
    /// <param name="maxX"></param>
    /// <param name="maxY"></param>
    /// <param name="maxZ"></param>
    public void setSelectionSphere(Vector3 center, float radius)
    {
		Debug.Log ("DataSpaceHandler.setSelectionSphere()");
        dataMappedTransparent.SetFloat("_SelectionSphereRadiusSquared", radius * radius);
        dataMappedTransparent.SetVector("_SelectionSphereCenter", center);

        //GameObject.FindGameObjectWithTag("SelectionText").transform.localScale = Vector3.Scale(transform.localScale, new Vector3(0.2f, 0.2f, 0.2f));

        //return;

        //update selected statistics TODO all that follows is not performant
        int count = 0;
        float squaredRad = radius * radius;

        Dictionary<string, string> selectedClasses = new Dictionary<string, string>();
        //        Dictionary<string, Vector3> selectedClasses = new Dictionary<string, Vector3>();

        float minDistance = 1000000000;
        string closestClassSource = "";
        for (int i = 0; i < dataPositions.Count; i++)
        {
            Vector3 pos = dataPositions[i];
            Vector3 diff = pos - center;
            float squaredDistance = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
            if (squaredDistance < squaredRad)
            //if (doesCubeIntersectSphere(dataPositionsC1[i], dataPositionsC2[i], center, radius))
            {
                if (squaredDistance <= minDistance) {
                    minDistance = squaredDistance;
                    closestClassSource = dataSrc[i];
                }


                count++;
                string dataClass = dataClasses[i];
                //insert class count
                if (!selectedClasses.ContainsKey(dataClass))
                {
                    selectedClasses[dataClass] = dataSrc[i];//dataMetrics[i];//1;
                }
                //else
                //{
                //    int oldCount = selectedClasses[dataClass];
                //    selectedClasses[dataClass] = ++oldCount;
                //}
            }
        }

        //set text
        try
        {
            if (countingTextList != null)
            {
                countingTextList.text = "";//"Total: " + count + "\n\n";

                //workarounds because unity uses an ancient version of c#
                List<KeyValuePair<string, string>> sortedClasses = new List<KeyValuePair<string, string>>(selectedClasses);
                //            List<KeyValuePair<string, Vector3>> sortedClasses = new List<KeyValuePair<string, Vector3>>(selectedClasses);
                sortedClasses.Sort(new ListStringStringComparer());
                KeyValuePair<string, string> entry = sortedClasses.ToArray()[0];
                //foreach (KeyValuePair<string, string> entry in sortedClasses)
                //            foreach (KeyValuePair<string, Vector3> entry in sortedClasses)
                //{
                    var fileStream = new FileStream(@"c:\moose\" + closestClassSource/*entry.Value*/, FileMode.Open, FileAccess.Read);
                    using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        //int i = 0;
                        string line;
                        bool isComment = false;
                        while ((line = streamReader.ReadLine()) != null /*&& i < 100 */&& countingTextList.text.Length < 10000)
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
                                countingTextList.text += line + "\n";
                            }
                            if (line.IndexOf("*/") > -1)
                            {
                                isComment = false;
                            }
                        }

                    }



                    //                countingTextList.text += entry.Value + " \n";
                    //                countingTextList.text += entry.Key + ": [NOM=" + truncate(entry.Value.x, 1) + ", NOA=" + truncate(entry.Value.y, 1) + ", NLOC=" + truncate(entry.Value.z, 1) + "] \n";
                //}
            }
        }
        catch (System.Exception)
        {

            ;
        }

    }
    float squared(float v) { return v * v; }
    bool doesCubeIntersectSphere(Vector3 C1, Vector3 C2, Vector3 S, float R)
    {
		Debug.Log ("DataSpaceHandler.doesCubeIntersectSphere()");
        float dist_squared = R * R;
        /* assume C1 and C2 are element-wise sorted, if not, do that now */
        if (S.x < C1.x) dist_squared -= squared(S.x - C1.x);
        else if (S.x > C2.x) dist_squared -= squared(S.x - C2.x);
        if (S.y < C1.y) dist_squared -= squared(S.y - C1.y);
        else if (S.y > C2.y) dist_squared -= squared(S.y - C2.y);
        if (S.z < C1.z) dist_squared -= squared(S.z - C1.z);
        else if (S.z > C2.z) dist_squared -= squared(S.z - C2.z);
        return dist_squared > 0;
    }
    public static float truncate(float value, int digits)
    {
		Debug.Log ("DataSpaceHandler.truncate()");
        double mult = System.Math.Pow(10.0, digits);
        double result = System.Math.Truncate(mult * value) / mult;
        return (float)result;
    }
}
