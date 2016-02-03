using UnityEngine;
using System.Collections;

public class StageTrigger_Link : MonoBehaviour {

	// === 외부 파라미터（Inspector 표시） =====================
	public string 	jumpSceneName;
	public string 	jumpLabelName;

	public int 		jumpDir 			= 0;
	public bool		jumpInput 			= true; // fales = AutoJump
	public float	jumpDelayTime 		= 0.0f;

	// === 내부 파라미터 ======================================
	Transform		 playerTrfm;
	PlayerController playerCtrl;
	
	// === 코드（Monobehaviour 기본 기능 구현） ================
	void Awake() {
		playerTrfm = PlayerController.GetTranform();
		playerCtrl = playerTrfm.GetComponent<PlayerController> ();
	}

	void OnTriggerEnter2D_PlayerEvent (GameObject go) {
		if (!jumpInput) {
			Jump ();
		}
	}

	// === 코드（씬 점프 구현） ========================
	public void Jump() {
		// 점프할 곳을 설정
		if (jumpSceneName == "") {
			jumpSceneName = Application.loadedLevelName;
		}

		// 체크 포인트
		PlayerController.checkPointEnabled   = true;
		PlayerController.checkPointLabelName = jumpLabelName;
		PlayerController.checkPointSceneName = jumpSceneName;
		PlayerController.checkPointHp 		 = PlayerController.nowHp;

		playerCtrl.ActionMove (0.0f);
		playerCtrl.activeSts = false;

		Invoke("JumpWork",jumpDelayTime);
	}

	void JumpWork() {
		playerCtrl.activeSts = true;

		if (Application.loadedLevelName == jumpSceneName) {
			// 씬 안에서 점프
			GameObject[] stageLinkList = GameObject.FindGameObjectsWithTag ("EventTrigger");
			foreach (GameObject stageLink in stageLinkList) {
				if (stageLink.GetComponent<StageTrigger_CheckPoint>().labelName == jumpLabelName) {
					playerTrfm.position = stageLink.transform.position;
					playerCtrl.groundY 	= playerTrfm.position.y;
					Camera.main.transform.position = new Vector3(playerTrfm.position.x,playerTrfm.position.y,-10.0f);
					break;
				}
			}
		} else {
			// 씬 밖으로 점프
			Application.LoadLevel (jumpSceneName);
		}
	}
}
