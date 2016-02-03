using UnityEngine;
using System.Collections;

public class PlayerMain : MonoBehaviour {

	// === 내부 파라미터 ==========================================
	PlayerController 	playerCtrl;
	zFoxVirtualPad 		vpad;

	bool 				actionEtcRun = true;

	// === 코드（Monobehaviour 기본기능 구현） ================
	void Awake () {
		playerCtrl 		= GetComponent<PlayerController>();
		vpad 			= GameObject.FindObjectOfType<zFoxVirtualPad> ();
	}

	void Update () {
		// 조작할 수 있는지 검사
		if (!playerCtrl.activeSts) {
			return;
		}

		// 가상 패드
		float vpad_vertical 	= 0.0f;
		float vpad_horizontal 	= 0.0f;
		zFOXVPAD_BUTTON  vpad_btnA = zFOXVPAD_BUTTON.NON;
		zFOXVPAD_BUTTON  vpad_btnB = zFOXVPAD_BUTTON.NON;
		if (vpad != null) {
			vpad_vertical 	= vpad.vertical;
			vpad_horizontal = vpad.horizontal;
			vpad_btnA 		= vpad.buttonA;
			vpad_btnB 		= vpad.buttonB;
		}


		// 이동
		float joyMv = Input.GetAxis ("Horizontal");
//		joyMv = Mathf.Pow(Mathf.Abs(joyMv),3.0f) * Mathf.Sign(joyMv);

		float vpadMv = vpad_horizontal;
		vpadMv = Mathf.Pow(Mathf.Abs(vpadMv),1.5f) * Mathf.Sign(vpadMv);
		playerCtrl.ActionMove (joyMv + vpadMv);


		// 점프
		if (Input.GetButtonDown ("Jump") || vpad_btnA == zFOXVPAD_BUTTON.DOWN) {
			playerCtrl.ActionJump ();
			return;
		}

		// 공격
		if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2") || Input.GetButtonDown("Fire3") || 
		    vpad_btnB == zFOXVPAD_BUTTON.DOWN) {
			if (Input.GetAxisRaw ("Vertical") + vpad_vertical < 0.5f) {
				playerCtrl.ActionAttack();
			} else {
				//Debug.Log (string.Format ("Vertical {0} {1}",Input.GetAxisRaw ("Vertical"),vp.vertical));
				playerCtrl.ActionAttackJump();
			}
			return;
		}

		// 문을 열거나 통로에 들어간다
		if (Input.GetAxisRaw ("Vertical") + vpad_vertical > 0.7f) {
			Debug.Log("actionEtcRun = " +actionEtcRun);
			if (actionEtcRun) {
				playerCtrl.ActionEtc ();
				actionEtcRun = false;
			}
		} else {
			actionEtcRun = true;
		}
	}
}
