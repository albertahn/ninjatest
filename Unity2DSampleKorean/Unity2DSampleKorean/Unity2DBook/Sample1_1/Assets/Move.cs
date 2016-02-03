using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// 여기부터
		float vx = Input.GetAxis ("Horizontal");
		float vy = Input.GetAxis ("Vertical");
		transform.Translate(new Vector3 (vx, vy, 0.0f));
		// 여기까지 추가한다
	}
}
