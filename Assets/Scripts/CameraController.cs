using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {

	public enum CameraPositions{charOne,charTwo,main};

	public int normal;
	public int zoom;
	public float smooth;
	public float zoomGoal;
	public Vector3 main;
	private GameEngine scene;
	private Vector3 playerFocus, focus;
	private int currentCamera;
	private List<int> noCameraAccessTurnState, noCameraAccessGameState;

	Texture2D leftArrow, rightArrow;

	void Start(){
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine;
		InitializeCamera();
		InitializeCameraAccess();

	}


	void OnGUI(){
		//DisplayCameraButtons();
		DisplayCameraMainButton();
	}

	// Update is called once per frame
	void Update () {

		/* The following provides camera cycle button(s) movement
		 */ 
		if(camera.transform.position!=focus || camera.fieldOfView!=zoomGoal){
			camera.transform.position = Vector3.Lerp(camera.transform.position,focus,Time.deltaTime*smooth);
			camera.fieldOfView = Mathf.Lerp (camera.fieldOfView,zoomGoal,Time.deltaTime*smooth);
		}

		//check for player movement and adjust the camera position
		if(currentCamera!=(int)CameraPositions.main){ 
			Vector2 playersCurrentTileLocation = scene.ReturnSelectedPlayerPosition(currentCamera);
			Vector3 checkPosition = new Vector3(playersCurrentTileLocation.x*Tile.spacing,main.y,playersCurrentTileLocation.y*Tile.spacing);
			if(checkPosition!=focus){
				//Debug.Log ("checkPosition: "+checkPosition);
				//Debug.Log ("focus: "+focus);
				focus = checkPosition;
			}
		}

		if(scene.CurrentTurnState == (int)TurnState.States.End){
			currentCamera = (int)CameraPositions.main;
			focus = main;
			zoomGoal = normal;
		}


		/*
		 */ 


		/* // The following will zoom in on selected characters
		 */ 
		if(scene.CurrentTurnState==(int)TurnState.States.CharSelected){
			Vector2 playerTileLocation = scene.ReturnSelectedPlayer().TileLocation;
			Vector3 playerCameraLocation = new Vector3(playerTileLocation.x*Tile.spacing,main.y,playerTileLocation.y*Tile.spacing);
			if(focus!=playerCameraLocation){
				currentCamera = scene.ReturnSelectedPlayerIndex();
				focus = playerCameraLocation;
				zoomGoal = zoom;
			}
		}
		/*
		*/
	}

	public void InitializeCamera(){
		main = camera.transform.position;
		focus = main;
		zoomGoal = normal;
		currentCamera = (int)CameraPositions.main; 
	}

	public void InitializeCameraAccess(){
		noCameraAccessGameState = new List<int>();
		noCameraAccessTurnState = new List<int>();

		noCameraAccessGameState.Add ((int)GameState.States.GameOver);
		noCameraAccessGameState.Add ((int)GameState.States.Menu);
		noCameraAccessGameState.Add ((int)GameState.States.MatchCreated);

		noCameraAccessTurnState.Add((int)TurnState.States.CharSelected); //1-button camera mode
		noCameraAccessTurnState.Add ((int)TurnState.States.End); 
	}

	public void DisplayCameraMainButton(){
		string cameraString = GetCurrentCameraPositionString();
		GUI.Label(new Rect(0,20,100,20),"Camera:");
		GUI.Label(new Rect(0,40,100,20),cameraString);
		if(!noCameraAccessGameState.Contains((int)scene.CurrentGameState) && !noCameraAccessTurnState.Contains((int)scene.CurrentTurnState)){
			if(GUI.Button(new Rect(5,0,60,20),"Main")){
				currentCamera = (int)CameraPositions.main;
				focus = main;
				zoomGoal = normal;
			}
		}else{
			GUI.Label(new Rect(5,0,50,20),"----");
		}
	}

	public void DisplayCameraButtons(){
		string cameraString = GetCurrentCameraPositionString();
		GUI.Label(new Rect(0,20,100,20),"Camera:");
		GUI.Label(new Rect(0,40,100,20),cameraString);
		if(GUI.Button(new Rect(0,0,20,20),"L") && !noCameraAccessGameState.Contains((int)scene.CurrentGameState) && !noCameraAccessTurnState.Contains((int)scene.CurrentTurnState)){
			currentCamera--;
			if(currentCamera<0){ 
				currentCamera = (int)CameraPositions.main;
				focus = main;
				zoomGoal = normal;
			}else{
				Vector2 focus2d = scene.ReturnSelectedPlayerPosition(currentCamera);
				focus = new Vector3(focus2d.x*Tile.spacing,main.y,focus2d.y*Tile.spacing);
				zoomGoal = zoom;
			}
		}
		if(GUI.Button(new Rect(25,0,20,20),"R") && !noCameraAccessGameState.Contains((int)scene.CurrentGameState) && !noCameraAccessTurnState.Contains((int)scene.CurrentTurnState)){
			currentCamera++;
			if(currentCamera>2){
				currentCamera = (int)CameraPositions.charOne;
			}
			if(currentCamera<2){
				Vector2 focus2d = scene.ReturnSelectedPlayerPosition(currentCamera);
				focus = new Vector3(focus2d.x*Tile.spacing,main.y,focus2d.y*Tile.spacing);
				zoomGoal = zoom;
			}else{
				focus=main;
				zoomGoal = normal;
			}
			
		}
	}

	string GetCurrentCameraPositionString ()
	{
		switch(currentCamera){
		case (int)CameraPositions.charOne:
			return "Teammate 1";
		case (int)CameraPositions.charTwo:
			return "Teammate 2";
		case (int)CameraPositions.main:
			return "Overview";
		default: 
			return "Error";
		}
	}
}
