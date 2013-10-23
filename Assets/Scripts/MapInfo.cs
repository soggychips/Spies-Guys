using UnityEngine;
using System.Collections;

public class MapInfo{
	
	public static int spacing = 10;
	public int viewDistance = 4;
	
	private int mapSize = 35;
	
	
	private Tile[,] map;
	private bool updateMap = false;
	private int[,] visibility;
	private int currentPlayer; //used for visibility reference, set to 1 or 2 only!
	
	public Guy[] guys;
	public Spy[] spies;

	public int Spacing{
		get{return spacing;}	
	}
	
	public int GetTileType(int x, int z){
		return map[x,z].Type;	
	}
	
	public int MapSize{
		get{ return mapSize;}
		set{ mapSize = value;}
	}
	
	public int CurrentPlayer{
		set{currentPlayer=value;}	
	}
	
	// Use this for initialization
	public MapInfo () {
		map = new Tile[mapSize,mapSize];
		visibility = new int[mapSize,mapSize];
		MapInit();
	}
	
	public void MapInit(){
		for(int i=0;i<mapSize;i++){
			for(int j=0;j<mapSize;j++){
				map[i,j] = new Tile(i,j);	
			}
		}	
	}
	
	public void SetUpSGData(){
		Debug.Log ("Loading level...");
		//walls,doors,player tiles
		//outside walls
		GiveWallInRange(0,0,12,0);
		GiveWallInRange(16,0,32,0);
		GiveWallInRange(0,1,0,23);
		GiveWallInRange(1,23,32,23);
		GiveWallInRange(32,22,32,1);
		//walls arranged from NW to SE corners
		GiveWallInRange(5,21,5,22);
		GiveWallInRange(5,13,5,19);
		GiveWallInRange(5,1,5,11);
		map[6,8].GiveWall();
		GiveWallInRange(8,8,13,8);
		GiveWallInRange(8,1,8,4);
		GiveWallInRange(9,19,9,15);
		GiveWallInRange(9,13,9,9);
		map[9,4].GiveWall();
		GiveWallInRange(9,19,14,19);
		GiveWallInRange(11,4,16,4);
		map[12,3].GiveWall();
		map[12,1].GiveWall();
		GiveWallInRange(16,19,20,19);
		GiveWallInRange(15,8,20,8);
		GiveWallInRange(16,1,16,4);
		GiveWallInRange(20,19,20,14);
		GiveWallInRange(20,12,20,8);
		GiveWallInRange(27,23,27,21);
		GiveWallInRange(27,19,27,9);
		GiveWallInRange(27,1,27,7);
		GiveWallInRange(27,10,32,10);
		
		guys = new Guy[3];
		guys[0] = new Guy(30,3);
		guys[1] = new Guy(29,5);
		guys[2] = new Guy(30,7);
		spies = new Spy[2];
		spies[0] = new Spy(3,8);
		spies[1] = new Spy(3,16);
		map[3,8].Take(); map[3,16].Take(); //spies
		map[30,7].Take(); map[29,5].Take(); map[30,3].Take(); //guys
		
		SetAllTilesVisible();
	}
	
	
	//creates a vertical or horizontal wall in the tiles [(x1,z1),(x2,z2)]
	//Note: (x1==x2 || z1==z2) must be true
	public void GiveWallInRange(int x1, int z1, int x2, int z2){
		int posOrNeg; int counter;
		if(x1==x2){ //vertical wall
			if(z2>z1) posOrNeg = 1;
			else posOrNeg = -1;
			counter = z1;
			while(counter!=z2){
				map[x1,counter].GiveWall();
				counter+=posOrNeg;
			}
			map[x2,z2].GiveWall();
			
		}else if(z1==z2){ //horizontal wall
			if(x2>x1) posOrNeg = 1;
			else posOrNeg = -1;
			counter = x1;
			while(counter!=x2){
				map[counter,z1].GiveWall();
				counter+=posOrNeg;
			}
			map[x2,z2].GiveWall();
		}else{
			Debug.Log ("Error: straight line must be given in mapinfo.givewallinrange");	
		}
	}
	
	
	public void SetAllTilesVisible(){
		for(int i=0; i<mapSize; i++){
			for(int j=0; j<mapSize; j++){
				map[i,j].Visible=true;
			}
		}
		Debug.Log ("All Tiles Visible");
	}
	
	public void RemoveVisibility(){
		for(int i=0; i<mapSize; i++){
			for(int j=0; j<mapSize; j++){
				map[i,j].Visible=false;
			}
		}
		Debug.Log ("All Tiles Invisible");
	}
	
