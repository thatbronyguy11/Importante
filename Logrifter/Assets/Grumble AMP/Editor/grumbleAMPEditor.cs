// Inspector GUI For The grumbleAMP Class

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Globalization;

[CustomEditor (typeof(grumbleAMP))]
public class grumbleAMPEditor : Editor {

	public override void OnInspectorGUI ()
	{
		grumbleAMP gA = (grumbleAMP)target;

		serializedObject.Update ();

		// Whitespace
		EditorGUILayout.LabelField (" ");
		EditorGUILayout.LabelField (" ");

		// GLOBAL SETTINGS ENTRY
		float newGlobalVolume = EditorGUILayout.Slider ("Global Volume",gA.globalVolume,0f,1f);
		if (newGlobalVolume != gA.globalVolume) {
			Undo.RecordObject (gA,"Global Volume Change");
			gA.globalVolume = newGlobalVolume;
		}

		float newGlobalCrossFadeTime = EditorGUILayout.FloatField ("Global Crossfade Time",gA.globalCrossFadeTime);
		if (newGlobalCrossFadeTime != gA.globalCrossFadeTime) {
			Undo.RecordObject (gA,"Global Crossfade Change");
			gA.globalCrossFadeTime = newGlobalCrossFadeTime;
		}

		if (!Application.isPlaying) {
			bool newResourceRequestMode = EditorGUILayout.Toggle ("Resource Request Mode",gA.resourceRequestMode);
			if (newResourceRequestMode != gA.resourceRequestMode) {
				Undo.RecordObject (gA,"Resource Request Mode Change");
				gA.resourceRequestMode = newResourceRequestMode;
			}
		}


		EditorGUILayout.LabelField (" ");

		// WHILE PLAYING, SHOW LIMITED INFORMATION
		if (Application.isPlaying) {
			showPlayingInfo (gA);
		}

		// WHILE NOT PLAYING, ALLOW EDITING OF THIS INFO
		else {
			showEditableInfo (gA);

		}
		serializedObject.ApplyModifiedProperties();
	}


	// The limited information that is shown while the scene
	// so you can't break anything.
	public void showPlayingInfo (grumbleAMP gA) {
		EditorGUILayout.LabelField ("Currently Playing Song: " + gA.getCurrentSongNumber().ToString () + "   Layer: " + gA.getCurrentLayerNumber().ToString());
		if (gA.getResourceProgress() < 1f) {
			EditorGUILayout.LabelField ("Resource Loading Progress: " + (gA.getResourceProgress()*100f).ToString ("F2") + "%");
			this.Repaint ();
		}
		else {
			EditorGUILayout.LabelField ("Resource Loading Progress: " + "100%");
		}
		// Whitespace
		EditorGUILayout.LabelField (" ");
	}



	public static void showEditableInfo (grumbleAMP gA) {

		if (gA.songs == null) {
			createFirstBlankGrumbleSong (gA);
		}

		else if (gA.songs.Length == 0) {
			addSong (gA);
		}

		EditorGUILayout.LabelField ("NUMBER OF SONGS: " + gA.songs.Length.ToString ());
		
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Space();
		EditorGUILayout.BeginVertical();
		
		// TO ADD A NEW SONG
		// Resizes array +1 and adds song to the end of the array.
		
		if (GUILayout.Button ("ADD SONG",GUILayout.Width(120),GUILayout.Height(30))) {
			Undo.RecordObject (gA,"Add Song");
			addSong (gA);
		}
		
		// TO REMOVE A SONG
		// Resizes array -1 and erases the last song
		if (GUILayout.Button ("REMOVE SONG",GUILayout.Width(120), GUILayout.Height(30))) {
			Undo.RecordObject (gA,"Remove Song");
			removeSong (gA);
		}
		
		EditorGUILayout.EndVertical();
		EditorGUILayout.EndHorizontal();
		// Whitespace
		EditorGUILayout.LabelField (" ");

		showSongs(gA);
	}


