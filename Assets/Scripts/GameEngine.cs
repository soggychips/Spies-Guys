using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameEngine : MonoBehaviour {

	public Transform tile;
	public Material ft_hidden, ft_open, ft_taken, ft_wall, ft_item;
	public Transform sneakHighlight;
	public Transform sprintHighlight;
	
	
	private int winner;
	private Transform[,] tileGraphics;
	private MapInfo map;
	private GameState gstate;
	private TurnState tstate;
	private int turn;
	//private int[,] visibility;
	private bool updateFlag;
	private int currentPlayer;
	private Vector2 originalPosition; private int selectedPlayerIdx;
	
	
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
		HighlightTiles(x,z);
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
	
	public void HighlightTiles(int x, int z){
		int totalSneakDistance = map.MovesLeftForPlayer(x,z,currentPlayer)-1;
		List<Vector2> BFSFromOrigin = new List<Vector2>();
		if(totalSneakDistance>=1)
			BFSFromOrigin = map.BFS (x,z,totalSneakDistance);
		else if(totalSneakDistance==0) 
			BFSFromOrigin = map.BFS (x,z,2);
		foreach(Vector2 tile in BFSFromOrigin){
			map.TileAt(tile).Highlight=true;
			if(map.TileAt(tile).Depth<=totalSneakDistance+1)
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

	public void PrepareMovement ()
	{
		//save revert information
		selectedPlayerIdx = map.ReturnSelectedPlayerIdx(currentPlayer);
		originalPosition = map.ReturnSelectedPlayerPosition(selectedPlayerIdx, currentPlayer);
		
		Debug.Log ("CurrentPlayer: "+currentPlayer+". Idx: "+selectedPlayerIdx+". OG Position: "+originalPosition);
		Debug.Log ("Prepared for movement");
	}

	public void BeginMovement (int par1, int par2)
	{
		Debug.Log ("Beginning movement to: "+par1+","+par2);
		tstate.BeginMovement();
		AnimateMovement(par1,par2);
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
	
	public void EliminatePlayerAt(int x, int z){
		map.EliminatePlayerAt(x,z,currentPlayer);	
	}
	
	public List<int> MovesLeftForCurrentPlayer(){
		return map.MovesLeftForCurrentPlayer(currentPlayer);	
	}
}
