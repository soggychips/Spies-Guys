using UnityEngine;
using System.Collections;

public class Data {

	private bool taken;
	private bool uploaded;
	private Vector2 originalTileLocation;
	private Vector2 currentTileLocation;

	public bool Taken{
		get{return taken;}
	}

	public Vector2 OriginalTileLocation{
		get{return originalTileLocation;}
	}

	public Vector2 TileLocation{
		get{return currentTileLocation;}
	}

	public bool Uploaded{
		get{return uploaded;}
	}

	public Data(Vector2 location){
		taken = false;
		uploaded = false;
		originalTileLocation = location;
		currentTileLocation = originalTileLocation;
	}

	public Data(int x, int z){
		taken = false;
		uploaded = false;
		originalTileLocation = new Vector2(x,z);
		currentTileLocation = originalTileLocation;
	}

	public void Drop(Vector2 v){
		taken = false;
		currentTileLocation = v;
	}

	public void Take(){
		taken = true;
	}

	public void ResetPosition(){
		currentTileLocation = originalTileLocation;
	}

	public void Upload(){
		uploaded = true;
	}


}
