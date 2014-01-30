using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*

 	/* 
    Find Rooms: 
	Visit tiles incrementally, starting at [0,0] to [mapSize,mapSize] switching to FleshOutRoom mode when an unvisited (non wall/door) tile has been found. 
	When a wall or door is found, mark it as visited, and assign its room to -1, and continue without calling FleshOutRoom. 
	Repeat until all tile locations have been visited (assigned to a room or were found to be walls). 
	It's worth noting that the only tiles you will find if incrementing through X values for each z value (in ascending order) are 
		going to be the left-most bottom most tiles of their respective rooms, 
		OR the left-most bottom missed tiles from the initial Flesh Out Room call for that respective room (due to choke points).
	=>If an unvisited tile has a wall to the south, it's a new room.
	=>If an unvisited tile has a visited tile (NOT a wall) to the south, it's in an already established room.
	*/

 	/* 
    Flesh Out Room: 
	Move counter clockwise as outward as possible, spiraling in, always turning right if an unvisited tile is found, and only moving onto a tile if it hasn't been visited. 
	A facing is used to determine the next tile to visit, and in which direction to be looking for openings. 
	When it finds an opening (always looking to the right), it turns and continues in that direction. 
	If there's no opening to the right, it moves forward. 
	If it can't move forward, it turns left and continues. 
	The direction is biased, meaning there is a goal direction (e.g. south, east). 
	This means when heading in the biased direction, don't look to the right. 
	Return when surrounded by visited tiles.
	*/ 

public class RoomFinder {

	private enum drctns{north,east,south,west};

	private int size;
	private bool[,] visited;
	private int facing;
	private int facingBias = (int)drctns.south;
	private List<List<Vector2>> Rooms;
	private int roomCount;
	private Tile[,] map;
	private Vector2 oob = new Vector2(-1000,-1000); //out of bounds

	private Vector2 currentTile;

	public int RoomCount{
		get{return roomCount;}
	}

	public RoomFinder(Tile[,] mapgrid){
		map = mapgrid;
		size = map.GetLength(0);
		visited = new bool[size,size];
		for(int a=0;a<size;a++)
			for(int b=0;b<size;b++)
				visited[a,b]=false;
		facing = facingBias;
		Rooms  = new List<List<Vector2>>();
		roomCount = 0;
	}

	public List<List<Vector2>> ReturnRooms(){
		return Rooms;
	}

	public Tile[,] ReturnMap(){
		return map;
	}

	/* 
    Find Rooms: 
	Visit tiles incrementally, starting at [0,0] to [mapSize,mapSize] switching to FleshOutRoom mode when an unvisited (non wall/door) tile has been found. 
	When a wall or door is found, mark it as visited, and assign its room to -1, and continue without calling FleshOutRoom. 
	Repeat until all tile locations have been visited (assigned to a room or were found to be walls). 
	It's worth noting that the only tiles you will find if incrementing through X values for each z value (in ascending order) are 
		going to be the left-most bottom most tiles of their respective rooms, 
		OR the left-most bottom missed tiles from the initial Flesh Out Room call for that respective room (due to choke points).
	=>If an unvisited tile has a wall to the south, it's a new room.
	=>If an unvisited tile has a visited tile (NOT a wall) to the south, it's in an already established room.
	*/
	public void FindRooms(){
		for(int z=0;z<size;z++){
			for(int x=0;x<size;x++){
				if(!visited[x,z]){
					if(map[x,z].hasWall() || map[x,z].hasDoor()){ //wall/door case
						//Debug.Log ("wall/door case");
						visited[x,z] = true; 
						map[x,z].Room = -1;
					}else if(z==0){								//bottom (first) row case. Tile must be a new room
						//Debug.Log ("First row case. roomCount = "+roomCount);
						Rooms.Add (new List<Vector2>());
						FleshOutRoom(x,z,roomCount);
					}else{ 										//switch to FoR
						if(TileInEstablishedRoom(x,z)){
							//Debug.Log ("continuing established room case ");
							FleshOutRoom(x,z,GetRoomNumberOfSouthAdjacentTile(x,z));
						}else{
							//Debug.Log("new room case");
							roomCount++; //new room found, increase room count
							Rooms.Add (new List<Vector2>());
							FleshOutRoom(x,z,roomCount);
						}
					}
				}
			}
		}
	}


