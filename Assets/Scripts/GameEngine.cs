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
	private int[,] visibility;
	private bool updateFlag;
	
	public int Winner{
		get{return winner;}	
	}
	
	// Use this for initialization
	void Start () {
		map = new MapInfo();
		gstate = new GameState(); //Default: Menu
		tstate = new TurnState(); //Default: Neutral
		updateFlag = false;
		tileGraphics = new Transform[map.MapSize,map.MapSize];
	}
	
	
	// Update is called once per frame
	void Update () {
		if(updateFlag){
			CheckForWinner();
			updateFlag = false;
		}
		
	}

	void CheckForWinner ()
	{
		if(map.AllTeammatesDead()){
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
	
	public void StartGame(){
		gstate.StartGame();	
	}
	
	public void SelectCharacter(int x, int z){
		map.SelectCharacterAtTile(x,z);
		HighlightTiles(x,z);
		tstate.SelectCharacter();	
	}
	
	public void DeselectCharacter(){
		map.DeselectCharacter();
		DestroyHighlights();
		tstate.Neutralize();
	}
	
	public void GiveControlToPlayer1(){
		map.CurrentPlayer = 1;
		map.ResetPoints();
		SetPlayerVisibilityUsingFoV();
		gstate.GiveControlToPlayer1();	
	}
	
	public void GiveControlToPlayer2(){
		map.CurrentPlayer=2;
		map.ResetPoints();
		SetPlayerVisibilityUsingFoV();
		gstate.SwitchPlayers();
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
				Debug.Log("tile "+i+","+j+" mat set to "+ temp);
			}
		}
	}
	
	public void UpdateTileMaterials(){
		int[,] tileVisibility = map.ReturnAllVisibleTiles();
		for(int i=0; i<map.MapSize;i++){
			for(int j=0; j<map.MapSize;j++){
				if(tileVisibility[i,j]==0){
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
		int totalMovementDistance = map.MovesLeftForPlayer(x,z);
		List<Vector2> BFSFromOrigin = map.BFS (x,z,totalMovementDistance);
		foreach(Vector2 tile in BFSFromOrigin){
			map.TileAt(tile).Highlight=true;
			if(map.TileAt(tile).Depth<(int)totalMovementDistance/2)
				Instantiate(sneakHighlight,new Vector3(tile.x*Tile.spacing,.2f,tile.y*Tile.spacing),Quaternion.identity);
			else
				Instantiate(sprintHighlight,new Vector3(tile.x*Tile.spacing,.2f,tile.y*Tile.spacing),Quaternion.identity);
		}
	}
	
	public void DestroyHighlights(){
		foreach(GameObject g in GameObject.FindGameObjectsWithTag("highlight"))
			Destroy(g);
		map.ResetHighlights();
	}
	
	public void RemoveVisibility(){
		map.RemoveVisibility();
		UpdateTileMaterials();	
	}
	
	public void SetPlayerVisibility(){
		map.RemoveVisibility();
		map.FindVisibleTilesForPlayer();
		map.FindAllVisibleTiles();
		UpdateTileMaterials();
	}
	
	public void SetPlayerVisibilityUsingFoV(){
		map.FoVForCurrentPlayer((int)map.MapSize/2);
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
		return map.CurrentPlayerAtTile(x,z);	
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
		return map.TileTakenByEnemy(x,z);
	}
	
	public void MoveSelectedCharTo(int x, int z){
		DestroyHighlights();
		map.MoveSelectedCharTo(x,z);
		tstate.Neutralize();
		SetPlayerVisibilityUsingFoV();
	}
	
	public void EliminatePlayerAt(int x, int z){
		map.EliminatePlayerAt(x,z);	
	}
	
	public List<int> MovesLeftForCurrentPlayer(){
		return map.MovesLeftForCurrentPlayer();	
	}
}
