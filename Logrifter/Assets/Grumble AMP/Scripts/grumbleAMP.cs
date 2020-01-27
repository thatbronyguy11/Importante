// Grumble Labs Adaptive Music Player
// An simple solution for crossfading between synchronized audio tracks.
// Version 1.0 - December 8, 2015

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class grumbleAMP : MonoBehaviour {

	// Array of "song" class, which contains all the audio 
	// files and clip specific information.
	public grumbleSong[] songs;

	// A value of 2 will allow you to crossfade a song 
	// when it loops.
	private int multiplyLayersBy = 2;

	// A modulus that says which of the identical 
	// multiples is the active one.
	private int activeMultiple = 0;

	// Total time since the first Update has run, but can 
	// be paused, so that fades will continue after pause 
	// is released.  Not used for song looping.  (That
	// uses the audio player .time and .clip.length)
	private float accumulatedDeltaTime;

	// Total time since the first Update has run.
	// Unpausable.
	private float accumulatedDeltaTimeForPause;

	// The total time marker where the fade or crossfade began
	private float startedFadeAt;

	// The length of the fade, according to the song class.
	private float currentFadeLength;

	// The song numbers currently in each player.
	private int[] currentSongForPlayer;

	// The song layer numbers currently in each player.
	private int[] currentLayerForPlayer;

	// The player currently flagged as "switching to" or active.
	private int switchingToPlayer;

	// Indicates a crossfade is taking place.  Looping
	// or otherwise.
	private bool crossfadeActive = false;

	// Indicates a looping crossfade is taking place.
	// If a song needs to loop during a crossfade, it ignores
	// the initial crossfade and just performs the looping
	// crossfade.
	private bool loopCrossFadeActive = false;

	// Indicates a fadeout is active.
	private bool fadeoutActive = false;

	// Indicates a fadein is active.
	private bool fadeinActive = false;

	// Value between 0 and 1 that indicates the position of 
	// the crossfade between players.
	// Values slides between 0 (off) and 1 (on full).
	private float[] preVolumeFadeFactor;

	// All volumes get multiplied by this on their way out.
	public float globalVolume = 0.5f;

	// When unspecified, song crossfades will be this long.
	public float globalCrossFadeTime = 3f;

	// Used by the looping detector to signify that Play 
	// is currently enabled (to enable negative loop 
	// crossover times (pauses).
	private bool running = false;

	// Used for automated pause times.
	private float unPauseAt;

	// Indicates pause is down.
	private bool paused = false;

	// Indicates an automated pause is active.
	private bool pausedTimer = false;

	// All the players
	public AudioSource[] players;

	// When in resource request mode, these are the requests.
	private ResourceRequest[] theLayerRequests;

	// Indicates whether or not resource request mode is active.
	public bool resourceRequestMode = false;

	// True if the resources are not yet loaded.
	private bool needToCheckResourceStatus = false;

	// Progress, between 0 and 1
	private float totalResourceProgress;

	// If resources have not been loaded and a play song command
	// is issued, it will queue up here.
	private int songThatNeedsToPlayWhenLoaded = -1;

	// Indicates all the player-related arrays 
	// have not yet been populated
	private bool arraysPopulated = false;

	// Amount of time to add to the length of looping crossfades
	// in anticipation of latency.
	private float defaultExpectedLatency = 0.04f;






	////////////////////////////////////////
	/// STANDARD UNITY FUNCTIONS
	////////////////////////////////////////


	void Awake () {
		// Creates "multiplyLayersBy" number of
		// players per audio clip.
		populatePlayers ();

		// If we are using the resource request mode to
		// load songs in the background
		if (resourceRequestMode) {

			// Populate all the various players-related arrays
			// The argument of false indicates to not add the
			// clips, because they still need to be loaded.
			populateArrays (false);

			// Creates a resource request for each clip,
			// which is loaded by string/name.
			populateResourceRequests ();
		}

		// If we're not using resource request mode
		else {

			// Populate all the various players-related arrays
			// The argument of true indicates to add the
			// clips, because they have already been loaded.
			populateArrays (true);

			// Hits Play and then Pause on each player.
			// I don't know if this actually encourages Unity
			// to prioritize the latency on these clips or not
			// (as it's imperative to start each layer at exactly
			// the same time) but without empirical testing, it
			// seemed like layers were out of phase less often.
			loadResources();
		}
	}

	void Start () {
	
	}

	void FixedUpdate () {

		// If we're in resource request mode and resources
		// are not yet loaded, check them.
		if (resourceRequestMode && needToCheckResourceStatus) {
			handleCheckResourceStatus();
		}

		// If we're in resource request mode and resources
		// have been loaded, make sure to populate all the arrays.
		// This just happens once.
		else if (resourceRequestMode && !arraysPopulated) {
			populateArrays (true);
			arraysPopulated = true;
		}

		// Increments the (fixed) delta time, since we're calling
		// this from FixedUpdate and not Update.
		handleAccumulatedDeltaTime();

		// If we're not looping a song and the song ends, set
		// bool running to false.
		handleRunningStatus();

		// Handles the fade-out on clips
		handleFadeout();

		// Handles the fade-in on clips
		handleFadeIn();

		// Handles crossfades, either between songs, layers,
		// or looping crossfades.
		handleCrossfade();

		// Detects whether or not it's time to play the clips
		// in the inactive multiple of players for a loop with
		// potential slight overlap for smoothness.
		handleLoopDetection();

		// Handles automated unpausing.
		handleUnPause();

		// If a song gets queued (ordered to play before
		// resources are loaded) this will play the song once
		// resources are available.
		handleSongScheduling();
	}



	////////////////////////////////////////
	/// PUBLIC TRANSPORT FUNCTIONS
	////////////////////////////////////////


	// Default Play function.  Assumes no intro fade
	// unless you include three arguments.
	// Returns true if something goes wrong.

	public bool PlaySong (int songNumber, int layerNumber, float introFadeLength = 0f) {
		bool failure = false;

		// Is the request valid?
		if (songAndLayerValid(songNumber,layerNumber)) {
			
			// Set the current song and layer.  Fails if it can't find that layer.
			failure = setCurrentSong (songNumber,layerNumber) || failure;

			// If an intro fade was requested.
			if (introFadeLength != 0f) {
				if (crossfadeActive) {
					setAllPlayerVolumesTo(0f);
				}
				fadeinActive = true;
				fadeoutActive = false;
				crossfadeActive = false;
				startedFadeAt = accumulatedDeltaTime;
				currentFadeLength = introFadeLength;
			}

			// No intro fade requested.
			else {
				fadeinActive = false;
				fadeoutActive = false;
				crossfadeActive = false;
			}

			// This is not a crossfade function, so mute everything
			// and then either perform a fade in on the desired layer
			// or set that layer to full volume (multiplied by the
			// global volume in that function.
			setAllPlayerVolumesTo(0f);

			// If we're fading in
			if (introFadeLength != 0f) {

				// Set the volume of the active layer to 0 because
				// we're fading it in.
				// This is unnecessary because we already set
				// everything to zero two lines above, but this
				// could allow for starting a fade in at a volume
				// other than zero.
				failure = setPlayerVolumeToViaPlayer (switchingToPlayer,0f) || failure;
			}
			
			else {

				// Set the volume of the active layer to 1 (multiplied
				// by the global volume in the function) because we're
				// not fading in.
				failure = setPlayerVolumeToViaPlayer (switchingToPlayer,1f) || failure;
			}

			// If nothing went wrong, set our active multiple to 0
			// for cleanliness/sanity's sake.
			if (!failure) {
				activeMultiple = 0;
			}

			// Hit play on all the players that have layers of this song.
			failure = playAllLayersOfSong(songNumber) || failure;

			// If nothing went wrong, set running to active.
			if (!failure) {
				running = true;
			}
		}

		// Song or Layer is not valid.
		else {
			failure = true;
			Debug.LogError ("Tried To Play A Song/Layer That Doesn't Exist");
		}

		// If we're in a paused state when you order Play,
		// reset pause state.
		if (paused) {
			paused = false;
		}
		
		return failure;
	}


	// Play overloaded function that allows for
	// song to be referenced by string comparison.
	// Returns the number of the song if it finds it.
	// Returns -1 if it can't find a song by that name.
	public int PlaySong (string songName, int layerNumber, float introFadeLength = 0f) {
		int songNumber = searchForSongName(songName);

		if (songAndLayerValid(songNumber,layerNumber)) {
			if (PlaySong (songNumber,layerNumber,introFadeLength)) {
				songNumber = -1;
			}
		}
		return songNumber;
	}

	// Play overloaded function that allows for
	// song and layer to be referenced by string comparison.
	// Returns the number of the song if it finds it.
	// Returns -1 if it can't find a song by that name.
	public int PlaySong (string songName, string layerName, float introFadeLength = 0f) {
		int songNumber = searchForSongName(songName);
		int layerNumber = searchForLayerName(layerName,songNumber);

		if (songAndLayerValid (songNumber,layerNumber)) {
			if (PlaySong (songNumber,layerNumber,introFadeLength)) {
				songNumber = -1;
			}
		}

		else {
			songNumber = -1;
		}
		return songNumber;
	}


	// Play overloaded function that allows for
	// layer to be referenced by string comparison.
	// Returns the songNumber you submit, unless it
	// can't find a string with that layer name, and then
	// returns a -1.
	public int PlaySong (int songNumber, string layerName, float introFadeLength = 0f) {
		int layerNumber = -1;

		if (songValid (songNumber)) {
			layerNumber = searchForLayerName (layerName,songNumber);
		}
		
		if (songAndLayerValid(songNumber,layerNumber)) {
			if (PlaySong (songNumber,layerNumber,introFadeLength)) {
				songNumber = -1;
			}
		}
		
		else {
			songNumber = -1;
		}
		return songNumber;
	}

	
	// Pause function.  Also pauses fades.
	public bool Pause () {
		bool failure = true;
		for (int playerNumber=0; playerNumber < players.Length; playerNumber++) {
			if (players[playerNumber].isPlaying) {
				failure = false;
			}
			players[playerNumber].Pause ();
			paused = true;
		}
		return failure;
	}
	

	// Pauses for a preset amount of time.
	public bool Pause (float time) {
		bool failure = Pause ();
		if (time > 0f) {
			unPauseAt = accumulatedDeltaTimeForPause + time;
			paused = true;
			pausedTimer = true;
		}
		return failure;
	}

	// Unpauses either type of pause.
	public void UnPause () {
		for (int playerNumber=0; playerNumber < players.Length; playerNumber++) {
			players[playerNumber].UnPause ();
		}
		paused = false;
		pausedTimer = false;
	}







	// Stops playing songs and cancels pauses
	public void StopAll (float fadeOutTime = 0f) {
		if (fadeOutTime == 0f || paused) {
			fadeoutActive = false;
			running = false;
			stopAllPlayers();
			paused = false;
			pausedTimer = false;
			crossfadeActive = false;
		}
		else {
			crossfadeActive = false;
			currentFadeLength = fadeOutTime;
			startedFadeAt = accumulatedDeltaTime;
			fadeoutActive = true;
			running = false;
		}
	}



	
	// Crossfades to a new clip.  If it's the same layer 
	// and song that's already playing, does nothing and 
	// returns true.  If it's the same song but different
	// valid layer, it calls the CrossFadeToNewLayer
	// function instead.
	public bool CrossFadeToNewSong (int songNumber, int layerNumber = 0, float crossfadeTimeUser = Mathf.Infinity) {
		bool failure = false;
		float crossfadeTime = checkCrossfadeInput(crossfadeTimeUser);

		if (songAndLayerValid(songNumber,layerNumber) && !fadeoutActive) {

			if (fadeinActive) {
				fadeinActive = false;
			}

			if (currentSongForPlayer[switchingToPlayer] == songNumber && currentLayerForPlayer[switchingToPlayer] == layerNumber) {
				failure = true;
			}

			else if (currentSongForPlayer[switchingToPlayer] == songNumber) {
				failure = CrossFadeToNewLayer (layerNumber,crossfadeTimeUser);
			}

			else {
				int newSwitchingToPlayer = searchForActiveMultiplePlayerWith(songNumber,layerNumber);
				if (newSwitchingToPlayer >= 0) {
					playAllLayersOfSong(songNumber);
					setCurrentSong (songNumber,layerNumber);
					crossfadeActive = true;
					currentFadeLength = crossfadeTime;
					startedFadeAt = accumulatedDeltaTime;
					activeMultiple = 0;
				}
				else {
					failure = true;
				}
			}
		}

		else {
			failure = true;
		}

		return failure;
	}

	// CrossFade overloaded function that allows string search for song name.
	public bool CrossFadeToNewSong (string songName, int layerNumber, float crossfadeTimeUser = Mathf.Infinity) {
		float crossfadeTime = checkCrossfadeInput(crossfadeTimeUser);

		int songNumber = searchForSongName(songName);
		bool success = songAndLayerValid(songNumber,layerNumber);
		if (success) {
			CrossFadeToNewSong (songNumber,layerNumber,crossfadeTime);
		}
		return success;
	}

	// CrossFade overloaded function that allows string search for song name and layer name.
	public bool CrossFadeToNewSong (string songName, string layerName, float crossfadeTimeUser = Mathf.Infinity) {
		float crossfadeTime = checkCrossfadeInput(crossfadeTimeUser);

		int songNumber = searchForSongName(songName);
		int layerNumber = searchForLayerName(layerName,songNumber);
		bool success = songAndLayerValid(songNumber,layerNumber);
		if (success) {
			CrossFadeToNewSong (songNumber,layerNumber,crossfadeTime);
		}
		return success;
	}





	// CrossFade overloaded function that allows string search for layer name (assuming the same song number)
	public int CrossFadeToNewLayer (string layerName, float crossfadeTimeUser = Mathf.Infinity) {
		float crossfadeTime = checkCrossfadeInput(crossfadeTimeUser);

		int songNumber = currentSongForPlayer[switchingToPlayer];
		int layerNumber = searchForLayerName (layerName,songNumber);

		if (songAndLayerValid(songNumber,layerNumber)) { 
			CrossFadeToNewLayer (layerNumber,crossfadeTime);
		}
		return layerNumber;
	}

	// CrossFade overloaded function that uses the literal crossfade time typed into the player instead of an argument.
	public bool CrossFadeToNewLayer (int layerNumber) {
		return CrossFadeToNewLayer (layerNumber,songs[currentSongForPlayer[switchingToPlayer]].getLayerCrossfadeBy() + defaultExpectedLatency);
	}

	// CrossFade to new layer function.  Returns true if the player isn't running or it can't find that layer.
	public bool CrossFadeToNewLayer (int layerNumber, float crossfadeTime) {
		int newSwitchingToPlayer = searchForActiveMultiplePlayerWith(currentSongForPlayer[switchingToPlayer],layerNumber);
		if (newSwitchingToPlayer >= 0 && running && !fadeoutActive) {
			switchingToPlayer = newSwitchingToPlayer;
			crossfadeActive = true;
			startedFadeAt = accumulatedDeltaTime;
			currentFadeLength = crossfadeTime;

			if (fadeinActive) {
				fadeinActive = false;
			}

			return false;
		}
		else {
			return true;
		}
	}






	////////////////////////////////////////
	/// PUBLIC ACCESSORS
	////////////////////////////////////////

	// Returns the last song number that was played (or is playing)
	public int getCurrentSongNumber() {
		return currentSongForPlayer[switchingToPlayer];
	}

	// Returns the last layer number that was played (or is playing)
	public int getCurrentLayerNumber() {
		return currentLayerForPlayer[switchingToPlayer];
	}

	// Returns true/false whether or not the specified song is set to loop
	public bool getLoop(int songNumber) {
		if (songAndLayerValid (songNumber,0)) {
			return songs[songNumber].getLoop ();
		}
		else {
			Debug.LogError ("Tried To Get Loop Info On A Song That Doesn't Exist");
			return false;
		}
	}

	// Sets the specified song to loop or not loop
	public bool setLoop(int songNumber, bool loopOn) {
		if (songAndLayerValid (songNumber,0)) {
			songs[songNumber].setLoop (loopOn);
			return true;
		}
		else {
			Debug.LogError ("Tried To Set Loop Info On A Song That Doesn't Exist");
			return false;
		}
	}

	// Returns whether or not any player is playing an audio clip
	public bool isPlaying() {
		bool foundOne = false;
		for (int player=0; player < players.Length; player++) {
			if (players[player].isPlaying) {
				foundOne = true;
			}
		}

		return foundOne;
	}

	// Returns whether or not any player is playing the specified song
	public bool isPlaying(int songNumber) {
		if (songAndLayerValid(songNumber,0)) {
			bool foundOne = false;
			for (int player=0; player < players.Length; player++) {
				if (players[player].isPlaying &&
				    currentSongForPlayer[player] == songNumber) {
					foundOne = true;
				}
			}
			
			return foundOne;
		}
		else {
			Debug.LogError ("Tried To Get Info About A Song That Doesn't Exist");
			return false;
		}
	}

	// Returns whether or not any player is playing the specified song and layer
	public bool isPlaying(int songNumber, int layerNumber) {
		if (songAndLayerValid(songNumber,layerNumber)) {
			bool foundOne = false;
			for (int player=0; player < players.Length; player++) {
				if (players[player].isPlaying &&
				    currentSongForPlayer[player] == songNumber &&
				    currentLayerForPlayer[player] == layerNumber) {
					foundOne = true;
				}
			}
			
			return foundOne;
		}

		else {
			Debug.LogError ("Tried To Get Info About A Song/Layer That Doesn't Exist");
			return false;
		}
	}

	// If using resource request mode, returns percent of resources loaded, between 0f and 1f
	public float getResourceProgress() {
		return totalResourceProgress;
	}

	// Returns the current global volume scalar (0f to 1f)
	public float getGlobalVolume() {
		return globalVolume;
	}

	// Sets the current global volume scalar (0f to 1f)
	public bool setGlobalVolume(float newVolume) {
		bool failure = true;
		if (newVolume >= 0f && newVolume <= 1f) {
			globalVolume = newVolume;
			failure = false;
		}
		else if (newVolume < 0f) {
			globalVolume = 0f;
		}
		else if (newVolume > 1f) {
			globalVolume = 1f;
		}

		setGlobalVolumeToPlayers();

		return failure;
	}

	// Sets the global crossfade time (between songs, not layers)
	public void setGlobalCrossFadeTime(float newTime) {
		globalCrossFadeTime = newTime;
	}

	// Gets the global crossfade time (between songs, not layers)
	public float getGlobalCrossFadeTime() {
		return globalCrossFadeTime;
	}




	////////////////////////////////////////
	/// BEHIND THE SCENES
	////////////////////////////////////////

	// Called every fixed update cycle, performs a global fadeout.
	private void handleFadeout() {
		if (fadeoutActive && currentFadeLength > 0f) {
			float currentFadeProgress = Mathf.Clamp((accumulatedDeltaTime - startedFadeAt) / currentFadeLength,0f,1f);
			float reverseFadeProgress = 1f - currentFadeProgress;
			for (int player=0; player < players.Length; player++) {
				// Fade out 
				preVolumeFadeFactor[player] = Mathf.Clamp (reverseFadeProgress,0f,preVolumeFadeFactor[player]);
				// Scales with the master volume control
				setVolume(player,preVolumeFadeFactor[player]);
			}
			if (currentFadeProgress == 1f) {
				fadeoutActive = false;
				if (!running) {
					stopAllPlayers();
				}
			}
		}
	}

	// Called every fixed update cycle, performs a fade in of a track.
	private void handleFadeIn() {
		if (fadeinActive) {
			float currentFadeProgress = Mathf.Clamp((accumulatedDeltaTime - startedFadeAt) / currentFadeLength,0f,1f);
			for (int player=0; player < players.Length; player++) {
				if (switchingToPlayer == player) {
					// Fade In (Will never cause a source to get quieter during the crossfade)
					preVolumeFadeFactor[player] = Mathf.Clamp (currentFadeProgress,preVolumeFadeFactor[player],1f);
					// Scales with the master volume control
					setVolume(player,preVolumeFadeFactor[player]);
				}
			}
			if (currentFadeProgress == 1f) {
				fadeinActive = false;
			}
		}
	}

	// Called every fixed update cycle, performs a crossfade (fades all but one track down, while fading that track up).
	private void handleCrossfade() {
		if (crossfadeActive) {
			// currentFadeProgress is a linear taper from 0f to 1f that covers the duration of the entire crossfade
			float currentFadeProgress = Mathf.Clamp((accumulatedDeltaTime - startedFadeAt) / currentFadeLength,0f,1f);
			// currentFadeValue is a custom nonlinear taper from 0f to 1f that covers the duration of the entire crossfade
			float currentFadeValue = crossfadeTaper (currentFadeProgress,2);
			// Apply the currentFadeValue taper to the players
			for (int player=0; player < players.Length; player++) {
				// switchingToPlayer will always be the player we are fading to
				if (player == switchingToPlayer) {
					// Fade In (Will never cause a source to get quieter during the crossfade)
					preVolumeFadeFactor[player] = Mathf.Clamp (currentFadeValue,preVolumeFadeFactor[player],1f);
					// Scales with the master volume control
					setVolume(player,preVolumeFadeFactor[player]);
				}
				else {
					// Fade Out (Will never cause a source to get louder during the crossfade), so
					// fade everything that is above 0f.
					if (preVolumeFadeFactor[player] > 0f) {
						preVolumeFadeFactor[player] = Mathf.Clamp (1f-currentFadeValue,0f,preVolumeFadeFactor[player]);
						// Scales with the master volume control
						setVolume(player,preVolumeFadeFactor[player]);
					}
				}
			}
			// The fade is done.  Stop trying to fade.
			if (currentFadeProgress == 1f) {
				crossfadeActive = false;
				loopCrossFadeActive = false;
			}
		}
	}

	// Called every fixed update cycle, checks to see if it's time to start the next multiples of players for looping
	private void handleLoopDetection() {
		// If the current song is set to loop, we're playing audio, and we're not already doing a loop crossfade
		if (songs[currentSongForPlayer[switchingToPlayer]].getLoop () && running && !loopCrossFadeActive) {
			// If it's time to start playing the clip (time has reached the end, or in some scenarios (like in
			// an HTML5 build where the webpage is in the background, and fixedupdate is only allowed to run
			// once a second) the clip may actually completely stop).
			if (!players[switchingToPlayer].isPlaying || ((players[switchingToPlayer].clip != null) && (players[switchingToPlayer].time >= players[switchingToPlayer].clip.length - (songs[currentSongForPlayer[switchingToPlayer]].getLoopCrossfadeBy () + defaultExpectedLatency)))) {
				// Switch to the next active multiple of the player
				activeMultiple = (activeMultiple + 1) % multiplyLayersBy;
				crossfadeActive = true;
				loopCrossFadeActive = true;
				for (int playerNumber=0; playerNumber < players.Length; playerNumber++) {
					if (playerNumber % multiplyLayersBy == activeMultiple &&
					    currentSongForPlayer[switchingToPlayer] == currentSongForPlayer[playerNumber]) {
						players[playerNumber].Play ();
						currentFadeLength = songs[currentSongForPlayer[switchingToPlayer]].getLoopCrossfadeBy() + defaultExpectedLatency;
						startedFadeAt = accumulatedDeltaTime;
						
						if (currentLayerForPlayer[switchingToPlayer] == currentLayerForPlayer[playerNumber]) {
							// setVolume (playerNumber,preVolumeFadeFactor[switchingToPlayer]);
							switchingToPlayer = playerNumber;
						}
			
						// If we found a layer not playing that we turn on, skip the duplicates.
						// This is in addition to the for loop increment
						// So, if multiplyLayersBy is 2, if we find a player at players[1] that gets turned on
						// we want to skip to that same duplicate number for each layer, so then we only check
						// players[3], players[5], players[7] etc
						// If we multiplyLayersBy 3, we get 1, 4, 7, 10 etc
			
						playerNumber += multiplyLayersBy-1;
					}
				}
			}
		}
	}

	// Increments the accumulatedDeltaTime for the fade in and fade outs
	private void handleAccumulatedDeltaTime() {
		if (!paused) {
			accumulatedDeltaTime += Time.fixedDeltaTime;
		}
	}

	// Checks to see if pause automation time has triggered
	private void handleUnPause() {
		if (paused && pausedTimer) {
			accumulatedDeltaTimeForPause += Time.fixedDeltaTime;
			if (accumulatedDeltaTimeForPause > unPauseAt) {
				paused = false;
				UnPause ();
			}
		}
	}

	// Sets the current song and layer.  Internal only.
	private bool setCurrentSong(int songNumber, int layerNumber) {
		bool failure = true;

		for (int player=0; player < players.Length; player++) {
			if (currentSongForPlayer[player] == songNumber &&
			    currentLayerForPlayer[player] == layerNumber) {
				switchingToPlayer = player;
				failure = false;
				break;
			}
		}

		return failure;
	}

	// Sets all the player volumes to some value (usually you'd only use
	// this for zero.)  That value will still be multiplied against the
	// global volume in the setVolume function.
	private void setAllPlayerVolumesTo(float volume) {
		for (int player=0; player < players.Length; player++) {
			preVolumeFadeFactor[player] = volume;
			setVolume (player,preVolumeFadeFactor[player]);
		}
	}

	// Sets all the player volumes of a specific song to some value (usually you'd 
	// only use this for zero.)  That value will still be multiplied against the
	// global volume in the setVolume function.
	private void setAllPlayerVolumesOfSongTo(int songNumber, float volume) {
		for (int player=0; player < players.Length; player++) {
			if (currentSongForPlayer[player] == songNumber) {
				preVolumeFadeFactor[player] = volume;
				setVolume (player,preVolumeFadeFactor[player]);
			}
		}
	}
	
	// Sets all the volume of a specific player to some value.
	// That value will still be multiplied against the
	// global volume in the setVolume function.
	private bool setPlayerVolumeToViaPlayer(int playerNumber, float volume) {
		bool failure = false;
		if (playerNumber >= players.Length) {
			failure = true;
		}
		else {
			preVolumeFadeFactor[playerNumber] = volume;
			setVolume (playerNumber,preVolumeFadeFactor[playerNumber]);
		}
		return failure;
	}

	// Sets the volume of a specific player to a preVolumeFactor level.
	// This is then multiplied by the globalVolume and the layer volume.
	private bool setVolume(int playerNumber, float preVolumeFactor) {
		bool failure = false;
		if (playerNumber >= players.Length) {
			failure = true;
		}
		else {
			players[playerNumber].volume = preVolumeFactor * globalVolume * songs[currentSongForPlayer[playerNumber]].volumes[currentLayerForPlayer[playerNumber]];
		}
		return failure;
	}

	// Performs the setVolume function for each player with each player's
	// preVolumeFadeFactor level.
	private bool setGlobalVolumeToPlayers() {
		bool failure = false;
		for (int player=0; player < players.Length; player++) {
			failure = setVolume (player,preVolumeFadeFactor[player]) || failure;
		}
		return failure;
	}

	// Creates an array of AudioSource - as many as needed
	private void populatePlayers() {
		int needThisManyPlayers = 0;
		for (int songNumber=0; songNumber < songs.Length; songNumber++) {
			for (int layerNumber=0; layerNumber < songs[songNumber].layer.Length; layerNumber++) {
				for (int multiplyBy=0; multiplyBy < multiplyLayersBy; multiplyBy++) {
					needThisManyPlayers++;
				}
			}
		}

		players = new AudioSource[needThisManyPlayers];

		spawnPlayers ();
	}

	// Instantiates AudioSource components for the AudioSource array
	private void spawnPlayers() {
		for (int player=0; player < players.Length; player++) {
			if (players[player] == null) {
				players[player] = gameObject.AddComponent<AudioSource>();
				players[player].volume = 0f;
				players[player].loop = false;
				players[player].playOnAwake = false;
				players[player].pitch = 1f;
			}
		}
	}

	// If in resource request mode, creates an array of ResourceRequest
	// and then assigns resources to each.
	private void populateResourceRequests() {
		int needThisManyResources = 0;
		for (int songNumber=0; songNumber < songs.Length; songNumber++) {
			for (int layerNumber=0; layerNumber < songs[songNumber].layer.Length; layerNumber++) {
				needThisManyResources++;

			}
		}
		theLayerRequests = new ResourceRequest[needThisManyResources];

		int currentResourceRequest = 0;
		for (int songNumber=0; songNumber < songs.Length; songNumber++) {
			for (int layerNumber=0; layerNumber < songs[songNumber].layer.Length; layerNumber++) {
				if (songs[songNumber].layerResourceNames[layerNumber].Length > 0) {
					theLayerRequests[currentResourceRequest] = Resources.LoadAsync (songs[songNumber].layerResourceNames[layerNumber]);
				}
				currentResourceRequest++;
			}
		}

		needToCheckResourceStatus = true;
	}


	// This creates arrays to store the song numbers, layer numbers,
	// and preVolumeFadeFactors for each audio clip.
	// If passed a true argument, it also assigns an audio clip
	// to each player.
	// Only pass a true argument to this function if the audio clips
	// are ready to be used.  (ie. You are either not in resource request
	// mode, or the resources have been loaded.
	private void populateArrays(bool addClips) {
		currentSongForPlayer = new int[players.Length];
		currentLayerForPlayer = new int[players.Length];
		preVolumeFadeFactor = new float[players.Length];

		int playerNumber = 0;
		for (int songNumber=0; songNumber < songs.Length; songNumber++) {
			for (int layerNumber=0; layerNumber < songs[songNumber].layer.Length; layerNumber++) {
				for (int multiplyBy=0; multiplyBy < multiplyLayersBy; multiplyBy++) {
					if (addClips) {
						players[playerNumber].clip = songs[songNumber].layer[layerNumber];
					}
					currentSongForPlayer[playerNumber] = songNumber;
					currentLayerForPlayer[playerNumber] = layerNumber;
					preVolumeFadeFactor[playerNumber] = 0f;
					playerNumber++;
				}
			}
		}

	}

	// Checks if these are valid song and layer numbers.
	private bool songAndLayerValid(int songNumber, int layerNumber) {
		if (songNumber >= 0 && layerNumber >= 0 && songNumber < songs.Length && layerNumber < songs[songNumber].layer.Length) {
			return true;
		}
		else {
			return false;
		}
	}

	// Checks if this is a valid song number.
	private bool songValid(int songNumber) {
		if (songNumber >=0 && songNumber < songs.Length) {
			return true;
		}
		else {
			return false;
		}
	}

	// Returns the number of the first player it finds with this
	// song number and layer number.  Returns -1 if it can't find it.
	private int searchForPlayerWith(int songNumber, int layerNumber) {
		if (songAndLayerValid(songNumber,layerNumber)) {
			for (int player=0; player < players.Length; player++) {
				if (currentSongForPlayer[player] == songNumber &&
				    currentLayerForPlayer[player] == layerNumber) {
					return player;
				}
			}
		}
		Debug.LogError ("Couldn't find that song/layer.");
		return -1;
	}


	// Returns the number of the first ACTIVE player it finds with this
	// song number and layer number.  Returns -1 if it can't find it.
	private int searchForActiveMultiplePlayerWith(int songNumber, int layerNumber) {
		if (songAndLayerValid(songNumber,layerNumber)) {
			for (int player=0; player < players.Length; player++) {
				if (currentSongForPlayer[player] == songNumber &&
				    currentLayerForPlayer[player] == layerNumber &&
				    player % multiplyLayersBy == activeMultiple) {
					return player;
				}
			}
		}
		Debug.LogError ("Couldn't find that song/layer.");
		return -1;
	}

	// If we're "running" but the current audio player has stopped playing
	// and we're not going to loop, turn off "running".
	private void handleRunningStatus() {
		if (running && 
		    !songs[currentSongForPlayer[switchingToPlayer]].getLoop () &&
		    !players[switchingToPlayer].isPlaying) {

			running = false;
		}
	}

	// Plays all layers of a song on their active multiple, which is
	// required to fade between layers.
	private bool playAllLayersOfSong(int songNumber) {
		bool failure = false;

		for (int playerNumber=0; playerNumber < players.Length; playerNumber++) {
			if (players[playerNumber].clip != null) {
				if (players[playerNumber].clip.loadState != AudioDataLoadState.Loaded && currentSongForPlayer[playerNumber] == songNumber) {
					failure = true;
				}
			}
		}

		if (!failure) {
			for (int playerNumber=0; playerNumber < players.Length; playerNumber++) {
				if (currentSongForPlayer[playerNumber] == songNumber) {
					players[playerNumber].Play ();
					playerNumber += multiplyLayersBy-1;
				}
			}
		}

		else {
			Debug.LogWarning ("An Audio Clip Of This Song Is Not Yet Loaded.  Scheduled to play as soon as all are loaded.");
			songThatNeedsToPlayWhenLoaded = songNumber;
		}

		return failure;
	}

	// Searches for a song name by string.
	public int searchForSongName(string songName) {
		int songNumber = -1;
		if (songName.Length > 0) {
			for (int songNumberSearch=0; songNumberSearch < songs.Length; songNumberSearch++) {
				if (songName.CompareTo (songs[songNumberSearch].textName) == 0) {
					songNumber = songNumberSearch;
					break;
				}
			}
		}
		return songNumber;
	}

	// Searches for a layer name by string, with a specified song number.
	public int searchForLayerName(string layerName, int songNumber) {
		int layerNumber = -1;
		if (songNumber >= 0 && songNumber < songs.Length) {
			if (layerName.Length > 0) {
				for (int layerNumberSearch=0; layerNumberSearch < songs[songNumber].layer.Length; layerNumberSearch++) {
					if (layerName.CompareTo (songs[songNumber].layer[layerNumberSearch]) == 0) {
						layerNumber = layerNumberSearch;
						break;
					}
				}
			}
		}
		return layerNumber;
	}

	// If somebody doesn't enter in a crossfadeTimeUser, it uses
	// a default argument of Infinity (which we would never, ever
	// want), so if the value is still Infinity, no value was
	// specified and we should default to the globalCrossFadeTime.
	private float checkCrossfadeInput(float crossfadeTimeUser) {
		float crossfadeTime;
		if (crossfadeTimeUser == Mathf.Infinity) {
			crossfadeTime = globalCrossFadeTime;
		}
		else {
			crossfadeTime = crossfadeTimeUser;
		}

		return crossfadeTime;
	}

	// Stops all players.
	private void stopAllPlayers() {
		for (int playerNumber=0; playerNumber < players.Length; playerNumber++) {
			players[playerNumber].Stop ();
		}
	}

	// Run every update until needToCheckResourceStatus turns false.
	// This monitors the progress of the Resource Requests, and when
	// they are ready, it loads the audio into the player clips.
	// Once progress reaches 100%, it flags needToCheckResourceStatus as false.
	private void handleCheckResourceStatus() {
		bool notDoneWithResourceRequestYet = false;
		bool notDoneWithAudioDataLoadYet = false;
		float accumulatedProgress = 0f;
		float numberOfNonNullResources = 0f;

		int playerNumber = 0;
		for (int resourceNumber=0; resourceNumber < theLayerRequests.Length; resourceNumber++) {
			for (int repeat=0; repeat < multiplyLayersBy; repeat++) {
				if (theLayerRequests[resourceNumber] != null) {
					numberOfNonNullResources += 1f;
					if (theLayerRequests[resourceNumber].progress < 1.0f) {
						notDoneWithResourceRequestYet = true;
					}
					else if (players[playerNumber].clip == null) {
						if (theLayerRequests[resourceNumber].asset != null) {
							players[playerNumber].clip = (AudioClip)theLayerRequests[resourceNumber].asset as AudioClip;
							if (players[playerNumber].clip != null) {
								players[playerNumber].clip.LoadAudioData();
								songs[currentSongForPlayer[playerNumber]].layer[currentLayerForPlayer[playerNumber]] = players[playerNumber].clip;
							}
						}
					}
					accumulatedProgress += theLayerRequests[resourceNumber].progress;
				
					if (players[playerNumber].clip != null && players[playerNumber].clip.loadState == AudioDataLoadState.Loaded) {
						accumulatedProgress += 1f;
					}
					else {
						notDoneWithAudioDataLoadYet = true;
					}
				}
				playerNumber++;
			}
		}

		totalResourceProgress = accumulatedProgress / (numberOfNonNullResources * 2f);

		if (!notDoneWithResourceRequestYet && !notDoneWithAudioDataLoadYet) {
			needToCheckResourceStatus = false;
		}
	}

	// This hits play on the player and then Pause.  I don't know
	// if this helps or not, and I didn't do a lot of testing, but
	// with the testing that I did, I saw a decrease in times
	// that there were phase issues between layers, so I left it in.
	private void loadResources() {
		for (int player=0; player < players.Length; player++) {
			players[player].Play ();
			players[player].Pause ();
		}
	}


	// If a song has been scheduled to play as soon as resources are
	// loaded, check to see if resources are loaded, and when they
	// are, play the song.
	private void handleSongScheduling() {
		bool readyToPlay = true;
		if (songThatNeedsToPlayWhenLoaded != -1) {
			for (int playerNumber=0; playerNumber < players.Length; playerNumber++) {
				if (currentSongForPlayer[playerNumber] == songThatNeedsToPlayWhenLoaded) {
					if (!(players[playerNumber].clip.loadState == AudioDataLoadState.Loaded)) {
						readyToPlay = false;
						break;
					}
				}
			}
			if (readyToPlay) {
				playAllLayersOfSong(songThatNeedsToPlayWhenLoaded);
				running = true;
				songThatNeedsToPlayWhenLoaded = -1;
			}
		}
	}

	// Converts a linear 0f-1f to a non-linear 0f-1f to make a curve
	// that is much nicer sounding for layer crossfades.
	private float crossfadeTaper(float currentFadeProgress, int recursions) {
		if (recursions < 0) {
			return currentFadeProgress;
		}
		else {
			return crossfadeTaper (1f - Mathf.Pow (2f,-currentFadeProgress) * 2f + 1f,recursions-1);
		}
	}
}