	/* 
    Flesh Out Room (FoR): 
	Move counter clockwise as outward as possible, spiraling in, always turning right if an unvisited tile is found, and only moving onto a tile if it hasn't been visited. 
	A facing is used to determine the next tile to visit, and in which direction to be looking for openings. 
	When it finds an opening (always looking to the right), it turns and continues in that direction. 
	If there's no opening to the right, it moves forward. 
	If it can't move forward, it turns left and continues. 
	The direction bias is the initial movement direction.
	Return when surrounded by visited tiles.
	*/ 
	public void FleshOutRoom(int x,int z,int roomNumber){
		//Debug.Log ("FleshOutRoom "+roomNumber);

		//set facing
		facing = facingBias;

		//set initial tile to visited and assign room number
		currentTile = new Vector2(x,z);
		visited[(int)currentTile.x,(int)currentTile.y]=true;
		map[(int)currentTile.x,(int)currentTile.y].Room = roomNumber;
		Rooms[roomNumber].Add (currentTile);
		//Debug.Log ("Added "+currentTile + "to room "+roomNumber);
		Vector2 nextTile = GetNextTile((int)currentTile.x,(int)currentTile.y);
		while(nextTile!=oob){
			currentTile = nextTile;
			//Debug.Log ("currentTile = "+currentTile);
			visited[(int)currentTile.x,(int)currentTile.y]=true;
			map[(int)currentTile.x,(int)currentTile.y].Room = roomNumber;
			Rooms[roomNumber].Add (currentTile);
			//Debug.Log ("Added "+currentTile + "to room "+roomNumber);
			nextTile = GetNextTile((int)currentTile.x,(int)currentTile.y);

		}
	}

	/*GetNextTile(int x, int z)
	 * Steps:
	 * 1. Get the next tile given the current facing, the tile to the left, and the tile to the right. Assigned to oob if out of bounds.
	 * 2. Check if the current tile is surrounded by visited tiles. If it is, return oob (ending condition in FoR).
	 * 3. If not 2. and there's an unvisited, lightable tile to the right, turn right.
	 * 4. If not (2. or 3.) and the next tile is out of bounds, has a wall, or is visited, and there's an unvisited, lightable tile to the left, turn left.
	 * 5. Return the next tile given the NEW current facing, if it's unvisited. Otherwise return oob.
	 */ 
	public Vector2 GetNextTile(int x, int z){
		//Debug.Log ("GetNextTile("+x+","+z+")");
		//Debug.Log ("facing: "+facing);
		Vector2 nextTileInCurrentFacing = NextTileInCurrentDirection(x,z);
		Vector2 tileToRight = TileToRight(x,z);
		Vector2 tileToLeft = TileToLeft(x,z);
		//Debug.Log ("nextTileInFacing"+nextTileInCurrentFacing);
		//Debug.Log ("tileToRight = "+tileToRight);

		if(SurroundedByVisitedTiles(x,z)){ //no more tiles
			return oob;
		}else if(!HasWallOrDoor(tileToRight) && !visited[(int)tileToRight.x,(int)tileToRight.y]){ //opening to the right
			//Debug.Log ("Turn Right");
			TurnRight();
		}else if( nextTileInCurrentFacing==oob || HasWallOrDoor(nextTileInCurrentFacing) || visited[(int)nextTileInCurrentFacing.x,(int)nextTileInCurrentFacing.y]){ //next tile not availabe
			if(!HasWallOrDoor(tileToLeft) && !visited[(int)tileToLeft.x,(int)tileToLeft.y]){ //opening to the left
				//Debug.Log ("Turn left");
				TurnLeft();
			}
		}
		nextTileInCurrentFacing = NextTileInCurrentDirection(x,z);
		if(visited[(int)nextTileInCurrentFacing.x,(int)nextTileInCurrentFacing.y]) return oob;
		return nextTileInCurrentFacing;
	}