	public static void addSong (grumbleAMP gA) {

		grumbleSong[] tempSongs = new grumbleSong[gA.songs.Length];
		tempSongs = gA.songs;
		gA.songs = new grumbleSong[gA.songs.Length+1];
		for (int i=0; i < tempSongs.Length; i++) {
			gA.songs[i] = tempSongs[i];
		}

		// Initializes Default Settings For A New Song
		gA.songs[gA.songs.Length-1] = (grumbleSong)ScriptableObject.CreateInstance ("grumbleSong");
		gA.songs[gA.songs.Length-1].textName = "SONG " + (gA.songs.Length-1).ToString ();
		gA.songs[gA.songs.Length-1].layer = new AudioClip[1];
		gA.songs[gA.songs.Length-1].layerNames = new string[1];
		gA.songs[gA.songs.Length-1].layerNames[0] = "Layer 0";
		gA.songs[gA.songs.Length-1].volumes = new float[1];
		gA.songs[gA.songs.Length-1].volumes[0] = 1f;
		gA.songs[gA.songs.Length-1].layerResourceNames = new string[1];
		gA.songs[gA.songs.Length-1].layerResourceNames[0] = "";
	}

	public static void removeSong (grumbleAMP gA) {
		if (gA.songs.Length > 1) {
			grumbleSong[] tempSongs = new grumbleSong[gA.songs.Length];
			tempSongs = gA.songs;
			gA.songs[gA.songs.Length-1] = null;
			DestroyImmediate (gA.songs[gA.songs.Length-1]);
			gA.songs = new grumbleSong[gA.songs.Length-1];
			for (int i=0; i < gA.songs.Length; i++) {
				gA.songs[i] = tempSongs[i];
			}
		}
	}


	
	public static void showSongs (grumbleAMP gA) {
		EditorGUI.indentLevel += 2;

		if (gA.songs == null) {
			createFirstBlankGrumbleSong (gA);
		}
		
		for (int i=0; i < gA.songs.Length; i++) {
			// Whitespace
			EditorGUILayout.LabelField (" ");


			showGrumbleSongSpecificSettings (gA,i);
			// Whitespace
			EditorGUILayout.LabelField (" ");

			EditorGUI.indentLevel += 1;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical();


			if (GUILayout.Button ("ADD LAYER",GUILayout.Width(120),GUILayout.Height(30))) {
				Undo.RecordObject (gA.songs[i],"Add Song Layer");
				addLayer (gA,i);
			}

			if (GUILayout.Button ("REMOVE LAYER",GUILayout.Width(120),GUILayout.Height(30))) {
				Undo.RecordObject (gA.songs[i],"Remove Song Layer");
				removeLayer (gA,i);
			}

			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();

			checkLayerLengths (gA,i);

			showLayerInfoFields (gA,i);


			EditorGUI.indentLevel -= 1;
			// Whitespace
			EditorGUILayout.LabelField (" ");
		}

		EditorGUI.indentLevel -= 2;
	}

	public static void addLayer (grumbleAMP gA, int songNumber) {
		AudioClip[] tempLayers = new AudioClip[gA.songs[songNumber].layer.Length];
		string[] tempLayerNames = new string[gA.songs[songNumber].layerNames.Length];
		float[] tempVolumes = new float[gA.songs[songNumber].volumes.Length];
		string[] tempResourceNames = new string[gA.songs[songNumber].layerResourceNames.Length];
		tempLayers = gA.songs[songNumber].layer;
		tempLayerNames = gA.songs[songNumber].layerNames;
		tempVolumes = gA.songs[songNumber].volumes;
		tempResourceNames = gA.songs[songNumber].layerResourceNames;
		gA.songs[songNumber].layer = new AudioClip[gA.songs[songNumber].layer.Length+1];
		gA.songs[songNumber].layerNames = new string[gA.songs[songNumber].layerNames.Length+1];
		gA.songs[songNumber].volumes = new float[gA.songs[songNumber].volumes.Length+1];
		gA.songs[songNumber].layerResourceNames = new string[gA.songs[songNumber].layerResourceNames.Length+1];
		for (int j=0; j < gA.songs[songNumber].layer.Length-1; j++) {
			gA.songs[songNumber].layer[j] = tempLayers[j];
			gA.songs[songNumber].layerNames[j] = tempLayerNames[j];
			gA.songs[songNumber].volumes[j] = tempVolumes[j];
			gA.songs[songNumber].layerResourceNames[j] = tempResourceNames[j];
		}
		gA.songs[songNumber].layerNames[gA.songs[songNumber].layer.Length-1] = "Layer " + (gA.songs[songNumber].layerNames.Length-1).ToString ();
		gA.songs[songNumber].volumes[gA.songs[songNumber].volumes.Length-1] = 1f;
	}

