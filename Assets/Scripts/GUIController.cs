﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GUIController : MonoBehaviour {

	public float animationSpeed = 800;
	public bool confirmationButtonFlag, doorActionsFlag, lightSwitchFlag, lockdownFlag, EMPFlag, attackFlag, endTurnFlag;

	private bool playerHasConfirmedOrCancelled, playerHasFlippedLightswitch;
	private int confirmOrCancel; //set as 1 for confirm, 2 for cancel
	private float confirmationButtonLeft;
	private Vector2 confirmationButtonBoxLocation, confirmButtonLocation, cancelButtonLocation;
	private float turnConftop;
	private Vector2 turnConfBoxLocation, turnConfButtonLocation, turnCancelButtonLocation;
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
	public GUIStyle game_turnConfBox_iPhone;
	public GUIStyle game_turnConfBtn_iPhone;
	public GUIStyle game_turnCancelBtn_iPhone;
	public GUIStyle game_camBtn_p1_iPhone;
	public GUIStyle game_camBtn_p2_iPhone;
	public GUIStyle game_camBtn_base_iPhone;
	public GUIStyle game_lightSwitch_iPhone;
	public GUIStyle game_lightSwitch_off_iPhone;
	public GUIStyle game_camBtn_offline_iPhone;
	public GUIStyle game_item_shotgun_iPhone;
	public GUIStyle game_item_m14_iPhone;

	private GUIStyle game_item_s1_iphone, game_item_s2_iphone, game_item_g1_iphone, game_item_g2_iphone;



	void Awake(){
		confirmOrCancel = 0;
		ResetConfirmationButtonVariables();
		confirmationButtonBoxLocation = new Vector2(Screen.width-204,0);
		confirmButtonLocation = new Vector2(Screen.width-200,68);
		cancelButtonLocation = new Vector2(Screen.width-200,396);
		turnConfBoxLocation = new Vector2(Screen.width/2 - 512, Screen.height/2 - 256);
		turnConfButtonLocation = new Vector2(Screen.width/2 + 200, Screen.height/2 +100); 
		turnCancelButtonLocation = new Vector2(Screen.width/2 - 200, Screen.height/2 +100); 
		camButtonP1Location = new Vector2(0, 316);
		camButtonP2Location = new Vector2(0, 460);
		lightSwitchLocation = new Vector2(220, Screen.height - 100);
		cameraController = GameObject.FindWithTag("MainCamera").GetComponent("CameraController") as CameraController;
		scene = GameObject.Find("Engine").GetComponent("GameEngine") as GameEngine; //gives us access to the GameEngine script
	}


	void OnGUI(){
		CameraButtons();
		if(confirmationButtonFlag){
			ConfirmationButtons();
		}
		if(lightSwitchFlag){
			LightSwitch();
		}
		if(endTurnFlag){
			EndTurnConfirmationButtons();
		}
	} 

	void Update(){
		if(lightSwitchFlag && scene.CurrentTurnState!=(int)TurnState.States.CharSelected) lightSwitchFlag = false;
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

	/* (Evan)
	 * CHANGE THE ANIMATION 
	 * IN THE 
	 * FOLLOWING METHOD:
	 */ 
	public void EndTurnConfirmationButtons(){ //called in OnGUI
		if(turnConftop<=turnConfButtonLocation.y){ //box is in its final place. allow clicking by showing buttons
			if(GUI.Button(new Rect(turnConfButtonLocation.x,turnConfButtonLocation.y,128,88), "", game_turnConfBtn_iPhone)) {
				playerHasConfirmedOrCancelled = true;
				confirmOrCancel = 1;
			}
			if(GUI.Button (new Rect(turnCancelButtonLocation.x,turnCancelButtonLocation.y,128,88),"", game_turnCancelBtn_iPhone)) {
				playerHasConfirmedOrCancelled = true;
				confirmOrCancel = 2;
			}
			GUI.Box (new Rect(turnConfBoxLocation.x,turnConfBoxLocation.y, 256,512),"", game_turnConfBox_iPhone);
		}else{ //Animate box to location
			GUI.Box (new Rect(turnConftop,turnConfBoxLocation.y,256,512),"", game_turnConfBox_iPhone);  //Display texture containing the button images pasted on (unclickable)
			turnConftop -= Time.deltaTime * animationSpeed;
		}
	}

	public void EstablishGadgetTextures(){
		List<int> spyGear = scene.ReturnGadgetsForTeam((int)GameEngine.Players.One);
		List<int> guyGear = scene.ReturnGadgetsForTeam((int)GameEngine.Players.Two);
		//assign gadget textures based on what's equipped


	}

	public void LightSwitch(){
		if(scene.IsSelectedPlayersRoomLit()){
			if(GUI.Button(new Rect(lightSwitchLocation.x, lightSwitchLocation.y, 138, 100),"", game_lightSwitch_off_iPhone)){
				playerHasFlippedLightswitch = true;
			}
		}else{
			if(GUI.Button(new Rect(lightSwitchLocation.x, lightSwitchLocation.y, 138, 100),"", game_lightSwitch_iPhone)){
				playerHasFlippedLightswitch = true;
			}
		}
	}

	public bool LightSwitched(){
		return playerHasFlippedLightswitch;
	}

	public void ResetLightswitch(){
		playerHasFlippedLightswitch = false;
		lightSwitchFlag = false;
	}

	private void ResetConfirmationButtonVariables ()
	{
		playerHasConfirmedOrCancelled = false;
		confirmationButtonLeft = Screen.width;
		confirmationButtonFlag = false;
		endTurnFlag = false;
		turnConftop = Screen.height;
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

	public void EndTurnFlag(){
		endTurnFlag = true;
	}

	public void FlagLightswitchButtons(){ //called by TouchHandler.cs
		lightSwitchFlag = true;
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
						ResetLightswitch();
						cameraController.FirstPlayerButtonPress();
						if(scene.LightswitchButtonsDisplayed) 
							FlagLightswitchButtons();
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
						ResetLightswitch();
						cameraController.SecondPlayerButtonPress();
						if(scene.LightswitchButtonsDisplayed) 
							FlagLightswitchButtons();
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





}
