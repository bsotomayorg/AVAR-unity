using System.Collections;
using UnityEngine;

public class WorldCursor : MonoBehaviour
{
	private MeshRenderer meshRenderer;
	private int highlightFrames = 15;

	public float minScale = 0.25f;

	GameObject hit;
	Color baseColor;

	// Use this for initialization
	void Start()
	{
		// Grab the mesh renderer that's on the same object as this script.
		meshRenderer = this.gameObject.GetComponentInChildren<MeshRenderer>();
		baseColor = meshRenderer.material.color;
	}

	// Update is called once per frame
	void Update()
	{
		if (Time.frameCount % 2 != 0) {
			return;
		}
		// Do a raycast into the world based on the user's
		// head position and orientation.
		var headPosition = Camera.main.transform.position;
		var gazeDirection = Camera.main.transform.forward;

		RaycastHit hitInfo;

		if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
		{
			GameObject oldHit = hit;
			hit = hitInfo.collider.gameObject;
			// If the raycast hit a hologram...
			// Display the cursor mesh.
			meshRenderer.enabled = true;

			// Move the cursor to the point where the raycast hit.
			this.transform.position = hitInfo.point;

			// Rotate the cursor to hug the surface of the hologram.
			this.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

			// Scale the pointer according to the raycast hit
			this.transform.localScale = calculateScaleFromHit(hitInfo.collider.gameObject);
		}
		else
		{
			// If the raycast did not hit a hologram, hide the cursor mesh.
			meshRenderer.enabled = false;
			hit = null;
		}
	}

	private Vector3 calculateScaleFromHit(GameObject go)
	{
		Vector3 cursorScale = transform.localScale;

		if (go.tag == "GraphElement")
		{
			cursorScale.x = 0.35f;
			cursorScale.z = 0.35f;
		}
		else if (go.tag == "CityElement")
		{
			float scale = scaleFromVolume(calculateVolume(go));
			cursorScale.x = scale;
			cursorScale.z = scale;
		}
		else
		{
			cursorScale.x = 0.65f;
			cursorScale.z = 0.65f;
		}
		return cursorScale;
	}

	private Vector3 calculateScaleFromCityBlock(GameObject go)
	{
		Vector3 objectSize = Vector3.Scale(
			go.GetComponent<Renderer>().bounds.size,
			go.transform.localScale);
		float width = objectSize.x;
		Debug.Log(width);
		return new Vector3(width, width / 2, width);
	}

	private float smallestExtent(Vector3 scale)
	{
		return Mathf.Max(minScale, Mathf.Min(scale.x, Mathf.Min(scale.y, scale.z)));
	}

	private float calculateVolume(GameObject go)
	{
		if(go.tag == "CityElement")
		{
			Vector3 extends = go.GetComponent<Renderer>().bounds.extents;
			return extends.x * extends.y * extends.z;
		}
		else
		{
			return 1f;
		}

	}

	private float scaleFromVolume(float volume)
	{
		return volume < 0.0000005f ? 0.35f : 0.65f;
	}

	public IEnumerator glow()
	{
		Material mat = meshRenderer.GetComponent<Renderer> ().material;
		mat.color = Color.white;
		float step = 1f / highlightFrames;
		for(int i = 1; i <= highlightFrames; i++)
		{
			mat.color = Color.Lerp(Color.white, baseColor, step * i);
			yield return null;
		}
	}

	public GameObject getHit(){
		return hit;
	}
}