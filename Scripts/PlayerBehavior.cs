using UnityEngine;
using System.Collections;

public class PlayerBehavior : MonoBehaviour {
	Ray ray = new Ray();
	GameObject selectedObject;
	GameObject selectedObjectPos;
	GameObject targetObject; //puzzle piece with inserted position

	public int numSolved = 0;
	public int vibrate_count;
	private GameObject target; 
	private Quaternion targetDir;
	private Vector3 targetPos;
	private Vector3 startPos;
	private Quaternion startDir;
	public bool selecting = false;
	public bool deselecting = false;
	public bool inserting = false;
	public bool vibrating = false;
	public bool startLerp = false;
	public bool lerping = false;
	private float startTime;
	private float journeyLength;
	public float countPuzzle = 0;
	public float speed = 3.0f;

	// Use this for initialization
	void start (){
	}

	// Update is called once per frame
	void Update () {
		/*
		GameObject c4 = GameObject.Find ("Chunk4_selectedPos");
		c4.transform.position = (Camera.main.transform.position + 5*Camera.main.transform.forward);
		c4.transform.rotation =  Camera.main.transform.rotation;
		*/
		GameObject pz = GameObject.Find("PuzzleSphereCentered");

		Debug.Log (pz.name);

		if (countPuzzle == 6) {
			Debug.Log("You win!");
			GameObject.Find ("You Win").GetComponent<MeshRenderer>().enabled = true;
		}

		if (Input.GetKey("left")) {
			pz.transform.Rotate(Vector3.up, 50 * Time.deltaTime, Space.World);
		}
		if (Input.GetKey("down")) {
			pz.transform.Rotate(Vector3.left, -50 * Time.deltaTime, Space.World);
		}
		if (Input.GetKey("up")) {
			pz.transform.Rotate(Vector3.left, 50 * Time.deltaTime, Space.World);
		}
		if (Input.GetKey("right")) {
			pz.transform.Rotate(Vector3.up, -50 * Time.deltaTime, Space.World);
		}


		if (selectedObject != null) {
			//selectedObjectPos.transform.parent = Camera.main.transform;
			//selectedObjectPos.transform.position = (Camera.main.transform.position + 5 * Camera.main.transform.forward);
			//selectedObject.transform.LookAt(Camera.main.transform.position, Vector3.up);
			//selectedObject.transform.parent = Camera.main.transform;
			//selectedObject.transform.rotation = Quaternion.LookRotation(selectedObjectPos.transform.right, selectedObjectPos.transform.up);\
			//selectedObjectPos.transform.rotation = Quaternion.Inverse(selectedObjectPos.transform.localRotation);
			//Quaternion.Inverse(selectedObjectPos.transform.rotation);
		}
		if (Input.GetKeyDown("space")) {
			if (selectedObject == null) {
				ray = new Ray (GameObject.Find("CenterEyeAnchor").transform.position, GameObject.Find("CenterEyeAnchor").transform.forward);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, 500)) {
					Debug.Log ("collision object" + hit.collider.gameObject.name);
					selectedObject = hit.collider.gameObject;
					targetObject = GameObject.Find (selectedObject.name + "_target");
					selectedObjectPos = GameObject.Find (selectedObject.name + "_selectedPos");
					selecting = true;
					Debug.Log (selectedObjectPos.name);
					//selectedObject.renderer.material.color = Color.green;
					startLerp = true;
				}
			}
			if (selecting && insertableToTarget ()) {
				Debug.Log ("Inserting!");
				inserting = true;
				selecting = false;
				startLerp = true;
			} else if (selecting && vibratable()) {
				vibrating = true;
				vibrate_count = -60;
			}
		}

		if (Input.GetKeyDown (KeyCode.LeftControl)) {
			if (selecting){
				Debug.Log ("deselect");
				deselecting = true;
				selecting = false;
				startLerp = true;
			}
		}

		if (vibrating && selecting) {
			Debug.Log("vibrating!");
			/*
			selectedObject.transform.position =  new Vector3(2*Mathf.Sin(vibrate_count)/20,
			                                             selectedObject.transform.position.y,
			                                             selectedObject.transform.position.z);*/
			selectedObject.transform.rotation =  Quaternion.Slerp(selectedObject.transform.rotation, 
			                                                      Quaternion.Euler(Input.GetAxis("Horizontal")*2*Mathf.Sin(vibrate_count)*100, 0, 0),Time.deltaTime*2.0f);

			Debug.Log(vibrate_count);
			vibrate_count++;
			if (vibrate_count > 60){
				vibrating = false;
			}
		}

		if (startLerp) {
			//Setup
			startTime = Time.time;
			startPos = selectedObject.transform.position;
			startDir = selectedObject.transform.rotation;
			if (selectedObject.name == "chunk_13"){
				Debug.Log ("target = " + targetPos + " ; start = " + startPos);
			}
			//journeyLength = Vector3.Distance(startPos, targetPos);
			Debug.Log( "journeyDistance " + journeyLength);
			if (inserting){
				target = targetObject;
			}else if (deselecting){
				Debug.Log ("deselecting!");
				selectedObject.transform.parent = null;
				selectedObjectPos.transform.DetachChildren();
				//selectedObjectPos.GetComponent<MeshRenderer>().enabled = false;
				//selectedObject.GetComponent<MeshRenderer>().enabled = true;
				target = GameObject.Find(selectedObject.name + "_origin");
			}else{
				target = selectedObjectPos;
			}
			targetDir = target.transform.rotation;
			targetPos = target.transform.position;
			////
			journeyLength = Vector3.Distance(startPos, targetPos);
			startLerp = false;
			lerping = true;
		}
		if (lerping) {
			Qslerp();
		}
}

	void Qslerp(){
		float distCovered = (Time.time - startTime) * speed;
		float fracJourney = distCovered * distCovered / journeyLength;
		//Debug.Log ("startpos= " + startPos);
		//Debug.Log ("targetpos= " + target.transform.position);
		//Debug.Log ("selected object qslerp " + selectedObject.name + " " + distCovered + " " + fracJourney);
		selectedObject.transform.position = Vector3.Lerp(startPos, target.transform.position, fracJourney);
		selectedObject.transform.rotation = Quaternion.Lerp(startDir, target.transform.rotation, fracJourney);
		if (selectedObject.transform.position == targetPos){
			//nearlyEqual(selectedObject.transform.position, targetPos
			lerping = false;
			if (selecting){
				Debug.Log("selectinggggg");
				selectedObject.transform.parent = selectedObjectPos.transform;
				//selectedObjectPos.GetComponent<MeshRenderer>().enabled = true;
				//selectedObject.GetComponent<MeshRenderer>().enabled = false;
			}

			if (inserting){
				inserting = false;
				Destroy (selectedObject.collider);
				//selectedObject.collider.enable
				selecting = false;
				selectedObjectPos.transform.DetachChildren();
				//selectedObjectPos.GetComponent<MeshRenderer>().enabled = false;
				//selectedObject.GetComponent<MeshRenderer>().enabled = true;
				targetObject.GetComponent<MeshRenderer>().enabled = true;
				selectedObject.GetComponent<MeshRenderer>().enabled = false;
				//selectedObjectPos.transform.position = GameObject.Find(selectedObject.name + "_slurpOrigin").transform.position;
				selectedObject = null;
				countPuzzle++;
			}
			if (deselecting){

				deselecting = false;
				selecting = false;
				selectedObject = null;

				//selectedObject = null;
				//selectedObjectPos.transform.parent = null;
				//Debug.Log("selected object pos" + selectedObject.name); 
				//selectedObjectPos.transform.position = new Vector3(0.0f, 1.0f, 0.0f);//GameObject.Find(selectedObject.name + "_slurpOrigin").transform.position;
				//selectedObject = null;
			}

		}
	}

	bool insertableToTarget(){
		Debug.Log ("Tryna Insert!");
		if (selecting) {
			float dist = Vector3.Distance (selectedObject.transform.position, targetObject.transform.position);
			Debug.Log ("Dist= " + dist);
			if (selectedObject.name == "chunk6"){
				return (dist <= 12);
			}
			if (selectedObject.name == "chunk8"){
				return (dist <= 12.2);
			}
			if (selectedObject.name == "chunk16"){
				return (dist <= 13.3);
			}
			if (selectedObject.name == "chunk14"){
				return (dist <= 11.25);
			}
			if (selectedObject.name == "chunk13"){
				return (dist <= 12.1);
			}
			if (selectedObject.name == "chunk2"){
				return (dist <= 11.42);
			}
			if (selectedObject.name == "chunk10"){
				return (dist <= 12);
			}
			
			return (dist <= 9);
		} else {
			return false;
		}
	}
	
	bool vibratable(){
		float dist = Vector3.Distance (selectedObject.transform.position, targetObject.transform.position);
		Debug.Log ("Dist= " + dist);
		return (dist > 2 && dist <= 3);
	}

	bool nearlyEqual(Vector3 a, Vector3 b){
		float dist = Vector3.Distance (a, b);
		if (dist <= 0.2){
			return true;
		}else{
			return false;
		}
	}
}

