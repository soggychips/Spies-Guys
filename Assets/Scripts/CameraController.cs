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
	public bool unhinged; //allows free movement of camera
	private GameEngine scene;
	public Vector3 focus;
	private int currentCamera;
	private List<int> noCameraAccessTurnState, noCameraAccessGameState;
	private List<Vector2> playerPositions;

	Texture2D leftArrow, rightArrow;

	public GUIStyle game_camBtn_main_iPhone;
	private Vector2 camButtonMainLocation;
	public float animationSpeed = 8;

	public float dragSpeed = 2;
	private Vector3 mousePositionBeforeDrag;


	void Start(){
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine;
		InitializeCamera();
		InitializeCameraAccess();
		camButtonMainLocation = new Vector2(-128, 200);
		playerPositions = new List<Vector2>();
		unhinged=false;
	}


	void OnGUI(){
		DisplayCameraStatus();
		DisplayCameraMainButton();

	}

	// Update is called once per frame
	void Update () {
		//Get positions of players on team and Lerp halfway to center camera between players, set that as main
		if(scene.CurrentGameState==(int)GameState.States.P1 || scene.CurrentGameState==(int)GameState.States.P2){
			if(scene.ReturnNumberOfLiveCharactersOnCurrentTeam()>1){
				playerPositions = scene.ReturnTeamLocations();
				Vector2 inBetweenPosition = Vector2.Lerp ((Vector2)playerPositions[0],(Vector2)playerPositions[1],.5f);
				main = new Vector3(inBetweenPosition.x*Tile.spacing,main.y,inBetweenPosition.y*Tile.spacing);
			}else if(scene.ReturnNumberOfLiveCharactersOnCurrentTeam()==1){
				playerPositions = scene.ReturnTeamLocations();
				main = new Vector3(playerPositions[0].x*Tile.spacing,main.y,playerPositions[0].y*Tile.spacing);
			}

			if(!unhinged && currentCamera==(int)CameraPositions.main){
				focus = main;
				//zoomGoal = normal;
			}


			//checks to see if players are off screen and adjusts FoV accordingly
			if(focus==main && !unhinged /*&& camera.fieldOfView==zoomGoal*/){
				//Debug.Log ("Ok, what?");
				int multiplier = 0;
				playerPositions = scene.ReturnTeamLocations();
				foreach(Vector2 posit in playerPositions){
					Vector3 viewportPoint = camera.WorldToViewportPoint(new Vector3(posit.x*Tile.spacing,0.0f,posit.y*Tile.spacing));
					//Debug.Log ("viewportPoint = "+viewportPoint);
					if(!(viewportPoint.z>0 && (viewportPoint.x<1 && viewportPoint.x>0) && (viewportPoint.y<1 && viewportPoint.y>0))){ //not on screen
						multiplier++;
					}
				}
				zoomGoal += .5f*(multiplier);
			}
		}

		//move the camera's position to focus and set the camera's FoV to zoomGoal
		if((camera.transform.position!=focus || camera.fieldOfView!=zoomGoal) && !unhinged){
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

		//move the camera to the main position when ending the turn
		if(scene.CurrentTurnState == (int)TurnState.States.End){
			currentCamera = (int)CameraPositions.main;
			focus = main;
			//if(zoomGoal<normal)
			zoomGoal = normal;
		}

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

		/*
		//allow unhinging of camera
		if(Input.GetMouseButtonDown(0)){
			mousePositionBeforeDrag = Input.mousePosition;
			Debug.Log ("Mouse button 0 pressed down");
			unhinged=true;
			return;
		}
		
		if(!Input.GetMouseButton(0))
			return;
		
		Vector3 pos = Camera.main.ScreenToViewportPoint(mousePositionBeforeDrag - Input.mousePosition);
		Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.y * dragSpeed);
		if(move == Vector3.zero) unhinged = false;
		//Debug.Log ("Moving by" + move);
		transform.Translate(move, Space.World);
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
			unhinged=false;
		}
		
		if(!noCameraAccessGameState.Contains((int)scene.CurrentGameState) && !noCameraAccessTurnState.Contains((int)scene.CurrentTurnState) && (zoomGoal==zoom || unhinged) ){
			if(camButtonMainLocation.x < 0){
				camButtonMainLocation.x += Time.deltaTime * animationSpeed;
				if(camButtonMainLocation.x > 0){
					camButtonMainLocation.x = 0;
				}
			}
		}else{
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
			unhinged = false;
		}
	}

	public void SecondPlayerButtonPress(){
		if(scene.CurrentTurnState==(int)TurnState.States.Neutral || scene.CurrentTurnState==(int)TurnState.States.CharSelected){
			scene.DeselectCharacter();
			currentCamera = (int)CameraPositions.charTwo;
			Vector2 secondPlayerLocation = scene.ReturnSelectedPlayerPosition(currentCamera);
			scene.SelectCharacter((int)secondPlayerLocation.x,(int)secondPlayerLocation.y);
			zoomGoal = zoom;
			unhinged = false;
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
