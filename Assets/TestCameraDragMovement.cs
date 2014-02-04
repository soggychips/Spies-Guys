using UnityEngine;
using System.Collections;

public class TestCameraDragMovement : MonoBehaviour {
	private Vector3 mousePositionBeforeDrag;
	public float dragSpeed;
	

	// Update is called once per frame
	void Update () {
		//allow unhinging of camera
		if(Input.GetMouseButtonDown(0)){
			mousePositionBeforeDrag = Input.mousePosition;
			Debug.Log ("Mouse button 0 pressed down");
			//unhinged=true;
			return;
		}
		
		if(!Input.GetMouseButton(0))
			return;
		
		Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mousePositionBeforeDrag);
		Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);
		Debug.Log ("Moving by" + move);
		transform.Translate(move, Space.World);  
	}
}
