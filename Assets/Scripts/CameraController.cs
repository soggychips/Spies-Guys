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

	public GUIStyle game_camBtn_main_iPhone;
	private Vector2 camButtonMainLocation;
	public float animationSpeed = 8;


	void Start(){
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine;
		InitializeCamera();
		InitializeCameraAccess();
		camButtonMainLocation = new Vector2(-128, 200);

	}


	void OnGUI(){
		DisplayCameraStatus();
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


		 // The following will zoom in on selected characters selected NOT via the player buttons, but by clicking a player character

		if(scene.CurrentTurnState==(int)TurnState.States.CharSelected){
			Vector2 playerTileLocation = scene.ReturnSelectedPlayer().TileLocation;
			Vector3 playerCameraLocation = new Vector3(playerTileLocation.x*Tile.spacing,main.y,playerTileLocation.y*Tile.spacing);
			if(focus!=playerCameraLocation){
				currentCamera = scene.ReturnSelectedPlayerIndex();
				focus = playerCameraLocation;
				zoomGoal = zoom;
			}
		}


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

		noCameraAccessTurnState.Add((int)TurnState.States.MoveConfirm);
		noCameraAccessTurnState.Add((int)TurnState.States.ActionConfirm);
		noCameraAccessTurnState.Add((int)TurnState.States.End); 
	}

	public void DisplayCameraMainButton(){

		if(GUI.Button(new Rect(camButtonMainLocation.x,camButtonMainLocation.y,128,88),"", game_camBtn_main_iPhone)){
			scene.DeselectCharacter();
			currentCamera = (int)CameraPositions.main;
			focus = main;
			zoomGoal = normal;
			
		}
		
		if(!noCameraAccessGameState.Contains((int)scene.CurrentGameState) && !noCameraAccessTurnState.Contains((int)scene.CurrentTurnState)&&focus!=main){

		if(camButtonMainLocation.x < 0){
			camButtonMainLocation.x += Time.deltaTime * animationSpeed;
			
			if(camButtonMainLocation.x > 0){
				camButtonMainLocation.x = 0;
			}
		}

			
		}
		else{
			if(camButtonMainLocation.x > - 128){
				camButtonMainLocation.x -= Time.deltaTime * animationSpeed;
			if(camButtonMainLocation.x < -128){
				camButtonMainLocation.x = -128;
			}
			}
		}
	}

	public void DisplayCameraStatus(){
		string cameraString = GetCurrentCameraPositionString();
		GUI.Label(new Rect(0,20,100,20),"Camera:");
		GUI.Label(new Rect(0,40,100,20),cameraString);
	}

	public void FirstPlayerButtonPress(){
		if(scene.CurrentTurnState==(int)TurnState.States.Neutral || scene.CurrentTurnState==(int)TurnState.States.CharSelected){
			scene.DeselectCharacter();
			currentCamera = (int)CameraPositions.charOne;
			Vector2 firstPlayerLocation = scene.ReturnSelectedPlayerPosition(currentCamera);
			scene.SelectCharacter((int)firstPlayerLocation.x,(int)firstPlayerLocation.y);
			zoomGoal = zoom;
		}
	}

	public void SecondPlayerButtonPress(){
		if(scene.CurrentTurnState==(int)TurnState.States.Neutral || scene.CurrentTurnState==(int)TurnState.States.CharSelected){
			scene.DeselectCharacter();
			currentCamera = (int)CameraPositions.charTwo;
			Vector2 secondPlayerLocation = scene.ReturnSelectedPlayerPosition(currentCamera);
			scene.SelectCharacter((int)secondPlayerLocation.x,(int)secondPlayerLocation.y);
			zoomGoal = zoom;
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
