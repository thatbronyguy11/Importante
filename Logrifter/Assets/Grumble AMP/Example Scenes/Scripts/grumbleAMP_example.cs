// Example Implementation For
// The Grumble Labs Adaptive Music Player

using UnityEngine;
using System.Collections;
using System.Globalization;

public class grumbleAMP_example : MonoBehaviour {
	
	
	private float globalCrossFadeTime = 3f;
	private string globalCrossFadeTimeString = "3";
	private float defaultGlobalCrossFadeTime = 3f;
	private float fadeInTime = 0f;
	private string fadeInTimeString = "0";
	private float fadeOutTime = 4f;
	private string fadeOutTimeString = "4";
	private float masterVolume = 0.5f;
	private string masterVolumeString = "5";
	private float defaultMasterVolume = 5f;
	private int activeSong;
	private int activeLayer;
	public Texture2D activeTexture;
	public Texture2D inactiveTexture;
	public Texture2D whiteTexture;
	enum PlayState {
		Stopped,
		FadingIn,
		FadingOut,
		Playing,
		Paused };
	private PlayState thePlayState = PlayState.Stopped;
	private float accumulatedDeltaTime;
	private float alarmAt;
	private bool alarmSet;
	public Font computerFont;
	public Texture2D boxTexture;
	public Texture2D logo;
	
	
	
	
	// Drag the Music Player Game Object into this
	// in the inspector.  Or do a Search for it.
	public grumbleAMP gA;
	
	// PUBLIC COMMANDS
	// All references to the instance of the
	// adaptive music player are shown here
	// in functions describing the event in
	// which they would be called.
	
	void hitNewLayerButton(int layerNumber) {
		gA.CrossFadeToNewLayer (layerNumber);
	}
	
	void hitNewSongButton(int songNumber, int layerNumber) {
		gA.CrossFadeToNewSong (songNumber,layerNumber);
	}
	
	void hitFadeIn() {
		gA.PlaySong (activeSong,activeLayer,fadeInTime);
	}
	
	void hitPlayWhileStopped() {
		gA.PlaySong (activeSong,activeLayer);
	}
	
	void hitPlayWhilePaused() {
		gA.UnPause();
	}
	
	void hitPause() {
		gA.Pause ();
	}
	
	void hitStop() {
		gA.StopAll ();
	}
	
	void hitFadeOut() {
		gA.StopAll(fadeOutTime);
	}
	
	
	// ACCESSORS
	
	void setGlobalCrossFadeTime() {
		gA.setGlobalCrossFadeTime(globalCrossFadeTime);
	}
	
	void setGlobalCrossFadeTime(float crossFadeTime) {
		globalCrossFadeTime = crossFadeTime;
		gA.setGlobalCrossFadeTime(globalCrossFadeTime);
	}
	
	void setGlobalVolume() {
		gA.setGlobalVolume(masterVolume);
	}
	
	void setGlobalVolume(float volume) {
		masterVolume = Mathf.Clamp (volume,0f,1f);
		gA.setGlobalVolume (masterVolume);
	}
	
	int getNumberOfSongs() {
		return gA.songs.Length;
	}
	
	int getNumberOfLayersOfSong(int songNumber) {
		return gA.songs[songNumber].layer.Length;
	}
	
	float getLoopCrossFadeTime(int songNumber) {
		if (songNumber < gA.songs.Length && songNumber >= 0) {
			return gA.songs[songNumber].getLoopCrossfadeBy();
		}
		else {
			return 0f;
		}
	}
	
	bool setLoopCrossFadeTime(int songNumber, float newCrossFadeTime) {
		bool failure = false;
		if (songNumber < gA.songs.Length && songNumber >= 0) {
			gA.songs[songNumber].setLoopCrossfadeBy(newCrossFadeTime);
		}
		else {
			failure = true;
		}
		return failure;
	}
	
	float getLayerCrossFadeTime(int songNumber) {
		if (songNumber < gA.songs.Length && songNumber >= 0) {
			return gA.songs[songNumber].getLayerCrossfadeBy();
		}
		else {
			return 0f;
		}
	}
	
	bool setLayerCrossFadeTime(int songNumber, float newCrossFadeTime) {
		bool failure = false;
		if (songNumber < gA.songs.Length && songNumber >= 0) {
			gA.songs[songNumber].setLayerCrossfadeBy(newCrossFadeTime);
		}
		else {
			failure = true;
		}
		return failure;
	}
	
	
	
	
	
	
	
	
	
	
	