	public static void removeLayer (grumbleAMP gA, int songNumber) {
		if (gA.songs[songNumber].layer.Length > 1) {
			AudioClip[] tempLayers = new AudioClip[gA.songs[songNumber].layer.Length];
			string[] tempLayerNames = new string[gA.songs[songNumber].layerNames.Length];
			float[] tempVolumes = new float[gA.songs[songNumber].volumes.Length];
			string[] tempResourceNames = new string[gA.songs[songNumber].layerResourceNames.Length];
			tempLayers = gA.songs[songNumber].layer;
			tempLayerNames = gA.songs[songNumber].layerNames;
			tempVolumes = gA.songs[songNumber].volumes;
			tempResourceNames = gA.songs[songNumber].layerResourceNames;
			gA.songs[songNumber].layer = new AudioClip[gA.songs[songNumber].layer.Length-1];
			gA.songs[songNumber].layerNames = new string[gA.songs[songNumber].layerNames.Length-1];
			gA.songs[songNumber].volumes = new float[gA.songs[songNumber].volumes.Length-1];
			gA.songs[songNumber].layerResourceNames = new string[gA.songs[songNumber].layerResourceNames.Length-1];
			for (int j=0; j < gA.songs[songNumber].layer.Length; j++) {
				gA.songs[songNumber].layer[j] = tempLayers[j];
				gA.songs[songNumber].layerNames[j] = tempLayerNames[j];
				gA.songs[songNumber].volumes[j] = tempVolumes[j];
				gA.songs[songNumber].layerResourceNames[j] = tempResourceNames[j];
			}
		}
	}

	public static void checkLayerLengths (grumbleAMP gA, int songNumber) {
		if (gA.songs[songNumber].layer.Length > 0 && gA.songs[songNumber].layer[0] != null) {
			
			bool sameLength = true;
			float firstLength = gA.songs[songNumber].layer[0].length;
			float lengthEpsilon = 0.1f;  // "Epsilon" value.  ;)
			
			for (int layerNumber=0; layerNumber < gA.songs[songNumber].layer.Length; layerNumber++) {
				if (gA.songs[songNumber].layer[layerNumber] != null && Mathf.Abs (gA.songs[songNumber].layer[layerNumber].length - firstLength) > lengthEpsilon) {
					sameLength = false;
				}
			}
			
			if (!sameLength) {
				EditorGUILayout.LabelField ("WARNING: ALL LAYERS ARE NOT THE SAME LENGTH");
			}
		}
	}

	public static void showLayerInfoFields (grumbleAMP gA, int songNumber) {
		for (int j=0; j < gA.songs[songNumber].layer.Length; j++) {
			EditorGUILayout.LabelField ("Song Layer " + j.ToString () + " - \"" + gA.songs[songNumber].layerNames[j] + "\"");
			if (gA.resourceRequestMode) {
				string newResourceName = EditorGUILayout.TextField ("Resource Load Name",gA.songs[songNumber].layerResourceNames[j]);
				if (newResourceName.CompareTo (gA.songs[songNumber].layerResourceNames[j]) != 0) {
					Undo.RecordObject (gA.songs[songNumber],"Resource Load Name Change");
					gA.songs[songNumber].layerResourceNames[j] = newResourceName;
				}
			}
			
			else {
				AudioClip newAudioClip = (AudioClip)EditorGUILayout.ObjectField(gA.songs[songNumber].layer[j],typeof(AudioClip),false);
				if (gA.songs[songNumber].layer[j] != newAudioClip) {
					Undo.RecordObject (gA.songs[songNumber],"Audio File Change");
					gA.songs[songNumber].layer[j] = newAudioClip;
				}
			}

			string newLayerName = EditorGUILayout.TextField ("Custom Name",gA.songs[songNumber].layerNames[j]);
			if (gA.songs[songNumber].layerNames[j].CompareTo (newLayerName) != 0) {
				Undo.RecordObject (gA.songs[songNumber],"New Layer Name");
				gA.songs[songNumber].layerNames[j] = newLayerName;
			}

			float newLayerVolume = EditorGUILayout.Slider ("Volume Factor",gA.songs[songNumber].volumes[j],0f,2f);
			if (gA.songs[songNumber].volumes[j] != newLayerVolume) {
				Undo.RecordObject (gA.songs[songNumber],"New Layer Volume");
				gA.songs[songNumber].volumes[j] = newLayerVolume;
			}
			// Whitespace
			EditorGUILayout.LabelField (" ");
		}
	}

