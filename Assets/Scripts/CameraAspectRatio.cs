using UnityEngine;
using System.Collections;

public class CameraAspectRatio : MonoBehaviour {

	// ASPECT RATIOS:
	// iPad 	1.333
	// iPhone4S 1.500
	// iPhone5S 1.775
	// iPhone6S 1.7786

	public float defaultSize;

	void Awake() {
		UpdateAspectRatio();
	}

	void Update() {
#if UNITY_EDITOR
		UpdateAspectRatio();
#endif
	}

	private void UpdateAspectRatio() {
		float  ar= (float)Screen.width / (float)Screen.height;
//		Debug.Log (string.Format ("w:{0} h:{1} ar:{2}", Screen.width, Screen.height, ar));
		if (ar < 0.6666) {
			gameObject.GetComponent<Camera>().orthographicSize= defaultSize * 0.6666f / ar; 
		} else {
			gameObject.GetComponent<Camera>().orthographicSize = defaultSize;
		}
	}
	
}