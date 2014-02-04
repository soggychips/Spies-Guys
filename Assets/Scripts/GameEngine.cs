using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEngine : MonoBehaviour {

	public enum Players: int{One,Two};	//Spies, Guys
	public Transform tile;				//The tile transform

	//Materials
	public Material ft_hidden, ft_open, 
					ft_taken, ft_wall, //ft_wall is a fallback return element, showing there is a problem if it appears in game 
					ft_lightswitch, ft_noise, 
					ft_data, ft_extraction;
	public Material pt_spy, pt_guy;
	public Material pt_S1,pt_S2,pt_G1,pt_G2; //playerTile_Sp/GuyTile
	public Material wall_n_end, wall_s_end, wall_e_end, wall_w_end, wall_ne_corn,wall_nw_corn,
					wall_se_corn,wall_sw_corn,wall_h_mid,wall_v_mid,wall_s_t,wall_n_t,wall_e_t,wall_w_t;
	public Transform wallButton_unlock, wallButton_lock, wallButton_CloseDoor, wallButton_OpenDoor, wallButton_lightswitch, wallButton_LockDown;
	public Material door_NS, door_EW, lockedDoor;
	public Transform sneakHighlight;
	public Transform sprintHighlight;
	public Transform InteractionHighlight;
	public Color unlit = Color.gray; 
	public Color lit = Color.white;
	
	
	private int winner;					//whoever is the winner is awarded this int, as a humble prize from me
	private Transform[,] tileGraphics;	//2D array of the transforms for the game tiles (physical + cosmetic information)
	private MapInfo map; 	  			//single instantiation of the map, containing all tile information
	private GameState gstate; 		//used for changing/controlling/viewing the current state
	private TurnState tstate; 		//used for changing/controlling/viewing the current state 
	private int turn;		  		//a game-long counter
	private Guy specificGuy; 		//a guy used for method calls and returns across 
	private Spy specificSpy;
	public int currentPlayer; //set using Players enum
	private bool alertMissingData = false;
	private bool freeDoorButtonsAreDisplayed = false;
	private bool pricedDoorButtonsAreDisplayed = false;
	private bool lightswitchButtonsDisplayed = false;
	private bool lockdownButtonDisplayed = false;
	private bool lockdownButtonUsed = false;

	//movement variables
	private Vector2 originalPosition; private int selectedPlayerIdx;
	private List<Vector2> spyNoiseAlertLocations, guyNoiseAlertLocations;

	//door variable(s)
	private Vector2 positionOfDoor;

	//data variables
	private Vector2 positionofData;

	//attack variables
	private int damageDealt;
	private Vector2 enemyLocation;


	public bool LightswitchButtonsDisplayed{
		get{return lightswitchButtonsDisplayed;}
	}
	
	public int Winner{
		get{return winner;}	
	}
	

	// Use this for initialization
	void Start () {
		turn=0;
		map = new MapInfo(); 
		gstate = new GameState(); //Default: Menu
		tstate = new TurnState(); //Default: Neutral
		tileGraphics = new Transform[map.MapSize,map.MapSize];
		spyNoiseAlertLocations = new List<Vector2>();
		guyNoiseAlertLocations = new List<Vector2>();
	}
	

	public void CheckForWinner ()
	{
		if(map.AllTeammatesDead(currentPlayer)){
			Debug.Log ("All Teammates for player "+currentPlayer+" dead.");
			winner = map.Winner;
			gstate.EndGame();
			tstate.Neutralize();
		}
		if(map.AllDataExtracted()){
			Debug.Log ("All data has been extracted!");
			winner = map.Winner;
			gstate.EndGame();
			tstate.Neutralize();
		}
	}

	public void SGDataInit ()
	{
		map.SetUpSGData();
		//map.SetUpTestCaseForRoomFinder();
		LoadTiles();
	}
	
	public int CurrentGameState{
		get{return gstate.CurrentState;}
	}
	
	public int CurrentTurnState{
		get{return tstate.CurrentState;}	
	}

	public int CurrentTurnStateActionType{
		get{return tstate.ActionType;}
	}

	public bool MissingDataAlert{
		get{return alertMissingData;}
	}
	
	public int Turn{
		get{return turn;}	
	}
	
	public void CreateMatch(){
		gstate.CreateMatch();	
	}
	
	public void StartGame(){
		gstate.GiveControlToPlayer1();
		currentPlayer=(int)Players.One;
		tstate.BeginTurn();
	}
	

	public void BeginTurn ()
	{
		tstate.BeginTurn();
	}
	
	public void SelectCharacter(int x, int z){
		map.SelectCharacterAtTile(x,z,currentPlayer);
		HighlightMovementTiles(x,z);
		HighlightInteractionObjects(x,z);
		tstate.SelectCharacter();	
	}
	
	public void DeselectCharacter(){
		map.DeselectCharacter(currentPlayer);
		DestroyHighlights();
		tstate.Neutralize();
	}
	
	public void GiveControlToPlayer1(){ //Give control to Spies
		turn++;
		spyNoiseAlertLocations.Clear ();
		map.ResetPoints();
		SetPlayerVisibilityUsingFoV();	
		tstate.Neutralize();
	}
	
	public void GiveControlToPlayer2(){ //Give control to Guys
		turn++;
		guyNoiseAlertLocations.Clear();
		map.ResetPoints();
		SetPlayerVisibilityUsingFoV();
		tstate.Neutralize();
	}

	public void EndTurn ()
	{
		DestroyHighlights();
		tstate.EndTurn();
	}
	
	public void CancelEndTurn(){
		tstate.Neutralize();	
	}
	
	public void SwitchPlayers(){
		gstate.SwitchPlayers();
		if(currentPlayer==(int)Players.One) currentPlayer=(int)Players.Two;
		else if(currentPlayer==(int)Players.Two) currentPlayer=(int)Players.One;
	}

	
	public void LoadTiles(){
		for(int i=0; i<map.MapSize;i++){
			for(int j=0; j<map.MapSize;j++){
				tileGraphics[i,j] = (Transform)Instantiate(tile,new Vector3(i*map.Spacing,0,j*map.Spacing),Quaternion.identity);
				tileGraphics[i,j].Rotate(new Vector3(0,1,0),180); //textures on planes display rotated 180 degrees on z compared to texture in inspector pane -- ANNOYING!
			}
		}
		LoadTileMaterials();
	}
	
	public void LoadTileMaterials(){
		for(int i=0; i<map.MapSize;i++){
			for(int j=0; j<map.MapSize;j++){
				Material temp = AssignMaterial(i,j);
				tileGraphics[i,j].renderer.material = temp;
				tileGraphics[i,j].renderer.material.color = Color.white;
				//Debug.Log("tile "+i+","+j+" mat set to "+ temp);
			}
		}
	}
	
	public void UpdateTileMaterials(){
		int[,] tileVisibility = map.ReturnAllVisibleTiles();
		for(int i=0; i<map.MapSize;i++){
			for(int j=0; j<map.MapSize;j++){
				if(tileVisibility[i,j]==0 && !map.SelectedCharacterAtTile(i,j,currentPlayer)){ 
					if((currentPlayer==(int)Players.One && guyNoiseAlertLocations.Contains(new Vector2(i,j))) 
					|| (currentPlayer==(int)Players.Two && spyNoiseAlertLocations.Contains(new Vector2(i,j)))){
						tileGraphics[i,j].renderer.material=ft_noise;
					}else{
						tileGraphics[i,j].renderer.material=ft_hidden;
						tileGraphics[i,j].renderer.material.color = Color.grey;
					}
				}else{
					Material temp = AssignMaterial(i,j);
					tileGraphics[i,j].renderer.material=temp;
					if(map.TileAt(new Vector2(i,j)).hasWall() || map.TileAt(new Vector2(i,j)).hasLightswitch() || map.TileAt (new Vector2(i,j)).Lit) 
						tileGraphics[i,j].renderer.material.color = Color.white; //lit
					else tileGraphics[i,j].renderer.material.color = Color.gray; //unlit
					//Debug.Log("tile "+i+","+j+" mat set to "+ temp);
				}
			}
		}
	}

	public bool DoorAt(Vector2 mouseClick){
		return(map.TileAt(mouseClick).hasDoor());
	}

	public bool LightswitchAt(Vector2 mouseClick){
		return (map.TileAt(mouseClick).hasLightswitch());
	}

	public bool LockedDoorAt (Vector2 mouseClick)
	{
		return(map.TileAt(mouseClick).hasLockedDoor());
	}

	public bool UnlockedDoorAt (Vector2 mouseClick)
	{
		return(!map.TileAt(mouseClick).hasLockedDoor() && map.TileAt(mouseClick).hasClosedDoor());
	}

	public bool ClosedDoorAt (Vector2 mouseClick)
	{
		return(map.TileAt(mouseClick).hasClosedDoor() || map.TileAt(mouseClick).hasLockedDoor());
	}

	public bool DataAt(Vector2 mouseClick){
		return(map.TileAt(mouseClick).hasData());
	}

	public bool DroppedDataAt(Vector2 mouseClick){
		return(map.TileAt(mouseClick).hasData() && map.TileContainsDroppedData(mouseClick));
	}

	public bool UnblockedTileAt (Vector2 mouseClick)
	{
		return(!map.TileAt(mouseClick).isBlocked());
	}

	public bool WallAt(Vector2 mouseClick){
		return(map.TileAt(mouseClick).hasWall());
	}

	//Free Door Buttons - Open/Close
	//Priced Door Buttons - Un/Lock
	//
	public void DisplayFreeDoorButtonTiles(){
		if(!freeDoorButtonsAreDisplayed){	
			Vector2 playerPosition = map.ReturnSelectedPlayer(currentPlayer).TileLocation;
			Vector2 doorLocation = map.GetAdjacentDoorLocation((int)playerPosition.x,(int)playerPosition.y);
			if(doorLocation.x!=-1000){
				if(map.TileAt(doorLocation).hasOpenDoor()){
					ShowDoorCloseWallButton(doorLocation);
					freeDoorButtonsAreDisplayed = true;
				}else if(map.TileAt(doorLocation).hasClosedDoor()){
					ShowDoorOpenWallButton(doorLocation);
					freeDoorButtonsAreDisplayed = true;
				}else if(map.TileAt (doorLocation).hasLockedDoor() && currentPlayer==(int)Players.Two){
					ShowDoorOpenWallButton(doorLocation);
					freeDoorButtonsAreDisplayed=true;
				}
			}
		}
	}

	public void DisplayPricedDoorButtonTiles(){
		if(!pricedDoorButtonsAreDisplayed){	
			Vector2 playerPosition = map.ReturnSelectedPlayer(currentPlayer).TileLocation;
			Vector2 location = map.GetAdjacentDoorLocation((int)playerPosition.x,(int)playerPosition.y);
			if(location.x!=-1000){
				if(map.TileAt(location).hasLockedDoor() && currentPlayer == (int)Players.One 
				   && ReturnSelectedPlayer().HasPoint()){ //spies only get unlock
					ShowUnlockDoorWallButton(location);
					pricedDoorButtonsAreDisplayed = true;
				}else if(map.TileAt (location).hasClosedDoor() && !map.TileAt (location).hasLockedDoor() 
				         && currentPlayer == (int)Players.Two && ReturnSelectedPlayer().HasPoint()){ //guys only get lock
					ShowLockDoorWallButton(location);
					pricedDoorButtonsAreDisplayed = true;
				}
			}
		}
	}
	

	public void DisplayLightswitchWallButton(){
		if(!lightswitchButtonsDisplayed){	
			Vector2 playerPosition = map.ReturnSelectedPlayer(currentPlayer).TileLocation;
			if(playerPosition.x!=-1000){
				if(map.TileAt(playerPosition).hasLightswitch()){
					//show the lightswitch button
					//ShowLightswitchWallButton(playerPosition);
					lightswitchButtonsDisplayed = true;
				}
			}
		}
	}

	public void ShowLightswitchWallButton(Vector2 tileLocation){
		Vector2 wallButtonLocation = new Vector2();
		if(map.TileAt (tileLocation+Vector2.right).hasWall()){
			wallButtonLocation = tileLocation+Vector2.right;
		}else if(map.TileAt (tileLocation-Vector2.right).hasWall()){
			wallButtonLocation = tileLocation-Vector2.right;
		}else if(map.TileAt (tileLocation+Vector2.up).hasWall()){
			wallButtonLocation = tileLocation+Vector2.up;
		}else if(map.TileAt (tileLocation-Vector2.up).hasWall()){
			wallButtonLocation = tileLocation-Vector2.up;
		}else{
			Debug.Log ("Error: GameEngine.ShowLightswitchWallButton");
		}
		map.TileAt(wallButtonLocation).Highlight();
		Instantiate(wallButton_lightswitch, new Vector3(wallButtonLocation.x*Tile.spacing,.2f,wallButtonLocation.y*Tile.spacing),Quaternion.identity);
	}

	public void DisplayLockdownWallButton(){
		if(!lockdownButtonDisplayed && !lockdownButtonUsed){	
			Vector2 playerPosition = map.ReturnSelectedPlayer(currentPlayer).TileLocation;
			if(playerPosition.x!=-1000){
				if(map.TileAt(playerPosition).hasLockdownSwitch() && ReturnSelectedPlayer().HasPoint()){
					//show the lockdown button
					ShowLockdownWallButton(playerPosition);
					lockdownButtonDisplayed = true;
				}
			}
		}
	}

	public void ShowLockdownWallButton(Vector2 tileLocation){
		Vector2 wallButtonLocation = new Vector2();
		if(map.TileAt (tileLocation+Vector2.right).hasWall()){
			wallButtonLocation = tileLocation+Vector2.right;
		}else if(map.TileAt (tileLocation-Vector2.right).hasWall()){
			wallButtonLocation = tileLocation-Vector2.right;
		}else if(map.TileAt (tileLocation+Vector2.up).hasWall()){
			wallButtonLocation = tileLocation+Vector2.up;
		}else if(map.TileAt (tileLocation-Vector2.up).hasWall()){
			wallButtonLocation = tileLocation-Vector2.up;
		}else{
			Debug.Log ("Error: GameEngine.ShowLockdownWallButton");
		}
		map.TileAt(wallButtonLocation).Highlight();
		Instantiate(wallButton_LockDown, new Vector3(wallButtonLocation.x*Tile.spacing,.2f,wallButtonLocation.y*Tile.spacing),Quaternion.identity);
	}

	public void ShowDoorOpenWallButton(Vector2 tileLocation){
		Vector2 wallButtonLocation = new Vector2();
		if(map.TileAt (tileLocation).DoorFacing == (int)DoorFacings.EW){
			wallButtonLocation = tileLocation + Vector2.up;
		}else{
			wallButtonLocation = tileLocation - Vector2.right;
		}
		map.TileAt(wallButtonLocation).Highlight();
		Instantiate(wallButton_OpenDoor, new Vector3(wallButtonLocation.x*Tile.spacing,.2f,wallButtonLocation.y*Tile.spacing),Quaternion.identity);
		//Debug.Log ("Open door button instantiated at "+wallButtonLocation);
	}
	
	public void ShowDoorCloseWallButton(Vector2 tileLocation){
		Vector2 wallButtonLocation = new Vector2();
		if(map.TileAt (tileLocation).DoorFacing == (int)DoorFacings.EW){
			wallButtonLocation = tileLocation + Vector2.up;
		}else{
			wallButtonLocation = tileLocation - Vector2.right;
		}
		map.TileAt(wallButtonLocation).Highlight();
		Instantiate(wallButton_CloseDoor, new Vector3(wallButtonLocation.x*Tile.spacing,.2f,wallButtonLocation.y*Tile.spacing),Quaternion.identity);
		//Debug.Log ("Close door button instantiated at "+wallButtonLocation);
	}

	public void ShowLockDoorWallButton(Vector2 tileLocation){
		Vector2 wallButtonLocation = new Vector2();
		if(map.TileAt (tileLocation).DoorFacing == (int)DoorFacings.EW){
			wallButtonLocation = tileLocation - Vector2.up;
		}else{
			wallButtonLocation = tileLocation + Vector2.right;
		}
		map.TileAt(wallButtonLocation).Highlight();
		Instantiate(wallButton_lock, new Vector3(wallButtonLocation.x*Tile.spacing,.2f,wallButtonLocation.y*Tile.spacing),Quaternion.identity);
		//Debug.Log ("Lock door button instantiated at "+wallButtonLocation);
	}

	public void ShowUnlockDoorWallButton(Vector2 tileLocation){
		Vector2 wallButtonLocation = new Vector2();
		if(map.TileAt (tileLocation).DoorFacing == (int)DoorFacings.EW){
			wallButtonLocation = tileLocation - Vector2.up;
		}else{
			wallButtonLocation = tileLocation + Vector2.right;
		}
		map.TileAt(wallButtonLocation).Highlight();
		Instantiate(wallButton_unlock, new Vector3(wallButtonLocation.x*Tile.spacing,.2f,wallButtonLocation.y*Tile.spacing),Quaternion.identity);
	}

	public void HandleWallButtonClickAt(Vector2 mouseClick){
		Debug.Log ("HandleWallButtonClickAt called");
		if(currentPlayer == (int)Players.Two){	
			if(map.TileAt (mouseClick+Vector2.up).hasLockdownSwitch()){
				Lockdown();
			}else if(map.TileAt (mouseClick-Vector2.up).hasLockdownSwitch() ){
				Lockdown();
			}else if(map.TileAt (mouseClick+Vector2.right).hasLockdownSwitch()){
				Lockdown();
			}else if(map.TileAt (mouseClick-Vector2.right).hasLockdownSwitch() ){
				Lockdown();
			}
		}

		if(map.TileAt (mouseClick+Vector2.up).hasLightswitch()){
			FlipLightswitch(mouseClick+Vector2.up);
		}else if(map.TileAt (mouseClick-Vector2.up).hasLightswitch() ){
			FlipLightswitch(mouseClick-Vector2.up);
		}else if(map.TileAt (mouseClick+Vector2.right).hasLightswitch()){
			FlipLightswitch(mouseClick+Vector2.right);
		}else if(map.TileAt (mouseClick-Vector2.right).hasLightswitch() ){
			FlipLightswitch(mouseClick-Vector2.right);
		}else if(map.TileAt(mouseClick-Vector2.up).hasOpenDoor()){ //close - BOTH
			Debug.Log ("Close EW door");
			CloseDoor(mouseClick-Vector2.up);
		}else if(map.TileAt(mouseClick+Vector2.right).hasOpenDoor()){ //close - BOTH
			Debug.Log ("Close NS door");
			CloseDoor(mouseClick+Vector2.right);
		}else if(map.TileAt (mouseClick-Vector2.up).hasClosedDoor()){ //open - BOTH
			Debug.Log ("Open EW door");
			OpenDoor(mouseClick-Vector2.up);
		}else if(map.TileAt (mouseClick+Vector2.right).hasClosedDoor()){ //open - BOTH
			Debug.Log ("Open NS door");
			OpenDoor(mouseClick+Vector2.right);
		}else if(map.TileAt(mouseClick-Vector2.up).hasLockedDoor()){ //open - GUYS
			Debug.Log ("Open EW door");
			OpenDoor(mouseClick-Vector2.up);
		}else if(map.TileAt (mouseClick+Vector2.right).hasLockedDoor()){ //open - GUYS
			Debug.Log ("Open NS door");
			OpenDoor(mouseClick+Vector2.right);
		}else if(map.TileAt(mouseClick+Vector2.up).hasLockedDoor()){ //unlock - SPIES
			Debug.Log ("Unlock EW door");
			UnlockDoor(mouseClick+Vector2.up);
		}else if(map.TileAt(mouseClick-Vector2.right).hasLockedDoor() ){ //unlock - SPIES
			Debug.Log ("Unlock NS door");
			UnlockDoor(mouseClick-Vector2.right);
		}else if(map.TileAt(mouseClick+Vector2.up).hasClosedDoor() && !map.TileAt(mouseClick+Vector2.up).hasLockedDoor() ){ //lock - GUYS
			Debug.Log ("Lock EW door");
			LockDoor(mouseClick+Vector2.up);
		}else if(map.TileAt(mouseClick-Vector2.right).hasClosedDoor() && !map.TileAt(mouseClick-Vector2.right).hasLockedDoor() ){ //lock - GUYS
			Debug.Log ("Lock NS door");
			LockDoor(mouseClick-Vector2.right);
		}
	}
	
	public void HighlightClosedDoors(int x, int z)
	{ //if player is not next to a door, does nothing
		Vector2 doorLocation = map.GetAdjacentClosedDoorLocation(x,z);
		if(doorLocation.x!=-1000){
			map.TileAt(doorLocation).Highlight();
			Instantiate (InteractionHighlight, new Vector3(doorLocation.x*Tile.spacing,.2f,doorLocation.y*Tile.spacing),Quaternion.identity);
		}
	}


	public void HighlightData(int x, int z){
		Vector2 dataLocation = map.GetAdjacentDataLocation(x,z);
		if(dataLocation.x!=-1000){
			map.TileAt(dataLocation).Highlight();
			Instantiate(InteractionHighlight, new Vector3(dataLocation.x*Tile.spacing,.2f,dataLocation.y*Tile.spacing),Quaternion.identity);
		}

	}

	public void HighlightDroppedData(int x, int z){
		Vector2 dataLocation = map.GetAdjacentDataLocation(x,z);
		if(dataLocation.x!=-1000 && map.TileContainsDroppedData(dataLocation)){
			map.TileAt (dataLocation).Highlight();
			Instantiate(InteractionHighlight, new Vector3(dataLocation.x*Tile.spacing,.2f,dataLocation.y*Tile.spacing),Quaternion.identity);
		}
	}

	public void HighlightLightswitchTiles(int room){
		foreach(Vector2 tile in map.ReturnRoom(room)){
			if(map.TileAt(tile).hasLightswitch()&& !map.TileAt(tile).isTaken()){
				map.TileAt (tile).Highlight();
				Instantiate(InteractionHighlight, new Vector3(tile.x*Tile.spacing,.2f,tile.y*Tile.spacing),Quaternion.identity);
			}
		}
	}


	public void HighlightInteractionObjects (int x, int z)
	{
		if(currentPlayer==(int)Players.One){//spies
			HighlightData(x,z);
		}else{//guys
			HighlightDroppedData(x,z);
			DisplayLockdownWallButton();
		}
		DisplayFreeDoorButtonTiles();
		DisplayPricedDoorButtonTiles();
		DisplayLightswitchWallButton();
	}
	
	public void HighlightMovementTiles(int x, int z){
		int movesForPlayer = map.MovesLeftForPlayer(x,z,currentPlayer); 
		int totalSneakDistance = Player.sneakDistance;
		int totalDistance = totalSneakDistance+ ReturnSelectedPlayer().CurrentSprintDistance;
		//Debug.Log ("Current player's sprint distance: "+ReturnSelectedPlayer().CurrentSprintDistance);
		List<Vector2> BFSFromOrigin = new List<Vector2>();
		if(movesForPlayer==0){ 
			//do nothing
		}else{
			BFSFromOrigin = map.BFS (x,z,totalDistance);
		}
		foreach(Vector2 tile in BFSFromOrigin){
			map.TileAt(tile).Highlight();
			if(map.TileAt(tile).Depth<=totalSneakDistance)
				Instantiate(sneakHighlight,new Vector3(tile.x*Tile.spacing,.2f,tile.y*Tile.spacing),Quaternion.identity);
			else
				Instantiate(sprintHighlight,new Vector3(tile.x*Tile.spacing,.2f,tile.y*Tile.spacing),Quaternion.identity);
		}
	}

	public void MarkTileAsSprintTile (Vector2 mouseClick)
	{
		map.TileAt(mouseClick).SprintedTo=true;
		if(currentPlayer==(int)Players.One){ //spies
			Debug.Log ("tile marked as sprinted (spies)");
			spyNoiseAlertLocations.Add(mouseClick);
		}else{ //guys
			guyNoiseAlertLocations.Add (mouseClick);
		}
	}

	public void UnmarkTileAsSprintTile(Vector2 tileToUnmark){
		map.TileAt(tileToUnmark).SprintedTo = false;
		if(currentPlayer==(int)Players.One){ //spies
			Debug.Log ("tile unmarked as sprinted (spies)");
			spyNoiseAlertLocations.Remove(tileToUnmark);
		}else{ //guys
			guyNoiseAlertLocations.Remove(tileToUnmark);
		}
	}

	public bool TileIsSprintDistance(Vector2 tileLocation){
		return (map.TileAt(tileLocation).Depth>Player.sneakDistance);
	}
	
	public void DestroyHighlights(){
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("highlight")) Destroy(g);
		map.ResetHighlights();
		freeDoorButtonsAreDisplayed = false;
		pricedDoorButtonsAreDisplayed = false;
		lightswitchButtonsDisplayed = false;
		lockdownButtonDisplayed = false;
	}
	
	public void RemoveVisibility(){
		map.RemoveVisibility();
		UpdateTileMaterials();	
	}
	
	/*public void SetPlayerVisibility(){
		map.RemoveVisibility();
		map.FindVisibleTilesForPlayer(currentPlayer);
		map.FindAllVisibleTiles();
		UpdateTileMaterials();
	}*/
	
	public void SetPlayerVisibilityUsingFoV(){
		map.FoVForCurrentPlayer((int)map.MapSize/2,currentPlayer);
		UpdateTileMaterials();
	}

	public void AssignGearToSpy(int spyIndex,int gearToGive){
		map.AssignGearToSpy(spyIndex,gearToGive);
	}

	public void AssignGearToGuy(int guyIdx, int gearToGive){
		map.AssignGearToGuy(guyIdx,gearToGive);
	}
	
	public Material AssignMaterial(int x, int z){
		int type = map.GetTileType(x,z);
		if(type==(int)TileType.Wall){
			switch(map.GetWallType(x,z)){
			case (int)WallTypes.E_Horizontal_End:
				return wall_e_end;
			case (int)WallTypes.W_Horizontal_End:
				return wall_w_end;
			case (int)WallTypes.NE_Corner:
				return wall_ne_corn;
			case (int)WallTypes.NW_Corner:
				return wall_nw_corn;
			case (int)WallTypes.SE_Corner:
				return wall_se_corn;
			case (int)WallTypes.SW_Corner:
				return wall_sw_corn;
			case (int)WallTypes.S_Vertical_End:
				return wall_s_end;
			case (int)WallTypes.N_Vertical_End:
				return wall_n_end;
			case (int)WallTypes.Horizontal_Mid:
				return wall_h_mid;
			case (int)WallTypes.Vertical_Mid:
				return wall_v_mid;
			case (int)WallTypes.N_T:
				return wall_n_t;
			case (int)WallTypes.S_T:
				return wall_s_t;
			case (int)WallTypes.E_T:
				return wall_e_t;
			case (int)WallTypes.W_T:
				return wall_w_t;
			}
			return ft_wall;
		}else if(type==(int)TileType.Data){
			return ft_data;
		}else if(type==(int)TileType.Lightswitch){
			return ft_lightswitch;
		}
		if(type==(int)TileType.Open){
			return ft_open;
		}
		if(type==(int)TileType.Taken){
			switch(currentPlayer){
			case (int)Players.One: 
				if(CurrentPlayerAt(x,z)){ 
					switch(map.ReturnPlayerIdxFromPosition(x,z,currentPlayer)){
					case 0:
						return pt_S1;
					case 1:
						return pt_S2;
					default:
						return pt_spy;
					}
				}else{
					if(map.TileAt (new Vector2(x,z)).hasLightswitch() &&!TeamCanSeeTileAt(x,z)) return ft_lightswitch;
					switch(map.ReturnEnemyIdxFromPosition(x,z,currentPlayer)){
					case 0:
						return pt_G1;
					case 1:
						return pt_G2;
					default:
						return pt_guy;
					}
				}
			case (int)Players.Two:
				if(CurrentPlayerAt(x,z)){ 
					switch(map.ReturnPlayerIdxFromPosition(x,z,currentPlayer)){
					case 0:
						return pt_G1;
					case 1:
						return pt_G2;
					default:
						return pt_guy;
					} 
				}else{
					if(map.TileAt (new Vector2(x,z)).hasLightswitch() &&!TeamCanSeeTileAt(x,z)) return ft_lightswitch;
					switch(map.ReturnEnemyIdxFromPosition(x,z,currentPlayer)){
					case 0:
						return pt_S1;
					case 1:
						return pt_S2;
					default:
						return pt_spy;
					}
				}
			default:
				return ft_taken;
			}
		}
		if(type==(int)TileType.Door_Locked){
			return lockedDoor;
		}
		if(type==(int)TileType.Door_Closed){
			switch(map.GetDoorFacing(x,z)){
				case (int)DoorFacings.EW: return door_EW;
				case (int)DoorFacings.NS: return door_NS;
			}
		}
		if(type==(int)TileType.Door_Open){
			//Debug.Log ("Door is open with facing as: ");
			//if(map.GetDoorFacing(x,z)==(int)DoorFacings.EW) Debug.Log ("EW. Door set to NS");
			//else Debug.Log ("NS. Door set to EW");
			switch(map.GetDoorFacing(x,z)){
			case (int)DoorFacings.EW: return door_NS;
			case (int)DoorFacings.NS: return door_EW;
			}
		}
		if(type==(int)TileType.Extraction){
			return ft_extraction;
		}
		//Check for Noise Alerts
		if(currentPlayer==(int)Players.One){ //spies
			if(guyNoiseAlertLocations.Contains(new Vector2(x,z))) return ft_noise;
		}else{								//guys
			if(spyNoiseAlertLocations.Contains(new Vector2(x,z))){ 
				//Debug.Log ("spyNouseAlertLocations contains the vector "+x+","+z);
				return ft_noise;
			}
		}
		return ft_hidden;
	}
	
	public bool CurrentPlayerAt(int x, int z){
		return map.CurrentPlayerAtTile(x,z,currentPlayer);	
	}

	public int ReturnRoomContainingTile(int x, int z){
		return map.ReturnRoomContainingTile(x,z);
	}

	public Guy ReturnClickedOnGuy(int x, int z){
		return map.ReturnClickedOnGuy(x,z);
	}

	public Spy ReturnClickedOnSpy(int x, int z){
		return map.ReturnClickedOnSpy(x,z);
	}
	
	public Guy ReturnSelectedGuy(){
		return map.ReturnSelectedGuy();
	}

	public Spy ReturnSelectedSpy(){
		return map.ReturnSelectedSpy();
	}

	public Player ReturnSelectedPlayer(){
		return map.ReturnSelectedPlayer(currentPlayer);
	}

	public int ReturnSelectedPlayerIndex(){
		return map.ReturnSelectedPlayerIdx(currentPlayer);
	}

	public int ReturnNumberOfLiveCharactersOnCurrentTeam(){
		return map.ReturnNumberOfLivePlayersOnTeam(currentPlayer);
	}

	public Vector2 ReturnSelectedPlayerPosition(int idx){
		return map.ReturnPlayerPosition(idx,currentPlayer);
	}

	public List<int> ReturnTeamHP(){
		return map.ReturnTeamHP(currentPlayer);
	}

	public List<int>ReturnTeamAP(){
		return map.ReturnTeamAP(currentPlayer);
	}
	

	public bool OpenTileAt(int x, int z){
		return map.OpenTileAt(x,z);
	}
	
	public bool HighlightedTileAt(int x, int z){
		return map.HighlightedTileAt(x,z);
	}
	
	public bool VisibleTileAt(int x, int z){
		return map.VisibleTileAt(x,z);	
	}

	public bool BlockedTileAt(Vector2 tileLocation){
		return map.BlockedTileAt((int)tileLocation.x,(int)tileLocation.y);
	}
	
	public bool TileTakenByEnemy(int x, int z){
		return map.TileTakenByEnemy(x,z,currentPlayer);
	}

	public void Movement(int goalX, int goalZ) //(goalX,goalZ) is the location being moved to
	{
		PrepareMovement();
		BeginMovement(goalX,goalZ);
	}

	public void PrepareMovement ()
	{
		//save revert information
		selectedPlayerIdx = map.ReturnSelectedPlayerIdx(currentPlayer);
		originalPosition = map.ReturnPlayerPosition(selectedPlayerIdx, currentPlayer);
		
		//Debug.Log ("CurrentPlayer: "+currentPlayer+". Idx: "+selectedPlayerIdx+". OG Position: "+originalPosition);
		//Debug.Log ("Prepared for movement");
	}

	public void BeginMovement (int goalX, int goalZ)
	{
		//Debug.Log ("Beginning movement to: "+goalX+","+goalZ);
		tstate.BeginMovement();
		AnimateMovement(goalX,goalZ);
	}

	public void AnimateMovement (int goalX, int goalZ)
	{
		bool done = false;
		tstate.AnimateMovement();
		//until animation implementation:
		done=true;
		if(done){
			MoveSelectedCharTo(goalX,goalZ);
		}
	}

	public void MoveSelectedCharTo(int x, int z){
		Vector2 mouseClick = new Vector2(x,z);
		if(TileIsSprintDistance(mouseClick)){
			Debug.Log ("Sprinting to tile "+mouseClick);
			MarkTileAsSprintTile(mouseClick);
		}
		//DestroyHighlights();
		map.MoveSelectedCharTo(x,z,currentPlayer);
		tstate.EndMovement();
		UpdateTileMaterials();
	}

	public void ConfirmMove(){
		tstate.Neutralize();
		map.AttemptExtraction(currentPlayer);
		selectedPlayerIdx=new int(); originalPosition = new Vector2();
		SetPlayerVisibilityUsingFoV();
		map.DeselectCharacter(currentPlayer);
		DestroyHighlights();
	}
	
	public void CancelMove(){
		tstate.Neutralize();
		UnmarkTileAsSprintTile(ReturnSelectedPlayerPosition(selectedPlayerIdx));
		map.RevertMovement(selectedPlayerIdx,originalPosition,currentPlayer);
		UpdateTileMaterials();
		map.DeselectCharacter(currentPlayer);
		DestroyHighlights();
	}

	public void OpenDoor(Vector2 doorLocation){
		tstate.BeginAction((int)TurnState.ActionTypes.Door);
		positionOfDoor = doorLocation;
		map.OpenDoor((int)doorLocation.x,(int)doorLocation.y);
		//map.DeselectCharacter(currentPlayer);
		DestroyHighlights();
		tstate.EndAction();
		UpdateTileMaterials();
		ConfirmAction();
		
	}

	public void CloseDoor(Vector2 doorLocation){
		tstate.BeginAction((int)TurnState.ActionTypes.Door);
		positionOfDoor = doorLocation;
		map.CloseDoor((int)doorLocation.x,(int)doorLocation.y);
		//map.DeselectCharacter(currentPlayer);
		DestroyHighlights();
		tstate.EndAction();
		UpdateTileMaterials();
		ConfirmAction();
	}

	public void LockDoor(Vector2 doorLocation){
		tstate.BeginAction((int)TurnState.ActionTypes.Door_Priced);
		positionOfDoor = doorLocation;
		map.LockDoor((int)doorLocation.x,(int)doorLocation.y);
		ReturnSelectedGuy().SpendPoint();
		DestroyHighlights();
		tstate.EndAction();
		UpdateTileMaterials();
	}

	public void UnlockDoor(Vector2 doorLocation){
		tstate.BeginAction((int)TurnState.ActionTypes.Door_Priced);
		positionOfDoor = doorLocation;
		map.CloseDoor((int)doorLocation.x,(int)doorLocation.y);
		ReturnSelectedSpy().SpendPoint();
		//map.DeselectCharacter(currentPlayer);
		DestroyHighlights();
		tstate.EndAction();
		UpdateTileMaterials();
	}

	public void FlipLightswitch(Vector2 lightswitchLocation){
		tstate.BeginAction((int)TurnState.ActionTypes.Lightswitch);
		map.FlipLightswitch(map.TileAt(lightswitchLocation).Room);
		DestroyHighlights();
		tstate.EndAction();
		UpdateTileMaterials();
		ConfirmAction();
	}

	public void Lockdown(){
		tstate.BeginAction((int)TurnState.ActionTypes.Lockdown);
		map.Lockdown();
		lockdownButtonUsed = true;
		ReturnSelectedGuy().SpendPoint();
		DestroyHighlights();
		tstate.EndAction();
		UpdateTileMaterials();

	}

	public void TakeData(Vector2 dataLocation){
		tstate.BeginAction((int)TurnState.ActionTypes.Data);
		positionofData = dataLocation;
		tstate.EndAction();
	}
	

	public void ConfirmDataSteal(){
		DestroyHighlights();
		tstate.Neutralize();
		//steal data, assign to proper spy, open data tile
		map.TakeData(positionofData);
		SetPlayerVisibilityUsingFoV();
		map.DeselectCharacter(currentPlayer);
		alertMissingData = true;
		positionofData = new Vector2();
	}

	public void ResetDroppedData(Vector2 droppedDataLocation){
		tstate.BeginAction((int)TurnState.ActionTypes.Data);
		positionofData = droppedDataLocation;
		tstate.EndAction();
	}

	public void ConfirmDataReset(){
		DestroyHighlights();
		Debug.Log ("Confirm Data Reset");
		tstate.Neutralize();
		map.ResetDroppedDataAt(positionofData);
		//highlight the reset location for visual knowledge
		map.DeselectCharacter(currentPlayer);
		SetPlayerVisibilityUsingFoV();
		if(!map.MissingData()) alertMissingData = false;
		positionofData = new Vector2();
	}


	public void ConfirmAction(){
		tstate.Neutralize();
		SetPlayerVisibilityUsingFoV();
		map.DeselectCharacter(currentPlayer);
	}

	public void CancelAction(){
		DestroyHighlights();
		tstate.Neutralize();
		switch((int)tstate.ActionType){
		case (int)TurnState.ActionTypes.Door:
			map.RevertDoorOpening((int)positionOfDoor.x,(int)positionOfDoor.y);
			break;
		case (int)TurnState.ActionTypes.Door_Priced:
			map.RevertDoorOpening((int)positionOfDoor.x,(int)positionOfDoor.y);
			ReturnSelectedPlayer().GivePoint();
			break;
		case (int)TurnState.ActionTypes.Lockdown:
			map.Unlockdown();
			ReturnSelectedGuy().GivePoint();
			lockdownButtonUsed =false;
			break;
		//Attack case currently would do nothing
		}
		UpdateTileMaterials();
		map.DeselectCharacter(currentPlayer);
	}

	public void ConfirmAttack(){
		SelectedPlayerDamageEnemy(enemyLocation);
		tstate.Neutralize();
		SetPlayerVisibilityUsingFoV();
		map.DeselectCharacter(currentPlayer);
		enemyLocation = new Vector2();
	}

	public void Attack(Vector2 enemyLocationCoords)
	{
		if(ReturnSelectedSpy().GearEquipped() == (int)Spy.SpyGear.empGun){
			Debug.Log ("You can't attack with that weapon!");
			CancelAction();
		}else{
			enemyLocation = enemyLocationCoords;
			tstate.BeginAction((int)TurnState.ActionTypes.Attack);
			SelectedPlayerPredictDamageToEnemy(enemyLocationCoords);
			DestroyHighlights();
			tstate.EndAction();
		}
	}

	public bool TeamCanSeeTileAt(int x, int z){
		return map.TeamCanSeeTileAt(x,z,currentPlayer);
	}

	public bool LOSCheckBetweenPlayers (Vector2 start, Vector2 end){
		Vector2 vect = end-start;
		Vector2 check = start;
		float norm = Mathf.Sqrt((vect.x*vect.x) + (vect.y*vect.y));
		Vector2 unitVect = new Vector2(vect.x/norm,vect.y/norm);
		Vector2 roundedLocation = new Vector2((int)start.x,(int)start.y);
		while(roundedLocation!=end){
			check+=unitVect;
			roundedLocation = new Vector2(Mathf.Round(check.x),Mathf.Round(check.y));
			//Debug.Log ("roundedLocation: "+roundedLocation);
			if(TileTakenByEnemy((int)roundedLocation.x,(int)roundedLocation.y) && VisibleTileAt((int)roundedLocation.x,(int)roundedLocation.y)){
				//Debug.Log ("one way, now check other way");
				return (LOSCheckBetweenPlayers(end,start)); //check the other direction
			}else if((roundedLocation==end)&& VisibleTileAt((int)roundedLocation.x,(int)roundedLocation.y)){
				//Debug.Log ("Full LOS check complete");
				return true;
			}else if((roundedLocation!=start) && BlockedTileAt(roundedLocation)){
				//Debug.Log ("Blocked tile at "+roundedLocation);
				return false;
			}
		}
		Debug.Log ("Error: GameEngine.LOSCheckBetweenPlayers()");
		return false;
	}

	public bool SelectedPlayerCanAttackEnemyAt(Vector2 enemyLocation){
		switch(currentPlayer){
		case (int)Players.One:
			specificSpy = ReturnSelectedSpy();
			specificGuy = ReturnClickedOnGuy((int)enemyLocation.x,(int)enemyLocation.y);
			if(!LOSCheckBetweenPlayers(specificSpy.TileLocation,specificGuy.TileLocation)) Debug.Log ("You can't get a good shot from here!");
			return (LOSCheckBetweenPlayers(specificSpy.TileLocation,specificGuy.TileLocation));
		case (int)Players.Two:
			specificGuy = ReturnSelectedGuy();
			specificSpy = ReturnClickedOnSpy((int)enemyLocation.x,(int)enemyLocation.y);
			if(!LOSCheckBetweenPlayers(specificGuy.TileLocation,specificSpy.TileLocation)) Debug.Log ("You can't get a good shot from here!");
			return (LOSCheckBetweenPlayers(specificGuy.TileLocation,specificSpy.TileLocation));
		default:
			Debug.Log ("Error: GameEngine.SelectedPlayerCanAttackEnemyAt()");
			return false;
		}
	}

	public void SelectedPlayerPredictDamageToEnemy(Vector2 enemyTile){
		switch(currentPlayer){
		case (int)Players.One:
			specificSpy = ReturnSelectedSpy();
			specificGuy = ReturnClickedOnGuy((int)enemyTile.x,(int)enemyTile.y);
			if(specificSpy.HasPoint()){
				specificSpy.PredictDamage(specificGuy);
			}
			break;
		case (int)Players.Two:
			specificGuy = ReturnSelectedGuy();
			specificSpy = ReturnClickedOnSpy((int)enemyTile.x,(int)enemyTile.y);
			if(specificGuy.HasPoint()){
				specificGuy.PredictDamage(specificSpy);
			}
			break;
		}
	}

	public void SelectedPlayerDamageEnemy(Vector2 enemyTile){
		switch(currentPlayer){
		case (int)Players.One:
			specificSpy = ReturnSelectedSpy();
			specificGuy = ReturnClickedOnGuy((int)enemyTile.x,(int)enemyTile.y);
			if(specificSpy.HasPoint()){
				specificSpy.DealDamage(specificGuy);
				if(!specificGuy.Alive){
					EliminatePlayerAt((int)specificGuy.TileLocation.x,(int)specificGuy.TileLocation.y);
				}
			}
			break;
		case (int)Players.Two:
			specificGuy = ReturnSelectedGuy();
			specificSpy = ReturnClickedOnSpy((int)enemyTile.x,(int)enemyTile.y);
			if(specificGuy.HasPoint()){
				specificGuy.DealDamage(specificSpy);
				if(!specificSpy.Alive){
					EliminatePlayerAt((int)specificSpy.TileLocation.x,(int)specificSpy.TileLocation.y);
				}
			}
			break;
		}
	}
	
	public void EliminatePlayerAt(int x, int z){
		map.EliminatePlayerAt(x,z,currentPlayer);	
	}
	
	public List<int> MovesLeftForCurrentPlayer(){
		return map.MovesLeftForCurrentPlayer(currentPlayer);	
	}

	public List<int> HealthLeftForCurrentPlayer(){
		return map.HealthLeftForCurrentPlayer(currentPlayer);
	}

	public List<string> GearForCurrentPlayer(){
		return map.GearForCurrentPlayer(currentPlayer);
	}
}