	public Vector2 NextTileInCurrentDirection(int x, int z){
		switch(facing){
		case (int)drctns.east:
			if(x+1>=size) return oob;
			return new Vector2(x+1,z);
			//break;
		case (int)drctns.north:
			if(z+1>=size) return oob;
			return new Vector2(x,z+1);
			//break;
		case (int)drctns.west:
			if(x-1<0) return oob;
			return new Vector2(x-1,z);
			//break;
		case (int)drctns.south:
			if(z-1<0) return oob;
			return new Vector2(x,z-1);
			//break;
		}
		Debug.Log ("Error: RoomFinder.NextTileInCurrentDirection");
		return oob;
	}

	public Vector2 TileToRight(int x, int z){
		
		switch(facing){
		case (int)drctns.east:
			if(z-1<0) return oob;
			return new Vector2(x,z-1);
			//break;
		case (int)drctns.north:
			if(x+1>=size) return oob;
			return new Vector2(x+1,z);
			//break;
		case (int)drctns.west:
			if(z+1>=size) return oob;
			return new Vector2(x,z+1);
			//break;
		case (int)drctns.south:
			if(x-1<0) return oob;
			return new Vector2(x-1,z);
			//break;
		}
		Debug.Log ("ERROR: RoomFinder.TileToRight");
		return oob;
	}

	public Vector2 TileToLeft(int x, int z){
		
		switch(facing){
		case (int)drctns.west:
			if(z-1<0) return oob;
			return new Vector2(x,z-1);
			//break;
		case (int)drctns.south:
			if(x+1>=size) return oob;
			return new Vector2(x+1,z);
			//break;
		case (int)drctns.east:
			if(z+1>=size) return oob;
			return new Vector2(x,z+1);
			//break;
		case (int)drctns.north:
			if(x-1<0) return oob;
			return new Vector2(x-1,z);
			//break;
		}
		Debug.Log ("ERROR: RoomFinder.TileToLeft");
		return oob;
	}

	public bool HasWallOrDoor(Vector2 v){
		if(v.x==-1000) return true;
		return (map[(int)v.x,(int)v.y].hasWall() || map[(int)v.x,(int)v.y].hasDoor());
	}

	public bool HasWallOrDoor(int x, int z){
		if(x==-1000) return true;
		return (map[x,z].hasWall() || map[x,z].hasDoor());
	}

	public bool SurroundedByVisitedTiles(int x, int z){
		if(z-1>=0) //out of bounds checks
			if(!visited[x,z-1] && !HasWallOrDoor(x,z)) return false;
		if(z+1<size)
			if(!visited[x,z+1] && !HasWallOrDoor(x,z)) return false;
		if(x+1<size)
			if(!visited[x+1,z] && !HasWallOrDoor(x,z)) return false;
		if(x-1>=0)
			if(!visited[x-1,z] && !HasWallOrDoor(x,z)) return false;
		return true;
	}

	//returns true if the tile is north of a visited tile that's NOT a wall
	public bool TileInEstablishedRoom(int x, int z){
		return (visited[x,z-1] && !(map[x,z-1].hasWall() || map[x,z-1].hasDoor()) );
	}

	public int GetRoomNumberOfSouthAdjacentTile(int x, int z){
		return map[x,z-1].Room;
	}



	public void TurnLeft(){
		switch(facing){
		case (int)drctns.east:
			facing = (int)drctns.north;
			break;
		case (int)drctns.north:
			facing = (int)drctns.west;
			break;
		case (int)drctns.west:
			facing = (int)drctns.south;
			break;
		case (int)drctns.south:
			facing = (int)drctns.east;
			break;
		}
	}

	public void TurnRight(){
		switch(facing){
		case (int)drctns.north:
			facing = (int)drctns.east;
			break;
		case (int)drctns.west:
			facing = (int)drctns.north;
			break;
		case (int)drctns.south:
			facing = (int)drctns.west;
			break;
		case (int)drctns.east:
			facing = (int)drctns.south;
			break;
		}
	}



} 
