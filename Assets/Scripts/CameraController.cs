using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public int normal;
	public int zoom;
	public float smooth;
	public float zoomGoal;
	private bool isZoomed=false;
	private GameEngine scene;
	private Vector3 main,playerFocus, focus;
	private int currentCamLocation; 

	Texture2D leftArrow, rightArrow;

	void Start(){
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine;
		main = camera.transform.position;
		focus = main;
		zoomGoal = normal;
		currentCamLocation = 2; //main
	}


	void OnGUI(){
		DisplayCameraButtons();
	}

	// Update is called once per frame
	void Update () {

		/* The following provides camera cycle button(s) movement
		 */ 
		if(camera.transform.position!=focus || camera.fieldOfView!=zoomGoal){
			camera.transform.position = Vector3.Lerp(camera.transform.position,focus,Time.deltaTime*smooth);
			camera.fieldOfView = Mathf.Lerp (camera.fieldOfView,zoomGoal,Time.deltaTime*smooth);
		}
		/*
		 */ 


		/* // The following will zoom in on selected characters
		 * 
		if(scene.CurrentTurnState==(int)TurnState.States.CharSelected){
			if(!isZoomed){ //initialize player-zoomed camera location
				Vector2 playerTileLocation = scene.ReturnSelectedPlayer().TileLocation;
				Vector3 playerLocation = new Vector3(playerTileLocation.x*Tile.spacing,0,playerTileLocation.y*Tile.spacing);
				playerFocus = playerLocation + (Vector3.up*250);
				isZoomed = true;
			}else{ //zoom in on player
				camera.transform.position = Vector3.Lerp(camera.transform.position,playerFocus,Time.deltaTime*smooth);
				camera.fieldOfView = Mathf.Lerp (camera.fieldOfView,zoom,Time.deltaTime*smooth);
			}
		}else{ //normal view
			isZoomed=false;
			camera.transform.position = Vector3.Lerp(camera.transform.position,main,Time.deltaTime*smooth);
			camera.fieldOfView = Mathf.Lerp (camera.fieldOfView,normal,Time.deltaTime*smooth);
		}
		*
		*/
	}

	public void DisplayCameraButtons(){
		//GUI.Label(new Rect(0,20,100,20),"Camera: "+currentCamLocation);
		if(GUI.Button(new Rect(0,0,20,20),"L")){
			currentCamLocation--;
			if(currentCamLocation<0){ 
				currentCamLocation = 2;
				focus = main;
				zoomGoal = normal;
			}else{
				Vector2 focus2d = scene.ReturnSelectedPlayerPosition(currentCamLocation);
				focus = new Vector3(focus2d.x*Tile.spacing,main.y,focus2d.y*Tile.spacing);
				zoomGoal = zoom;
			}
		}
		if(GUI.Button(new Rect(25,0,20,20),"R")){
			currentCamLocation++;
			if(currentCamLocation>2){
				currentCamLocation = 0;
			}
			if(currentCamLocation<2){
				Vector2 focus2d = scene.ReturnSelectedPlayerPosition(currentCamLocation);
				focus = new Vector3(focus2d.x*Tile.spacing,main.y,focus2d.y*Tile.spacing);
				zoomGoal = zoom;
			}else{
				focus=main;
				zoomGoal = normal;
			}
			
		}
	}
}
