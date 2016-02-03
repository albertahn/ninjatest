using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour {

	GameObject 	goShell = null;
	bool		action 	= false;

	// Use this for initialization
	void Start () {
		// 포탄 게임 오브젝트를 가져오고, 포탄을 표시하지 않도록 설정
		goShell = transform.FindChild("Tank_Shell").gameObject;
		goShell.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		// 버튼이 눌러졌는지 검사
		if (Input.GetMouseButton(0)) {
			// 탱크가 클릭됐는지 검사
			Vector2 tapPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Collider2D collition2d = Physics2D.OverlapPoint(tapPoint);
			if (collition2d) {
				if (collition2d.gameObject == gameObject) {
					// 액션 유효화
					action = true;
				}
			}
			// 버튼이 눌려진 상태인지 검사
			if (action) {
				// 탱크 이동
				rigidbody2D.AddForce (new Vector2(+30.0f, 0.0f));
			}
		} else
		// 버튼이 놓아졌는지 검사
		if (Input.GetMouseButtonUp(0) && action) {
			// 포탄 발사
			if (goShell)	{
				goShell.SetActive (true);
				goShell.rigidbody2D.AddForce (new Vector2(+300.0f,500.0f));
				Destroy(goShell.gameObject,3.0f);
			}
			action = false;
		}
	}
	
	void OnGUI() {
		GUI.TextField(new Rect(10,10,300,60), 
		              "[Unity 2D Sample 2-1]\n" + 
		              "탱크를 클릭하면 가속\n놓으면 발사!");
		if (GUI.Button(new Rect(10, 80, 100, 20), "다시 시작")) {
			Application.LoadLevel(Application.loadedLevelName);
		}
	}
}
