using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * One instance of DataCollection should exist in MapInfo. Use this object to assign data to spies, check if all the data is uploaded, and to access each data object
 * in the map.
 * 
 * DataCollection holds all data that is still existing in physical form in the map (not yet uploaded to a remote server by a spy). This is used instead of individual data
 * objects in the mapinfo.
 * 
 */
public class DataCollection  {

	private List<Data> data;

	public List<Data> Data{
		get{return data;}
	}

	public DataCollection(){
		data = new List<Data>();
	}

	public void Add(Data d){
		data.Add(d);
	}


	public void DropData(Data d, Vector2 dropLocation){
		data[data.IndexOf(d)].Drop(dropLocation);
		//d.Drop(dropLocation);
	}


	public Data DataAtTile(Vector2 v){
		foreach(Data d in data){
			if((d.TileLocation == v)){
				return d;
			}
		}
		Debug.Log ("Error: DataCollection.DataAtTile");
		return new Data(-1000,-1000);
	}

	//use the output of DataAtTile(Vector2 v) as the input argument 
	public void TakeData(Data d){
		//data.Find(d).Take();
		data[data.IndexOf(d)].Take();
	}

	public void UploadData(Vector2 v){
		foreach(Data d in data){
			if((d.Taken)){
				d.Upload();
				data.Remove(d);
			}
		}
	}

	public bool AllDataIsUploaded(){
		return (data.Count == 0);
	}

}
