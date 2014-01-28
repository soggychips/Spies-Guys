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
	private int facingBias = (int)drctns.east;
	private List<List<Vector2>> Rooms;
	private int roomCount;
	private Tile[,] map;

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
						visited[x,z] = true; 
						map[x,z].Room = -1;
					}else if(z==0){								//bottom (first) row case. Tile must be a new room
						FleshOutRoom(x,z,roomCount);
					}else{ 										//switch to FoR
						if(TileInEstablishedRoom(x,z)){
							FleshOutRoom(x,z,GetRoomNumberOfSouthAdjacentTile(x,z));
						}else{
							roomCount++; //new room found, increase room count
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
	The direction is biased, meaning there is a goal direction (e.g. east). 
	This means when heading in the biased direction, don't look to the right. 
	Return when surrounded by visited tiles.
	*/ 
	public void FleshOutRoom(int x,int z,int roomNumber){
		//set facing
		facing = facingBias;
		//set initial tile to visited and assign room number
		currentTile = new Vector2(x,z);
		visited[(int)currentTile.x,(int)currentTile.y]=true;
		map[(int)currentTile.x,(int)currentTile.y].Room = roomNumber;
		Rooms[roomNumber].Add (currentTile);
		while(GetNextTile((int)currentTile.x,(int)currentTile.y).x!=-1000){
			currentTile = GetNextTile((int)currentTile.x,(int)currentTile.y);
			visited[(int)currentTile.x,(int)currentTile.y]=true;
			map[(int)currentTile.x,(int)currentTile.y].Room = roomNumber;
			Rooms[roomNumber].Add (currentTile);
		}
	}

	public Vector2 GetNextTile(int x, int z){
		Vector2 nextTileInCurrentFacing = new Vector2();
		switch(facing){
		case (int)drctns.east:
			nextTileInCurrentFacing = new Vector2(x+1,z);
			break;
		case (int)drctns.north:
			nextTileInCurrentFacing = new Vector2(x,z+1);
			break;
		case (int)drctns.west:
			nextTileInCurrentFacing = new Vector2(x-1,z);
			break;
		case (int)drctns.south:
			nextTileInCurrentFacing = new Vector2(x,z-1);
			break;
		}

		Vector2 tileToRight = TileToRight(x,z);
		if(SurroundedByVisitedTiles(x,z)){ //no more tiles
			return new Vector2(-1000,-1000);
		}else if(facing!=facingBias && !HasWallOrDoor(tileToRight) && !visited[(int)tileToRight.x,(int)tileToRight.y]){ //opening to the right
			TurnRight();
		}else if((nextTileInCurrentFacing.x>=size || nextTileInCurrentFacing.x<0 || nextTileInCurrentFacing.y>=size || nextTileInCurrentFacing.y<0) 
		         || HasWallOrDoor(nextTileInCurrentFacing) || visited[(int)nextTileInCurrentFacing.x,(int)nextTileInCurrentFacing.y]){ 

			TurnLeft();
		}
		switch(facing){
		case (int)drctns.east:
			return new Vector2(x+1,z);
			//break;
		case (int)drctns.north:
			return new Vector2(x,z+1);
			//break;
		case (int)drctns.west:
			return new Vector2(x-1,z);
			//break;
		case (int)drctns.south:
			return new Vector2(x,z-1);
			//break;
		}

		Debug.Log ("Error: RoomFinder.GetNextTile");
		return Vector2.up;
	}

	public bool HasWallOrDoor(Vector2 v){
		return (map[(int)v.x,(int)v.y].hasWall() || map[(int)v.x,(int)v.y].hasDoor());
	}

	public bool SurroundedByVisitedTiles(int x, int z){
		return (visited[x,z-1] && visited[x,z+1] && visited[x+1,z] && visited[x-1,z] );
	}

	//returns true if the tile is north of a visited tile that's NOT a wall
	public bool TileInEstablishedRoom(int x, int z){
		return (visited[x,z-1] && !(map[x,z-1].hasWall() || map[x,z-1].hasDoor()) );
	}

	public int GetRoomNumberOfSouthAdjacentTile(int x, int z){
		return map[x,z-1].Room;
	}

	public Vector2 TileToRight(int x, int z){
		switch(facing){
		case (int)drctns.east:
			return new Vector2(x,z-1);
			//break;
		case (int)drctns.north:
			return new Vector2(x+1,z);
			//break;
		case (int)drctns.west:
			return new Vector2(x,z-1);
			//break;
		case (int)drctns.south:
			return new Vector2(x-1,z);
			//break;
		}
		Debug.Log ("ERROR: RoomFinder.TileToRight");
		return new Vector2(-1000,-1000);
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
		}
	}



} 