	// The rest of this is just to make the
	// GUI and handle the state logic.
	// It is not a good example of anything.
	
	
	// Use this for initialization
	void Start () {
		Screen.SetResolution(731,411,false);
		setGlobalCrossFadeTime();
		setGlobalVolume ();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
		
		accumulatedDeltaTime += Time.deltaTime;
		if (alarmSet && accumulatedDeltaTime >= alarmAt) {
			alarmSet = false;
			switch (thePlayState) {
				
			case PlayState.FadingIn:
				thePlayState = PlayState.Playing;
				break;
				
			case PlayState.FadingOut:
				thePlayState = PlayState.Stopped;
				break;
				
			}
		}
	}
	
	
	// This is not a great example of how to write
	// a GUI.  Get inspiration elsewhere.
	void OnGUI() {
		
		bool downReturn = false;
		Event currentEvent = Event.current;
		if (currentEvent.keyCode == KeyCode.Return) {
			downReturn = true;
		}
		
		// Song Layer Buttons
		
		float buttonWidth = Screen.width / 14f;
		float buttonHeight = Screen.height / 14f;
		float widthBetweenButtons = Screen.width / 12f;
		float firstButtonLeft = Screen.width / 5f;
		float heightBetweenButtons = Screen.height / 8f;
		float firstButtonTop = Screen.height / 10f;
		
		for (int i=0; i < getNumberOfSongs (); i++) {
			for (int j=0; j < getNumberOfLayersOfSong (i); j++) {
				GUIStyle buttonStyle = new GUIStyle();
				buttonStyle.alignment = TextAnchor.MiddleCenter;
				buttonStyle.font = computerFont;
				buttonStyle.fontSize = 12;
				if (activeSong == i && activeLayer == j) {
					buttonStyle.normal.background = activeTexture;
				}
				else {
					buttonStyle.normal.background = inactiveTexture;
				}
				if (GUI.Button(new Rect(firstButtonLeft+widthBetweenButtons*j, firstButtonTop+heightBetweenButtons*i, buttonWidth, buttonHeight), j.ToString (),buttonStyle)) {
					if (thePlayState == PlayState.Playing || thePlayState == PlayState.FadingIn || thePlayState == PlayState.FadingOut) {
						if (activeSong != i) {
							hitNewSongButton (i,j);
						}
						else {
							hitNewLayerButton (j);
						}
					}
					activeSong = i;
					activeLayer = j;
				}
			}
		}
		
		
		// Transport Controls
		
		float firstTransportButtonTop = Screen.height/1.15f;
		float firstButtonLeftOffset = Screen.width/8f;
		
		GUIStyle fadeInButtonStyle = new GUIStyle();
		GUIStyle playButtonStyle = new GUIStyle();
		GUIStyle pauseButtonStyle = new GUIStyle();
		GUIStyle stopButtonStyle = new GUIStyle();
		GUIStyle fadeOutButtonStyle = new GUIStyle();
		
		fadeInButtonStyle.font = computerFont;
		playButtonStyle.font = computerFont;
		pauseButtonStyle.font = computerFont;
		stopButtonStyle.font = computerFont;
		fadeOutButtonStyle.font = computerFont;
		
		fadeInButtonStyle.alignment = TextAnchor.MiddleCenter;
		playButtonStyle.alignment = TextAnchor.MiddleCenter;
		pauseButtonStyle.alignment = TextAnchor.MiddleCenter;
		stopButtonStyle.alignment = TextAnchor.MiddleCenter;
		fadeOutButtonStyle.alignment = TextAnchor.MiddleCenter;
		
		fadeInButtonStyle.normal.background = inactiveTexture;
		playButtonStyle.normal.background = inactiveTexture;
		pauseButtonStyle.normal.background = inactiveTexture;
		stopButtonStyle.normal.background = inactiveTexture;
		fadeOutButtonStyle.normal.background = inactiveTexture;
		
		fadeInButtonStyle.fontSize = 10;
		playButtonStyle.fontSize = 10;
		pauseButtonStyle.fontSize = 10;
		stopButtonStyle.fontSize = 10;
		fadeOutButtonStyle.fontSize = 10;
		
		if (thePlayState == PlayState.FadingIn) {
			fadeInButtonStyle.normal.background = activeTexture;
		}
		
		if (thePlayState == PlayState.Playing) {
			playButtonStyle.normal.background = activeTexture;
		}
		
		if (thePlayState == PlayState.Paused) {
			pauseButtonStyle.normal.background = activeTexture;
		}
		
		if (thePlayState == PlayState.Stopped) {
			stopButtonStyle.normal.background = activeTexture;
		}
		
		if (thePlayState == PlayState.FadingOut) {
			fadeOutButtonStyle.normal.background = activeTexture;
		}
		
		if (GUI.Button(new Rect(firstButtonLeft-firstButtonLeftOffset+widthBetweenButtons*0, firstTransportButtonTop, buttonWidth, buttonHeight), "FADE\nIN",fadeInButtonStyle)) {
			thePlayState = PlayState.FadingIn;
			hitFadeIn ();
			alarmSet = true;
			alarmAt = accumulatedDeltaTime + fadeInTime;
		}
		
		if (GUI.Button(new Rect(firstButtonLeft-firstButtonLeftOffset+widthBetweenButtons*1, firstTransportButtonTop, buttonWidth, buttonHeight), "PLAY",playButtonStyle)) {
			if (thePlayState == PlayState.Stopped) {
				thePlayState = PlayState.Playing;
				hitPlayWhileStopped();
			}
			else if (thePlayState == PlayState.Paused) {
				hitPlayWhilePaused();
				thePlayState = PlayState.Playing;
			}
		}
		
		if (GUI.Button(new Rect(firstButtonLeft-firstButtonLeftOffset+widthBetweenButtons*2, firstTransportButtonTop, buttonWidth, buttonHeight), "PAUSE",pauseButtonStyle)) {
			thePlayState = PlayState.Paused;
			hitPause ();
		}
		
		if (GUI.Button(new Rect(firstButtonLeft-firstButtonLeftOffset+widthBetweenButtons*3, firstTransportButtonTop, buttonWidth, buttonHeight), "STOP",stopButtonStyle)) {
			thePlayState = PlayState.Stopped;
			hitStop ();
		}
		
		if (GUI.Button(new Rect(firstButtonLeft-firstButtonLeftOffset+widthBetweenButtons*4, firstTransportButtonTop, buttonWidth, buttonHeight), "FADE\nOUT",fadeOutButtonStyle)) {
			thePlayState = PlayState.FadingOut;
			alarmSet = true;
			alarmAt = accumulatedDeltaTime + fadeOutTime;
			hitFadeOut ();
		}
		
		
		// Text Labels
		
		GUIStyle textFieldStyle = new GUIStyle();
		GUIStyle labelStyle = new GUIStyle();
		GUIStyle titleStyle = new GUIStyle();
		GUIStyle borderStyle = new GUIStyle();
		textFieldStyle.font = computerFont;
		labelStyle.font = computerFont;
		titleStyle.font = computerFont;
		labelStyle.fontSize = 10;
		textFieldStyle.fontSize = 10;
		titleStyle.fontSize = 13;
		labelStyle.alignment = TextAnchor.MiddleCenter;
		textFieldStyle.alignment = TextAnchor.MiddleCenter;
		titleStyle.alignment = TextAnchor.MiddleCenter;
		textFieldStyle.normal.background = activeTexture;
		textFieldStyle.focused.background = whiteTexture;
		borderStyle.normal.background = boxTexture;
		GUI.skin.settings.cursorColor = Color.black;
		float layerLabelWidth = 100f;
		float layerLabelHeight = 10f;
		float borderSize = 50f;
		GUI.Label(new Rect(Screen.width/4f,Screen.height/20f,layerLabelWidth,layerLabelHeight),"LAYERS",labelStyle);
		GUI.Label(new Rect(Screen.width/12f,Screen.height/3.3f,layerLabelWidth,layerLabelHeight),"SONGS",labelStyle);
		
		
		// Settings
		
		// Global Crossfade Time
		GUI.Label (new Rect(Screen.width/1.25f,Screen.height/20f,layerLabelWidth,layerLabelHeight),"SONG CROSSFADE\nTIME OVERRIDE",labelStyle);
		globalCrossFadeTimeString = GUI.TextField (new Rect(Screen.width/1.20f,Screen.height/20f+20f,50f,15f),globalCrossFadeTimeString,textFieldStyle);
		if (globalCrossFadeTimeString == "" && downReturn) {
			globalCrossFadeTime = 0f;
			setGlobalCrossFadeTime ();
			globalCrossFadeTimeString = "0";
			GUI.FocusControl("empty");
		}
		else if (downReturn){
			if (float.TryParse(globalCrossFadeTimeString,NumberStyles.Any,CultureInfo.InvariantCulture.NumberFormat,out globalCrossFadeTime)) {
				setGlobalCrossFadeTime ();
				GUI.FocusControl("empty");
			}
			else {
				globalCrossFadeTime = defaultGlobalCrossFadeTime;
				setGlobalCrossFadeTime ();
				globalCrossFadeTimeString = defaultGlobalCrossFadeTime.ToString ();
				GUI.FocusControl("empty");
			}
		}
		
		// Fade In Time
		GUI.Label (new Rect(Screen.width/1.25f,Screen.height/20f+borderSize,layerLabelWidth,layerLabelHeight),"FADE IN TIME",labelStyle);
		fadeInTimeString = GUI.TextField (new Rect(Screen.width/1.20f,Screen.height/20f+20f+borderSize,50f,15f),fadeInTimeString,textFieldStyle);
		if (fadeInTimeString == "" && downReturn) {
			fadeInTime = 0f;
			fadeInTimeString = "0";
			GUI.FocusControl("empty");
		}
		else if (downReturn){
			float enteredNumber;
			if (float.TryParse(fadeInTimeString,NumberStyles.Any,CultureInfo.InvariantCulture.NumberFormat,out enteredNumber)) {
				if (enteredNumber < 0f) {
					enteredNumber = 0f;
				}
				fadeInTime = enteredNumber;
				GUI.FocusControl("empty");
			}
			else {
				fadeInTime = 0f;
				fadeInTimeString = "0";
				GUI.FocusControl("empty");
			}
		}
		
		
		// Fade Out Time
		GUI.Label (new Rect(Screen.width/1.25f,Screen.height/20f+borderSize*2f,layerLabelWidth,layerLabelHeight),"FADE OUT TIME",labelStyle);
		fadeOutTimeString = GUI.TextField (new Rect(Screen.width/1.20f,Screen.height/20f+20f+borderSize*2f,50f,15f),fadeOutTimeString,textFieldStyle);
		if (fadeOutTimeString == "" && downReturn) {
			fadeOutTime = 0f;
			fadeOutTimeString = "0";
			GUI.FocusControl("empty");
		}
		else if (downReturn){
			float enteredNumber;
			if (float.TryParse(fadeOutTimeString,NumberStyles.Any,CultureInfo.InvariantCulture.NumberFormat,out enteredNumber)) {
				if (enteredNumber < 0f) {
					enteredNumber = 0f;
				}
				fadeOutTime = enteredNumber;
				GUI.FocusControl("empty");
			}
			else {
				fadeOutTime = 0f;
				fadeOutTimeString = "0";
				GUI.FocusControl("empty");
			}
		}
		
		
		// Master Volume
		GUI.Label (new Rect(Screen.width/1.25f,Screen.height/20f+borderSize*3f,layerLabelWidth,layerLabelHeight),"MASTER VOLUME",labelStyle);
		masterVolumeString = GUI.TextField (new Rect(Screen.width/1.20f,Screen.height/20f+20f+borderSize*3f,50f,15f),masterVolumeString,textFieldStyle);
		if (masterVolumeString == "" && downReturn) {
			masterVolume = 0f;
			masterVolumeString = "0";
			GUI.FocusControl("empty");
		}
		else if (downReturn){
			float enteredNumber;
			if (float.TryParse(masterVolumeString,NumberStyles.Any,CultureInfo.InvariantCulture.NumberFormat,out enteredNumber)) {
				if (enteredNumber > 10f) {
					enteredNumber = 10f;
					masterVolumeString = "10";
				}
				else if (enteredNumber < 0f) {
					enteredNumber = 0f;
					masterVolumeString = "0";
				}
				masterVolume = Mathf.Clamp (enteredNumber / 10f,0f,1f);
				setGlobalVolume();
				GUI.FocusControl("empty");
			}
			else {
				masterVolume = defaultMasterVolume;
				masterVolumeString = (defaultMasterVolume*10).ToString ();
				GUI.FocusControl("empty");
			}
		}
		
		
		// Logo Text
		GUI.Label (new Rect(Screen.width/1.6f,(Screen.height/20f)*16.7f,layerLabelWidth*2f,layerLabelHeight),"GRUMBLE LABS\nADAPTIVE MUSIC PLAYER\nIMPLEMENTATION EXAMPLE",titleStyle);
		GUI.Label (new Rect(Screen.width/1.6f,(Screen.height/20f)*18.4f,layerLabelWidth*2f,layerLabelHeight),"www.grumblelabs.com",labelStyle);
		
		
		// Border
		GUI.Label (new Rect(0f,0f,Screen.width,Screen.height)," ",borderStyle);
		
		// Logo
		GUI.DrawTexture (new Rect(Screen.width/1.4f,Screen.height/1.7f,80f,80f),logo);
	}
}