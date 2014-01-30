using UnityEngine;
using System.Collections;

public class GUIController : MonoBehaviour {

	public float animationSpeed = 800;
	public bool confirmationButtonFlag;

	private bool playerHasConfirmedOrCancelled;
	private int confirmOrCancel; //set as 1 for confirm, 2 for cancel
	private float confirmationButtonLeft;
	private Vector2 confirmationButtonBoxLocation, confirmButtonLocation, cancelButtonLocation;
	private Vector2 camButtonMainLocation, camButtonP1Location, camButtonP2Location;

	public GUIStyle game_actionPointText_iPhone;
	public GUIStyle game_healthPointText_iPhone;
	public GUIStyle game_confSlider_base_iPhone;
	public GUIStyle game_confSlider_green_iPhone;
	public GUIStyle game_confSlider_red_iPhone;
	public GUIStyle game_camBtn_main_iPhone;
	public GUIStyle game_camBtn_p1_iPhone;
	public GUIStyle game_camBtn_p2_iPhone;
	public GUIStyle game_item_shotgun_iPhone;
	public GUIStyle game_item_m14_iPhone;


	void Awake(){
		confirmOrCancel = 0;
		ResetConfirmationButtonVariables();
		confirmationButtonBoxLocation = new Vector2(Screen.width-204,0);
		confirmButtonLocation = new Vector2(Screen.width-200,68);
		cancelButtonLocation = new Vector2(Screen.width-200,396);
		camButtonMainLocation = new Vector2(0, 200);
		camButtonP1Location = new Vector2(0, 316);
		camButtonP2Location = new Vector2(0, 460);
	}

	void OnGUI(){
		if(confirmationButtonFlag){
			ConfirmationButtons();
		}
		CameraButtons();
	} 


	public void ConfirmationButtons(){ //called in OnGUI
		if(confirmationButtonLeft<=confirmationButtonBoxLocation.x){ 								//box is in its final place. allow clicking by showing buttons

			if(GUI.Button(new Rect(confirmButtonLocation.x,confirmButtonLocation.y,200,244), "", game_confSlider_green_iPhone)) {
				playerHasConfirmedOrCancelled = true;
				confirmOrCancel = 1;
			}
			if(GUI.Button (new Rect(cancelButtonLocation.x,cancelButtonLocation.y,200,244),"", game_confSlider_red_iPhone)) {
				playerHasConfirmedOrCancelled = true;
				confirmOrCancel = 2;
			}
			GUI.Box (new Rect(confirmationButtonBoxLocation.x,confirmationButtonBoxLocation.y, 204,Screen.height),"", game_confSlider_base_iPhone);
		}else{																						//Animate box to location
			GUI.Box (new Rect(confirmationButtonLeft,confirmationButtonBoxLocation.y,204,Screen.height),"", game_confSlider_base_iPhone);  //Display texture containing the button images pasted on (unclickable)
			confirmationButtonLeft -= Time.deltaTime * animationSpeed;
		}
	}

	private void ResetConfirmationButtonVariables ()
	{
		playerHasConfirmedOrCancelled = false;
		confirmationButtonLeft = Screen.width;
		confirmationButtonFlag = false;
	}

	public int ConfirmationButtonPlayerInput(){ //called by TouchHandler.cs 
		if(playerHasConfirmedOrCancelled){
			ResetConfirmationButtonVariables();
			return confirmOrCancel;
		}
		return 0;
	}

	public void FlagConfirmationButtons(){ //called by TouchHandler.cs
		confirmationButtonFlag = true;
	}

	public void CameraButtons(){
		if(GUI.Button(new Rect(camButtonMainLocation.x,camButtonMainLocation.y,128,88),"", game_camBtn_main_iPhone)){
		}
		if(GUI.Button(new Rect(camButtonP1Location.x,camButtonP1Location.y,128,136),"", game_camBtn_p1_iPhone)){
		}
		GUI.Label(new Rect(camButtonP1Location.x+52,camButtonP1Location.y+8, 64,32),"AP: 3", game_actionPointText_iPhone);
		GUI.Label(new Rect(camButtonP1Location.x+52,camButtonP1Location.y+48, 64,32),"Health: 5", game_healthPointText_iPhone);
		GUI.Label(new Rect(camButtonP1Location.x+4,camButtonP1Location.y+90, 114, 26),"", game_item_shotgun_iPhone);
		if(GUI.Button(new Rect(camButtonP2Location.x,camButtonP2Location.y,128,136),"", game_camBtn_p2_iPhone)){
		}
		GUI.Label(new Rect(camButtonP2Location.x+52,camButtonP2Location.y+8, 64,32),"AP: 3", game_actionPointText_iPhone);
		GUI.Label(new Rect(camButtonP2Location.x+52,camButtonP2Location.y+48, 64,32),"Health: 5", game_healthPointText_iPhone);
		GUI.Label (new Rect(camButtonP2Location.x+4, camButtonP2Location.y+90, 114, 26),"", game_item_m14_iPhone);
	}


}
