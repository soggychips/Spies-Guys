using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapInfo{
	
	public static int spacing = 10;
	public int viewDistance = 4;
	
	private int mapSize = 35;
	
	private int winner;
	private Tile[,] map;
	private int[,] visibility;
	private int currentPlayer; //used for visibility reference, set to 1 or 2 only!
	
	public Guy[] guys;
	public Spy[] spies;
	
	public int Winner{
		get{return winner;}	
	}
	
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
		
		guys = new Guy[2];
		guys[0] = new Guy(30,3);
		guys[1] = new Guy(29,5);
		spies = new Spy[2];
		spies[0] = new Spy(3,8);
		spies[1] = new Spy(3,16);
		map[3,8].Take(); map[3,16].Take(); //spies
		map[29,5].Take(); map[30,3].Take(); //guys
		
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
				if(map[i,j].Type!=(int)TileType.Wall)
					map[i,j].Visible=false;
			}
		}
		Debug.Log ("All Tiles Invisible");
	}

	public void ResetPoints ()
	{
		foreach(Spy spy in spies) spy.ResetPoints();
		foreach(Guy guy in guys) guy.ResetPoints();
	}
	
	public void ResetHighlights(){
		for(int i=0; i<mapSize; i++){
			for(int j=0; j<mapSize; j++){
				map[i,j].Highlight=false;
			}
		}
		Debug.Log ("All Tile Highlights Reset");
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
	
	
	/*//rewrite similar to CurrentPlayerAtTile to fix vision problem
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
	}*/
	
	//above method, rewritten using lists and checking for dead players
	public void FindVisibleTilesForPlayer(){
		//for the spies:
		if(currentPlayer==1){
			List<Vector2> spyLocations = new List<Vector2>();
			//populate list of alive spy locations
			foreach(Spy spy in spies){
				if(spy.Alive) spyLocations.Add (spy.TileLocation);
			}
			//add a 4x4 vision box around each spy location
			foreach(Vector2 loc in spyLocations){
				int[] bounds = BoundCheck(loc);
				for(int i = bounds[0];i<=bounds[3];i++){
					for(int j = bounds[1];j<=bounds[2];j++){
						map[i,j].Visible=true;	
					}
				}
			}
		//for the guys:
		}else if(currentPlayer==2){
			List<Vector2> guyLocations = new List<Vector2>();
			foreach(Guy guy in guys){
				if(guy.Alive) guyLocations.Add (guy.TileLocation);	
			}
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
		Vector2 tile = new Vector2(x,z);
		if(currentPlayer==1){
			foreach(Spy spy in spies){
				if(spy.Alive && spy.TileLocation==tile)
					return true;
			}
		}else if(currentPlayer==2){
			foreach(Guy guy in guys){
				if(guy.Alive && guy.TileLocation==tile)
					return true;
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
		Debug.Log ("Player selected");
	}
	
	public void DeselectCharacter(){
		if(currentPlayer==1){ //spies
			foreach(Spy spy in spies)
				spy.Selected=false;
		}else if(currentPlayer==2){ //guys
			foreach(Guy guy in guys)
				guy.Selected=false;
		}	
		Debug.Log ("Players Deselected");
	}
	
	public void MoveSelectedCharTo(int x, int z){
		int depth  = map[x,z].Depth;
		if(currentPlayer==1){ //spies
			foreach(Spy spy in spies){
				if(spy.Selected && spy.CanMove (depth)){ 
					map[(int)spy.TileLocation.x,(int)spy.TileLocation.y].Open();
					spy.Move(x,z, depth);
					spy.Selected=false;
					map[(int)spy.TileLocation.x,(int)spy.TileLocation.y].Take();
				}
			}
		}else if(currentPlayer==2){ //guys
			foreach(Guy guy in guys){
				if(guy.Selected && guy.CanMove(depth)){ 
					map[(int)guy.TileLocation.x,(int)guy.TileLocation.y].Open();
					guy.Move(x,z, depth);
					guy.Selected=false;
					map[(int)guy.TileLocation.x,(int)guy.TileLocation.y].Take();
				}
			}
		}
	}
	
	public bool OpenTileAt(int x, int z){
		if(x==-1000 || z==-1000) return false;
		if(map[x,z].Type == (int)TileType.Open)	return true;
		return false;
	}
	
	public bool HighlightedTileAt(int x, int z){
		if(map[x,z].Highlight) return true;
		return false;
	}
	
	public bool VisibleTileAt(int x, int z){
		return map[x,z].Visible;	
	}
	
	public bool TileTakenByEnemy(int x, int z){
		if(currentPlayer==1){
			foreach(Guy guy in guys){
				if(guy.TileLocation.x==x && guy.TileLocation.y==z){ 
					return true;
				}
			}
		}else if(currentPlayer==2){
			foreach(Spy spy in spies){
				if(spy.TileLocation.x==x && spy.TileLocation.y==z){ 
					return true;
				}
			}
		}else
			Debug.Log ("error: currentPlayer in MapInfo set incorrectly (TileTakenByEnemy(int x,int z))");
		return false;
	}
	
	public List<int> MovesLeftForCurrentPlayer(){
		List<int> moves = new List<int>();
		if(currentPlayer==1){
			foreach(Spy spy in spies){
				if(spy.Alive) moves.Add(spy.MovesLeft);
				else moves.Add (-1);
			}
		}else if(currentPlayer==2){
			foreach(Guy guy in guys){
				if(guy.Alive) moves.Add(guy.MovesLeft);
				else moves.Add (-1);
			}
		}
		return moves;
	}
	
	public int MovesLeftForPlayer(int x, int z){
		if(currentPlayer==1){
			foreach(Spy spy in spies){
				if(spy.TileLocation.x==x && spy.TileLocation.y ==z)
					return spy.MovesLeft;
			}
		}else if(currentPlayer==2){
			foreach(Guy guy in guys){
				if(guy.TileLocation.x==x && guy.TileLocation.y ==z)
					return guy.MovesLeft;
			}
		}
		Debug.Log ("Error: MapInfo.MovesLeftForPlayer");
		return 0;
	}
	
	public void EliminatePlayerAt(int x, int z){
		bool kill=false;
		if(currentPlayer==1){
			foreach(Spy spy in spies){
				if(spy.Selected){
					if(spy.SpendAllPoints()) kill=true;	
				}
			}
			foreach(Guy guy in guys){
				if(guy.TileLocation.x==x && guy.TileLocation.y==z && kill){ 
					map[x,z].Open();
					guy.Die();
				}
			}
		}else if(currentPlayer==2){
			foreach(Guy guy in guys){
				if(guy.Selected){
					if(guy.SpendAllPoints()) kill=true;	
				}
			}
			foreach(Spy spy in spies){
				if(spy.TileLocation.x==x && spy.TileLocation.y==z && kill){ 
					map[x,z].Open ();
					spy.Die();
				}
			}
		}else
			Debug.Log ("error: currentPlayer in MapInfo set incorrectly (TileTakenByEnemy(int x,int z))");
	}
	
	public bool AllTeammatesDead(){
		if(currentPlayer==1){
			foreach(Spy spy in spies)
				if(spy.Alive) return false;
			winner = 2;
		}else if(currentPlayer==2){
			foreach(Guy guy in guys)
				if(guy.Alive) return false;
			winner = 1;
		}else{
			Debug.Log ("error: currentPlayer set incorrectly (AllTeammatesDead())");
		}
		return true;
	}
	
	public Tile TileAt(Vector2 v){
		return map[(int)v.x,(int)v.y];	
	}
	
	
	public List<Vector2> BFS(int x, int z, int maxDistance){
		Vector2 v = new Vector2(x,z);
		List<Vector2> V = new List<Vector2>();
		Queue q = new Queue();
		int depth = 0;
		q.Enqueue(v);
		V.Add(v);
		TileAt(v).Depth=depth;
		while(q.Count!=0){
			Vector2 t = (Vector2)q.Dequeue();
			depth = TileAt(t).Depth;
			if(depth>maxDistance){
				q.Clear();	
			}else{
				foreach(Vector2 tile in GetAdjacentOpenTiles((int)t.x,(int)t.y)){
					bool VContainsTile = (bool)V.Contains(tile);
					bool tileAtTileIsOpen = (bool)TileAt(tile).isOpen();
					if(tileAtTileIsOpen && !VContainsTile){
						q.Enqueue(tile);
						TileAt (tile).Depth = depth+1;
						TileAt(tile).PathPredecessor = t;
						if(depth+1<=maxDistance) V.Add (tile);
					}
				}
			}
		}
		return V;
	}
	
	/* BFS algorithm: */
	/* returns a List of tile coordinates, V
	 BFS(Vector2 v,int maxDistance)  //where v is the starting Vector2 
	 	int depth = 0
	 	queue Q
	 	List V
	 	Q.enqueue(v)
	 	V.Add(v)
	 	TileAt(v).Depth=depth
	 	while(Q is not empty)
	 		t=Q.dequeue
	 		depth = TileAt(t).Depth
	 		if(depth > maxDistance) 
	 			Q.Clear
	 		else
	 			foreach(Vector2 tile in GetAdjacentOpenTiles(t.x,t.z)
	 				if(tile is open and not already in V)
	 					Q.enqueue(tile)
	 					TileAt(tile).Depth = depth+1
	 					TileAt(tile).PathPredecessor = t
	 					if(depth+1<=maxDistance) V.Add(tile)
		return V
	 */ 
	
	//only returns OPEN adjacent tiles
	public List<Vector2> GetAdjacentOpenTiles(int x, int z){
		List<Vector2> adjTiles = new List<Vector2>();
		if(x>=mapSize || z>=mapSize || x<0 || z<0){
			Debug.Log ("Error: MapInfo.getAdjacentTiles");	
		}
		if(x-1>=0 && map[x-1,z].isOpen()) adjTiles.Add(new Vector2(x-1,z));
		if(z-1>=0 && map[x,z-1].isOpen()) adjTiles.Add(new Vector2(x,z-1));
		if(x+1<mapSize && map[x+1,z].isOpen()) adjTiles.Add(new Vector2(x+1,z));
		if(z+1<mapSize && map[x,z+1].isOpen()) adjTiles.Add(new Vector2(x,z+1));
		return adjTiles;
	}
	
	public void FoVForCurrentPlayer(int maxViewDist){
		RemoveVisibility();
		if(currentPlayer==1){
			foreach(Spy spy in spies){
				if(spy.Alive)
					FoV(spy.TileLocation,maxViewDist);	
			}
		}else if(currentPlayer==2){
			foreach(Guy guy in guys){
				if(guy.Alive)
					FoV (guy.TileLocation,maxViewDist);	
			}
		}else{
			Debug.Log ("Error: MapInfo.FoVForCurrentPlayer");	
		}
		FindAllVisibleTiles();
	}
	
	public void FoV(Vector2 playerLocation, int maxDistance){
		List<Vector2> edgeOfVisionTiles = ReturnAllMaxDistanceTiles((int)playerLocation.x,(int)playerLocation.y,maxDistance);
		foreach(Vector2 endpoint in edgeOfVisionTiles)
			ScanLine(playerLocation,endpoint);
	}

	public void ScanLine(Vector2 start, Vector2 end){
		Vector2 vect = end-start;
		float norm = Mathf.Sqrt((vect.x*vect.x) + (vect.y*vect.y));
		Vector2 unitVect = new Vector2(vect.x/norm,vect.y/norm);
		TileAt(start).Visible=true;
		//Debug.Log ("starting start = "+start.ToString());
		//Debug.Log ("end = "+end.ToString());
		Vector2 roundedLocation = new Vector2((int)start.x,(int)start.y);
		while(roundedLocation!=end){
			start+=unitVect;
			roundedLocation = new Vector2(Mathf.Round(start.x),Mathf.Round(start.y));
			//Debug.Log ("location = ["+start.x+","+start.y+"]");
			//Debug.Log ("rounded location = ["+roundedLocation.x+","+roundedLocation.y+"]");
			if(!TileAt(roundedLocation).Visible){
				TileAt(roundedLocation).Visible=true;
			}
			if(!TileAt(roundedLocation).isOpen()) return;
		}
	}
	
	public void ScanningLineTest(){
		Vector2 start = new Vector2(3,8);
		Vector2 end = new Vector2(0,0);
		Vector2 vect = end-start;
		double norm = Mathf.Sqrt((vect.x*vect.x) + (vect.y*vect.y));
		Debug.Log ("norm = "+norm);
		Vector2 unitVect = new Vector2((float)(vect.x/norm),(float)(vect.y/norm));
		Debug.Log ("vector = "+ vect.ToString());
		Debug.Log ("Unit vector = ["+unitVect.x+","+unitVect.y+"]");
		Vector2 roundedLocation = new Vector2((int)start.x,(int)start.y);
		while(roundedLocation!=end){
			start+=unitVect;
			roundedLocation = new Vector2((int)start.x,(int)start.y);
			Debug.Log ("location = ["+start.x+","+start.y+"]");
			Debug.Log ("rounded location = ["+(int)start.x+","+(int)start.y+"]");
		}
	}

	public List<Vector2> ReturnAllMaxDistanceTiles(int x, int z, int maxDistance){
		List<Vector2> maxDistTiles = new List<Vector2>();
		int leftMostX = x-maxDistance; int rightMostX = x+maxDistance;
		int topMostZ = z+maxDistance; int bottomMostZ = z-maxDistance;
		if(leftMostX<0) leftMostX=0;
		if(rightMostX>=mapSize) rightMostX=mapSize-1;
		if(bottomMostZ<0) bottomMostZ=0;
		if(topMostZ>=mapSize) topMostZ = mapSize-1;
		for(int i=leftMostX;i<=rightMostX;i++)
			for(int j=bottomMostZ;j<=topMostZ;j++)
				if(i==leftMostX || i==rightMostX || j==bottomMostZ || j==topMostZ)
					maxDistTiles.Add(new Vector2(i,j));
		//Debug.Log ("x="+x+", z="+z+", maxDistance="+maxDistance);
		//Debug.Log ("l,d,u,r: "+leftMostX+" "+ bottomMostZ + " " +topMostZ+" "+rightMostX);
		return maxDistTiles;
	}
}
