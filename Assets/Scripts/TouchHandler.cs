using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchHandler : MonoBehaviour {
	
	public bool debug;
	//public Camera camera;
	
	private GameEngine scene;
	private GUIController guiController;
	private bool mainMenu;
	private bool gameMenu;
	private Vector2 mouseClick;

	int page = 1;
	int gearPage = 1;
	int spyToAssignGear=0;
	int guyToAssignGear=0;
	private int BeginStateCheck = 0;

	public GUIStyle game_turnButton_iPhone;
	public GUIStyle game_turnStateText_iPhone;


	
	
	// Use this for initialization
	void Start () {
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine; //gives us access to the GameEngine script
		guiController = GameObject.Find("GUIController").GetComponent("GUIController") as GUIController;
	}

	public bool Player1(){
		return (scene.CurrentGameState==(int)GameState.States.P1);
	}

	public bool Player2(){
		return (scene.CurrentGameState==(int)GameState.States.P2);
	}
	
	void Update(){
		//if it's midgame
		if(scene.CurrentGameState==(int)GameState.States.P1 ||scene.CurrentGameState==(int)GameState.States.P2){
			switch(scene.CurrentTurnState){
			//if turnstate: Neutral
				case (int)TurnState.States.Begin:
						if(BeginStateCheck==0){
							scene.CheckForWinner();
							scene.RemoveVisibility();
							BeginStateCheck=1;
						}
					break;
				case (int)TurnState.States.Neutral:
					//scene.DisplayFreeDoorButtonTiles();
					mouseClick = MouseClickToTileCoords();
					if(scene.CurrentPlayerAt((int)mouseClick.x,(int)mouseClick.y)){
						//Debug.Log("Player clicked on");
						scene.SelectCharacter((int)mouseClick.x,(int)mouseClick.y);
						if(scene.LightswitchButtonsDisplayed) 
							guiController.FlagLightswitchButtons();
					}else if(scene.HighlightedTileAt((int)mouseClick.x,(int)mouseClick.y) && scene.WallAt(mouseClick)){
						//door controls... open/close,pick/lock
						scene.HandleWallButtonClickAt(mouseClick);
					}
					break;
				//if turnState: CharSelected
				case (int)TurnState.States.CharSelected:
					if(scene.LightswitchButtonsDisplayed){
						if(guiController.LightSwitched()){
							scene.FlipLightswitch(scene.ReturnSelectedPlayer().TileLocation);
							guiController.ResetLightswitch();
						}
					}
					mouseClick = MouseClickToTileCoords();
					if(scene.HighlightedTileAt((int)mouseClick.x,(int)mouseClick.y) && !scene.CurrentPlayerAt((int)mouseClick.x,(int)mouseClick.y)){
						if(scene.UnblockedTileAt(mouseClick)){
							scene.Movement((int)mouseClick.x,(int)mouseClick.y);
						}else if(scene.DataAt(mouseClick)){ 
							if(Player1()){
								scene.TakeData(mouseClick);
							}else if(scene.DroppedDataAt(mouseClick)){ //only player 2 can reach this
								scene.ResetDroppedData(mouseClick);
							}
						}else if(scene.WallAt(mouseClick)){ //DELETE
							//door controls... open/close,pick/lock
							//lightswitch controls
							scene.HandleWallButtonClickAt(mouseClick);
						}
					}else if(scene.CurrentPlayerAt((int)mouseClick.x,(int)mouseClick.y)){
						scene.DeselectCharacter();
					}else if(scene.TileTakenByEnemy((int)mouseClick.x,(int)mouseClick.y) && scene.SelectedPlayerCanAttackEnemyAt(mouseClick)){
						scene.Attack(mouseClick);
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
				Debug.Log(x+","+z+" in room "+scene.ReturnRoomContainingTile((int)x,(int)z));
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
				BeginTurnMenu();
				break;
			case (int)TurnState.States.Neutral:
				DisplaySubmitButton();
				DisplayPlayerData();
				break;
			case (int)TurnState.States.CharSelected:
				DisplaySelectedPlayerData();
				break;
			case (int)TurnState.States.MoveConfirm:
				MovementConfirmation();
				break;
			case (int)TurnState.States.ActionConfirm:
				ActionConfirmation();
				break;
			case (int)TurnState.States.End:
				CompleteTurnConfirmation();
				break;
			}
		}else if(scene.CurrentGameState==(int)GameState.States.P2){
			//Debug.Log("P2 turn");
			switch(scene.CurrentTurnState){
			case (int)TurnState.States.Begin:
				BeginTurnMenu();
				break;
			case (int)TurnState.States.Neutral:
				DisplaySubmitButton();
				DisplayPlayerData();
				DisplayDataIsMissingOnGameScreen();
				break;
			case (int)TurnState.States.CharSelected:
				DisplaySelectedPlayerData();
				break;
			case (int)TurnState.States.MoveConfirm:
				MovementConfirmation();
				break;
			case (int)TurnState.States.ActionConfirm:
				ActionConfirmation();
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
		GUI.Label(new Rect(Screen.width-400,0,150,50),"Game State: "+gameStateString);
		GUI.Label(new Rect(Screen.width-400,30,150,50),"Turn State: "+turnStateString);
		GUI.Label(new Rect(Screen.width-400,60,150,50),"Turn: "+ scene.Turn);
	}

	public void DisplaySelectedPlayerData(){
		//Debug.Log ("Displaying Selected Player Data");
		List<int> movesLeftForPlayers = scene.MovesLeftForCurrentPlayer();
		List<int> healthLeftForPlayers = scene.HealthLeftForCurrentPlayer();
		List<string> gearForPlayers = scene.GearForCurrentPlayer();
		int selectedPlayerIndex = scene.ReturnSelectedPlayerIndex();
		switch(scene.CurrentGameState){
		case (int)GameState.States.P1:
			if(selectedPlayerIndex==0){
				GUI.Label(new Rect(Screen.width-700,0,150,30),"Spy[0]: "+movesLeftForPlayers[0]);
				GUI.Label(new Rect(Screen.width-400,80,150,30),"Health:");
				GUI.Label(new Rect(Screen.width-400,95,150,30),"Spy[0]: "+ healthLeftForPlayers[0]);
				GUI.Label(new Rect(Screen.width-400,175,150,30),"Gear:");
				GUI.Label(new Rect(Screen.width-400,200,150,30),"Spy[0]: "+ gearForPlayers[0]);
			}else if(selectedPlayerIndex==1){
				GUI.Label(new Rect(Screen.width-700,30,150,30),"Spy[1]: "+movesLeftForPlayers[1]);
				GUI.Label(new Rect(Screen.width-400,80,150,30),"Health:");
				GUI.Label(new Rect(Screen.width-400,125,150,30),"Spy[1]: "+ healthLeftForPlayers[1]);
				GUI.Label(new Rect(Screen.width-400,175,150,30),"Gear:");
				GUI.Label(new Rect(Screen.width-400,230,150,30),"Spy[1]: "+ gearForPlayers[1]);
			}else{
				Debug.Log ("Error: TouchHandler.DisplaySelectedPlayerData()");
			}
			break;
		case (int)GameState.States.P2:
			if(selectedPlayerIndex==0){
				GUI.Label(new Rect(Screen.width-700,0,150,30),"Guy[0]: "+movesLeftForPlayers[0]);
				GUI.Label(new Rect(Screen.width-400,80,150,30),"Health:");
				GUI.Label(new Rect(Screen.width-400,95,150,30),"Guy[0]: "+ healthLeftForPlayers[0]);
				GUI.Label(new Rect(Screen.width-400,175,150,30),"Gear:");
				GUI.Label(new Rect(Screen.width-400,200,150,30),"Guy[0]: "+ gearForPlayers[0]);
			}else{
				GUI.Label(new Rect(Screen.width-700,30,150,30),"Guy[1]: "+movesLeftForPlayers[1]);
				GUI.Label(new Rect(Screen.width-400,80,150,30),"Health:");
				GUI.Label(new Rect(Screen.width-400,125,150,30),"Guy[1]: "+ healthLeftForPlayers[1]);
				GUI.Label(new Rect(Screen.width-400,175,150,30),"Gear:");
				GUI.Label(new Rect(Screen.width-400,230,150,30),"Guy[1]: "+ gearForPlayers[1]);
			}
			break;
		}
	}

	public void DisplayPlayerData(){
		List<int> movesLeftForPlayers = scene.MovesLeftForCurrentPlayer();
		List<int> healthLeftForPlayers = scene.HealthLeftForCurrentPlayer();
		List<string> gearForPlayers = scene.GearForCurrentPlayer();
		switch(scene.CurrentGameState){
		case (int)GameState.States.P1:
			GUI.Label(new Rect(Screen.width-700,0,150,30),"Spy[0]: "+movesLeftForPlayers[0]);
			GUI.Label(new Rect(Screen.width-700,30,150,30),"Spy[1]: "+movesLeftForPlayers[1]);
			GUI.Label(new Rect(Screen.width-400,80,150,30),"Health:");
			GUI.Label(new Rect(Screen.width-400,95,150,30),"Spy[0]: "+ healthLeftForPlayers[0]);
			GUI.Label(new Rect(Screen.width-400,125,150,30),"Spy[1]: "+ healthLeftForPlayers[1]);
			GUI.Label(new Rect(Screen.width-400,175,150,30),"Gear:");
			GUI.Label(new Rect(Screen.width-400,200,150,30),"Spy[0]: "+ gearForPlayers[0]);
			GUI.Label(new Rect(Screen.width-400,230,150,30),"Spy[1]: "+ gearForPlayers[1]);
			break;
		case (int)GameState.States.P2:
			GUI.Label(new Rect(Screen.width-700,0,150,30),"Guy[0]: "+movesLeftForPlayers[0]);
			GUI.Label(new Rect(Screen.width-700,30,150,30),"Guy[1]: "+movesLeftForPlayers[1]);
			GUI.Label(new Rect(Screen.width-400,80,150,30),"Health:");
			GUI.Label(new Rect(Screen.width-400,95,150,30),"Guy[0]: "+ healthLeftForPlayers[0]);
			GUI.Label(new Rect(Screen.width-400,125,150,30),"Guy[1]: "+ healthLeftForPlayers[1]);
			GUI.Label(new Rect(Screen.width-400,175,150,30),"Gear:");
			GUI.Label(new Rect(Screen.width-400,200,150,30),"Guy[0]: "+ gearForPlayers[0]);
			GUI.Label(new Rect(Screen.width-400,230,150,30),"Guy[1]: "+ gearForPlayers[1]);
			break;
		}
		
	}

	public void DisplayDataIsMissingOnGameScreen(){
		if(scene.MissingDataAlert){
			GUI.Label(new Rect(Screen.width-650,15,150,30),"DATA IS MISSING!");
		}
	}
	

	public void DisplaySubmitButton ()
	{
		List<int> movesLeftForPlayers = scene.MovesLeftForCurrentPlayer();

		// Submit
		if(GUI.Button(new Rect(Screen.width-128,Screen.height-134,128,134), "", game_turnButton_iPhone)) { 
			scene.EndTurn();
		}
		if(movesLeftForPlayers[0]>0 && movesLeftForPlayers[1]>0)
			GUI.Label(new Rect(Screen.width-160,Screen.height-124,150,120), "Turn: Incomplete", game_turnStateText_iPhone);
		else
			GUI.Label(new Rect(Screen.width-160,Screen.height-124,150,120), "Turn: Complete", game_turnStateText_iPhone);

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
		switch(scene.CurrentTurnStateActionType){
		case (int)TurnState.ActionTypes.Attack:
			if(buttonPressed==1)		scene.ConfirmAttack();
			else if(buttonPressed==2) 	scene.CancelAction();
			break;
		case (int)TurnState.ActionTypes.Door:
			if(buttonPressed==1)		scene.ConfirmAction();
			else if(buttonPressed==2) 	scene.CancelAction();
			break;
		case (int)TurnState.ActionTypes.Door_Priced:
			if(buttonPressed==1)		scene.ConfirmAction();
			else if(buttonPressed==2) 	scene.CancelAction();
			break;
		case (int)TurnState.ActionTypes.Data:
			if(buttonPressed==1){
				if(Player1())	scene.ConfirmDataSteal();
				else 			scene.ConfirmDataReset();
			}else if(buttonPressed==2) 	
				scene.CancelAction();
			break;
		default:
			if(buttonPressed==1)		scene.ConfirmAction();
			else if(buttonPressed==2) 	scene.CancelAction();
			break;
		}
	}
	
	public int ConfirmationButtons(){
		guiController.FlagConfirmationButtons();
		return guiController.ConfirmationButtonPlayerInput();
	}

	public int EndTurnConfirmationButtons(){
		guiController.EndTurnFlag();
		return guiController.ConfirmationButtonPlayerInput();
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
			scene.CreateMatch();
			scene.SGDataInit();
			page=1;
			break;
		}
	}
	
	public void ReadyMenu(){
		switch(page){
		case 1:
			AssignGearMenu();
			break;
		case 2:
			//guiController.EstablishGadgetTextures();
			GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,500,500), "Match Ready...");
			if(GUI.Button(new Rect(Screen.width/2-100,Screen.height/2-50,100,50), "Start Game")) 
				scene.StartGame();
			break;
		}	
	}

	public void AssignGearMenu(){
		switch(gearPage){
		case 1:
			switch(spyToAssignGear){
			case 0:
				GUI.Label(new Rect(Screen.width/2,Screen.height/2-250,600,50), "Spy #1");
				GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Choose A Gadget");
				if(GUI.Button(new Rect(Screen.width/2-150,Screen.height/2-25,100,50), "Emp Pistol")) { 
					scene.AssignGearToSpy(0,(int)Spy.SpyGear.empGun);
					spyToAssignGear=1;
				}
				if(GUI.Button(new Rect(Screen.width/2+50,Screen.height/2-25,100,50), "Shock Rifle")) { 
					scene.AssignGearToSpy(0,(int)Spy.SpyGear.shockRifle);
					spyToAssignGear=1;
				}
				break;
			case 1:
				GUI.Label(new Rect(Screen.width/2,Screen.height/2-250,600,50), "Spy #2");
				GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Choose A Gadget");
				if(GUI.Button(new Rect(Screen.width/2-150,Screen.height/2-25,100,50), "Emp Pistol")) { 
					scene.AssignGearToSpy(1,(int)Spy.SpyGear.empGun);
					gearPage=2;
				}
				if(GUI.Button(new Rect(Screen.width/2+50,Screen.height/2-25,100,50), "Shock Rifle")) { 
					scene.AssignGearToSpy(1,(int)Spy.SpyGear.shockRifle);
					gearPage=2;
				}
				break;
			}
			break;
		case 2:
			switch(guyToAssignGear){
			case 0:
				GUI.Label(new Rect(Screen.width/2,Screen.height/2-250,600,50), "Guy #1");
				GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Choose A Gadget");
				if(GUI.Button(new Rect(Screen.width/2-150,Screen.height/2-25,100,50), "Shotgun")) { 
					scene.AssignGearToGuy(0,(int)Guy.GuyGear.shotgun);
					guyToAssignGear=1;
				}
				if(GUI.Button(new Rect(Screen.width/2+50,Screen.height/2-25,100,50), "Rifle")) { 
					scene.AssignGearToGuy(0,(int)Guy.GuyGear.rifle);
					guyToAssignGear=1;
				}
				break;
			case 1:
				GUI.Label(new Rect(Screen.width/2,Screen.height/2-250,600,50), "Guy #2");
				GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,600,600), "Choose A Gadget");
				if(GUI.Button(new Rect(Screen.width/2-150,Screen.height/2-25,100,50), "Shotgun")) { 
					scene.AssignGearToGuy(1,(int)Guy.GuyGear.shotgun);
					page=2;
				}
				if(GUI.Button(new Rect(Screen.width/2+50,Screen.height/2-25,100,50), "Rifle")) { 
					scene.AssignGearToGuy(1,(int)Guy.GuyGear.rifle);
					page=2;
				}
				break;
			}
			break;
		}
	}
	
	public void BeginTurnMenu(){
		GUI.Box(new Rect(Screen.width/2-300,Screen.height/2-300,500,500), "Your Turn");
		if(scene.CurrentGameState==(int)GameState.States.P2 && (scene.MissingDataAlert)) 
			GUI.Label(new Rect(Screen.width/2-100,Screen.height/2-150,100,50), "DATA MISSING");
		if(GUI.Button(new Rect(Screen.width/2-100,Screen.height/2-50,100,50), "Begin")) { 
			if(scene.CurrentGameState==(int)GameState.States.P1) scene.GiveControlToPlayer1();
			else if(scene.CurrentGameState==(int)GameState.States.P2) scene.GiveControlToPlayer2();
			BeginStateCheck = 0;
		}	
	}




}
