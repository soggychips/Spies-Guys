using UnityEngine;
using System.Collections;

public class TouchHandler : MonoBehaviour {
	
	public bool debug;
	//public Camera camera;
	
	private GameEngine scene;
	private bool mainMenu;
	private bool gameMenu;
	
	// Use this for initialization
	void Start () {
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine; //gives us access to the GameEngine script
		
	}
	
	void Update(){
		if(scene.CurrentGameState==(int)GameState.States.P1 ||scene.CurrentGameState==(int)GameState.States.P2){
			if(scene.CurrentTurnState==(int)TurnState.States.Neutral){
				Vector2 mouseClick = MouseClickToTileCoords();
				if(scene.CurrentPlayerAt((int)mouseClick.x,(int)mouseClick.y)){
					scene.SelectCharacter((int)mouseClick.x,(int)mouseClick.y);	
				}
			}else if(scene.CurrentTurnState==(int)TurnState.States.CharSelected){
				Vector2 mouseClick = MouseClickToTileCoords();
				if(scene.OpenTileAt((int)mouseClick.x,(int)mouseClick.y) && scene.VisibleTileAt((int)mouseClick.x,(int)mouseClick.y)){
					scene.MoveSelectedCharTo((int)mouseClick.x,(int)mouseClick.y);
				}
			}
		}
	}

	Vector2 MouseClickToTileCoords ()
	{
		if(Input.GetMouseButtonDown(0)){
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit,1000)){
				float x = hit.point.x;
				float z = hit.point.z;
				if(Mathf.Floor(x)%Tile.spacing<=5)
					x = Mathf.Floor(x) - Mathf.Floor(x)%Tile.spacing;
				else
					x = Mathf.Floor(x) + (Tile.spacing-Mathf.Floor(x)%Tile.spacing);
				if(Mathf.Floor(z)%Tile.spacing<=5)
					z = Mathf.Floor(z)-Mathf.Floor(z)%Tile.spacing;
				else
					z = Mathf.Floor(z) + (Tile.spacing-Mathf.Floor(z)%Tile.spacing);
				x=x/Tile.spacing; z=z/Tile.spacing;
				Debug.Log("Clicked on ("+x+","+z+")");
				return new Vector2(x,z);
			}
		}
		//no click 
		return new Vector2(-1000,-1000);
	}
	
	void OnGUI() {
		DebugButton();
		if(debug){
			DisplayStates();
		}
		if(scene.CurrentGameState==(int)GameState.States.Menu){
			//Debug.Log("Menu GUI");
			LoadMainMenu();
		}else if(scene.CurrentGameState==(int)GameState.States.MatchStart){
			//Debug.Log("Match Start");
			ReadyMenu();
		}else if(scene.CurrentGameState==(int)GameState.States.P1){ //P1
			//Debug.Log("P1 turn");
			if(scene.CurrentTurnState==(int)TurnState.States.Neutral){
				DisplayPlayerButtons();
			}
		}else if(scene.CurrentGameState==(int)GameState.States.P2){
			//Debug.Log("P2 turn");
			if(scene.CurrentTurnState==(int)TurnState.States.Neutral){
				DisplayPlayerButtons();
			}
		}else{
			Debug.Log("TouchHandler's currentGameState: "+scene.CurrentGameState);	
		}
		
		
	}

	void DebugButton ()
	{
		debug=true;
	}
	
	public void DisplayStates(){
		string gameStateString="??";
		string turnStateString="??";
		switch(scene.CurrentGameState){
		case (int)GameState.States.Menu:
			gameStateString = "Menu";
			break;
		case (int)GameState.States.MatchStart:
			gameStateString = "Match Start";
			break;
		case (int)GameState.States.P1:
			gameStateString = "P1";
			break;
		case (int)GameState.States.P2:
			gameStateString = "P2";
			break;
		case (int)GameState.States.GameOver:
			gameStateString = "Game Over";
			break;
		}
		switch(scene.CurrentTurnState){
		case (int)TurnState.States.Neutral:
			turnStateString = "Neutral";
			break;
		case (int)TurnState.States.CharSelected:
			turnStateString = "CharSelected";
			break;
		case (int)TurnState.States.MoveBegin:
			turnStateString = "MoveBegin";
			break;
		case (int)TurnState.States.MoveAnimate:
			turnStateString = "MoveAnimate";
			break;
		case (int)TurnState.States.ActionBegin:
			turnStateString = "ActionBegin";
			break;
		case (int)TurnState.States.ActionAnimate:
			turnStateString = "ActionAnimate";
			break;
		case (int)TurnState.States.Confirmation:
			turnStateString = "Confirmation";
			break;
		}
		GUI.Label(new Rect(Screen.width-200,0,150,30),"Game State: "+gameStateString);
		GUI.Label(new Rect(Screen.width-200,30,150,30),"Turn State: "+turnStateString);
	}

	public void DisplayPlayerButtons ()
	{
		GUI.Box(new Rect(Screen.width-100,Screen.height-120,100,120), "Actions");
	
		// Undo
		if(GUI.Button(new Rect(Screen.width-90,Screen.height-60,80,20), "Undo")) { 
			
		}
		// Submit
		if(GUI.Button(new Rect(Screen.width-90,Screen.height-30,80,20), "Submit")) { 
			if(scene.CurrentGameState==(int)GameState.States.P1) scene.GiveControlToPlayer2();
			else if(scene.CurrentGameState==(int)GameState.States.P2) scene.GiveControlToPlayer1();
			
			
		}
	}
	
	public void LoadMainMenu(){
		GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Spies Guys: Murders and Lies");
		if(GUI.Button(new Rect(Screen.width/2-50,Screen.height/2-25,100,50), "Start")) { 
			scene.StartGame();
			scene.SGDataInit();
		}
	}
	
	public void ReadyMenu(){
		GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,500,500), "Match about to begin...");
		if(GUI.Button(new Rect(Screen.width/2-100,Screen.height/2-50,100,50), "Begin")) { 
			scene.GiveControlToPlayer1();
		}	
	}
}
