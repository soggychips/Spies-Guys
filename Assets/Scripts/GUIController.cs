using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIController : MonoBehaviour {

	public float animationSpeed = 800;
	public bool confirmationButtonFlag, doorActionsFlag, lightSwitchFlag, lockdownFlag, EMPFlag, attackFlag;

	private bool playerHasConfirmedOrCancelled;
	private int confirmOrCancel; //set as 1 for confirm, 2 for cancel
	private float confirmationButtonLeft;
	private Vector2 confirmationButtonBoxLocation, confirmButtonLocation, cancelButtonLocation;
	private Vector2 camButtonP1Location, camButtonP2Location;
	private Vector2 lightSwitchLocation;
	private CameraController cameraController;
	private GameEngine scene;
	private List<int> teamHP, teamAP;

	public GUIStyle game_actionPointText_iPhone;
	public GUIStyle game_healthPointText_iPhone;
	public GUIStyle game_confSlider_base_iPhone;
	public GUIStyle game_confSlider_green_iPhone;
	public GUIStyle game_confSlider_red_iPhone;
	public GUIStyle game_camBtn_p1_iPhone;
	public GUIStyle game_camBtn_p2_iPhone;
	public GUIStyle game_camBtn_base_iPhone;
	public GUIStyle game_lightSwitch_iPhone;
	public GUIStyle game_camBtn_offline_iPhone;
	public GUIStyle game_item_shotgun_iPhone;
	public GUIStyle game_item_m14_iPhone;



	void Awake(){
		confirmOrCancel = 0;
		ResetConfirmationButtonVariables();
		confirmationButtonBoxLocation = new Vector2(Screen.width-204,0);
		confirmButtonLocation = new Vector2(Screen.width-200,68);
		cancelButtonLocation = new Vector2(Screen.width-200,396);
		camButtonP1Location = new Vector2(0, 316);
		camButtonP2Location = new Vector2(0, 460);
		lightSwitchLocation = new Vector2(220, Screen.height - 100);
		cameraController = GameObject.FindWithTag("MainCamera").GetComponent("CameraController") as CameraController;
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine; //gives us access to the GameEngine script
	}


	void OnGUI(){
		if(confirmationButtonFlag){
			ConfirmationButtons();
		}
		CameraButtons();
		if(lightSwitchFlag){
			LightSwitch();
		}
	} 


	public void ConfirmationButtons(){ //called in OnGUI
		if(confirmationButtonLeft<=confirmationButtonBoxLocation.x){ //box is in its final place. allow clicking by showing buttons
			if(GUI.Button(new Rect(confirmButtonLocation.x,confirmButtonLocation.y,200,244), "", game_confSlider_green_iPhone)) {
				playerHasConfirmedOrCancelled = true;
				confirmOrCancel = 1;
			}
			if(GUI.Button (new Rect(cancelButtonLocation.x,cancelButtonLocation.y,200,244),"", game_confSlider_red_iPhone)) {
				playerHasConfirmedOrCancelled = true;
				confirmOrCancel = 2;
			}
			GUI.Box (new Rect(confirmationButtonBoxLocation.x,confirmationButtonBoxLocation.y, 204,Screen.height),"", game_confSlider_base_iPhone);
		}else{ //Animate box to location
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
		if(scene.CurrentGameState==(int)GameState.States.P1 || scene.CurrentGameState==(int)GameState.States.P2){
			if(scene.CurrentTurnState!=(int)TurnState.States.Begin){
				teamHP = scene.ReturnTeamHP();
				teamAP = scene.ReturnTeamAP();

				//p1 cam button
				if(teamHP[0]>0){
					GUI.Label(new Rect(camButtonP1Location.x,camButtonP1Location.y, 128, 136),"", game_camBtn_base_iPhone);
					if(GUI.Button(new Rect(camButtonP1Location.x,camButtonP1Location.y+4,122,76),"", game_camBtn_p1_iPhone)){
							cameraController.FirstPlayerButtonPress();
					}
					GUI.Label(new Rect(camButtonP1Location.x+52,camButtonP1Location.y+8, 64, 32),"AP: "+teamAP[0], game_actionPointText_iPhone);
					GUI.Label(new Rect(camButtonP1Location.x+52,camButtonP1Location.y+48, 64, 32),"Health: "+teamHP[0], game_healthPointText_iPhone);
					GUI.Label(new Rect(camButtonP1Location.x+4,camButtonP1Location.y+90, 114, 26),"", game_item_shotgun_iPhone);
				}else{
					GUI.Label(new Rect(camButtonP1Location.x,camButtonP1Location.y, 128, 136),"", game_camBtn_offline_iPhone);
				}

				//p2 cam button
				if(teamHP[1]>0){
					GUI.Label(new Rect(camButtonP2Location.x,camButtonP2Location.y, 128, 136),"", game_camBtn_base_iPhone);
					if(GUI.Button(new Rect(camButtonP2Location.x,camButtonP2Location.y+4, 122, 76),"", game_camBtn_p2_iPhone)){
							cameraController.SecondPlayerButtonPress();
					}
					GUI.Label(new Rect(camButtonP2Location.x+52,camButtonP2Location.y+8, 64,32),"AP: "+teamAP[1], game_actionPointText_iPhone);
					GUI.Label(new Rect(camButtonP2Location.x+52,camButtonP2Location.y+48, 64,32),"Health: "+teamHP[1], game_healthPointText_iPhone);
					GUI.Label (new Rect(camButtonP2Location.x+4, camButtonP2Location.y+90, 114, 26),"", game_item_m14_iPhone);
				}else{
					GUI.Label(new Rect(camButtonP2Location.x,camButtonP2Location.y, 128, 136),"", game_camBtn_offline_iPhone);
				}
			}
		}
	}

	public void LightSwitch(){
		if(GUI.Button(new Rect(lightSwitchLocation.x, lightSwitchLocation.y, 138, 100),"", game_lightSwitch_iPhone)){
		}
	}



}