	public void FindAllVisibleTiles(){
		for(int i=0; i<mapSize; i++){
			for(int j=0; j<mapSize; j++){
				if(map[i,j].Visible)
					visibility[i,j] = 1;
				else
					visibility[i,j] = 0;
			}
		}
	}
	
	public void FindVisibleTilesForPlayer(){
		if(currentPlayer==1){ //spies
			Vector2[] spyLocations = {spies[0].TileLocation,spies[1].TileLocation};
			foreach(Vector2 loc in spyLocations){
				int[] bounds = BoundCheck(loc);
				for(int i = bounds[0];i<=bounds[3];i++){
					for(int j = bounds[1];j<=bounds[2];j++){
						map[i,j].Visible=true;	
					}
				}
			}
		}else if(currentPlayer==2){ //guys
			Vector2[] guyLocations = {guys[0].TileLocation,guys[1].TileLocation,guys[2].TileLocation};
			foreach(Vector2 loc in guyLocations){
				int[] bounds = BoundCheck(loc);
				for(int i = bounds[0];i<=bounds[3];i++){
					for(int j = bounds[1];j<=bounds[2];j++){
						map[i,j].Visible=true;	
					}
				}
			}	
		}
	}
	
	public int[] BoundCheck(Vector2 playerLocation){ //returns {leftX,bottomZ,topZ,rightX}
		int leftX,rightX,topZ,bottomZ;
		if(playerLocation.x - viewDistance < 0) leftX=0;
		else leftX = (int)playerLocation.x-viewDistance;
		if(playerLocation.x + viewDistance >= mapSize) rightX = mapSize-1;
		else rightX = (int)playerLocation.x+viewDistance;
		if(playerLocation.y - viewDistance < 0) bottomZ=0;
		else bottomZ = (int)playerLocation.y-viewDistance;
		if(playerLocation.y + viewDistance >= mapSize) topZ = mapSize-1;
		else topZ = (int)playerLocation.y+viewDistance;
		int[] playerBounds = {leftX,bottomZ,topZ,rightX};
		return playerBounds;
	}
	
	public int[,] ReturnAllVisibleTiles(){
		FindAllVisibleTiles();
		return visibility;	
	}
	
	public bool CurrentPlayerAtTile(int x, int z){
		if(currentPlayer==1){ //spies
			Vector2[] spyLocations = {spies[0].TileLocation,spies[1].TileLocation};
			foreach(Vector2 loc in spyLocations){
				if(loc.x==x && loc.y==z) return true;
			}
		}else if(currentPlayer==2){ //guys
			Vector2[] guyLocations = {guys[0].TileLocation,guys[1].TileLocation,guys[2].TileLocation};
			foreach(Vector2 loc in guyLocations){
				if(loc.x==x && loc.y==z) return true;
			}	
		}
		return false;
	}
	
	public void SelectCharacterAtTile(int x, int z){
		if(currentPlayer==1){ //spies
			if(spies[0].TileLocation.x==x && spies[0].TileLocation.y==z) spies[0].Selected=true;
			else if(spies[1].TileLocation.x==x && spies[1].TileLocation.y==z) spies[1].Selected=true;
		}else if(currentPlayer==2){ //guys
			if(guys[0].TileLocation.x==x && guys[0].TileLocation.y==z) guys[0].Selected=true;
			else if(guys[1].TileLocation.x==x && guys[1].TileLocation.y==z) guys[1].Selected=true;
			else if(guys[2].TileLocation.x==x && guys[2].TileLocation.y==z) guys[2].Selected=true;
		}
	}
	
	public void MoveSelectedCharTo(int x, int z){
		if(currentPlayer==1){ //spies
			foreach(Spy spy in spies){
				if(spy.Selected){ 
					map[(int)spy.TileLocation.x,(int)spy.TileLocation.y].Open();
					spy.Move(x,z);
					spy.Selected=false;
					map[(int)spy.TileLocation.x,(int)spy.TileLocation.y].Take();
				}
			}
		}else if(currentPlayer==2){ //guys
			foreach(Guy guy in guys){
				if(guy.Selected){ 
					map[(int)guy.TileLocation.x,(int)guy.TileLocation.y].Open();
					guy.Move(x,z);
					guy.Selected=false;
					map[(int)guy.TileLocation.x,(int)guy.TileLocation.y].Take();
				}
			}
		}
	}
	
	public bool OpenTileAt(int x, int z){
		if(map[x,z].Type == (int)TileType.Open)	return true;
		return false;
	}
	
	public bool VisibleTileAt(int x, int z){
		return map[x,z].Visible;	
	}
	
}
