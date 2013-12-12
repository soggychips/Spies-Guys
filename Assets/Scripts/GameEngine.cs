using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEngine : MonoBehaviour {

	public Transform tile;
	public Material ft_hidden, ft_open, ft_taken, ft_wall, ft_item;
	public Material wall_n_end, wall_s_end, wall_e_end, wall_w_end, wall_ne_corn,wall_nw_corn,
					wall_se_corn,wall_sw_corn,wall_h_mid,wall_v_mid,wall_s_t,wall_n_t,wall_e_t,wall_w_t;
	public Material door_NS, door_EW;
	public Transform sneakHighlight;
	public Transform sprintHighlight;
	public Transform lockedDoorHighlight;
	
	
	private int winner;
	private Transform[,] tileGraphics;
	private MapInfo map;
	private GameState gstate;
	private TurnState tstate;
	private int turn;
	//private int[,] visibility;
	private bool updateFlag;
	public int currentPlayer;
	//movement variables
	private Vector2 originalPosition; private int selectedPlayerIdx;
	//attack variables


	
	
	public int Winner{
		get{return winner;}	
	}
	
	// Use this for initialization
	void Start () {
		turn=0;
		map = new MapInfo(); 
		gstate = new GameState(); //Default: Menu
		tstate = new TurnState(); //Default: Neutral
		updateFlag = false;
		tileGraphics = new Transform[map.MapSize,map.MapSize];
	}
	

	public void CheckForWinner ()
	{
		if(map.AllTeammatesDead(currentPlayer)){
			Debug.Log ("All Teammates for player "+currentPlayer+" dead.");
			winner = map.Winner;
			gstate.EndGame();
			tstate.Neutralize();
		}
	}

	public void SGDataInit ()
	{
		map.SetUpSGData();
		LoadTiles();
	}
	
	public int CurrentGameState{
		get{return gstate.CurrentState;}
	}
	
	public int CurrentTurnState{
		get{return tstate.CurrentState;}	
	}
	
	public int Turn{
		get{return turn;}	
	}
	
	public void CreateMatch(){
		gstate.CreateMatch();	
	}
	
	public void StartGame(){
		gstate.GiveControlToPlayer1();
		currentPlayer=1;
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
	
	public void GiveControlToPlayer1(){
		turn++;
		map.ResetPoints();
		SetPlayerVisibilityUsingFoV();	
		tstate.Neutralize();
	}
	
	public void GiveControlToPlayer2(){
		turn++;
		map.ResetPoints();
		SetPlayerVisibilityUsingFoV();
		tstate.Neutralize();
	}

	public void EndTurn ()
	{
		tstate.EndTurn();
	}
	
	public void CancelEndTurn(){
		tstate.Neutralize();	
	}
	
	public void SwitchPlayers(){
		gstate.SwitchPlayers();
		if(currentPlayer==1) currentPlayer=2;
		else if(currentPlayer==2) currentPlayer=1;
	}
	
	public void FlagForUpdate(){
		updateFlag = true;	
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
				//Debug.Log("tile "+i+","+j+" mat set to "+ temp);
			}
		}
	}
	
	public void UpdateTileMaterials(){
		int[,] tileVisibility = map.ReturnAllVisibleTiles();
		for(int i=0; i<map.MapSize;i++){
			for(int j=0; j<map.MapSize;j++){
				if(tileVisibility[i,j]==0 && !map.SelectedCharacterAtTile(i,j,currentPlayer)){
					tileGraphics[i,j].renderer.material.SetColor("_Color",ft_hidden.color);
					//Debug.Log("INVIS TILE "+i+","+j);
				}else{
					Material temp = AssignMaterial(i,j);
					tileGraphics[i,j].renderer.material.SetColor("_Color",temp.color);
					//Debug.Log("tile "+i+","+j+" mat set to "+ temp);
				}
			}
		}
	}

	public bool ClosedDoorAt (Vector2 mouseClick)
	{
		return(map.TileAt(mouseClick).hasClosedDoor());
	}

	public bool UnblockedTileAt (Vector2 mouseClick)
	{
		return(!map.TileAt(mouseClick).isBlocked());
	}

	public void HighlightPickableDoors (int x, int z)
	{ //if player is not next to a door, does nothing
		Vector2 doorLocation = map.GetAdjacentClosedDoorLocation(x,z);
		if(doorLocation.x!=-1000){
			map.TileAt(doorLocation).Highlight();
			Instantiate (lockedDoorHighlight, new Vector3(doorLocation.x*Tile.spacing,.2f,doorLocation.y*Tile.spacing),Quaternion.identity);
		}
	}

	public void HighlightInteractionObjects (int x, int z)
	{
		if(currentPlayer==1){//spies
			HighlightPickableDoors(x,z);
		}else{//guys

		}
	}
	
	public void HighlightMovementTiles(int x, int z){
		int movesForPlayer = map.MovesLeftForPlayer(x,z,currentPlayer); 
		int totalSneakDistance = Player.sneakDistance;
		int totalDistance = totalSneakDistance+Player.sprintDistnace;
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
	
	public void DestroyHighlights(){
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("highlight")) Destroy(g);
		map.ResetHighlights();
	}
	
	public void RemoveVisibility(){
		map.RemoveVisibility();
		UpdateTileMaterials();	
	}
	
	public void SetPlayerVisibility(){
		map.RemoveVisibility();
		map.FindVisibleTilesForPlayer(currentPlayer);
		map.FindAllVisibleTiles();
		UpdateTileMaterials();
	}
	
	public void SetPlayerVisibilityUsingFoV(){
		map.FoVForCurrentPlayer((int)map.MapSize/2,currentPlayer);
		UpdateTileMaterials();
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
		}
		if(type==(int)TileType.Item){
			return ft_item;
		}
		if(type==(int)TileType.Open){
			return ft_open;
		}
		if(type==(int)TileType.Taken){
			return ft_taken;
		}
		if(type==(int)TileType.Door_Closed){
			switch(map.GetDoorFacing(x,z)){
				case (int)DoorFacings.EW: return door_EW;
				case (int)DoorFacings.NS: return door_NS;
			}
		}
		if(type==(int)TileType.Door_Open){
			switch(map.GetDoorFacing(x,z)){
			case (int)DoorFacings.EW: return door_NS;
			case (int)DoorFacings.NS: return door_EW;
			}
		}
		return ft_hidden;
	}
	
	public bool CurrentPlayerAt(int x, int z){
		return map.CurrentPlayerAtTile(x,z,currentPlayer);	
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
		originalPosition = map.ReturnSelectedPlayerPosition(selectedPlayerIdx, currentPlayer);
		
		Debug.Log ("CurrentPlayer: "+currentPlayer+". Idx: "+selectedPlayerIdx+". OG Position: "+originalPosition);
		Debug.Log ("Prepared for movement");
	}

	public void BeginMovement (int goalX, int goalZ)
	{
		Debug.Log ("Beginning movement to: "+goalX+","+goalZ);
		tstate.BeginMovement();
		AnimateMovement(goalX,goalZ);
	}

	public void AnimateMovement (int goalX, int goalZ)
	{
		bool done = false;
		tstate.AnimateMovement();
		MoveSelectedCharTo(goalX,goalZ);
		
	}

	public void MoveSelectedCharTo(int x, int z){
		DestroyHighlights();
		map.MoveSelectedCharTo(x,z,currentPlayer);
		tstate.EndMovement();
		UpdateTileMaterials();
	}

	public void ConfirmMove(){
		tstate.Neutralize();
		selectedPlayerIdx=new int(); originalPosition = new Vector2();
		SetPlayerVisibilityUsingFoV();
		map.DeselectCharacter(currentPlayer);
	}
	
	public void CancelMove(){
		tstate.Neutralize();
		map.RevertMovement(selectedPlayerIdx,originalPosition,currentPlayer);
		UpdateTileMaterials();
		map.DeselectCharacter(currentPlayer);
	}

	public void Attack(int enemyX, int enemyZ)
	{
		Debug.Log ("This will kill the enemy, are you sure?");
		tstate.EndAction();
	}
	

	public void BeginAction(){
		tstate.BeginAction();
	}

	public void AnimateAction(){
		tstate.AnimateAction();
	}

	public void ConfirmAttack(){

	}


	
	public void EliminatePlayerAt(int x, int z){
		map.EliminatePlayerAt(x,z,currentPlayer);	
	}
	
	public List<int> MovesLeftForCurrentPlayer(){
		return map.MovesLeftForCurrentPlayer(currentPlayer);	
	}
}
