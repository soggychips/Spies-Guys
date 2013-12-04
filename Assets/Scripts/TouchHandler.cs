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

	int page = 1;
	int gadget1=0, gadget2=0;
	int spyToAssignGear=1;
	int guyToAssignGear=1;

	
	
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
				if(scene.HighlightedTileAt((int)mouseClick.x,(int)mouseClick.y) && !scene.CurrentPlayerAt((int)mouseClick.x,(int)mouseClick.y)){
						if(scene.UnblockedTileAt(mouseClick)){
							scene.Movement((int)mouseClick.x,(int)mouseClick.y);
						}else if(scene.ClosedDoorAt(mouseClick)){
							//unlock door
							Debug.Log ("Unlock Door");
						}
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
		}else if(scene.CurrentGameState==(int)GameState.States.MatchCreated){
			//Debug.Log("Match Start");
			ReadyMenu();
		}else if(scene.CurrentGameState==(int)GameState.States.P1){ //P1
			//Debug.Log("P1 turn");
			switch(scene.CurrentTurnState){
			case (int)TurnState.States.Begin:
				scene.CheckForWinner();
				scene.RemoveVisibility();
				BeginTurnMenu();
				break;
			case (int)TurnState.States.Neutral:
				DisplaySubmitButton();
				DisplayPlayerData();
				break;
			case (int)TurnState.States.MoveConfirm:
				MovementConfirmation();
				break;
			case (int)TurnState.States.ActionConfirm:
				//ActionConfirmation();
				break;
			case (int)TurnState.States.End:
				CompleteTurnConfirmation();
				break;
			}
		}else if(scene.CurrentGameState==(int)GameState.States.P2){
			//Debug.Log("P2 turn");
			switch(scene.CurrentTurnState){
			case (int)TurnState.States.Begin:
				scene.CheckForWinner();
				scene.RemoveVisibility();
				BeginTurnMenu();
				break;
			case (int)TurnState.States.Neutral:
				DisplaySubmitButton();
				DisplayPlayerData();
				break;
			case (int)TurnState.States.MoveConfirm:
				MovementConfirmation();
				break;
			case (int)TurnState.States.End:
				CompleteTurnConfirmation();
				break;
			}
		}else if(scene.CurrentGameState==(int)GameState.States.GameOver){
			DisplayEndGameMenu();
		}else{
			Debug.Log("TouchHandler's currentGameState: "+scene.CurrentGameState);	
		}
		
		
	}

	void DisplayEndGameMenu ()
	{
		int winner = scene.Winner;
		GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Game Over");
		GUI.Label(new Rect(Screen.width/2-50,Screen.height/2-25,100,50), "Player "+winner+" wins!");
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
		case (int)GameState.States.MatchCreated:
			gameStateString = "Match Created";
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
		case (int)TurnState.States.MoveConfirm:
			turnStateString = "Confirmation";
			break;
		case (int)TurnState.States.Begin:
			turnStateString = "Begin";
			break;
		case (int)TurnState.States.End:
			turnStateString = "End";
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

	public void DisplaySubmitButton ()
	{
		List<int> movesLeftForPlayers = scene.MovesLeftForCurrentPlayer();
		if(movesLeftForPlayers[0]>0 && movesLeftForPlayers[1]>0)
			GUI.Box(new Rect(Screen.width-150,Screen.height-120,150,120), "Turn: Incomplete");
		else
			GUI.Box(new Rect(Screen.width-150,Screen.height-120,150,120), "Turn: Complete");
	
		// Submit
		if(GUI.Button(new Rect(Screen.width-140,Screen.height-95,130,80), "Submit")) { 
			scene.EndTurn();
		}
	}

	public void CompleteTurnConfirmation ()
	{
		int buttonPressed = ConfirmationButtons();	
		if(buttonPressed==1){ 		
			scene.SwitchPlayers();
			scene.BeginTurn();
		}else if(buttonPressed==2){ 	
			scene.CancelEndTurn();
		}
		
	}

	
	public void MovementConfirmation()
	{
		int buttonPressed = ConfirmationButtons();	
		if(buttonPressed==1) 		scene.ConfirmMove();
		else if(buttonPressed==2) 	scene.CancelMove();
	}
	
	public void ActionConfirmation()
	{
		int buttonPressed = ConfirmationButtons();
		if(buttonPressed==1)		buttonPressed++;//CHANGE THE TRASH
		else if(buttonPressed==2) 	buttonPressed++;//CHANGE THE TRASH
	}
	
	public int ConfirmationButtons(){
		GUI.Box (new Rect(Screen.width-100,Screen.height/2 -120,100,240),"");
		if(GUI.Button(new Rect(Screen.width-95,Screen.height/2-115,90,110), "Confirm")) {
			return 1;
		}
		if(GUI.Button (new Rect(Screen.width-95,Screen.height/2,90,110),"Cancel")) {
			return 2;
		}
		return 0;
	}
	
	public void LoadMainMenu(){
		 
		GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Spies Guys: Murders and Lies");
		switch(page){
		case 1:
			if(GUI.Button(new Rect(Screen.width/2-50,Screen.height/2-25,100,50), "Create a Match")) { 
				page=2;
			}
			if(GUI.Button(new Rect(Screen.width/2-50,Screen.height/2+50,100,50), "About")) { 

			}
			break;
		case 2:
			switch(spyToAssignGear){
			case 1:
				GUI.Label(new Rect(Screen.width/2,Screen.height/2-250,600,50), "Spy #1");
				GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Choose A Gadget");
				if(GUI.Button(new Rect(Screen.width/2-150,Screen.height/2-25,100,50), "Emp Pistol")) { 
					gadget1=1;
					spyToAssignGear=2;
				}
				if(GUI.Button(new Rect(Screen.width/2+50,Screen.height/2-25,100,50), "Shock Rifle")) { 
					gadget1=2;
					spyToAssignGear=2;
				}
				break;
			case 2:
				GUI.Label(new Rect(Screen.width/2,Screen.height/2-250,600,50), "Spy #2");
				GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Choose A Gadget");
				if(GUI.Button(new Rect(Screen.width/2-150,Screen.height/2-25,100,50), "Emp Pistol")) { 
					gadget2=1;
					page++;
				}
				if(GUI.Button(new Rect(Screen.width/2+50,Screen.height/2-25,100,50), "Shock Rifle")) { 
					gadget2=2;
					page++;
				}
				break;
			}
			break;
		case 3:
			scene.CreateMatch();
			scene.SGDataInit();
			page=1;
			break;
		}
	}
	
	public void ReadyMenu(){
		GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,500,500), "Match Ready...");
		if(GUI.Button(new Rect(Screen.width/2-100,Screen.height/2-50,100,50), "Start Game")) { 
			scene.StartGame(); 
		}	
	}
	
	public void BeginTurnMenu(){
		GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,500,500), "Your Turn");
		if(GUI.Button(new Rect(Screen.width/2-100,Screen.height/2-50,100,50), "Begin")) { 
			if(scene.CurrentGameState==(int)GameState.States.P1) scene.GiveControlToPlayer1();
			else if(scene.CurrentGameState==(int)GameState.States.P2) scene.GiveControlToPlayer2();
		}	
	}
}
