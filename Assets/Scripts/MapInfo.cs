using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapInfo{
	
	public static int spacing = 10;
	public int viewDistance = 4;
	
	private int mapSize = 50;
	
	private int winner;
	private Tile[,] map;
	private int[,] visibility; //used for visibility reference, set each entry to 0 or 1 only!
	private List<List<Vector2>> Rooms;
	private int[,] previouslySeenTiles;
	
	public List<Guy> guys;
	public List<Spy> spies;

	private DataCollection data;
	//private List<Vector2> lightswitchLocations;
	private List<Vector2> extractionPoints;

	public int Winner{
		get{return winner;}	
	}
	
	public int Spacing{
		get{return spacing;}	
	}
	
	public int GetTileType(int x, int z){
		return map[x,z].Type;	
	}

	public int GetWallType (int x, int z)
	{
		return map[x,z].WallType;
	}

	public int GetDoorFacing(int x, int z)
	{
		return map[x,z].DoorFacing;
	}
	
	public int MapSize{
		get{ return mapSize;}
		set{ mapSize = value;}
	}
	
	
	public Tile[,] Map{
		get{return map;}
		set{map=value;}
	}
	
	// Use this for initialization
	public MapInfo () {
		map = new Tile[mapSize,mapSize];
		visibility = new int[mapSize,mapSize];
		//lightswitchLocations = new List<Vector2>();
		extractionPoints = new List<Vector2>();
		data = new DataCollection();
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
		Debug.Log ("Loading level: SG_DATA...");

		//walls,doors,player tiles

		//outside walls
		GiveWallInRange(5,5,18,5);
		GiveWallInRange(22,5,37,5);
		GiveWallInRange(5,5,5,18);
		GiveWallInRange(5,20,5,28);
		GiveWallInRange(5,28,26,28);
		GiveWallInRange(28,28,37,28);
		GiveWallInRange(37,5,37,28);
		//walls arranged from SW to NE corners
		GiveWallInRange(10,5,10,10);
		GiveWallInRange(10,12,10,16);
		GiveWallInRange(10,18,10,24);
		GiveWallInRange(10,26,10,28);
		GiveWallInRange(10,13,11,13);
		GiveWallInRange(13,5,13,9);
		GiveWallInRange(13,9,14,9);
		GiveWallInRange(13,13,18,13);
		GiveWallInRange(14,13,14,18);
		GiveWallInRange(14,20,14,24);
		GiveWallInRange(14,24,19,24);
		GiveWallInRange(18,5,18,6);
		GiveWallInRange(16,9,22,9);
		GiveWallInRange(18,8,18,9);
		GiveWallInRange(20,13,25,13);
		GiveWallInRange(21,24,25,24);
		GiveWallInRange(22,5,22,9);
		GiveWallInRange(25,13,25,17);
		GiveWallInRange(25,19,25,24);
		GiveWallInRange(32,5,32,12);
		GiveWallInRange(32,14,32,24);
		GiveWallInRange(32,15,37,15);
		GiveWallInRange(32,26,32,28);


		//PLAYERS
		spies = new List<Spy>();
		//CreateSpy(6,18);
		//CreateSpy(6,20);
		CreateSpy(15,21);
		CreateSpy(16,17);

		guys = new List<Guy>();
		//CreateGuy(35,25);
		//CreateGuy(35,11);
		CreateGuy(24,17);
		CreateGuy(24,19);

		//OBJECTIVES
		CreateData(20,19);
		CreateExtractionPoint(20,5);
		CreateExtractionPoint(27,30);

		//LIGHTSWITCHES
		CreateLightswitch(9,23);
		CreateLightswitch(9,19);
		CreateLightswitch(18,23);
		CreateLightswitch(24,20);
		CreateLightswitch(21,14);
		CreateLightswitch(15,17);
		CreateLightswitch(33,23);
		CreateLightswitch(33,11);
		CreateLightswitch(15,6);

		CreateLockdownSwitch(6,15);
		CreateLockdownSwitch(36,11);

		//DOORS
		CreateDoor(5,19);
		CreateDoor(10,11);
		CreateDoor(10,25);
		CreateDoor(10,17);
		CreateDoor(12,13);
		CreateDoor(14,19);
		CreateDoor(15,9);
		CreateDoor(18,7);
		CreateDoor(19,13);
		CreateDoor(20,24);
		CreateDoor(25,18);
		CreateDoor(32,13);
		CreateDoor(32,25);
		CreateDoor(27,28);
		CreateDoor(41,4);

		RoomFinder rf = new RoomFinder(map);
		rf.FindRooms();
		Rooms = rf.ReturnRooms();
		//map = rf.ReturnMap();
		LightAllRooms();

		SetAllTilesVisible();
		Debug.Log("Level Loaded.");
	}

	public void SetUpTestCaseForRoomFinder(){

		RoomFinder rf = new RoomFinder(map);
		GiveWallInRange(0,0,20,0);
		GiveWallInRange(20,0,20,20);
		GiveWallInRange(0,0,0,20);
		GiveWallInRange(0,20,20,20);
		rf.FindRooms();
		//rf.FleshOutRoom(1,1,0);
	}

	public void FlipLightswitch(int roomNumber){
		if(roomNumber>=Rooms.Count) Debug.Log ("Error: MapInfo.FlipLightswitch");
		else{
			if(!TileAt (Rooms[roomNumber][0]).Lit){
			//Debug.Log ("Lighting Room "+roomNumber+": BEGIN");
				foreach(Vector2 tile in Rooms[roomNumber]){
					//Debug.Log (tile + " lit");
					LightTile (tile);
				}
			}else{
				foreach(Vector2 tile in Rooms[roomNumber]){
					//Debug.Log (tile + " lit");
					UnlightTile (tile);
				}
			}
			//Debug.Log ("Lighting Room "+roomNumber+": END");
		}
	}

	public void LightAllRooms(){
		for(int i=0;i<Rooms.Count;i++){
			LightRoom (i);
		}
	}

	public void LightRoom(int roomNumber){
		if(roomNumber>=Rooms.Count) Debug.Log ("Error: MapInfo.LightRoom");
		else{
			//Debug.Log ("Lighting Room "+roomNumber+": BEGIN");
			foreach(Vector2 tile in Rooms[roomNumber]){
				//Debug.Log (tile + " lit");
				LightTile (tile);
			}
			//Debug.Log ("Lighting Room "+roomNumber+": END");
		}
	}

	public void LightTile(Vector2 v){
		map[(int)v.x,(int)v.y].Lit = true;
	}

	public void UnlightTile(Vector2 v){
		TileAt (v).Lit = false;
	}

	public int ReturnRoomContainingTile(int x, int z){
		return map[x,z].Room;
	}

	public void CreateData(int x, int z){
		map[x,z].GiveData();
		//Data d = new Data(x,z);
		data.Add(new Data(x,z));
	}

	public void CreateExtractionPoint(int x, int z){
		extractionPoints.Add (new Vector2(x,z));
		map[x,z].MakeExtractionPoint();
	}

	public void CreateSpy(int x, int z){
		spies.Add(new Spy(x,z));
		map[x,z].Take();
	}

	public void CreateGuy(int x, int z){
		guys.Add(new Guy(x,z));
		map[x,z].Take ();
	}

	public void TakeData(Vector2 v){
		data.TakeData(data.DataAtTile(v));					//marks data object as taken 
		ReturnSelectedSpy().TakeData(data.DataAtTile(v)); 	//assigns data to appripriate spy
		//TileAt(v).StoreType(); 								//stores tile type as DATA
		TileAt(v).Open();									//sets tile type as open
	}

	public void DropDataHeldBySpyAt(Vector2 deadSpyLocation){
		foreach(Spy spy in spies){
			if(!spy.Alive && spy.TileLocation==deadSpyLocation && spy.HasData){
				foreach(Data d in data.Data){
					data.DropData(d,deadSpyLocation); 	//set data's new tileLocation and update data object to !taken
					spy.RemoveData(d); 					//remove data from spy's data cache
					TileAt(deadSpyLocation).GiveData(); //set the tile at the location of death to type Data
				}
				spy.FreeMove(Player.deadPlayerTile);
			}
		}
	}

	public void ResetDroppedDataAt(Vector2 droppedDataLocation){
		Debug.Log ("Map.ResetDroppedDataAt");
		foreach(Data d in data.Data){
			if(d.TileLocation == droppedDataLocation){
				TileAt (droppedDataLocation).LoadStoredType ();			//use this line for being adjacent to the data for reset
				//TileAt(d.TileLocation).StoreTypeAs((int)TileType.Open);	//sets the tile's saved type at the data location to Open (guy should be standing on it currently)
				if(TileAt(d.OriginalTileLocation).isTaken()) 						//{all												}
					TileAt(d.OriginalTileLocation).StoreTypeAs((int)TileType.Data);	//{of												}
				else 																//{this												}
					TileAt(d.OriginalTileLocation).GiveData();						//{sets the tile at the OG data location to Data	}
				d.ResetPosition();										//sets the data's current position to its original position
			}
		}
	}

	public bool TileContainsDroppedData(Vector2 tile){
		foreach(Data d in data.Data){
			if(d.TileLocation == tile && d.TileLocation!=d.OriginalTileLocation) return true;
		}
		Debug.Log ("Tile does not contain data");
		return false;
	}

	public bool MissingData(){
		foreach(Data d in data.Data){
			if(d.TileLocation!=d.OriginalTileLocation) return true;
		}
		return false;
	}

	public void CreateDoor (int x, int z)
	{
		int facing = GetAppropriateDoorFacing(x,z);
		map[x,z].GiveDoor(facing);
	}

	public void CreateLightswitch (int x, int z)
	{
		map[x,z].GiveLightswitch();
		//lightswitchLocations.Add(new Vector2(x,z));
	}

	public void CreateLockdownSwitch(int x, int z){
		map[x,z].GiveLockdownSwitch();
	}
	

	public void AssignGearToSpy(int spyIndex, int gearToGive){
		spies[spyIndex].Equip(gearToGive);
	}

	public void AssignGearToGuy(int guyIndex, int gearToGive){
		guys[guyIndex].Equip(gearToGive);
	}
	
	//creates a vertical or horizontal wall in the tiles [(x1,z1),(x2,z2)]
	//Note: (x1==x2 || z1==z2) must be true
	//wall coordinate arguments must be given in W->E or S->N for proper material assignment
	public void GiveWallInRange(int x1, int z1, int x2, int z2){
		int counter;
		/*
		 * Vertical Wall
		 */ 
		if(x1==x2){ 
			//Southmost piece of wall
			if(map[x1,z1].hasWall()){ 
				switch(map[x1,z1].WallType){
				case (int)WallTypes.E_Horizontal_End:
					map[x1,z1].GiveWall((int)WallTypes.SE_Corner);
					break;
				case (int)WallTypes.W_Horizontal_End:
					map[x1,z1].GiveWall((int)WallTypes.SW_Corner);
					break;
				default:
					map[x1,z1].GiveWall((int)WallTypes.S_T);
					break;
				}
			}else{
				map[x1,z1].GiveWall((int)WallTypes.S_Vertical_End);
			}
			//midpieces
			counter = z1+1;
			while(counter<z2){
				map[x1,counter].GiveWall((int)WallTypes.Vertical_Mid);
				counter+=1;
			}
			//northmost piece of wall
			if(map[x2,z2].hasWall()){
			   switch(map[x2,z2].WallType){
				case (int)WallTypes.E_Horizontal_End:
					map[x2,z2].GiveWall((int)WallTypes.NE_Corner);
					break;
				case (int)WallTypes.W_Horizontal_End:
					map[x2,z2].GiveWall((int)WallTypes.NW_Corner);
					break;
				default:
					map[x2,z2].GiveWall((int)WallTypes.N_T);
					break;
			   }
			}else{
				map[x2,z2].GiveWall((int)WallTypes.N_Vertical_End);
			}
		/*
		 * Horizontal Wall
		 */ 
		}else if(z1==z2){ 
			//Westmost piece of the wall
			if(map[x1,z1].hasWall()){ 
				switch(map[x1,z1].WallType){
				case (int)WallTypes.S_Vertical_End:
					map[x1,z1].GiveWall((int)WallTypes.SW_Corner);
					break;
				case (int)WallTypes.N_Vertical_End:
					map[x1,z1].GiveWall((int)WallTypes.NW_Corner);
					break;
				default:
					map[x1,z1].GiveWall((int)WallTypes.W_T);
					break;
				}
			}else{
				map[x1,z1].GiveWall((int)WallTypes.W_Horizontal_End);
			}
			//Mid pieces
			counter = x1+1;
			while(counter<x2){
				map[counter,z1].GiveWall((int)WallTypes.Horizontal_Mid);
				counter+=1;
			}
			//Eastmost piece of wall
			if(map[x2,z2].hasWall()){ 
				switch(map[x2,z2].WallType){
				case (int)WallTypes.S_Vertical_End:
					map[x2,z2].GiveWall((int)WallTypes.SE_Corner);
					break;
				case (int)WallTypes.N_Vertical_End:
					map[x2,z2].GiveWall((int)WallTypes.NE_Corner);
					break;
				default:
					map[x2,z2].GiveWall((int)WallTypes.E_T);
					break;
				}
			}else{
				map[x2,z2].GiveWall((int)WallTypes.E_Horizontal_End);
			}
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
				if(map[i,j].Type!=(int)TileType.Wall && !map[i,j].hasLightswitch()) //&& map[i,j].Type!=(int)TileType.Door_Closed
					map[i,j].Visible=false;
			}
		}
		//Debug.Log ("All Tiles Invisible");
	}

	public void ResetPoints ()
	{
		foreach(Spy spy in spies) spy.ResetPoints();
		foreach(Guy guy in guys) guy.ResetPoints();
	}
	
	public void ResetHighlights(){
		for(int i=0; i<mapSize; i++){
			for(int j=0; j<mapSize; j++){
				map[i,j].Highlighted=false;
			}
		}
		//Debug.Log ("All Tile Highlights Reset");
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
	
	

	public void FindVisibleTilesForPlayer(int currentPlayer){
		//for the spies:
		if(currentPlayer==(int)GameEngine.Players.One){
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
		}else if(currentPlayer==(int)GameEngine.Players.Two){
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

	public List<Vector2> ReturnRoom(int roomNumber){
		if(roomNumber>=Rooms.Count) Debug.Log ("ERROR: MapInfo.ReturnRoom");
		return Rooms[roomNumber];
	}

	public List<int> ReturnTeamHP(int currentPlayer){
		List<int> teamHP = new List<int>();
		switch(currentPlayer){
		case (int)GameEngine.Players.One:
			for(int i = 0;i<spies.Count;i++)
				teamHP.Add (spies[i].Health);
			break;
		case (int)GameEngine.Players.Two:
			for(int i = 0;i<guys.Count;i++)
				teamHP.Add(guys[i].Health);
			break;
		default:
			Debug.Log ("Error: MapInfo.ReturnTeamHP");
			break;
		}
		return teamHP;
	}

	public List<int> ReturnTeamAP(int currentPlayer){
		List<int> teamAP = new List<int>();
		switch(currentPlayer){
		case (int)GameEngine.Players.One:
			for(int i = 0;i<spies.Count;i++)
				teamAP.Add (spies[i].MovesLeft);
			break;
		case (int)GameEngine.Players.Two:
			for(int i = 0;i<guys.Count;i++)
				teamAP.Add(guys[i].MovesLeft);
			break;
		default:
			Debug.Log ("Error: MapInfo.ReturnTeamAP");
			break;
		}
		return teamAP;
	}

	public int ReturnSelectedPlayerIdx (int currentPlayer)
	{	//must use indexes to iterate through spies/guys instead of foreach
		switch(currentPlayer){
		case (int)GameEngine.Players.One:
			for(int i=0;i<spies.Count;i++)
				if(spies[i].Selected) return i;
			break;
		case (int)GameEngine.Players.Two:
			for(int i=0;i<guys.Count;i++)
				if(guys[i].Selected) return i;
			break;
		default:
			Debug.Log ("Error: MapInfo.ReturnSelectedPlayerIdx");
			break;
		}
		return -1;
	}

	public int ReturnNumberOfLivePlayersOnTeam(int currentPlayer){
		int answer = 0;
		switch(currentPlayer){
		case (int)GameEngine.Players.One:
			for(int i=0;i<spies.Count;i++)
				if(spies[i].Alive) answer++;
			break;
		case (int)GameEngine.Players.Two:
			for(int i=0;i<guys.Count;i++)
				if(guys[i].Alive) answer++;
			break;
		default:
			Debug.Log ("Error: MapInfo.ReturnSelectedPlayerIdx");
			break;
		}
		return answer;
	}
	

	public Vector2 ReturnPlayerPosition (int playerIdx, int currentPlayer)
	{
		switch(currentPlayer){
		case (int)GameEngine.Players.One:
			return spies[playerIdx].TileLocation;
		case (int)GameEngine.Players.Two:
			return guys[playerIdx].TileLocation;
		default:
			Debug.Log ("Error: MapInfo.ReturnSelectedPlayerPosition");
			return Vector2.zero;
		}
	}

	public List<Vector2> ReturnPlayerPositions(int currentPlayer){
		List<Vector2> positions = new List<Vector2>();
		if(currentPlayer == (int)GameEngine.Players.One){
			foreach(Spy spy in spies){
				positions.Add (spy.TileLocation);
			}
		}else if(currentPlayer == (int)GameEngine.Players.Two){
			foreach(Guy guy in guys){
				positions.Add (guy.TileLocation);
			}
		}else{
			Debug.Log ("Error: MapInfo.ReturnPlayerPositions");
		}
		return positions;
	}

	public Player ReturnSelectedPlayer(int currentPlayer){
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Spy spy in spies){
				if(spy.Selected) return spy;
			}
		}else{
			foreach(Guy guy in guys){
				if(guy.Selected) return guy;
			}
		}
		Debug.Log ("Error: MapInfo.ReturnSelectedPlayer");
		return new Player();
	}

	public Guy ReturnSelectedGuy(){
		foreach(Guy guy in guys){
			if(guy.Selected) return guy;
		}
		Debug.Log ("error! MapInfo: ReturnSelectedGuy();");
		return null;
	}

	public Spy ReturnSelectedSpy(){
		foreach(Spy spy in spies){
			if(spy.Selected) return spy;
		}
		Debug.Log ("error! MapInfo: ReturnSelectedSpy();");
		return null;
	}

	public Guy ReturnClickedOnGuy(int x, int z){
		Vector2 tile = new Vector2(x,z);
		foreach(Guy guy in guys){
			if(guy.Alive && guy.TileLocation==tile)
				return guy;
		}
		Debug.Log ("error! MapInfo: ReturnClickedOnGuy();");
		return null;
	}

	public Spy ReturnClickedOnSpy(int x, int z){
		Vector2 tile = new Vector2(x,z);
		foreach(Spy spy in spies){
			if(spy.Alive && spy.TileLocation==tile)
				return spy;
		}
		Debug.Log ("error! MapInfo: ReturnClickedOnSpy();");
		return null;
	}

	public bool TeamCanSeeTileAt(int x, int z, int currentPlayer){
		bool theyCan = false;
		switch(currentPlayer){
		case (int)GameEngine.Players.One:
			foreach(Spy spy in spies){
				if(spy.Alive){
					theyCan = true;
					Vector2 start = spy.TileLocation; Vector2 end = new Vector2(x,z);
					Vector2 vect = end-start;
					Vector2 check = start;
					float norm = Mathf.Sqrt((vect.x*vect.x) + (vect.y*vect.y));
					Vector2 unitVect = new Vector2(vect.x/norm,vect.y/norm);
					Vector2 roundedLocation = new Vector2((int)start.x,(int)start.y);
					while(roundedLocation!=end){
						check+=unitVect;
						roundedLocation = new Vector2(Mathf.Round(check.x),Mathf.Round(check.y));
						if(((roundedLocation!=start) && (roundedLocation!=end) && TileAt (roundedLocation).isBlocked()) || !TileAt(roundedLocation).Visible){
							//Debug.Log ("Blocked tile at "+roundedLocation);
							theyCan = false;
							break;
						}
					}
					if(theyCan) return true;
				}
			}
			break;
		case (int)GameEngine.Players.Two:
			foreach(Guy guy in guys){
				if(guy.Alive){
					theyCan = true;
					Vector2 start = guy.TileLocation; Vector2 end = new Vector2(x,z);
					Vector2 vect = end-start;
					Vector2 check = start;
					float norm = Mathf.Sqrt((vect.x*vect.x) + (vect.y*vect.y));
					Vector2 unitVect = new Vector2(vect.x/norm,vect.y/norm);
					Vector2 roundedLocation = new Vector2((int)start.x,(int)start.y);
					while(roundedLocation!=end){
						check+=unitVect;
						roundedLocation = new Vector2(Mathf.Round(check.x),Mathf.Round(check.y));
						if(((roundedLocation!=start) && (roundedLocation!=end) && TileAt (roundedLocation).isBlocked()) || !TileAt(roundedLocation).Visible){
							//Debug.Log ("Blocked tile at "+roundedLocation);
							theyCan = false;
						}
					}
					if(theyCan) return true;
				}
			}
			break;
		}
		return theyCan;
	}

	public bool CurrentPlayerAtTile(int x, int z, int currentPlayer){
		Vector2 tile = new Vector2(x,z);
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Spy spy in spies){
				if(spy.Alive && spy.TileLocation==tile)
					return true;
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Guy guy in guys){
				if(guy.Alive && guy.TileLocation==tile)
					return true;
			}
		}
		return false;
	}
	
	public void SelectCharacterAtTile(int x, int z, int currentPlayer){
		if(currentPlayer==(int)GameEngine.Players.One){ //spies
			if(spies[0].TileLocation.x==x && spies[0].TileLocation.y==z) spies[0].Selected=true;
			else if(spies[1].TileLocation.x==x && spies[1].TileLocation.y==z) spies[1].Selected=true;
		}else if(currentPlayer==(int)GameEngine.Players.Two){ //guys
			if(guys[0].TileLocation.x==x && guys[0].TileLocation.y==z) guys[0].Selected=true;
			else if(guys[1].TileLocation.x==x && guys[1].TileLocation.y==z) guys[1].Selected=true;
			else if(guys[2].TileLocation.x==x && guys[2].TileLocation.y==z) guys[2].Selected=true;
		}
		//Debug.Log ("Player selected");
	}
	
	public bool SelectedCharacterAtTile(int x, int z, int currentPlayer){
		Vector2 location = new Vector2(x,z);
		switch(currentPlayer){
		case (int)GameEngine.Players.One:
				foreach(Spy spy in spies)
					if(spy.Selected && spy.TileLocation==location) return true;
				break;
		case (int)GameEngine.Players.Two:
				foreach(Guy guy in guys)
					if(guy.Selected && guy.TileLocation==location) return true;
				break;
		}
		return false;
	}
	
	public void DeselectCharacter(int currentPlayer){
		if(currentPlayer==(int)GameEngine.Players.One){ //spies
			foreach(Spy spy in spies)
				spy.Selected=false;
		}else if(currentPlayer==(int)GameEngine.Players.Two){ //guys
			foreach(Guy guy in guys)
				guy.Selected=false;
		}	
		//Debug.Log ("Players Deselected");
	}

	public void OpenDoor(int x, int z){
		map[x,z].StoreType();
		map[x,z].OpenDoor();
	}

	public void CloseDoor(int x, int z){
		map[x,z].StoreType();
		map[x,z].CloseDoor();
	}

	public void LockDoor(int x, int z){
		map[x,z].StoreType();
		map[x,z].LockDoor();
	}

	public void Lockdown(){
		foreach(Tile tile in map){
				//Debug.Log ("a");
			if(tile.hasDoor()){
				tile.StoreType();
				tile.LockDoor();
			}

		}
	}

	public void Unlockdown(){
		foreach(Tile tile in map){
			if(tile.hasDoor()){
				tile.LoadStoredType();
			}
		}
	}
	

	public void RevertDoorOpening(int x, int z){
		map[x,z].LoadStoredType();
	}
	
	public void MoveSelectedCharTo(int x, int z, int currentPlayer){
		int depth  = map[x,z].Depth;
		if(currentPlayer==(int)GameEngine.Players.One){ //spies
			foreach(Spy spy in spies){
				if(spy.Selected){ 
					map[(int)spy.TileLocation.x,(int)spy.TileLocation.y].LoadStoredType();
					spy.Move(x,z,depth);
					map[(int)spy.TileLocation.x,(int)spy.TileLocation.y].StoreType();
					map[(int)spy.TileLocation.x,(int)spy.TileLocation.y].Take();
				}
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){ //guys
			foreach(Guy guy in guys){
				if(guy.Selected){ 
					map[(int)guy.TileLocation.x,(int)guy.TileLocation.y].LoadStoredType();
					guy.Move(x,z,depth);
					map[(int)guy.TileLocation.x,(int)guy.TileLocation.y].StoreType();
					map[(int)guy.TileLocation.x,(int)guy.TileLocation.y].Take();
				}
			}
		}
	}

	public void RevertMovement (int selectedPlayerIdx, Vector2 originalPosition, int currentPlayer)
	{
		switch(currentPlayer){
		case (int)GameEngine.Players.One:
			map[(int)spies[selectedPlayerIdx].TileLocation.x,(int)spies[selectedPlayerIdx].TileLocation.y].LoadStoredType();
			spies[selectedPlayerIdx].MoveBack((int)originalPosition.x,(int)originalPosition.y);
			map[(int)spies[selectedPlayerIdx].TileLocation.x,(int)spies[selectedPlayerIdx].TileLocation.y].StoreType();
			map[(int)spies[selectedPlayerIdx].TileLocation.x,(int)spies[selectedPlayerIdx].TileLocation.y].Take();
			break;
		case (int)GameEngine.Players.Two:
			map[(int)guys[selectedPlayerIdx].TileLocation.x,(int)guys[selectedPlayerIdx].TileLocation.y].LoadStoredType();
			guys[selectedPlayerIdx].MoveBack((int)originalPosition.x,(int)originalPosition.y);
			map[(int)guys[selectedPlayerIdx].TileLocation.x,(int)guys[selectedPlayerIdx].TileLocation.y].StoreType();
			map[(int)guys[selectedPlayerIdx].TileLocation.x,(int)guys[selectedPlayerIdx].TileLocation.y].Take();
			break;
		}
	}
	
	public bool OpenTileAt(int x, int z){
		if(x==-1000 || z==-1000) return false;
		if(map[x,z].Type == (int)TileType.Open)	return true;
		return false;
	}

	public bool BlockedTileAt(int x, int z){
		if(x==-1000 || z==-1000) return false;
		if(map[x,z].isBlocked())	return true;
		return false;
	}
	
	public bool HighlightedTileAt(int x, int z){
		if(x==-1000 || z==-1000) return false;
		if(map[x,z].Highlighted) return true;
		return false;
	}
	
	public bool VisibleTileAt(int x, int z){
		return map[x,z].Visible;	
	}
	
	public bool TileTakenByEnemy(int x, int z, int currentPlayer){
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Guy guy in guys){
				if(guy.TileLocation.x==x && guy.TileLocation.y==z){ 
					return true;
				}
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Spy spy in spies){
				if(spy.TileLocation.x==x && spy.TileLocation.y==z){ 
					return true;
				}
			}
		}else
			Debug.Log ("error: currentPlayer in MapInfo set incorrectly (TileTakenByEnemy(int x,int z))");
		return false;
	}

	public List<string> GearForCurrentPlayer(int currentPlayer){
		List<string> gear = new List<string>();
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Spy spy in spies){
				gear.Add (spy.GearEquipped_String());
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Guy guy in guys){
				gear.Add (guy.GearEquipped());
			}
		}else{
			Debug.Log ("Error: MapInfo.GearForCurrentPlayer");
		}
		return gear;
	}

	public List<int> HealthLeftForCurrentPlayer(int currentPlayer){
		List<int> health = new List<int>();
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Spy spy in spies){
				health.Add (spy.Health);
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Guy guy in guys){
				health.Add (guy.Health);
			}
		}else{
			Debug.Log ("Error: MapInfo.HealthLeftForCurrentPlayer");
		}
		return health;
	}
	
	public List<int> MovesLeftForCurrentPlayer(int currentPlayer){
		List<int> moves = new List<int>();
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Spy spy in spies){
				if(spy.Alive) moves.Add(spy.MovesLeft);
				else moves.Add (-1);
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Guy guy in guys){
				if(guy.Alive) moves.Add(guy.MovesLeft);
				else moves.Add (-1);
			}
		}else{
			Debug.Log ("MapInfo.MovesLeftForCurrentPlayer");
		}
		return moves;
	}
	
	public int MovesLeftForPlayer(int x, int z, int currentPlayer){
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Spy spy in spies){
				if(spy.TileLocation.x==x && spy.TileLocation.y ==z)
					return spy.MovesLeft;
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Guy guy in guys){
				if(guy.TileLocation.x==x && guy.TileLocation.y ==z)
					return guy.MovesLeft;
			}
		}
		Debug.Log ("Error: MapInfo.MovesLeftForPlayer");
		return 0;
	}
	
	public void EliminatePlayerAt(int x, int z, int currentPlayer){
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Guy guy in guys){
				if(guy.TileLocation.x==x && guy.TileLocation.y==z){ 
					map[x,z].LoadStoredType();
					//map[x,z].Open(); //change it to the previous stored tile type
				}
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Spy spy in spies){
				if(spy.TileLocation.x==x && spy.TileLocation.y==z){ 
					map[x,z].LoadStoredType();
					//map[x,z].Open ();
					if(spy.HasData) DropDataHeldBySpyAt(spy.TileLocation);

				}
			}
		}else
			Debug.Log ("error: currentPlayer in MapInfo set incorrectly (TileTakenByEnemy(int x,int z))");
	}
	
	public bool AllTeammatesDead(int currentPlayer){
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Spy spy in spies)
				if(spy.Alive) return false;
			winner = 2;
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Guy guy in guys)
				if(guy.Alive) return false;
			winner = 1;
		}else{
			Debug.Log ("error: currentPlayer set incorrectly (AllTeammatesDead())");
		}
		return true;
	}

	public bool AllDataExtracted(){
		if(data.AllDataIsUploaded()){
			winner = 1;
			return true;
		}
		return false;
	}

	public void AttemptExtraction(int currentPlayer){
		if(	currentPlayer==(int)GameEngine.Players.One 
		   && ReturnSelectedSpy().HasData 
		   && extractionPoints.Contains(ReturnSelectedSpy().TileLocation)	){
			ExtractDataFromSelectedSpy();
		}
	}
	

	public void ExtractDataFromSelectedSpy(){
		List<Data> stolenData = spies[spies.IndexOf(ReturnSelectedSpy())].StolenData;
		List<Data> s2 = new List<Data>();
		foreach(Data s in stolenData){
			data.Extract(s);
			s2.Add (s);
		}
		foreach(Data s in s2){
			ReturnSelectedSpy().RemoveData(s);
		}
	}

	public void AttemptLockdown(int currentPlayer){
		if(	currentPlayer==(int)GameEngine.Players.One 
		   && ReturnSelectedSpy().HasData 
		   && extractionPoints.Contains(ReturnSelectedSpy().TileLocation)	){
			ExtractDataFromSelectedSpy();
		}
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
				foreach(Vector2 tile in GetAdjacentUnblockedTiles((int)t.x,(int)t.y)){
					bool VContainsTile = (bool)V.Contains(tile);
					if(!VContainsTile){
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
	
	//returns unblocked adjacent tiles
	public List<Vector2> GetAdjacentUnblockedTiles(int x, int z){
		List<Vector2> adjTiles = new List<Vector2>();
		if(x>=mapSize || z>=mapSize || x<0 || z<0){
			Debug.Log ("Error: MapInfo.getAdjacentTiles");	
		}
		if(x-1>=0 && !map[x-1,z].isBlocked()) adjTiles.Add(new Vector2(x-1,z));
		if(z-1>=0 && !map[x,z-1].isBlocked()) adjTiles.Add(new Vector2(x,z-1));
		if(x+1<mapSize && !map[x+1,z].isBlocked()) adjTiles.Add(new Vector2(x+1,z));
		if(z+1<mapSize && !map[x,z+1].isBlocked()) adjTiles.Add(new Vector2(x,z+1));
		return adjTiles;
	}

	//returns location of an adjacent CLOSED (or locked) door
	public Vector2 GetAdjacentClosedDoorLocation(int x, int z){
		if(x>=mapSize || z>=mapSize || x<0 || z<0){
			Debug.Log ("Error: MapInfo.GetAdjacentClosedDoorLocation");	
		}
		if(x-1>=0 && (map[x-1,z].hasClosedDoor() || map[x-1,z].hasLockedDoor())) return new Vector2(x-1,z);
		if(z-1>=0 && (map[x,z-1].hasClosedDoor() || map[x,z-1].hasLockedDoor())) return new Vector2(x,z-1);
		if(x+1<mapSize && (map[x+1,z].hasClosedDoor() || map[x+1,z].hasLockedDoor())) return new Vector2(x+1,z);
		if(z+1<mapSize && (map[x,z+1].hasClosedDoor() || map[x,z+1].hasLockedDoor())) return new Vector2(x,z+1);
		//Debug.Log ("Spy at "+x+","+z+" not next to a door");
		return new Vector2(-1000,-1000);
	}

	public Vector2 GetAdjacentDataLocation(int x, int z){
		if(x>=mapSize || z>=mapSize || x<0 || z<0){
			Debug.Log ("Error: MapInfo.GetAdjacentDataLocation");	
		}
		if(x-1>=0 && map[x-1,z].hasData()) return new Vector2(x-1,z);
		if(z-1>=0 && map[x,z-1].hasData()) return new Vector2(x,z-1);
		if(x+1<mapSize && map[x+1,z].hasData()) return new Vector2(x+1,z);
		if(z+1<mapSize && map[x,z+1].hasData()) return new Vector2(x,z+1);
		//Debug.Log ("Spy at "+x+","+z+" not next to data");
		return new Vector2(-1000,-1000);
	}

	//returns location of an adjacent door
	public Vector2 GetAdjacentDoorLocation(int x, int z){
		if(x>=mapSize || z>=mapSize || x<0 || z<0){
			Debug.Log ("Error: MapInfo.GetAdjacentDoorLocation");
			return new Vector2(-1000,-1000);
		}
		if(x-1>=0 && map[x-1,z].hasDoor()) return new Vector2(x-1,z);
		if(z-1>=0 && map[x,z-1].hasDoor()) return new Vector2(x,z-1);
		if(x+1<mapSize && map[x+1,z].hasDoor()) return new Vector2(x+1,z);
		if(z+1<mapSize && map[x,z+1].hasDoor()) return new Vector2(x,z+1);
		//Debug.Log ("Spy at "+x+","+z+" not next to a door");
		return new Vector2(-1000,-1000);
	}

	public int GetAppropriateDoorFacing(int x, int z){
		if(x>=mapSize || z>=mapSize || x<0 || z<0){
			Debug.Log ("Error: MapInfo.GetAppropriateDoorFacing");	
		}
		if(x-1>=0 && (map[x-1,z].hasWall())) return (int)DoorFacings.NS;
		if(x+1<mapSize && (map[x+1,z].hasWall())) return (int)DoorFacings.NS;
		if(z-1>=0 && (map[x,z-1].hasWall())) return (int)DoorFacings.EW;
		if(z+1>=0 && (map[x,z+1].hasWall())) return (int)DoorFacings.EW;

		return -1000;
	}
	
	public void FoVForCurrentPlayer(int maxViewDist, int currentPlayer){
		RemoveVisibility();
		if(currentPlayer==(int)GameEngine.Players.One){
			foreach(Spy spy in spies){
				if(spy.Alive){
					if(GetAdjacentClosedDoorLocation((int)spy.TileLocation.x,(int)spy.TileLocation.y).x!=-1000)
						FoVForSpy_IgnoreDoor(spy.TileLocation,(int)maxViewDist/2,GetAdjacentClosedDoorLocation((int)spy.TileLocation.x,(int)spy.TileLocation.y));
					else
						FoV(spy.TileLocation,maxViewDist);
				}
			}
		}else if(currentPlayer==(int)GameEngine.Players.Two){
			foreach(Guy guy in guys){
				if(guy.Alive){
					FoVForGuy (guy.TileLocation,maxViewDist);	
				}
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

	public void ScanLine(Vector2 start, Vector2 end){ //used for Field of View Algorithms
		Vector2 vect = end-start;
		Vector2 check = start;
		float norm = Mathf.Sqrt((vect.x*vect.x) + (vect.y*vect.y));
		Vector2 unitVect = new Vector2(vect.x/norm,vect.y/norm);
		TileAt(start).Visible=true;
		//Debug.Log ("starting start = "+start.ToString());
		//Debug.Log ("end = "+end.ToString());
		Vector2 roundedLocation = new Vector2((int)start.x,(int)start.y);
		while(roundedLocation!=end){
			check+=unitVect;
			//start+=unitVect;
			//roundedLocation = new Vector2(Mathf.Round(start.x),Mathf.Round(start.y));
			roundedLocation = new Vector2(Mathf.Round(check.x),Mathf.Round(check.y));
			if(!TileAt(roundedLocation).Visible){
				TileAt(roundedLocation).Visible=true;
			}
			if(TileAt(roundedLocation).isBlocked()) return;
		}
	} 

	public void FoVForSpy_IgnoreDoor(Vector2 playerLocation, int maxViewDistance, Vector2 doorLocationToIgnore){
		//Debug.Log("Ignoring door at "+doorLocationToIgnore);
		List<Vector2> edgeOfVisionTiles = ReturnAllMaxDistanceTiles((int)playerLocation.x,(int)playerLocation.y,maxViewDistance);
		foreach(Vector2 endpoint in edgeOfVisionTiles){
			Vector2 start = playerLocation;
			Vector2 end = endpoint;
			//Debug.Log("Calculating vision from "+start+" to "+end);
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
				//if(roundedLocation==doorLocationToIgnore){
				//	Debug.Log ("roundedLoc=doorLoc");
				//}
				//Debug.Log ("location = ["+start.x+","+start.y+"]");
				//Debug.Log ("rounded location = ["+roundedLocation.x+","+roundedLocation.y+"]");
				if(!TileAt(roundedLocation).Visible){
					TileAt(roundedLocation).Visible=true;
				}
				if(TileAt(roundedLocation).isBlocked() && roundedLocation!=doorLocationToIgnore) break;
			}
		}
	}

	public void FoVForGuy(Vector2 playerLocation, int maxViewDistance){
		//Debug.Log("Ignoring door at "+doorLocationToIgnore);
		List<Vector2> edgeOfVisionTiles = ReturnAllMaxDistanceTiles((int)playerLocation.x,(int)playerLocation.y,maxViewDistance);
		foreach(Vector2 endpoint in edgeOfVisionTiles){
			int unlitCount = 0;
			Vector2 start = playerLocation;
			Vector2 end = endpoint;
			//Debug.Log("Calculating vision from "+start+" to "+end);
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
				//if(roundedLocation==doorLocationToIgnore){
				//	Debug.Log ("roundedLoc=doorLoc");
				//}
				//Debug.Log ("location = ["+start.x+","+start.y+"]");
				//Debug.Log ("rounded location = ["+roundedLocation.x+","+roundedLocation.y+"]");
				if(!TileAt(roundedLocation).Visible){
					TileAt(roundedLocation).Visible=true;
				}
				if(TileAt (roundedLocation).Lit==false){
					unlitCount++;
					//Debug.Log ("unlitCount = "+unlitCount);
				}
				if(TileAt(roundedLocation).isBlocked() || unlitCount>=3) break;
			}
		}
	}


	/*
	 * Used in FOV algorithms
	 *ReturnAllMaxDistnaceTiles returns all edge tiles of the square [(maxDistnace*2)*(maxDistnace*2)] surrounding (x,z)  
	 */
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
