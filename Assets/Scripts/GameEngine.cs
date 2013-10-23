using UnityEngine;
using System.Collections;

public class GameEngine : MonoBehaviour {

	public Transform tile;
	public Material ft_hidden, ft_open, ft_taken, ft_wall, ft_item;
	
	private Transform[,] tileGraphics;
	private MapInfo map;
	private GameState gstate;
	private TurnState tstate;
	private int[,] visibility;
	private bool updateFlag;
	
	
	// Use this for initialization
	void Start () {
		map = new MapInfo();
		gstate = new GameState(); //Default: Menu
		tstate = new TurnState(); //Default: Neutral
		updateFlag = false;
		tileGraphics = new Transform[map.MapSize,map.MapSize];
	}
	
	public void DefineVisibility(){
		if(gstate.CurrentState==(int)GameState.States.P1)
			map.CurrentPlayer = 1;
		else if(gstate.CurrentState==(int)GameState.States.P2)
			map.CurrentPlayer = 2;
		else Debug.Log("GameEngine.DefineVisibility called during an improper state: "+gstate.CurrentState);
		
	}
	
	// Update is called once per frame
	void Update () {
		if(updateFlag){
			updateFlag = false;
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
		tstate.SelectCharacter();	
	}
	
	public void GiveControlToPlayer1(){
		map.CurrentPlayer = 1;
		SetPlayerVisibility();
		gstate.GiveControlToPlayer1();	
	}
	
	public void GiveControlToPlayer2(){
		map.CurrentPlayer=2;
		SetPlayerVisibility();
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
					Debug.Log("INVIS TILE "+i+","+j);
				}else{
					Material temp = AssignMaterial(i,j);
					tileGraphics[i,j].renderer.material.SetColor("_Color",temp.color);
					Debug.Log("tile "+i+","+j+" mat set to "+ temp);
				}
			}
		}
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
	
	public bool VisibleTileAt(int x, int z){
		return map.VisibleTileAt(x,z);	
	}
	
	public void MoveSelectedCharTo(int x, int z){
		map.MoveSelectedCharTo(x,z);
		tstate.Neutralize();
		SetPlayerVisibility();
	}
}
