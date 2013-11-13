using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchHandler : MonoBehaviour {
	
	public bool debug;
	//public Camera camera;
	
	private GameEngine scene;
	private bool mainMenu;
	private bool gameMenu;
	private Vector2 mouseClick;
	
	
	// Use this for initialization
	void Start () {
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine; //gives us access to the GameEngine script
		
	}
	
	void Update(){
		//if it's midgame
		if(scene.CurrentGameState==(int)GameState.States.P1 ||scene.CurrentGameState==(int)GameState.States.P2){
			switch(scene.CurrentTurnState){
			//if turnstate: Neutral
				case (int)TurnState.States.Neutral:
					mouseClick = MouseClickToTileCoords();
					if(scene.CurrentPlayerAt((int)mouseClick.x,(int)mouseClick.y)){
						//Debug.Log("Player clicked on");
						scene.SelectCharacter((int)mouseClick.x,(int)mouseClick.y);	
					}
					break;
				//if turnState: CharSelected
				case (int)TurnState.States.CharSelected:
					mouseClick = MouseClickToTileCoords();
					if(scene.OpenTileAt((int)mouseClick.x,(int)mouseClick.y) && scene.HighlightedTileAt((int)mouseClick.x,(int)mouseClick.y)){
						scene.PrepareMovement();
						scene.BeginMovement((int)mouseClick.x,(int)mouseClick.y);
						//scene.MoveSelectedCharTo((int)mouseClick.x,(int)mouseClick.y);
					}else if(scene.CurrentPlayerAt((int)mouseClick.x,(int)mouseClick.y)){
						scene.DeselectCharacter();
					}else if(scene.TileTakenByEnemy((int)mouseClick.x,(int)mouseClick.y)){
						Debug.Log("Enemy at "+mouseClick.x+","+mouseClick.y+" eliminated!");
						scene.EliminatePlayerAt((int)mouseClick.x,(int)mouseClick.y);
						scene.DeselectCharacter();
						scene.SetPlayerVisibilityUsingFoV();
					}
					break;
			} //end switch
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
				//Debug.Log("Clicked on ("+x+","+z+")");
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
				DisplayPlayerData();
			}
		}else if(scene.CurrentGameState==(int)GameState.States.P2){
			//Debug.Log("P2 turn");
			if(scene.CurrentTurnState==(int)TurnState.States.Neutral){
				DisplayPlayerButtons();
				DisplayPlayerData();
			}
		}else if(scene.CurrentGameState==(int)GameState.States.GameOver){
			DisplayEndGameMenu();
		}else{
			Debug.Log("TouchHandler's currentGameState: "+scene.CurrentGameState);	
		}
		
		if(scene.CurrentTurnState==(int)TurnState.States.Confirmation){
			ConfirmationButtons();	
		}
		
	}

	void DisplayEndGameMenu ()
	{
		int winner = scene.Winner;
		GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Winner:");
		GUI.Label(new Rect(Screen.width/2-50,Screen.height/2-25,100,50), "Player "+winner);
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
		GUI.Label(new Rect(Screen.width-200,60,150,30),"Turn: "+ scene.Turn);
	}
	
	public void DisplayPlayerData(){
		List<int> movesLeftForPlayers = scene.MovesLeftForCurrentPlayer();
		switch(scene.CurrentGameState){
		case (int)GameState.States.P1:
			GUI.Label(new Rect(Screen.width-500,0,150,30),"Spy[0]: "+movesLeftForPlayers[0]);
			GUI.Label(new Rect(Screen.width-500,30,150,30),"Spy[1]: "+movesLeftForPlayers[1]);
			break;
		case (int)GameState.States.P2:
			GUI.Label(new Rect(Screen.width-500,0,150,30),"Guy[0]: "+movesLeftForPlayers[0]);
			GUI.Label(new Rect(Screen.width-500,30,150,30),"Guy[1]: "+movesLeftForPlayers[1]);
			break;
		}
		
	}

	public void DisplayPlayerButtons ()
	{
		GUI.Box(new Rect(Screen.width-100,Screen.height-120,100,120), "Actions");
	
		// Submit
		if(GUI.Button(new Rect(Screen.width-90,Screen.height-95,80,80), "Submit")) { 
			if(scene.CurrentGameState==(int)GameState.States.P1) scene.GiveControlToPlayer2();
			else if(scene.CurrentGameState==(int)GameState.States.P2) scene.GiveControlToPlayer1();
			scene.CheckForWinner();
			
		}
	}

	void ConfirmationButtons ()
	{
		GUI.Box (new Rect(Screen.width-100,Screen.height/2 -120,100,240),"");
		if(GUI.Button(new Rect(Screen.width-95,Screen.height/2-115,90,110), "Confirm")) {
			scene.ConfirmMove();
		}
		if(GUI.Button (new Rect(Screen.width-95,Screen.height/2,90,110),"Cancel")) {
			scene.CancelMove();
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
