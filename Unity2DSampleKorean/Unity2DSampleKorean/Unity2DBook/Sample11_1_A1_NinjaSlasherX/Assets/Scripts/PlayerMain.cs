using UnityEngine;
using System.Collections;

public class PlayerMain : MonoBehaviour {

	// === 내부 파라미터 ==========================================
	PlayerController 	playerCtrl;

	bool 				actionEtcRun = true;

	// === 코드（Monobehaviour 기본기능 구현） ================
	void Awake () {
		playerCtrl 		= GetComponent<PlayerController>();
	}

	void Update () {
		// 조작 가능한지 검사
		if (!playerCtrl.activeSts) {
			return;
		}

		// 이동
		float joyMv = Input.GetAxis ("Horizontal");
//		joyMv = Mathf.Pow(Mathf.Abs(joyMv),3.0f) * Mathf.Sign(joyMv);
		playerCtrl.ActionMove (joyMv);

		// 점프
		if (Input.GetButtonDown ("Jump")) {
			playerCtrl.ActionJump ();
			return;
		}

		// 공격
		if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2") || Input.GetButtonDown("Fire3")) {
			if (Input.GetAxisRaw ("Vertical") < 0.5f) {
				playerCtrl.ActionAttack();
			} else {
				//Debug.Log (string.Format ("Vertical {0} {1}",Input.GetAxisRaw ("Vertical"),vp.vertical));
				playerCtrl.ActionAttackJump();
			}
			return;
		}

		// 문을 열거나 통로에 들어간다
		if (Input.GetAxisRaw ("Vertical") > 0.7f) {
			if (actionEtcRun) {
				playerCtrl.ActionEtc ();
				actionEtcRun = false;
			}
		} else {
			actionEtcRun = true;
		}
	}
}
