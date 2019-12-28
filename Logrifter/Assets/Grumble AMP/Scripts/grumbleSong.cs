// Container that holds all the layers of a song,
// and the settings for that song.

using UnityEngine;
using System.Collections;

[System.Serializable]
public class grumbleSong : ScriptableObject {
	public string textName = "SONG 0";
	[SerializeField]
	public AudioClip[] layer;
	public string[] layerNames;
	public float[] volumes;
	public float loopCrossfadeBy = 0f;
	public float layerCrossfadeBy = 1.5f;
	public bool loop = true;
	public string[] layerResourceNames;

	public void setLoop(bool loopOn) {
		loop = loopOn;
	}

	public bool getLoop() {
		return loop;
	}

	public void setLoopCrossfadeBy(float crossfadeBy) {
		loopCrossfadeBy = crossfadeBy;
	}

	public void setLayerCrossfadeBy(float crossfadeBy) {
		layerCrossfadeBy = crossfadeBy;
	}

	public float getLoopCrossfadeBy() {
		return loopCrossfadeBy;
	}

	public float getLayerCrossfadeBy() {
		return layerCrossfadeBy;
	}

	public float getLayerVolume(int layerNumber) {
		if (layerNumber < volumes.Length && layerNumber >= 0) {
			return volumes[layerNumber];
		}
		else {
			return 0f;
		}
	}

	public bool setLayerVolume(int layerNumber, float newVolume) {
		bool failure = false;
		if (layerNumber < volumes.Length && layerNumber >= 0) {
			volumes[layerNumber] = Mathf.Clamp (newVolume,0f,1f);
		}
		else {
			failure = true;
		}
		return failure;
	}

	public string getLayerName(int layerNumber) {
		if (layerNumber < layerNames.Length && layerNumber >= 0) {
			return layerNames[layerNumber];
		}
		else {
			return "";
		}
	}

	public bool setLayerName(int layerNumber, string newName) {
		bool failure = false;
		if (layerNumber < layerNames.Length && layerNumber >= 0) {
			layerNames[layerNumber] = newName;
		}
		else {
			failure = true;
		}
		return failure;
	}

	public string getLayerResourceName(int layerNumber) {
		if (layerNumber < layerResourceNames.Length && layerNumber >= 0) {
			return layerResourceNames[layerNumber];
		}
		else {
			return "";
		}
	}

	public bool setLayerResourceName(int layerNumber, string newName) {
		bool failure = false;
		if (layerNumber < layerResourceNames.Length && layerNumber >= 0) {
			layerResourceNames[layerNumber] = newName;
		}
		else {
			failure = true;
		}
		return failure;
	}
	
	public AudioClip getLayerAudioClip(int layerNumber) {
		if (layerNumber < layer.Length && layerNumber >= 0) {
			return layer[layerNumber];
		}
		else {
			return null;
		}
	}

	public bool setLayerAudioClip(int layerNumber, AudioClip newClip) {
		bool failure = false;
		if (layerNumber < layer.Length && layerNumber >= 0) {
			layer[layerNumber] = newClip;
		}
		else {
			failure = true;
		}
		return failure;
	}
}