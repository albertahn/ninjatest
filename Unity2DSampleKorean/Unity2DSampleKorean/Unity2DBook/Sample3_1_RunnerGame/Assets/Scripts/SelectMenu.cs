using UnityEngine;
using System.Collections;

public class SelectMenu : MonoBehaviour {

	// UnityGUI표시
	void OnGUI() {
		// GUI 표시 스케일 설정
		Vector3 screenScale = new Vector3 (Screen.width / 640.0f, Screen.height / 480.0f, 1.0f);
		GUI.matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, screenScale);

		// 디버그 텍스트
		GUI.TextField(new Rect(10,10,300,60), "[Unity2D Sample 3-1]");
		// 버튼
		if ( GUI.Button( new Rect(10, 80, 200, 20), "Sample 3-1 StageA" ) ) {
			Application.LoadLevel ("StageA");
		}
		if ( GUI.Button( new Rect(10,110, 200, 20), "Sample 3-1 StageB1" ) ) {
			Application.LoadLevel ("StageB1");
		}
		if ( GUI.Button( new Rect(10,140, 200, 20), "Sample 3-1 StageB2" ) ) {
			Application.LoadLevel ("StageB2");
		}
		if ( GUI.Button( new Rect(10,170, 200, 20), "Sample 3-1 StageB3" ) ) {
			Application.LoadLevel ("StageB3");
		}
		if ( GUI.Button( new Rect(10,200, 200, 20), "Sample 3-1 StageB4" ) ) {
			Application.LoadLevel ("StageB4");
		}
		if ( GUI.Button( new Rect(10,230, 200, 20), "Sample 3-1 StageC" ) ) {
			Application.LoadLevel ("StageC");
		}
	}

}