	public static void createFirstBlankGrumbleSong (grumbleAMP gA) {
		gA.songs = new grumbleSong[1];
		gA.songs[0] = (grumbleSong)ScriptableObject.CreateInstance ("grumbleSong");
		gA.songs[0].textName = "SONG 0";
		gA.songs[0].layer = new AudioClip[1];
		gA.songs[0].layerNames = new string[1];
		gA.songs[0].layerNames[0] = "Layer 0";
		gA.songs[0].volumes = new float[1];
		gA.songs[0].volumes[0] = 1f;
		gA.songs[0].layerResourceNames = new string[1];
		gA.songs[0].layerResourceNames[0] = "";
	}

	public static void createGrumbleSong (grumbleAMP gA, int songNumber) {
		gA.songs[songNumber] = (grumbleSong)ScriptableObject.CreateInstance ("grumbleSong");
		gA.songs[songNumber].textName = "SONG " + songNumber.ToString ();
		gA.songs[songNumber].layer = new AudioClip[1];
		gA.songs[songNumber].layerNames = new string[1];
		gA.songs[songNumber].layerNames[0] = "Layer 0";
		gA.songs[songNumber].volumes = new float[1];
		gA.songs[songNumber].volumes[0] = 1f;
		gA.songs[songNumber].layerResourceNames = new string[1];
		gA.songs[songNumber].layerResourceNames[0] = "";
	}


	public static void showGrumbleSongSpecificSettings (grumbleAMP gA, int songNumber) {
		GUIStyle songTitleStyle = new GUIStyle();
		songTitleStyle.fontStyle = FontStyle.Bold;

		if (gA.songs[songNumber] == null) {
			createGrumbleSong(gA,songNumber);
		}

		EditorGUILayout.LabelField (gA.songs[songNumber].textName + "  (SONG " + songNumber.ToString () + ")",songTitleStyle);

		string newSongName = EditorGUILayout.TextField ("Custom Name",gA.songs[songNumber].textName);
		if (gA.songs[songNumber].textName.CompareTo (newSongName) != 0) {
			Undo.RecordObject (gA.songs[songNumber],"New Song Name");
			gA.songs[songNumber].textName = newSongName;
		}

		float newLoopingCrossfade = EditorGUILayout.FloatField ("Looping Crossfade",gA.songs[songNumber].getLoopCrossfadeBy());
		if (gA.songs[songNumber].getLoopCrossfadeBy() != newLoopingCrossfade) {
			Undo.RecordObject (gA.songs[songNumber],"New Looping Crossfade");
			gA.songs[songNumber].setLoopCrossfadeBy (newLoopingCrossfade);
		}

		float newLayerCrossfade = EditorGUILayout.FloatField ("Layer Crossfade",gA.songs[songNumber].getLayerCrossfadeBy());
		if (gA.songs[songNumber].getLayerCrossfadeBy() != newLayerCrossfade) {
			Undo.RecordObject (gA.songs[songNumber],"New Layer Crossfade");
			gA.songs[songNumber].setLayerCrossfadeBy (newLayerCrossfade);
		}
		
		bool loopingEnabled = EditorGUILayout.Toggle("Looping Enabled",gA.songs[songNumber].getLoop ());
		if (gA.songs[songNumber].getLoop () != loopingEnabled) {
			Undo.RecordObject (gA.songs[songNumber],"Change Song Looping");
			gA.songs[songNumber].setLoop (loopingEnabled);
		}
	}
}
